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
                var jobRunnerService = serviceScope.ServiceProvider.GetRequiredService<JobRunner>();

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

                            _ = Task.Run(async () => await jobRunnerService.SetupJobRun(jobToRun, cancellationToken), cancellationToken);
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
