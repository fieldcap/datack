using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Datack.Web.Service.Services;

/// <summary>
/// The SchedulerHost fires new jobs and makes sure that timeouts occur when agents are down.
/// This service always runs in the background on a 60 seconds interval.
/// </summary>
public class SchedulerHostedService : IHostedService
{
    private readonly ILogger<SchedulerHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    private CancellationToken _cancellationToken;

    public SchedulerHostedService(ILogger<SchedulerHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        AgentHub.OnClientConnect += (_, evt) => HandleClientConnect(evt.AgentKey);
        AgentHub.OnClientDisconnect += (_, evt) => HandleClientDisconnect(evt.AgentKey);
        AgentHub.OnProgressTasks += async (_, evt) => await HandleProgressTasks(evt, CancellationToken.None);
        AgentHub.OnCompleteTasks += async (_, evt) => await HandleCompleteTasks(evt, CancellationToken.None);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        _logger.LogDebug("Starting SchedulerHostedService");

        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

        // Job initiator.
        _ = Task.Run(async () =>
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var jobsService = serviceScope.ServiceProvider.GetRequiredService<Jobs>();
                
            await Task.Delay((60 - DateTime.Now.Second) * 1000, cancellationToken);

            _logger.LogDebug($"Starting Job initiator");

            // The main scheduler loop.
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

                // Get all the jobs and group them by Group.
                // This enables the user to have multiple tasks fire at the same time
                // and give priority to certain tasks in a group.
                var jobs = await jobsService.GetList(cancellationToken);

                jobs = jobs.Where(m => m.IsActive).ToList();

                foreach (var jobsGroup in jobs.GroupBy(m => m.Group))
                {
                    var groupResults = new List<Job>();

                    foreach (var job in jobsGroup)
                    {
                        var nextDate = CronHelper.GetNextOccurrence(job.Cron, now);

                        if (nextDate.HasValue && nextDate.Value == now)
                        {
                            _logger.LogDebug("Cron matches for job {job.Name}", job.Name);

                            groupResults.Add(job);
                        }
                    }

                    // If there are more jobs found for a group, only start the one with the higest priority,
                    // the other jobs are ignored from execution.
                    if (groupResults.Count > 0)
                    {
                        var jobToRun = groupResults.OrderBy(m => m.Priority).First();

                        _logger.LogDebug("Starting run for job {jobToRun.Name}", jobToRun.Name);

                        _ = Task.Run(async () =>
                        {
                            using var blockServiceScope = _serviceProvider.CreateScope();
                            var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                            await jobRunnerService.SetupJobRun(jobToRun, null, cancellationToken);
                        }, cancellationToken);
                    }
                }
                    
                // Do not trigger more than every 60 seconds as the cron tasks have a max resolution of 1 minute.
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
            }
        }, cancellationToken);

        // Timeout checker.
        _ = Task.Run(async () =>
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();
            var remoteService = serviceScope.ServiceProvider.GetRequiredService<RemoteService>();
                
            while (!cancellationToken.IsCancellationRequested)
            {
                var runningJobs = await jobRunsService.GetRunning(cancellationToken);

                foreach (var runningJob in runningJobs)
                {
                    // Skip checking if the job has just been created to prevent race conditions in the
                    // job initialization procedures.
                    if (runningJob.Started.AddMinutes(5) > DateTime.UtcNow)
                    {
                        continue;
                    }

                    var jobRunTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, cancellationToken);

                    var allTasks = jobRunTasks.ToList();
                    var pendingTasks = jobRunTasks.Where(m => m.Started == null && m.Completed == null).ToList();
                    var runningTasks = jobRunTasks.Where(m => m.Started != null && m.Completed == null).ToList();

                    if (pendingTasks.Count == 0 && allTasks.Count > 0)
                    {
                        _logger.LogWarning("No pending or running tasks found for job {runningJob.JobRunId}, marking job run complete", runningJob.JobRunId);

                        await jobRunsService.UpdateComplete(runningJob.JobRunId, cancellationToken);
                    }

                    foreach (var jobRunTask in runningTasks)
                    {
                        var timeout = jobRunTask.JobTask.Timeout ?? 3600;

                        var timespan = DateTimeOffset.UtcNow - jobRunTask.Started;

                        if (timespan.Value.TotalSeconds > timeout)
                        {
                            // Try sending a signal to the client to force it to stop it's task.
                            try
                            {
                                _logger.LogWarning("Server timeout for job run task {jobRunTask.JobRunTaskId} after {timeout} seconds", jobRunTask.JobRunTaskId, timeout);

                                _ = Task.Run(async () =>
                                             {
                                                 await remoteService.Stop(jobRunTask, cancellationToken);
                                             },
                                             cancellationToken);
                            }
                            catch
                            {
                                _logger.LogDebug("Killing job run task {jobRunTask.JobRunTaskId}", jobRunTask.JobRunTaskId);
                            }
                            finally
                            {
                                await HandleCompleteTasks(new List<RpcCompleteEvent>
                                {
                                    new RpcCompleteEvent
                                    {
                                        IsError = true,
                                        Message = $"Task has timed out on the server after {timeout} seconds",
                                        ResultArtifact = null,
                                        JobRunTaskId = jobRunTask.JobRunTaskId
                                    }
                                }, cancellationToken);
                            }
                        }
                    }
                }
                    
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
            }
        }, cancellationToken);

        // Log cleanup.
        _ = Task.Run(async () =>
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var jobsService = serviceScope.ServiceProvider.GetRequiredService<Jobs>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();
            var jobRunTaskRepository = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunTaskLogRepository = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var jobs = await jobsService.GetList(cancellationToken);

                foreach (var job in jobs)
                {
                    if (String.IsNullOrWhiteSpace(job.DeleteLogsTimeSpanType) || job.DeleteLogsTimeSpanAmount == null)
                    {
                        continue;
                    }

                    var deleteDate = DateTime.UtcNow;

                    deleteDate = job.DeleteLogsTimeSpanType switch
                    {
                        "Year" => deleteDate.AddYears(-job.DeleteLogsTimeSpanAmount.Value),
                        "Month" => deleteDate.AddMonths(-job.DeleteLogsTimeSpanAmount.Value),
                        "Day" => deleteDate.AddDays(-job.DeleteLogsTimeSpanAmount.Value),
                        "Hour" => deleteDate.AddHours(-job.DeleteLogsTimeSpanAmount.Value),
                        "Minute" => deleteDate.AddMinutes(-job.DeleteLogsTimeSpanAmount.Value),
                        _ => deleteDate
                    };

                    var result1 = await jobRunTaskLogRepository.DeleteForJob(job.JobId, deleteDate, cancellationToken);
                    var result2 = await jobRunTaskRepository.DeleteForJob(job.JobId, deleteDate, cancellationToken);
                    var result3 = await jobRunsService.DeleteForJob(job.JobId, deleteDate, cancellationToken);

                    _logger.LogDebug("Cleaned {result1} job run task logs, {result2} job run tasks, {result3} job runs that are older then {deleteDate} for job {job.Name}.",
                                     result1,
                                     result2,
                                     result3,
                                     deleteDate,
                                     job.Name);
                }

                await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
            }
        }, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle the connection of an agent.
    /// </summary>
    private async void HandleClientConnect(String agentKey)
    {
        _logger.LogDebug("Connect agent with key {agentKey}", agentKey);

        using var serviceScope = _serviceProvider.CreateScope();

        var agentsService = serviceScope.ServiceProvider.GetRequiredService<Agents>();
        var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
        var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
        var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();
        var remoteService = serviceScope.ServiceProvider.GetRequiredService<RemoteService>();

        var agent = await agentsService.GetByKey(agentKey, _cancellationToken);

        if (agent == null)
        {
            _logger.LogDebug("Agent with key {agentKey} not found!", agentKey);
            return;
        }

        // Because this process alters the state of some tasks, make sure it's locked with other tasks.
        var receivedLockSuccesfully = await JobRunner.ExecuteJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), _cancellationToken);

        if (!receivedLockSuccesfully)
        {
            // Lock timed out
            throw new Exception($"Could not obtain runSetupLock within 30 seconds!");
        }

        try
        {
            // Get the list of current running jobs on the agent
            var runningJobRunTaskIds = await remoteService.GetRunningTasks(agent, _cancellationToken);

            var runningJobRunTaskIdsList = String.Join(", ", runningJobRunTaskIds);

            _logger.LogDebug("Agent with {agent.Name} has {runningJobRunTaskIds.Count} pending events: {runningJobRunTaskIdsList}",
                             agent.Name,
                             runningJobRunTaskIds.Count,
                             runningJobRunTaskIdsList);

            var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

            // Check if this agent has running tasks, if it does, reset the task and execute them again.
            foreach (var runningJob in runningJobs)
            {
                try
                {
                    var jobRunTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

                    var runningTasks = jobRunTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.AgentId == agent.AgentId).ToList();
                    var pendingTasks = jobRunTasks.Where(m => m.Completed == null && m.Started == null && m.JobTask.AgentId == agent.AgentId).ToList();

                    if (runningTasks.Count > 0)
                    {
                        foreach (var runningTask in runningTasks)
                        {
                            if (runningJobRunTaskIds.Contains(runningTask.JobRunTaskId))
                            {
                                _logger.LogDebug("Not restarting task {runningTask.JobRunTaskId} for job run {runningTask.JobRunId}",
                                                 runningTask.JobRunTaskId,
                                                 runningTask.JobRunId);
                            }
                            else
                            {
                                _logger.LogDebug("Restarting task {runningTask.JobRunTaskId} for job run {runningTask.JobRunId}", runningTask.JobRunTaskId, runningTask.JobRunId);

                                await jobRunTasksService.UpdateStarted(runningTask.JobRunTaskId, null, _cancellationToken);
                            }

                            await jobRunTaskLogsService.Add(new JobRunTaskLog
                                                            {
                                                                JobRunTaskId = runningTask.JobRunTaskId,
                                                                DateTime = DateTimeOffset.UtcNow,
                                                                Message = $"Agent '{agent.Name}' is connected",
                                                                IsError = false
                                                            },
                                                            _cancellationToken);
                        }

                        _ = Task.Run(async () =>
                                     {
                                         using var blockServiceScope = _serviceProvider.CreateScope();
                                         var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                                         await jobRunnerService.ExecuteJobRun(runningJob.JobRunId, _cancellationToken);
                                     },
                                     _cancellationToken);
                    }
                    else if (pendingTasks.Count > 0)
                    {
                        _ = Task.Run(async () =>
                                     {
                                         using var blockServiceScope = _serviceProvider.CreateScope();
                                         var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                                         await jobRunnerService.ExecuteJobRun(runningJob.JobRunId, _cancellationToken);
                                     },
                                     _cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    await jobRunsService.UpdateError(runningJob.JobRunId, ex.Message, _cancellationToken);
                    await jobRunsService.UpdateComplete(runningJob.JobRunId, _cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting: {message}", ex.Message);
        }
        finally
        {
            JobRunner.ExecuteJobRunLock.Release();
        }
    }

    /// <summary>
    /// Handle the disconnect of an agent.
    /// </summary>
    private async void HandleClientDisconnect(String agentKey)
    {
        _logger.LogDebug("Disconnect agent with key {agentKey}", agentKey);

        using var serviceScope = _serviceProvider.CreateScope();

        var agentsService = serviceScope.ServiceProvider.GetRequiredService<Agents>();
        var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
        var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
        var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();

        var agent = await agentsService.GetByKey(agentKey, _cancellationToken);

        if (agent == null)
        {
            _logger.LogDebug("Agent with key {agentKey} not found!", agentKey);
            return;
        }

        var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

        foreach (var runningJob in runningJobs)
        {
            var runningTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

            foreach (var runningTask in runningTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.AgentId == agent.AgentId))
            {
                try
                {
                    await jobRunTaskLogsService.Add(new JobRunTaskLog
                                                    {
                                                        JobRunTaskId = runningTask.JobRunTaskId,
                                                        DateTime = DateTimeOffset.UtcNow,
                                                        Message = $"Agent '{agent.Name}' has disconnected",
                                                        IsError = false
                                                    },
                                                    _cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to disconnect: {message}", ex.Message);
                }
            }
        }
    }

    private async Task HandleProgressTasks(IList<RpcProgressEvent> progressEvents, CancellationToken cancellationToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();
        var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();

        foreach (var progressEvent in progressEvents)
        {
            try
            {
                await jobRunTaskLogsService.Add(new JobRunTaskLog
                                                {
                                                    JobRunTaskId = progressEvent.JobRunTaskId,
                                                    DateTime = DateTimeOffset.UtcNow,
                                                    Message = progressEvent.Message,
                                                    IsError = progressEvent.IsError
                                                },
                                                cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving progress task: {message}", ex.Message);
            }
        }
    }

    private async Task HandleCompleteTasks(IList<RpcCompleteEvent> completeEvents, CancellationToken cancellationToken)
    {
        var distinctJobRunIds = new List<Guid>();

        using var serviceScope = _serviceProvider.CreateScope();
        var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
        var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();

        foreach (var completeEvent in completeEvents)
        {
            try
            {
                _logger.LogDebug("Received Complete Event for jobRunTaskId {jobRunTaskId}", completeEvent.JobRunTaskId);

                var jobRunTask = await jobRunTasksService.GetById(completeEvent.JobRunTaskId, cancellationToken);

                distinctJobRunIds.Add(jobRunTask.JobRunId);

                await jobRunTaskLogsService.Add(new JobRunTaskLog
                                                {
                                                    JobRunTaskId = completeEvent.JobRunTaskId,
                                                    DateTime = DateTimeOffset.UtcNow,
                                                    Message = completeEvent.Message,
                                                    IsError = completeEvent.IsError
                                                },
                                                cancellationToken);

                await jobRunTasksService.UpdateCompleted(completeEvent.JobRunTaskId, completeEvent.Message, completeEvent.ResultArtifact, completeEvent.IsError, cancellationToken);
            }
            catch (Exception ex)
            {
                await jobRunTasksService.UpdateCompleted(completeEvent.JobRunTaskId, ex.Message, null, true, cancellationToken);
            }
        }

        distinctJobRunIds = distinctJobRunIds.Distinct().ToList();

        foreach (var jobRunId in distinctJobRunIds)
        {
            _ = Task.Run(async () =>
                         {
                             using var blockServiceScope = _serviceProvider.CreateScope();
                             var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                             await jobRunnerService.ExecuteJobRun(jobRunId, cancellationToken);
                         },
                         _cancellationToken);

            await Task.Delay(1000, cancellationToken);
        }
    }
}