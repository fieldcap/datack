using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class JobRuns
    {
        private readonly DataContextFactory _dataContextFactory;

        public JobRuns(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task Create(JobRun jobRun)
        {
            await using var context = _dataContextFactory.Create();

            await context.JobRuns.AddAsync(jobRun);
            await context.SaveChangesAsync();
        }

        public async Task<List<JobRun>> GetRunning(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context
                         .JobRuns
                         .AsNoTracking()
                         .Where(m => m.Completed == null && m.JobId == jobId)
                         .ToListAsync();
        }

        public async Task<JobRun> GetById(Guid jobRunId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.JobRuns.AsNoTracking().Include(m => m.Job).FirstOrDefaultAsync(m => m.JobRunId == jobRunId);
        }

        public async Task Update(JobRun jobRun)
        {
            await using var context = _dataContextFactory.Create();

            context.Update(jobRun);
            await context.SaveChangesAsync();
        }

        public async Task UpdateComplete(Guid jobRunId)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobRun = await context.JobRuns.FirstOrDefaultAsync(m => m.JobRunId == jobRunId);

            if (dbJobRun == null)
            {
                return;
            }

            dbJobRun.Completed = DateTimeOffset.Now;

            var dbJobRunTasks = await context.JobRunTasks.Where(m => m.JobRunId == jobRunId).ToListAsync();
            var dbJobRunTasksWithErrors = dbJobRunTasks.Count(m => m.IsError);

            var timespan = dbJobRun.Completed - dbJobRun.Started;

            if (dbJobRunTasksWithErrors > 0)
            {
                dbJobRun.IsError = true;
                dbJobRun.Result = $"Job completed with {dbJobRunTasksWithErrors} errors in {timespan:g}";
            }
            else
            {
                dbJobRun.IsError = false;
                dbJobRun.Result = $"Job completed succesfully in {timespan:g}";
            }
            
            await context.SaveChangesAsync();
        }
    }
}
