using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobRunTaskLogs(JobRunTaskLogRepository jobRunTaskLogRepository, RemoteService remoteService)
{
    public async Task Add(JobRunTaskLog jobRunTaskLog, CancellationToken cancellationToken)
    {
        var result = await jobRunTaskLogRepository.Add(jobRunTaskLog, cancellationToken);

        _ = Task.Run(async () =>
        {
            await remoteService.WebJobRunTaskLog(result);
        }, cancellationToken);
    }

    public async Task<IList<JobRunTaskLog>> GetByJobRunTaskId(Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        return await jobRunTaskLogRepository.GetByJobRunTaskId(jobRunTaskId, cancellationToken);
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTime deleteDate, CancellationToken cancellationToken)
    {
        return await jobRunTaskLogRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
    }
}