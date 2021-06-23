using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Data.Data
{
    public class JobData
    {
        private readonly DataContext _dataContext;

        public JobData(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Job>> GetList(CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs.OrderBy(m => m.Name).ToListAsync(cancellationToken);
        }

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs.Where(m => m.Steps.Any(s => s.ServerId == serverId)).OrderBy(m => m.Name).ToListAsync(cancellationToken);
        }

        public async Task<Job> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs.FirstOrDefaultAsync(m => m.JobId == jobId, cancellationToken);
        }

        public async Task<Guid> Add(Job job, CancellationToken cancellationToken)
        {
            job.JobId = Guid.NewGuid();

            await _dataContext.Jobs.AddAsync(job, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return job.JobId;
        }

        public async Task Update(Job job, CancellationToken cancellationToken)
        {
            var dbJob = await _dataContext.Jobs.FirstOrDefaultAsync(m => m.JobId == job.JobId, cancellationToken);

            if (dbJob == null)
            {
                throw new Exception($"Job with ID {job.JobId} not found");
            }

            dbJob.Name = job.Name;
            dbJob.Description = job.Description;
            dbJob.Settings = job.Settings;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
