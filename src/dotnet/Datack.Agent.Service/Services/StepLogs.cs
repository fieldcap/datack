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
        private readonly DataContext _dataContext;

        public StepLogs(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<StepLog>> GetByJobLogId(Guid jobLogId)
        {
            return await _dataContext.StepLogs.AsNoTracking().Where(m => m.JobLogId == jobLogId).ToListAsync();
        }

        public async Task Create(List<StepLog> stepLogs)
        {
            await _dataContext.AddRangeAsync(stepLogs);
            await _dataContext.SaveChangesAsync();
        }
    }
}
