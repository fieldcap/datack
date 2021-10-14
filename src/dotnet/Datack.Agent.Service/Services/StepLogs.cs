using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class StepLogs
    {
        private readonly DataContextFactory _dataContextFactory;

        public StepLogs(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task<IList<StepLog>> GetByJobLogId(Guid jobLogId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.StepLogs
                                .AsNoTracking()
                                .Include(m => m.JobLog)
                                .Include(m => m.Step)
                                .Where(m => m.JobLogId == jobLogId)
                                .ToListAsync();
        }

        public async Task Create(IList<StepLog> stepLogs)
        {
            await using var context = _dataContextFactory.Create();

            await context.AddRangeAsync(stepLogs);
            await context.SaveChangesAsync();
        }

        public async Task UpdateStarted(Guid stepLogId)
        {
            await using var context = _dataContextFactory.Create();

            var stepLog = await context.StepLogs.FirstOrDefaultAsync(m => m.StepLogId == stepLogId);

            if (stepLog == null)
            {
                return;
            }

            stepLog.Started = DateTimeOffset.Now;
            stepLog.Result = null;
            stepLog.IsError = false;

            await context.SaveChangesAsync();
        }

        public async Task<Int32> UpdateCompleted(Guid stepLogId, Guid jobLogId, String result, Boolean isError)
        {
            await using var context = _dataContextFactory.Create();

            var stepLog = await context.StepLogs.FirstOrDefaultAsync(m => m.StepLogId == stepLogId);

            if (stepLog == null)
            {
                return -1;
            }

            stepLog.Completed = DateTimeOffset.Now;
            stepLog.Result = result;
            stepLog.IsError = isError;

            await context.SaveChangesAsync();

            return await context.StepLogs.CountAsync(m => m.JobLogId == jobLogId && m.Completed == null);
        }
    }
}
