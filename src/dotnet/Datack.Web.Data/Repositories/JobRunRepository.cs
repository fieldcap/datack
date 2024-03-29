﻿using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class JobRunRepository
{
    private readonly DataContext _dataContext;

    public JobRunRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<List<JobRun>> GetAll(Guid? jobId, CancellationToken cancellationToken)
    {
        IQueryable<JobRun> query = _dataContext.JobRuns
                                               .Include(m => m.Job);

        if (jobId.HasValue)
        {
            query = query.Where(m => m.JobId == jobId);
        }

        query = query.OrderByDescending(m => m.Started);
            
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<JobRun>> GetRunning(CancellationToken cancellationToken)
    {
        return await _dataContext
                     .JobRuns
                     .Include(m => m.Job)
                     .AsNoTracking()
                     .Where(m => m.Completed == null)
                     .ToListAsync(cancellationToken);
    }

    public async Task<JobRun?> GetById(Guid jobRunId, CancellationToken cancellationToken)
    {
        return await _dataContext.JobRuns
                                 .AsNoTracking()
                                 .Include(m => m.Job)
                                 .FirstOrDefaultAsync(m => m.JobRunId == jobRunId, cancellationToken);
    }

    public async Task Create(JobRun jobRun, CancellationToken cancellationToken)
    {
        await _dataContext.JobRuns.AddAsync(jobRun, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Update(JobRun jobRun, CancellationToken cancellationToken)
    {
        _dataContext.Update(jobRun);
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateComplete(Guid jobRunId, CancellationToken cancellationToken)
    {
        var dbJobRun = await _dataContext.JobRuns.FirstOrDefaultAsync(m => m.JobRunId == jobRunId, cancellationToken);

        if (dbJobRun == null)
        {
            return;
        }

        dbJobRun.Completed = DateTimeOffset.UtcNow;

        if (!dbJobRun.IsError)
        {
            var dbJobRunTasks = await _dataContext.JobRunTasks.Where(m => m.JobRunId == jobRunId).ToListAsync(cancellationToken);
            var dbJobRunTasksWithErrors = dbJobRunTasks.Count(m => m.IsError);

            var timespan = dbJobRun.Completed - dbJobRun.Started;

            dbJobRun.RunTime = (Int64) timespan.Value.TotalSeconds;

            if (dbJobRunTasksWithErrors > 0)
            {
                dbJobRun.IsError = true;
                dbJobRun.Result = $"Job completed with {dbJobRunTasksWithErrors} errors in {timespan:g}";
            }
            else
            {
                dbJobRun.IsError = false;
                dbJobRun.Result = $"Job completed succesfully in {timespan:g}";
            }
        }

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateStop(Guid jobRunId, CancellationToken cancellationToken)
    {
        var dbJobRun = await _dataContext.JobRuns.FirstOrDefaultAsync(m => m.JobRunId == jobRunId, cancellationToken);

        if (dbJobRun == null)
        {
            return;
        }

        dbJobRun.Completed = DateTimeOffset.UtcNow;

        var timespan = dbJobRun.Completed - dbJobRun.Started;

        dbJobRun.RunTime = (Int64) timespan.Value.TotalSeconds;

        dbJobRun.IsError = true;
        dbJobRun.Result = $"Job was manually stopped after {timespan:g}";
            
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateError(Guid jobRunId, String errorMsg, CancellationToken cancellationToken)
    {
        var dbJobRun = await _dataContext.JobRuns.FirstOrDefaultAsync(m => m.JobRunId == jobRunId, cancellationToken);

        if (dbJobRun == null)
        {
            return;
        }

        dbJobRun.IsError = true;
        dbJobRun.Result = errorMsg;
            
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTimeOffset deleteDate, CancellationToken cancellationToken)
    {
        return await _dataContext.Database.ExecuteSqlInterpolatedAsync(@$"DELETE FROM JobRuns WHERE JobRuns.JobId = {jobId} AND JobRuns.Started < {deleteDate.Ticks}", cancellationToken);
    }
}