using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Service.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services
{
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
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _logger.LogDebug("Starting SchedulerHostedService");

            // Job initiator.
            _ = Task.Run(async () =>
            {
                using var serviceScope = _serviceProvider.CreateScope();

                var jobsService = serviceScope.ServiceProvider.GetRequiredService<Jobs>();

                // The main scheduler loop.
                while (!cancellationToken.IsCancellationRequested)
                {
                    var now = DateTimeOffset.Now;
                    now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

                    // Get all the jobs and group them by Group.
                    // This enables the user to have multiple tasks fire at the same time
                    // and give priority to certain tasks in a group.
                    var jobs = await jobsService.GetList(cancellationToken);

                    foreach (var jobsGroup in jobs.GroupBy(m => m.Group))
                    {
                        var groupResults = new List<Job>();

                        foreach (var job in jobsGroup)
                        {
                            var nextDate = CronHelper.GetNextOccurrence(job.Cron, now);

                            if (nextDate.HasValue && nextDate.Value == now)
                            {
                                _logger.LogDebug($"Cron matches for job {job.Name}");

                                groupResults.Add(job);
                            }
                        }

                        // If there are more jobs found for a group, only start the one with the higest priority,
                        // the other jobs are ignored from execution.
                        if (groupResults.Count > 0)
                        {
                            var jobToRun = groupResults.OrderBy(m => m.Priority).First();

                            _logger.LogDebug($"Starting run for job {jobToRun.Name}");

                            _ = Task.Run(async () =>
                            {
                                using var blockServiceScope = _serviceProvider.CreateScope();
                                var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                                await jobRunnerService.SetupJobRun(jobToRun, cancellationToken);
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
                        var jobRunTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, cancellationToken);

                        foreach (var jobRunTask in jobRunTasks.Where(m => m.Completed == null && m.JobTask.Timeout > 0))
                        {
                            if (jobRunTask.Started == null)
                            {
                                continue;
                            }

                            var timespan = DateTimeOffset.UtcNow - jobRunTask.Started;

                            if (timespan.Value.TotalSeconds > jobRunTask.JobTask.Timeout)
                            {
                                // Try sending a signal to the client to force it to stop it's task.
                                try
                                {
                                    _logger.LogDebug($"Timeout for job run task {jobRunTask.JobRunTaskId}");

                                    _ = Task.Run(async () =>
                                    {
                                        await remoteService.Stop(jobRunTask, cancellationToken);
                                    }, cancellationToken);
                                }
                                catch
                                {
                                    _logger.LogDebug($"Killing job run task {jobRunTask.JobRunTaskId}");

                                    // If that doesn't work (client disconnected?) force complete the task.
                                    _ = Task.Run(async () =>
                                    {
                                        using var blockServiceScope = _serviceProvider.CreateScope();
                                        var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();

                                        await jobRunnerService.CompleteTask(jobRunTask.JobRunTaskId,
                                                                            $"Task has timed out after {jobRunTask.JobTask.Timeout} seconds",
                                                                            null,
                                                                            true,
                                                                            cancellationToken);
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

                        _logger.LogDebug($"Cleaned {result1} job run task logs, {result2} job run tasks, {result3} job runs that are older then {deleteDate} for job {job.Name}.");
                    }

                    await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
                }
            }, cancellationToken);

            return Task.CompletedTask;
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
            _logger.LogDebug($"Connect agent with key {agentKey}");

            using var serviceScope = _serviceProvider.CreateScope();

            var agentsService = serviceScope.ServiceProvider.GetRequiredService<Agents>();
            var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
            var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();

            var agent = await agentsService.GetByKey(agentKey, _cancellationToken);

            if (agent == null)
            {
                _logger.LogDebug($"Agent with key {agentKey} not found!");
                return;
            }
            
            var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

            // Check if this agent has running tasks, if it does, reset the task and execute them again.
            foreach (var runningJob in runningJobs)
            {
                var runningTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

                runningTasks = runningTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.AgentId == agent.AgentId).ToList();

                if (runningTasks.Count > 0)
                {
                    foreach (var runningTask in runningTasks)
                    {
                        _logger.LogDebug($"Restarting task {runningTask.JobRunTaskId} for job run {runningTask.JobRunId}");

                        await jobRunTasksService.UpdateStarted(runningTask.JobRunTaskId, runningTask.JobRunId, null, _cancellationToken);

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
                    }, _cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handle the disconnect of an agent.
        /// </summary>
        private async void HandleClientDisconnect(String agentKey)
        {
            _logger.LogDebug($"Disconnect agent with key {agentKey}");

            using var serviceScope = _serviceProvider.CreateScope();

            var agentsService = serviceScope.ServiceProvider.GetRequiredService<Agents>();
            var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
            var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();

            var agent = await agentsService.GetByKey(agentKey, _cancellationToken);

            if (agent == null)
            {
                _logger.LogDebug($"Agent with key {agentKey} not found!");
                return;
            }

            var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

            foreach (var runningJob in runningJobs)
            {
                var runningTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

                foreach (var runningTask in runningTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.AgentId == agent.AgentId))
                {
                    await jobRunTaskLogsService.Add(new JobRunTaskLog
                    {
                        JobRunTaskId = runningTask.JobRunTaskId,
                        DateTime = DateTimeOffset.UtcNow,
                        Message = $"Agent '{agent.Name}' has disconnected",
                        IsError = false
                    }, _cancellationToken);
                }
            }
        }
    }
}
