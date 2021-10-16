using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories
{
    public class JobRepository
    {
        private readonly DataContext _dataContext;

        public JobRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Job>> GetList(CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs
                                     .AsNoTracking()
                                     .OrderBy(m => m.Name)
                                     .ToListAsync(cancellationToken);
        }

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _dataContext.JobTasks
                                     .AsNoTracking()
                                     .Where(m => m.ServerId == serverId)
                                     .Select(m => m.Job)
                                     .Distinct()
                                     .OrderBy(m => m.Name)
                                     .ToListAsync(cancellationToken);
        }

        public async Task<Job> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(m => m.JobId == jobId, cancellationToken);
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
