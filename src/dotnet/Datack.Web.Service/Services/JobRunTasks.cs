using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobRunTasks(JobRunTaskRepository jobRunTaskRepository, RemoteService remoteService)
{
    public async Task<JobRunTask?> GetById(Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        return await jobRunTaskRepository.GetById(jobRunTaskId, cancellationToken);
    }

    public async Task<IList<JobRunTask>> GetByJobRunId(Guid jobRunId, CancellationToken cancellationToken)
    {
        return await jobRunTaskRepository.GetByJobRunId(jobRunId, cancellationToken);
    }

    public async Task Create(IList<JobRunTask> jobRunTasks, CancellationToken cancellationToken)
    {
        await jobRunTaskRepository.Create(jobRunTasks, cancellationToken);
    }

    public async Task UpdateStarted(Guid jobRunTaskId, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        await jobRunTaskRepository.UpdateStarted(jobRunTaskId, date, cancellationToken);

        var jobRunTask = await GetById(jobRunTaskId, CancellationToken.None);

        if (jobRunTask == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await remoteService.WebJobRunTask(jobRunTask);
        }, cancellationToken);
    }

    public async Task UpdateCompleted(Guid jobRunTaskId, String result, String? resultArtifact, Boolean isError, CancellationToken cancellationToken)
    {
        await jobRunTaskRepository.UpdateCompleted(jobRunTaskId, result, resultArtifact, isError, cancellationToken);

        var jobRunTask = await GetById(jobRunTaskId, CancellationToken.None);

        if (jobRunTask == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await remoteService.WebJobRunTask(jobRunTask);
        }, cancellationToken);
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTime deleteDate, CancellationToken cancellationToken)
    {
        return await jobRunTaskRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
    }
}