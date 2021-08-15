using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.Extensions.Hosting;

namespace Datack.Agent.Services.Jobs
{
    public class JobRunner : BackgroundService
    {
        private Job _job;
        private Server _server;

        private IList<CronOccurrence> _schedule;
        
        public void Start(Server server, Job job)
        {
            _server = server;
            _job = job;

            UpdateSchedule();

            Task.Run(Trigger);
        }

        public Guid JobId => _job.JobId;

        public void Update(Job job)
        {
            _job = job;

            UpdateSchedule();
        }
        public void UpdateSchedule()
        {
            
        }

        public void Trigger()
        {
            _schedule = CronHelper.GetNextOccurrences(_job.Settings.CronFull, _job.Settings.CronDiff, _job.Settings.CronLog, TimeSpan.FromDays(2));
            if (_schedule == null || _schedule.Count == 0)
            {
                return;
            }

            var now = DateTimeOffset.Now;
            now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.BaseUtcOffset);

            var triggers = _schedule.Where(m => m.DateTime == now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
