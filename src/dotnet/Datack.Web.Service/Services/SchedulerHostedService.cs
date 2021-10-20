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

namespace Datack.Web.Service.Services
{
    /// <summary>
    /// The SchedulerHost fires new jobs and makes sure that timeouts occur when agents are down.
    /// This service always runs in the background on a 60 seconds interval.
    /// </summary>
    public class SchedulerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        
        private CancellationToken _cancellationToken;

        public SchedulerHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            DatackHub.OnClientConnect += (_, evt) => HandleClientConnect(evt.ServerKey);
            DatackHub.OnClientDisconnect += (_, evt) => HandleClientDisconnect(evt.ServerKey);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _ = Task.Run(async () =>
            {
                using var serviceScope = _serviceProvider.CreateScope();

                var jobService = serviceScope.ServiceProvider.GetRequiredService<Jobs>();
                var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
                var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();
                var remoteService = serviceScope.ServiceProvider.GetRequiredService<RemoteService>();
                
                // The main scheduler loop.
                while (!cancellationToken.IsCancellationRequested)
                {
                    var now = DateTimeOffset.Now;
                    now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

                    // Get all the jobs and group them by Group.
                    // This enables the user to have multiple tasks fire at the same time
                    // and give priority to certain tasks in a group.
                    var jobs = await jobService.GetList(cancellationToken);

                    foreach (var jobsGroup in jobs.GroupBy(m => m.Group))
                    {
                        var groupResults = new List<Job>();

                        foreach (var job in jobsGroup)
                        {
                            var nextDate = CronHelper.GetNextOccurrence(job.Cron, now);

                            if (nextDate.HasValue && nextDate.Value == now)
                            {
                                groupResults.Add(job);
                            }
                        }

                        // If there are more jobs found for a group, only start the one with the higest priority,
                        // the other jobs are ignored from execution.
                        if (groupResults.Count > 0)
                        {
                            var jobToRun = groupResults.OrderBy(m => m.Priority).First();

                            _ = Task.Run(async () =>
                            {
                                using var blockServiceScope = _serviceProvider.CreateScope();
                                var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                                await jobRunnerService.SetupJobRun(jobToRun, cancellationToken);
                            }, cancellationToken);
                        }
                    }

                    // Check if tasks need to be timed out.
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

                            var timespan = DateTimeOffset.Now - jobRunTask.Started;

                            if (timespan.Value.TotalSeconds > jobRunTask.JobTask.Timeout)
                            {
                                // Try sending a signal to the client to force it to stop it's task.
                                try
                                {
                                    _ = Task.Run(async () =>
                                    {
                                        await remoteService.Stop(jobRunTask, cancellationToken);
                                    }, cancellationToken);
                                }
                                catch
                                {
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
                    
                    // Do not trigger more than every 60 seconds as the cron tasks have a max resolution of 1 minute.
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
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
        private async void HandleClientConnect(String serverKey)
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var serversService = serviceScope.ServiceProvider.GetRequiredService<Servers>();
            var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
            var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();

            var server = await serversService.GetByKey(serverKey, _cancellationToken);

            var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

            // Check if this server has running tasks, if it does, reset the task and execute them again.
            foreach (var runningJob in runningJobs)
            {
                var runningTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

                runningTasks = runningTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.ServerId == server.ServerId).ToList();

                if (runningTasks.Count > 0)
                {
                    foreach (var runningTask in runningTasks)
                    {
                        await jobRunTasksService.UpdateStarted(runningTask.JobRunTaskId, null, _cancellationToken);

                        await jobRunTaskLogsService.Add(new JobRunTaskLog
                                                        {
                                                            JobRunTaskId = runningTask.JobRunTaskId,
                                                            DateTime = DateTimeOffset.Now,
                                                            Message = $"Server '{server.Name}' is connected",
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
        private async void HandleClientDisconnect(String serverKey)
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var serversService = serviceScope.ServiceProvider.GetRequiredService<Servers>();
            var jobRunTaskLogsService = serviceScope.ServiceProvider.GetRequiredService<JobRunTaskLogs>();
            var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
            var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();

            var server = await serversService.GetByKey(serverKey, _cancellationToken);

            var runningJobs = await jobRunsService.GetRunning(_cancellationToken);

            foreach (var runningJob in runningJobs)
            {
                var runningTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, _cancellationToken);

                foreach (var runningTask in runningTasks.Where(m => m.Completed == null && m.Started.HasValue && m.JobTask.ServerId == server.ServerId))
                {
                    await jobRunTaskLogsService.Add(new JobRunTaskLog
                    {
                        JobRunTaskId = runningTask.JobRunTaskId,
                        DateTime = DateTimeOffset.Now,
                        Message = $"Server '{server.Name}' has disconnected",
                        IsError = false
                    }, _cancellationToken);
                }
            }
        }
    }
}
