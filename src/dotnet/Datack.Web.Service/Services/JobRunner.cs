using Datack.Common.Models.Data;
using Datack.Web.Service.Tasks;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services;

public class JobRunner
{
    public static readonly SemaphoreSlim ExecuteJobRunLock = new(1, 1);
    private static readonly SemaphoreSlim SetupJobRunLock = new(1, 1);

    private readonly JobRuns _jobRuns;
    private readonly JobRunTasks _jobRunTasks;
    private readonly JobTasks _jobTasks;
    private readonly ILogger<JobRunner> _logger;
    private readonly RemoteService _remoteService;
    private readonly Emails _emails;

    private readonly Dictionary<String, IBaseTask> _tasks;

    /// <summary>
    ///     Initialize the job scheduler and cache all the tasks, then setup the events for each task.
    /// </summary>
    public JobRunner(ILogger<JobRunner> logger,
                     RemoteService remoteService,
                     Emails emails,
                     JobRuns jobRuns,
                     JobTasks jobTasks,
                     JobRunTasks jobRunTasks,
                     CreateBackupTask createBackupTask,
                     DownloadAzureFileTask downloadAzureFileTask)
    {
        _logger = logger;
        _remoteService = remoteService;
        _emails = emails;
        _jobRuns = jobRuns;
        _jobTasks = jobTasks;
        _jobRunTasks = jobRunTasks;

        _logger.LogTrace("Constructor");

        _tasks = new()
        {
            {
                "createBackup", createBackupTask
            },
            {
                "downloadAzure", downloadAzureFileTask
            }
        };
    }
        
