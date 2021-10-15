using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class JobRunTasks
    {
        private readonly DataContextFactory _dataContextFactory;

        public JobRunTasks(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task<IList<JobRunTask>> GetByJobRunTaskId(Guid jobRunTaskId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.JobRunTasks
                                .AsNoTracking()
                                .Include(m => m.JobTask)
                                .Where(m => m.JobRunTaskId == jobRunTaskId)
                                .ToListAsync();
        }

        public async Task Create(IList<JobRunTask> jobRunTasks)
        {
            await using var context = _dataContextFactory.Create();

            await context.AddRangeAsync(jobRunTasks);
            await context.SaveChangesAsync();
        }

        public async Task UpdateStarted(Guid jobRunTaskId)
        {
            await using var context = _dataContextFactory.Create();

            var jobRunTask = await context.JobRunTasks.FirstOrDefaultAsync(m => m.JobRunTaskId == jobRunTaskId);

            if (jobRunTask == null)
            {
                return;
            }

            jobRunTask.Started = DateTimeOffset.Now;
            jobRunTask.Result = null;
            jobRunTask.IsError = false;

            await context.SaveChangesAsync();
        }

        public async Task UpdateCompleted(Guid jobRunTaskId, String result, Boolean isError)
        {
            await using var context = _dataContextFactory.Create();

            var jobRunTask = await context.JobRunTasks.FirstOrDefaultAsync(m => m.JobRunTaskId == jobRunTaskId);

            if (jobRunTask == null)
            {
                return;
            }

            jobRunTask.Completed = DateTimeOffset.Now;
            jobRunTask.Result = result;
            jobRunTask.IsError = isError;

            await context.SaveChangesAsync();
        }
    }
}
