using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class JobTasks
    {
        private readonly DataContextFactory _dataContextFactory;

        public JobTasks(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task UpdateJobTasks(IList<JobTask> jobTasks, Guid serverId)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobTasks = await context.JobTasks.ToListAsync();

            jobTasks = jobTasks.Where(m => m.ServerId == serverId).ToList();

            foreach (var dbJobTask in dbJobTasks)
            {
                var newJobTask = dbJobTasks.FirstOrDefault(m => m.JobTaskId == dbJobTask.JobId);

                if (newJobTask == null)
                {
                    context.Remove(dbJobTask);
                    await context.SaveChangesAsync();
                }
                else
                {
                    dbJobTask.Name = newJobTask.Name;
                    dbJobTask.Description = newJobTask.Description;
                    dbJobTask.Settings = newJobTask.Settings;
                    
                    jobTasks = jobTasks.Where(m => m.JobId != dbJobTask.JobId).ToList();
                }
            }

            foreach (var jobTask in jobTasks)
            {
                await context.JobTasks.AddAsync(jobTask);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IList<JobTask>> GetForJob(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.JobTasks
                                .AsNoTracking()
                                .Where(m => m.JobId == jobId).ToListAsync();
        }
    }
}
