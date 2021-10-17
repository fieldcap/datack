using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories
{
    public class JobRunTaskLogRepository
    {
        private readonly DataContext _dataContext;

        public JobRunTaskLogRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<JobRunTaskLog>> GetByJobRunTaskId(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            return await _dataContext.JobRunTaskLogs.AsNoTracking().Where(m => m.JobRunTaskId == jobRunTaskId).OrderBy(m => m.DateTime).ToListAsync(cancellationToken);
        }

        public async Task Add(JobRunTaskLog message, CancellationToken cancellationToken)
        {
            await _dataContext.JobRunTaskLogs.AddAsync(message, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