    public async Task Stop(Guid jobRunId, CancellationToken cancellationToken)
    {
        var jobRunTasks = await _jobRunTasks.GetByJobRunId(jobRunId, cancellationToken);

        await _jobRuns.UpdateStop(jobRunId, cancellationToken);

        foreach (var jobRunTask in jobRunTasks.Where(m => m.Completed == null))
        {
            await _jobRunTasks.UpdateCompleted(jobRunTask.JobRunTaskId, "Task was stopped", null, true, cancellationToken);

            try
            {
                await _remoteService.Stop(jobRunTask.JobTask.Agent, jobRunTask.JobRunTaskId, cancellationToken);
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    ///     Setup a new job.
    /// </summary>
    public async Task<Guid> SetupJobRun(Job job, IList<String>? overrideItemList, CancellationToken cancellationToken)
    {
        _logger.LogDebug("SetJobRun {jobId} for backup job {name}", job.JobId, job.Name);

        // Make sure only 1 process setup a new job otherwise it's possible that a job is duplicated.
        var receivedLockSuccesfully = await SetupJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

        if (!receivedLockSuccesfully)
        {
            // Lock timed out
            throw new($"Could not obtain runSetupLock within 30 seconds for job {job.Name}!");
        }

        _logger.LogDebug("Entered lock for job {name}", job.Name);

        try
        {
            // Always keep a record of the run.
            var jobRun = new JobRun
            {
                JobRunId = Guid.NewGuid(),
                JobId = job.JobId,
                Started = DateTimeOffset.UtcNow,
                IsError = false,
                Result = null
            };

            await _jobRuns.Create(jobRun, cancellationToken);

            jobRun.Job = job;

            _logger.LogDebug("Created jobRun record {jobRunId} for job {name}", jobRun.JobRunId, job.Name);

            try
            {
                // Figure out if this job is already running, if so, stop execution.
                var runningJobs = await _jobRuns.GetRunning(cancellationToken);

                // Only check for tasks for this group, and filter itself out
                runningJobs = runningJobs.Where(m => m.JobRunId != jobRun.JobRunId && m.Job.Group == job.Group).ToList();

                _logger.LogDebug("Found {count} already running tasks for job group {group}", runningJobs.Count, job.Group);

                // Get all the tasks for the job.
                var runningTasks = new List<JobRunTask>();
                foreach (var runningJob in runningJobs)
                {
                    var runningTasksForJob = await _jobRunTasks.GetByJobRunId(runningJob.JobRunId, cancellationToken);
                    runningTasks.AddRange(runningTasksForJob);
                }

                // Get all the tasks for the job and run the through the task Setup procedure.
                // This will return a list of JobRunTasks, basically a queue for each item.
                // This queue will be used to run the tasks for each item.
                var jobTasks = await _jobTasks.GetForJob(job.JobId, cancellationToken);

                jobTasks = jobTasks.Where(m => m.IsActive).ToList();

                _logger.LogDebug("Found {count} job tasks for job {name}", jobTasks.Count, job.Name);

                var allJobRunTasks = new List<JobRunTask>();

                foreach (var jobTask in jobTasks)
                {
                    _logger.LogDebug("Setting up job run task {type} for job {name}", jobTask.Type, job.Name);

                    var previousJobRunTasks = new List<JobRunTask>();

                    if (jobTask.UsePreviousTaskArtifactsFromJobTaskId != null)
                    {
                        previousJobRunTasks = allJobRunTasks.Where(m => m.JobTaskId == jobTask.UsePreviousTaskArtifactsFromJobTaskId).ToList();
                    }

                    List<JobRunTask> jobRunTasks;

                    if (overrideItemList != null && overrideItemList.Count > 0)
                    {
                        var itemIndex = 0;
                        jobRunTasks = overrideItemList.Select(m => new JobRunTask
                                                      {
                                                          JobRunTaskId = Guid.NewGuid(),
                                                          JobTaskId = jobTask.JobTaskId,
                                                          JobRunId = jobRun.JobRunId,
                                                          Type = jobTask.Type,
                                                          ItemName = m,
                                                          ItemOrder = itemIndex++,
                                                          IsError = false,
                                                          Result = null,
                                                          Settings = jobTask.Settings
                                                      })
                                                      .ToList();
                    }
                    else if (_tasks.TryGetValue(jobTask.Type, out var task))
                    {
                        jobRunTasks = await task.Setup(job, jobTask, previousJobRunTasks, jobRun.JobRunId, cancellationToken);   
                    }
                    else
                    {
                        jobRunTasks = previousJobRunTasks
                                      .Select(m => new JobRunTask
                                      {
                                          JobRunTaskId = Guid.NewGuid(),
                                          JobTaskId = jobTask.JobTaskId,
                                          JobRunId = jobRun.JobRunId,
                                          Type = jobTask.Type,
                                          ItemName = m.ItemName,
                                          ItemOrder = m.ItemOrder,
                                          IsError = false,
                                          Result = null,
                                          Settings = jobTask.Settings
                                      })
                                      .ToList();
                    }

                    _logger.LogDebug("Received {count} new job run tasks for {type} for job {name}", jobRunTasks.Count, jobTask.Type, job.Name);

                    // Check if the task is being executed or pending execution in a previous task,
                    // If so, skip it in the run.
                    foreach (var jobRunTask in jobRunTasks)
                    {
                        if (runningTasks.Count(m => m.ItemName == jobRunTask.ItemName && m.Completed == null) > 0)
                        {
                            _logger.LogDebug("Skipping task {type} for job {name} as it's already running", jobTask.Type, job.Name);
                        }
                        else
                        {
                            allJobRunTasks.Add(jobRunTask);
                        }
                    }
                }

                _logger.LogDebug("Received  {count} new job run tasks in total for job {name}", allJobRunTasks.Count, job.Name);

                var index = 0;
                // Make sure that the order the queue runs in is the same as the order of the tasks of the job.
                foreach (var jobRunTaskGroup in allJobRunTasks.GroupBy(m => m.JobTaskId))
                {
                    foreach (var jobRunTask in jobRunTaskGroup)
                    {
                        jobRunTask.TaskOrder = index;
                    }

                    index++;
                }

                // Add all the run tasks to the database and execute the job.
                await _jobRunTasks.Create(allJobRunTasks, cancellationToken);

                _logger.LogDebug("Finished setting up job run tasks for job {name}", job.Name);

                await ExecuteJobRun(jobRun.JobRunId, cancellationToken);
            }
            catch (Exception ex)
            {
                jobRun.Result = $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
                jobRun.IsError = true;
                jobRun.Completed = DateTimeOffset.UtcNow;

                await _jobRuns.Update(jobRun, cancellationToken);

                await _emails.SendComplete(jobRun, cancellationToken);
            }

            return jobRun.JobRunId;
        }
        finally
        {
            _logger.LogDebug("Releasing lock for job {name}", job.Name);
            SetupJobRunLock.Release();
        }
    }

    public async Task ExecuteJobRun(Guid jobRunId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("ExecuteJobRun for job run {jobRunId}", jobRunId);

        // Make sure only 1 process executes a job run otherwise it might run duplicate tasks.
        var receivedLockSuccesfully = await ExecuteJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

        if (!receivedLockSuccesfully)
        {
            // Lock timed out
            _logger.LogError("Could not obtain executeJobRunLock within 30 seconds for job run {jobRunId}!", jobRunId);

            return;
        }

        _logger.LogDebug("Entering lock for job run {jobRunId}", jobRunId);

        try
        {
            var jobRun = await _jobRuns.GetById(jobRunId, cancellationToken);

            if (jobRun == null)
            {
                _logger.LogDebug("Job run not found for {jobRunId}", jobRunId);

                return;
            }

            _logger.LogDebug("Found run for run job {jobRunId} {name}", jobRunId, jobRun.Job.Name);

            if (jobRun.Completed.HasValue)
            {
                _logger.LogDebug("Job run is already completed for job {jobRunId} {name}", jobRunId, jobRun.Job.Name);

                return;
            }

            // Get all the run tasks for the job.
            var jobRunTasks = await _jobRunTasks.GetByJobRunId(jobRun.JobRunId, cancellationToken);

            // Figure out the Pending, Running and Completed statuses for the run task list.
            var pendingJobRunTasks = jobRunTasks.Where(m => m.Started == null && m.Completed == null).ToList();
            var runningJobRunTasks = jobRunTasks.Where(m => m.Started != null && m.Completed == null).ToList();
            var completedJobRunTasks = jobRunTasks.Where(m => m.Started != null && m.Completed != null).ToList();

            _logger.LogDebug("Found {jobRunTasksCount} run tasks, Pending: {pendingJobRunTasksCount}, Running: {runningJobRunTasksCount}, Completed: {completedJobRunTasksCount} for job {jobRunId} {name}",
                             jobRunTasks.Count,
                             pendingJobRunTasks.Count,
                             runningJobRunTasks.Count,
                             completedJobRunTasks.Count,
                             jobRunId,
                             jobRun.Job.Name);

            // If there are no pending or running tasks left, the job run is completed.
            if (!pendingJobRunTasks.Any() && !runningJobRunTasks.Any())
            {
                _logger.LogDebug("Marking job run as complete for job {jobRunId} {name}", jobRunId, jobRun.Job.Name);

                await _jobRuns.UpdateComplete(jobRunId, cancellationToken);

                return;
            }

            foreach (var jobRunTask in pendingJobRunTasks)
            {
                // Update tasks as they could've updated it in this loop
                runningJobRunTasks = jobRunTasks.Where(m => m.Started != null && m.Completed == null).ToList();
                completedJobRunTasks = jobRunTasks.Where(m => m.Started != null && m.Completed != null).ToList();

                // Check if the previous task is completed for this item
                if (jobRunTask.TaskOrder > 0)
                {
                    var previousTask = completedJobRunTasks.FirstOrDefault(m => m.ItemName == jobRunTask.ItemName && m.TaskOrder == jobRunTask.TaskOrder - 1);

                    if (previousTask == null)
                    {
                        continue;
                    }
                }

                // Check how many job run tasks are currently in progress for the same task.
                var taskRunning = runningJobRunTasks.Count(m => m.JobTaskId == jobRunTask.JobTaskId);

                // Calculate how many parallel tasks we can still run for this task.
                var taskSpacePending = jobRunTask.JobTask.Parallel - taskRunning;

                // Check how many items are pending for the next task, otherwise skip.
                if (jobRunTask.JobTask.MaxItemsToKeep > 0)
                {
                    var pendingForNextTask = jobRunTasks
                                             .Where(m => m.JobTaskId == jobRunTask.JobTaskId & m.Completed != null)
                                             .Select(t => jobRunTasks.FirstOrDefault(m => m.ItemName == t.ItemName && m.TaskOrder == jobRunTask.TaskOrder + 1))
                                             .Where(t => t != null)
                                             .Count(nextTask => nextTask!.Completed == null);

                    if (pendingForNextTask > jobRunTask.JobTask.MaxItemsToKeep)
                    {
                        _logger.LogTrace("Skipping Task {jobRunTaskId} ({itemName}) for type {type}. Found {pendingForNextTask} tasks pending for the next task, max items to keep is {maxItemsToKeep} for {jobRunId} {name}",
                                         jobRunTask.JobRunTaskId,
                                         jobRunTask.ItemName,
                                         jobRunTask.Type,
                                         pendingForNextTask,
                                         jobRunTask.JobTask.MaxItemsToKeep,
                                         jobRunId,
                                         jobRun.Job.Name);
                        continue;
                    }
                }
                    
                // If there is a place in the queue, start the task.
                if (taskSpacePending <= 0)
                {
                    _logger.LogTrace("Skipping Task {jobRunTaskId} ({itemName}) for type {type}. Found {count} tasks running, max parallel is {parallel} for {jobRunId} {name}",
                                     jobRunTask.JobRunTaskId,
                                     jobRunTask.ItemName,
                                     jobRunTask.Type,
                                     taskRunning,
                                     jobRunTask.JobTask.Parallel,
                                     jobRunId,
                                     jobRun.Job.Name);

                    continue;
                }

                    
                _logger.LogDebug("Starting task {jobRunTaskId} ({itemName}) for type {type}. Found {count} tasks running, max parallel is {parallel} for {jobRunId} {name}",
                                 jobRunTask.JobRunTaskId,
                                 jobRunTask.ItemName,
                                 jobRunTask.Type,
                                 taskRunning,
                                 jobRunTask.JobTask.Parallel,
                                 jobRunId,
                                 jobRun.Job.Name);

                // Find the previous task for this item and pass it down
                JobRunTask? previousArtifactTask = null;

                if (jobRunTask.JobTask.UsePreviousTaskArtifactsFromJobTaskId != null)
                {
                    previousArtifactTask = completedJobRunTasks.FirstOrDefault(m => m.ItemName == jobRunTask.ItemName &&
                                                                                    m.JobTaskId == jobRunTask.JobTask.UsePreviousTaskArtifactsFromJobTaskId);
                }

                // Mark the task as started
                jobRunTask.Started = DateTimeOffset.UtcNow;
                await _jobRunTasks.UpdateStarted(jobRunTask.JobRunTaskId, jobRunTask.Started, cancellationToken);

                _ = Task.Run(async () =>
                             {
                                 await _remoteService.Run(jobRunTask.JobTask.Agent, jobRunTask, previousArtifactTask, cancellationToken);
                             },
                             cancellationToken);

                await Task.Delay(100, cancellationToken);
            }
        }
        finally
        {
            _logger.LogDebug("Releasing lock for job run {jobRunId}", jobRunId);
            ExecuteJobRunLock.Release();
        }
    }
}