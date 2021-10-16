using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories
{
    public class JobRunTaskRepository
    {
        private readonly DataContext _dataContext;

        public JobRunTaskRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<JobRunTask> GetById(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            return await _dataContext.JobRunTasks
                                     .AsNoTracking()
                                     .Include(m => m.JobTask)
                                     .Include(m => m.JobRun)
                                     .FirstOrDefaultAsync(m => m.JobRunTaskId == jobRunTaskId, cancellationToken);
        }

        public async Task<IList<JobRunTask>> GetByJobRunId(Guid jobRunId, CancellationToken cancellationToken)
        {
            return await _dataContext.JobRunTasks
                                     .AsNoTracking()
                                     .Include(m => m.JobTask)
                                     .Include(m => m.JobTask.Server)
                                     .Include(m => m.JobRun)
                                     .Where(m => m.JobRunId == jobRunId)
                                     .OrderBy(m => m.TaskOrder)
                                     .ThenBy(m => m.ItemOrder)
                                     .ToListAsync(cancellationToken);
        }

        public async Task Create(IList<JobRunTask> jobRunTasks, CancellationToken cancellationToken)
        {
            await _dataContext.AddRangeAsync(jobRunTasks, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateStarted(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            var jobRunTask = await _dataContext.JobRunTasks.FirstOrDefaultAsync(m => m.JobRunTaskId == jobRunTaskId, cancellationToken);

            if (jobRunTask == null)
            {
                return;
            }

            jobRunTask.Started = DateTimeOffset.Now;
            jobRunTask.Result = null;
            jobRunTask.IsError = false;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateCompleted(Guid jobRunTaskId, String result, String resultArtifact, Boolean isError, CancellationToken cancellationToken)
        {
            var jobRunTask = await _dataContext.JobRunTasks.FirstOrDefaultAsync(m => m.JobRunTaskId == jobRunTaskId, cancellationToken);

            if (jobRunTask == null)
            {
                return;
            }

            jobRunTask.Completed = DateTimeOffset.Now;
            jobRunTask.Result = result;
            jobRunTask.ResultArtifact = resultArtifact;
            jobRunTask.IsError = isError;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
