﻿using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

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

    public async Task<Job?> GetById(Guid jobId, CancellationToken cancellationToken)
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
            throw new($"Job with ID {job.JobId} not found");
        }

        dbJob.Name = job.Name;
        dbJob.IsActive = job.IsActive;
        dbJob.Description = job.Description;
        dbJob.Cron = job.Cron;
        dbJob.Settings = job.Settings;
        dbJob.Group = job.Group;
        dbJob.Priority = job.Priority;
        dbJob.DeleteLogsTimeSpanAmount = job.DeleteLogsTimeSpanAmount;
        dbJob.DeleteLogsTimeSpanType = job.DeleteLogsTimeSpanType;
        dbJob.Settings = job.Settings;

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Job> Duplicate(Guid jobId, CancellationToken cancellationToken)
    {
        var dbJob = await _dataContext.Jobs.FirstOrDefaultAsync(m => m.JobId == jobId, cancellationToken);

        if (dbJob == null)
        {
            throw new($"Job with ID {jobId} not found");
        }

        var dbJobTasks = await _dataContext.JobTasks
                                           .Where(m => m.JobId == jobId)
                                           .OrderBy(m => m.Order)
                                           .ToListAsync(cancellationToken);

        var newJob = new Job
        {
            JobId = Guid.NewGuid(),
            Name = $"{dbJob.Name} (Copy)",
            IsActive = dbJob.IsActive,
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
            IsActive = dbJobTask.IsActive,
            Parallel = dbJobTask.Parallel,
            MaxItemsToKeep = dbJobTask.MaxItemsToKeep,
            Name = dbJobTask.Name,
            Description = dbJobTask.Description,
            Order = dbJobTask.Order,
            UsePreviousTaskArtifactsFromJobTaskId = dbJobTask.UsePreviousTaskArtifactsFromJobTaskId,
            Settings = dbJobTask.Settings,
            AgentId = dbJobTask.AgentId
        }).ToList();

        await _dataContext.Jobs.AddAsync(newJob, cancellationToken);
        await _dataContext.JobTasks.AddRangeAsync(newJobTasks, cancellationToken);

        foreach (var newJobTask in newJobTasks)
        {
            if (newJobTask.UsePreviousTaskArtifactsFromJobTaskId == null)
            {
                continue;
            }

            var dbJobTask = dbJobTasks.FirstOrDefault(m => m.JobTaskId == newJobTask.UsePreviousTaskArtifactsFromJobTaskId);

            if (dbJobTask == null)
            {
                newJobTask.UsePreviousTaskArtifactsFromJobTaskId = null;
            }
            else
            {
                var newJobTask2 = newJobTasks.FirstOrDefault(m => m.Name == dbJobTask.Name);

                if (newJobTask2 == null)
                {
                    newJobTask.UsePreviousTaskArtifactsFromJobTaskId = null;
                }
                else
                {
                    newJobTask.UsePreviousTaskArtifactsFromJobTaskId = newJobTask2.JobTaskId;
                }
            }
        }

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