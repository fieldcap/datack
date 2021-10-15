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

        public async Task UpdateJobTasks(IList<JobTask> jobTasks)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobTasks = await context.JobTasks.ToListAsync();

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

        public async Task UpdateJobTask(JobTask jobTask)
        {
            await using var context = _dataContextFactory.Create();

            var dbJobTask = await context.JobTasks.FirstOrDefaultAsync(m => m.JobTaskId == jobTask.JobTaskId);

            if (dbJobTask != null)
            {
                dbJobTask.Name = jobTask.Name;
                dbJobTask.Description = jobTask.Description;
                dbJobTask.Order = jobTask.Order;
                dbJobTask.UsePreviousTaskArtifactsFromJobTaskId = jobTask.UsePreviousTaskArtifactsFromJobTaskId;
                dbJobTask.Type = jobTask.Type;
                dbJobTask.Parallel = jobTask.Parallel;
                dbJobTask.Settings = jobTask.Settings;
            }
            else
            {
                await context.JobTasks.AddAsync(jobTask);
            }

            await context.SaveChangesAsync();
        }

        public async Task<IList<JobTask>> GetForJob(Guid jobId)
        {
            await using var context = _dataContextFactory.Create();

            return await context.JobTasks
                                .AsNoTracking()
                                .Where(m => m.JobId == jobId)
                                .OrderBy(m => m.Order)
                                .ToListAsync();
        }
    }
}
