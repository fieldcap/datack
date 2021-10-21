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

        public async Task<List<Job>> GetAll(CancellationToken cancellationToken)
        {
            return await _dataContext.Jobs
                                     .AsNoTracking()
                                     .ToListAsync(cancellationToken);
        }

        public async Task<IList<Job>> GetForAgent(Guid agentId, CancellationToken cancellationToken)
        {
            return await _dataContext.JobTasks
                                     .AsNoTracking()
                                     .Where(m => m.AgentId == agentId)
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
            dbJob.Cron = job.Cron;
            dbJob.Settings = job.Settings;
            dbJob.Group = job.Group;
            dbJob.Priority = job.Priority;
            dbJob.DeleteLogsTimeSpanAmount = job.DeleteLogsTimeSpanAmount;
            dbJob.DeleteLogsTimeSpanType = job.DeleteLogsTimeSpanType;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Job> Duplicate(Guid jobId, CancellationToken cancellationToken)
        {
            var dbJob = await _dataContext.Jobs.FirstOrDefaultAsync(m => m.JobId == jobId, cancellationToken);

            if (dbJob == null)
            {
                throw new Exception($"Job with ID {jobId} not found");
            }

            var dbJobTasks = await _dataContext.JobTasks.Where(m => m.JobId == jobId).ToListAsync(cancellationToken);

            var newJob = new Job
            {
                JobId = Guid.NewGuid(),
                Name = $"{dbJob.Name} (Copy)",
                Group = dbJob.Group,
                Description = dbJob.Description,
                Cron = dbJob.Cron,
                Priority = dbJob.Priority + 1,
                DeleteLogsTimeSpanAmount = dbJob.DeleteLogsTimeSpanAmount,
                DeleteLogsTimeSpanType = dbJob.DeleteLogsTimeSpanType,
                Settings = dbJob.Settings
            };

            var newJobTasks = dbJobTasks.Select(dbJobTask => new JobTask
            {
                JobTaskId = Guid.NewGuid(),
                JobId = newJob.JobId,
                Type = dbJobTask.Type,
                Parallel = dbJobTask.Parallel,
                Name = dbJobTask.Name,
                Description = dbJobTask.Description,
                Order = dbJobTask.Order,
                UsePreviousTaskArtifactsFromJobTaskId = dbJobTask.UsePreviousTaskArtifactsFromJobTaskId,
                Settings = dbJobTask.Settings,
                AgentId = dbJobTask.AgentId
            });

            await _dataContext.Jobs.AddAsync(newJob, cancellationToken);
            await _dataContext.JobTasks.AddRangeAsync(newJobTasks, cancellationToken);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return newJob;
        }

        public async Task Delete(Guid jobId, CancellationToken cancellationToken)
        {
            var jobTasks = _dataContext.JobTasks.Where(m => m.JobId == jobId);
            _dataContext.RemoveRange(jobTasks);

            var jobs = _dataContext.Jobs.Where(m => m.JobId == jobId);
            _dataContext.RemoveRange(jobs);

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
