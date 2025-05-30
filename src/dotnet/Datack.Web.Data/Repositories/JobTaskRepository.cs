﻿using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class JobTaskRepository
{
    private readonly DataContext _dataContext;

    public JobTaskRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IList<JobTask>> GetForJob(Guid jobId, CancellationToken cancellationToken)
    {
        return await _dataContext.JobTasks
                                 .AsNoTracking()
                                 .Include(m => m.Agent)
                                 .Include(m => m.UsePreviousTaskArtifactsFromJobTask)
                                 .Where(m => m.JobId == jobId)
                                 .OrderBy(m => m.Order)
                                 .ToListAsync(cancellationToken);
    }

    public async Task<List<JobTask>> GetByAgentId(Guid agentId, CancellationToken cancellationToken)
    {
        return await _dataContext.JobTasks
                                 .AsNoTracking()
                                 .Include(m => m.Agent)
                                 .Include(m => m.Job)
                                 .Where(m => m.AgentId == agentId)
                                 .ToListAsync(cancellationToken);
    }

    public async Task<JobTask?> GetById(Guid jobTaskId, CancellationToken cancellationToken)
    {
        return await _dataContext.JobTasks
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(m => m.JobTaskId == jobTaskId, cancellationToken);
    }

    public async Task<JobTask> Add(JobTask jobTask, CancellationToken cancellationToken)
    {
        jobTask.JobTaskId = Guid.NewGuid();

        var jobTaskCount = await _dataContext.JobTasks
                                             .AsNoTracking()
                                             .CountAsync(m => m.JobId == jobTask.JobId, cancellationToken);

        jobTask.Order = jobTaskCount;

        await _dataContext.JobTasks.AddAsync(jobTask, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);
            
        return jobTask;
    }

    public async Task Update(JobTask jobTask, CancellationToken cancellationToken)
    {
        var dbJobTask = await _dataContext
                              .JobTasks
                              .FirstOrDefaultAsync(m => m.JobTaskId == jobTask.JobTaskId, cancellationToken);

        if (dbJobTask == null)
        {
            throw new($"Job task with ID {jobTask.JobTaskId} not found");
        }

        dbJobTask.Name = jobTask.Name;
        dbJobTask.IsActive = jobTask.IsActive;
        dbJobTask.Description = jobTask.Description;
        dbJobTask.Order = jobTask.Order;
        dbJobTask.UsePreviousTaskArtifactsFromJobTaskId = jobTask.UsePreviousTaskArtifactsFromJobTaskId;
        dbJobTask.Type = jobTask.Type;
        dbJobTask.Parallel = jobTask.Parallel;
        dbJobTask.MaxItemsToKeep = jobTask.MaxItemsToKeep;
        dbJobTask.Timeout = jobTask.Timeout;
        dbJobTask.Settings = jobTask.Settings;
        dbJobTask.AgentId = jobTask.AgentId;

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReOrder(Guid jobId, IList<Guid> jobTaskIds, CancellationToken cancellationToken)
    {
        var dbJobTasks = await _dataContext
                               .JobTasks
                               .Where(m => m.JobId == jobId)
                               .ToListAsync(cancellationToken);

        var index = 0;
        foreach (var jobTaskId in jobTaskIds)
        {
            var dbJobTask = dbJobTasks.First(m => m.JobTaskId == jobTaskId);
            dbJobTask.Order = index;

            index++;
        }

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteForJob(Guid jobId, CancellationToken cancellationToken)
    {
        var jobTasks = await _dataContext.JobTasks.Where(m => m.JobId == jobId).ToListAsync(cancellationToken);

        _dataContext.JobTasks.RemoveRange(jobTasks);

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(Guid jobTaskId, CancellationToken cancellationToken)
    {
        var jobTask = await _dataContext.JobTasks.FirstOrDefaultAsync(m => m.JobTaskId == jobTaskId, cancellationToken);

        if (jobTask != null)
        {
            _dataContext.JobTasks.Remove(jobTask);

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}