using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class Jobs
    {
        private readonly DataContext _dataContext;

        private static IList<Job> _jobs;

        public Jobs(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Job>> GetJobs()
        {
            if (_jobs == null)
            {
                _jobs = await _dataContext.Jobs.ToListAsync();
            }

            return _jobs;
        }

        public async Task UpdateJobs(IList<Job> jobs)
        {
            var dbJobs = await _dataContext.Jobs.ToListAsync();

            foreach (var dbJob in dbJobs)
            {
                var newJob = jobs.FirstOrDefault(m => m.JobId == dbJob.JobId);

                if (newJob == null)
                {
                    _dataContext.Remove(dbJob);
                    await _dataContext.SaveChangesAsync();
                }
                else
                {
                    dbJob.Name = newJob.Name;
                    dbJob.Description = newJob.Description;
                    dbJob.Settings = newJob.Settings;
                    
                    jobs = jobs.Where(m => m.JobId != dbJob.JobId).ToList();
                }
            }

            foreach (var job in jobs)
            {
                await _dataContext.Jobs.AddAsync(job);
                await _dataContext.SaveChangesAsync();
            }

            _jobs = null;
        }

        public async Task<Job> GetById(Guid jobId)
        {
            return await _dataContext.Jobs.FirstOrDefaultAsync(m => m.JobId == jobId);
        }
    }
}
