using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class JobLogs
    {
        private readonly DataContextFactory _dataContextFactory;

        public JobLogs(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task Create(JobLog jobLog)
        {
            await using var context = _dataContextFactory.Create();

            await context.JobLogs.AddAsync(jobLog);
            await context.SaveChangesAsync();
        }

        public async Task<List<JobLog>> GetRunning(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context
                         .JobLogs
                         .AsNoTracking()
                         .Where(m => m.Completed == null && m.JobId == jobId)
                         .ToListAsync();
        }

        public async Task<JobLog> GetById(Guid jobLogId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.JobLogs.AsNoTracking().FirstOrDefaultAsync(m => m.JobLogId == jobLogId);
        }

        public async Task Update(JobLog jobLog)
        {
            await using var context = _dataContextFactory.Create();

            context.Update(jobLog);
            await context.SaveChangesAsync();
        }

        public async Task UpdateComplete(Guid jobLogId)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobLog = await context.JobLogs.FirstOrDefaultAsync(m => m.JobLogId == jobLogId);

            if (dbJobLog == null)
            {
                return;
            }

            dbJobLog.Completed = DateTimeOffset.Now;

            var dbJobStepLogs = await context.StepLogs.Where(m => m.JobLogId == jobLogId).ToListAsync();
            var dbJobStepErrorLogs = dbJobStepLogs.Count(m => m.IsError);

            var timespan = dbJobLog.Completed - dbJobLog.Started;

            if (dbJobStepErrorLogs > 0)
            {
                dbJobLog.IsError = true;
                dbJobLog.Result = $"Job completed with {dbJobStepErrorLogs} errors in {timespan:g}";
            }
            else
            {
                dbJobLog.IsError = false;
                dbJobLog.Result = $"Job completed succesfully in {timespan:g}";
            }
            
            await context.SaveChangesAsync();
        }
    }
}
