using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Service.Tasks;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services
{
    public class JobRunner
    {
        private readonly JobRuns _jobRuns;
        private readonly JobRunTaskLogs _jobRunTaskLogs;
        private readonly JobRunTasks _jobRunTasks;
        private readonly Jobs _jobs;
        private readonly JobTasks _jobTasks;
        private readonly ILogger<JobRunner> _logger;
        private readonly RemoteService _remoteService;

        private readonly SemaphoreSlim _setupJobRunLock = new(1, 1);
        private readonly SemaphoreSlim _executeJobRunLock = new(1, 1);

        private readonly Dictionary<String, IBaseTask> _tasks;

        /// <summary>
        ///     Initialize the job scheduler and cache all the tasks, then setup the events for each task.
        /// </summary>
        public JobRunner(ILogger<JobRunner> logger,
                         RemoteService remoteService,
                         Jobs jobs,
                         JobRuns jobRuns,
                         JobTasks jobTasks,
                         JobRunTasks jobRunTasks,
                         JobRunTaskLogs jobRunTaskLogs,
                         CreateBackupTask createBackupTask,
                         CompressTask compressTask,
                         UploadAzureTask uploadAzureTask,
                         UploadS3Task uploadS3Task)
        {
            _logger = logger;
            _remoteService = remoteService;
            _jobs = jobs;
            _jobRuns = jobRuns;
            _jobTasks = jobTasks;
            _jobRunTasks = jobRunTasks;
            _jobRunTaskLogs = jobRunTaskLogs;

            _logger.LogTrace("Constructor");

            _tasks = new Dictionary<String, IBaseTask>
            {
                {
                    "create_backup", createBackupTask
                },
                {
                    "compress", compressTask
                },
                {
                    "upload_azure", uploadAzureTask
                },
                {
                    "upload_s3", uploadS3Task
                }
            };
        }

        /// <summary>
        ///     Run a job based on the ID.
        /// </summary>
        /// <param name="jobId">The ID of the Job</param>
        /// <param name="cancellationToken"></param>
        public async Task<Guid> Run(Guid jobId, CancellationToken cancellationToken)
        {
            var job = await _jobs.GetById(jobId, cancellationToken);

            if (job == null)
            {
                throw new Exception($"Job with ID {jobId} not found");
            }

            return await SetupJobRun(job, cancellationToken);
        }

        public async Task Stop(Guid jobRunId, CancellationToken cancellationToken)
        {
            var jobRunTasks = await _jobRunTasks.GetByJobRunId(jobRunId, cancellationToken);

            await _jobRuns.UpdateStop(jobRunId, cancellationToken);

            foreach (var jobRunTask in jobRunTasks.Where(m => m.Completed == null))
            {
                if (jobRunTask.Started == null)
                {
                    await _jobRunTasks.UpdateStarted(jobRunTask.JobRunTaskId, cancellationToken);
                }
                await _jobRunTasks.UpdateCompleted(jobRunTask.JobRunTaskId, "Task was stopped", null, true, cancellationToken);
                await _remoteService.Stop(jobRunTask, cancellationToken);
            }
        }

        /// <summary>
        ///     Setup a new job.
        /// </summary>
        private async Task<Guid> SetupJobRun(Job job, CancellationToken cancellationToken)
        {
            _logger.LogDebug("SetJobRun {jobId} for backup job {name}", job.JobId, job.Name);

            // Make sure only 1 process setup a new job otherwise it's possible that a job is duplicated.
            var receivedLockSuccesfully = await _setupJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

            if (!receivedLockSuccesfully)
            {
                // Lock timed out
                throw new Exception($"Could not obtain runSetupLock within 30 seconds for job {job.Name}!");
            }

            _logger.LogDebug("Entered lock for job {name}", job.Name);

            try
            {
                // Always keep a record of the run.
                var jobRun = new JobRun
                {
                    JobRunId = Guid.NewGuid(),
                    JobId = job.JobId,
                    Started = DateTimeOffset.Now,
                    IsError = false,
                    Result = null
                };

                await _jobRuns.Create(jobRun, cancellationToken);

                _logger.LogDebug("Created jobRun record {jobRunId} for job {name}", jobRun.JobRunId, job.Name);

                try
                {
                    // Figure out if this job is already running, if so, stop execution.
                    var runningTasks = await _jobRuns.GetRunning(job.JobId, cancellationToken);

                    // Filter out itself..
                    runningTasks = runningTasks.Where(m => m.JobRunId != jobRun.JobRunId).ToList();

                    _logger.LogDebug("Found {count} already running tasks for job {name}", runningTasks.Count, job.Name);

                    if (runningTasks.Count > 0)
                    {
                        var runningTasksList = String.Join(", ", runningTasks.Select(m => $"{m.JobRunId} (started {m.Started:g})"));

                        throw new Exception($"Cannot start job {job.Name}, there is already another job still running ({runningTasksList})");
                    }

                    // Get all the tasks for the job and run the through the task Setup procedure.
                    // This will return a list of JobRunTasks, basically a queue for each item.
                    // This queue will be used to run the tasks for each item.
                    var jobTasks = await _jobTasks.GetForJob(job.JobId, cancellationToken);

                    _logger.LogDebug("Found {count} job tasks for job {name}", jobTasks.Count, job.Name);

                    var allJobRunTasks = new List<JobRunTask>();

                    foreach (var jobTask in jobTasks)
                    {
                        var task = GetTask(jobTask.Type);

                        _logger.LogDebug("Setting up job run task {type} for job {name}", jobTask.Type, job.Name);

                        var previousJobRunTasks = new List<JobRunTask>();

                        if (jobTask.UsePreviousTaskArtifactsFromJobTaskId != null)
                        {
                            previousJobRunTasks = allJobRunTasks.Where(m => m.JobTaskId == jobTask.UsePreviousTaskArtifactsFromJobTaskId).ToList();
                        }

                        var jobRunTasks = await task.Setup(job, jobTask, previousJobRunTasks, jobRun.JobRunId, cancellationToken);

                        _logger.LogDebug("Received {count} new job run tasks for {type} for job {name}", jobRunTasks.Count, jobTask.Type, job.Name);

                        allJobRunTasks.AddRange(jobRunTasks);
                    }

                    _logger.LogDebug("Received  {count} new job run tasks in total for job {name}", allJobRunTasks.Count, job.Name);

                    // Make sure that the order the queue runs in is the same as the order of the tasks of the job.
                    foreach (var jobRunTask in allJobRunTasks)
                    {
                        var jobTask = jobTasks.First(m => m.JobTaskId == jobRunTask.JobTaskId);

                        jobRunTask.TaskOrder = jobTask.Order;
                    }

                    // Add all the run tasks to the database and execute the job.
                    await _jobRunTasks.Create(allJobRunTasks, cancellationToken);

                    _logger.LogDebug("Finished setting up job run tasks for job {name}", job.Name);

                    await ExecuteJobRun(jobRun.JobRunId, cancellationToken);
                }
                catch (Exception ex)
                {
                    jobRun.Result = ex.Message;
                    jobRun.IsError = true;
                    jobRun.Completed = DateTimeOffset.Now;

                    await _jobRuns.Update(jobRun, cancellationToken);
                }

                return jobRun.JobRunId;
            }
            finally
            {
                _logger.LogDebug("Releasing lock for job {name}", job.Name);
                _setupJobRunLock.Release();
            }
        }

        private async Task ExecuteJobRun(Guid jobRunId, CancellationToken cancellationToken)
        {
            _logger.LogDebug("ExecuteJobRun for job run {jobRunId}", jobRunId);

            // Make sure only 1 process executes a job run otherwise it might run duplicate tasks.
            var receivedLockSuccesfully = await _executeJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

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
                    // Update running tasks as it could've updated it in this loop
                    runningJobRunTasks = jobRunTasks.Where(m => m.Started != null && m.Completed == null).ToList();

                    // Check if the previous task is completed for this item
                    if (jobRunTask.JobTask.Order > 0)
                    {
                        var previousTask = completedJobRunTasks.FirstOrDefault(m => m.ItemName == jobRunTask.ItemName && m.JobTask.Order == jobRunTask.JobTask.Order - 1);

                        if (previousTask == null)
                        {
                            continue;
                        }
                    }

                    // Check how many job run tasks are currently in progress for the same task.
                    var taskRunning = runningJobRunTasks.Count(m => m.JobTaskId == jobRunTask.JobTaskId);

                    // Calculate how many parallel tasks we can still run for this task.
                    var taskSpacePending = jobRunTask.JobTask.Parallel - taskRunning;

                    // If there is a place in the queue, start the task.
                    if (taskSpacePending > 0)
                    {
                        _logger.LogDebug("Starting task {jobRunTaskId} ({itemName}) for type {type}. Found {count} tasks running, max parallel is {parallel} for {jobRunId} {name}",
                                         jobRunTask.JobRunTaskId,
                                         jobRunTask.ItemName,
                                         jobRunTask.Type,
                                         taskRunning,
                                         jobRunTask.JobTask.Parallel,
                                         jobRunId,
                                         jobRun.Job.Name);

                        // Find the previous task for this item and pass it down
                        JobRunTask previousTask = null;

                        if (jobRunTask.JobTask.UsePreviousTaskArtifactsFromJobTaskId != null)
                        {
                            previousTask = completedJobRunTasks.FirstOrDefault(m => m.ItemName == jobRunTask.ItemName &&
                                                                                    m.JobTaskId == jobRunTask.JobTask.UsePreviousTaskArtifactsFromJobTaskId);
                        }

                        // Mark the task as started
                        jobRunTask.Started = DateTimeOffset.Now;
                        await _jobRunTasks.UpdateStarted(jobRunTask.JobRunTaskId, cancellationToken);
                        
                        _ = Task.Run(async () =>
                        {
                            await _remoteService.Run(jobRunTask, previousTask, cancellationToken);
                        }, cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug("Skipping Task {jobRunTaskId} ({itemName}) for type {type}. Found {count} tasks running, max parallel is {parallel} for {jobRunId} {name}",
                                         jobRunTask.JobRunTaskId,
                                         jobRunTask.ItemName,
                                         jobRunTask.Type,
                                         taskRunning,
                                         jobRunTask.JobTask.Parallel,
                                         jobRunId,
                                         jobRun.Job.Name);
                    }
                }
            }
            finally
            {
                _logger.LogDebug("Releasing lock for job run {jobRunId}", jobRunId);
                _executeJobRunLock.Release();
            }
        }

        private IBaseTask GetTask(String type)
        {
            if (type == null)
            {
                throw new ArgumentException("Type cannot be null");
            }

            if (!_tasks.TryGetValue(type, out var task))
            {
                throw new Exception($"Unknown task type {type}");
            }

            return task;
        }

        public async Task ProgressTask(Guid jobRunTaskId, String message, Boolean isError, CancellationToken cancellationToken)
        {
            await _jobRunTaskLogs.Add(new JobRunTaskLog
            {
                JobRunTaskId = jobRunTaskId,
                DateTime = DateTimeOffset.Now,
                Message = message,
                IsError = isError
            }, cancellationToken);
        }

        public async Task CompleteTask(Guid jobRunTaskId, String message, String resultArtifact, Boolean isError, CancellationToken cancellationToken)
        {
            await _jobRunTasks.UpdateCompleted(jobRunTaskId, message, resultArtifact, isError, cancellationToken);

            var jobRunTask = await _jobRunTasks.GetById(jobRunTaskId, cancellationToken);

            await ExecuteJobRun(jobRunTask.JobRunId, cancellationToken);
        }
    }
}
