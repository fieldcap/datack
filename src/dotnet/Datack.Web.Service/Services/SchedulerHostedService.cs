using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datack.Web.Service.Services
{
    public class SchedulerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public SchedulerHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                using var serviceScope = _serviceProvider.CreateScope();

                var jobService = serviceScope.ServiceProvider.GetRequiredService<Jobs>();
                var jobRunTasksService = serviceScope.ServiceProvider.GetRequiredService<JobRunTasks>();
                var jobRunsService = serviceScope.ServiceProvider.GetRequiredService<JobRuns>();
                
                // Mark tasks that are still running as not start to restart them.
                var runningJobs = await jobRunsService.GetRunning(cancellationToken);

                foreach (var runningJob in runningJobs)
                {
                    var jobRunTasks = await jobRunTasksService.GetByJobRunId(runningJob.JobRunId, cancellationToken);

                    foreach (var jobRunTask in jobRunTasks.Where(m => m.Completed == null && m.Started != null))
                    {
                        await jobRunTasksService.UpdateStarted(jobRunTask.JobRunTaskId, null, cancellationToken);
                    }

                    _ = Task.Run(async () =>
                    {
                        using var blockServiceScope = _serviceProvider.CreateScope();
                        var jobRunnerService = blockServiceScope.ServiceProvider.GetRequiredService<JobRunner>();
                        await jobRunnerService.ExecuteJobRun(runningJob.JobRunId, cancellationToken);
                    }, cancellationToken);
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    var now = DateTimeOffset.Now;
                    now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

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

                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
