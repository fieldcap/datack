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
        private readonly DataContext _dataContext;

        public JobLogs(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Create(JobLog jobLog)
        {
            await _dataContext.JobLogs.AddAsync(jobLog);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<JobLog>> GetRunning(Guid jobId)
        {
            return await _dataContext
                         .JobLogs
                         .AsNoTracking()
                         .Where(m => m.Completed == null && m.JobId == jobId)
                         .ToListAsync();
        }

        public async Task<JobLog> GetById(Guid jobLogId)
        {
            return await _dataContext.JobLogs.AsNoTracking().FirstOrDefaultAsync(m => m.JobLogId == jobLogId);
        }

        public async Task Update(JobLog jobLog)
        {
            _dataContext.Update(jobLog);
            await _dataContext.SaveChangesAsync();
        }
    }
}
