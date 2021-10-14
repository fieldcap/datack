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
        private readonly DataContextFactory _dataContextFactory;

        public Jobs(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        private static IList<Job> _jobs;

        public async Task<IList<Job>> GetJobs()
        {
            await using var context = _dataContextFactory.Create();

            if (_jobs == null)
            {
                _jobs = await context.Jobs.ToListAsync();
            }

            return _jobs;
        }

        public async Task UpdateJobs(IList<Job> jobs)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobs = await context.Jobs.ToListAsync();

            foreach (var dbJob in dbJobs)
            {
                var newJob = jobs.FirstOrDefault(m => m.JobId == dbJob.JobId);

                if (newJob == null)
                {
                    context.Remove(dbJob);
                    await context.SaveChangesAsync();
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
                await context.Jobs.AddAsync(job);
                await context.SaveChangesAsync();
            }

            _jobs = null;
        }

        public async Task<Job> GetById(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.Jobs.FirstOrDefaultAsync(m => m.JobId == jobId);
        }
    }
}
