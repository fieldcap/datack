using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
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

                    foreach (var job in jobs)
                    {
                        var backupType = CronHelper.GetNextOccurrence(job.Settings.CronFull, job.Settings.CronDiff, job.Settings.CronLog, now);

                        if (backupType != null)
                        {
                            await jobRunnerService.Run(job.JobId, backupType.Value, cancellationToken);
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
