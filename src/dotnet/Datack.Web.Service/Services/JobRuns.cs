using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobRuns(
    JobRunRepository jobRunRepository,
    Emails emails,
    RemoteService remoteService)
{
    public async Task<List<JobRun>> GetAll(Guid? jobId, CancellationToken cancellationToken)
    {
        return await jobRunRepository.GetAll(jobId, cancellationToken);
    }

    public async Task<List<JobRun>> GetRunning(CancellationToken cancellationToken)
    {
        return await jobRunRepository.GetRunning(cancellationToken);
    }
        
    public async Task<JobRun?> GetById(Guid jobRunId, CancellationToken cancellationToken)
    {
        return await jobRunRepository.GetById(jobRunId, cancellationToken);
    }

    public async Task Create(JobRun jobRun, CancellationToken cancellationToken)
    {
        await jobRunRepository.Create(jobRun, cancellationToken);
    }

    public async Task Update(JobRun jobRun, CancellationToken cancellationToken)
    {
        await jobRunRepository.Update(jobRun, cancellationToken);
    }

    public async Task UpdateComplete(Guid jobRunId, CancellationToken cancellationToken)
    {
        await jobRunRepository.UpdateComplete(jobRunId, cancellationToken);

        var jobRun = await GetById(jobRunId, cancellationToken);

        if (jobRun == null)
        {
            return;
        }

        try
        {
            await emails.SendComplete(jobRun, cancellationToken);
        }
        catch (Exception ex)
        {
            await jobRunRepository.UpdateError(jobRunId, ex.Message, cancellationToken);
        }

        _ = Task.Run(async () =>
        {
            await remoteService.WebJobRun(jobRun);
        }, cancellationToken);
    }

    public async Task UpdateStop(Guid jobRunId, CancellationToken cancellationToken)
    {
        await jobRunRepository.UpdateStop(jobRunId, cancellationToken);

        var jobRun = await GetById(jobRunId, CancellationToken.None);
        
        if (jobRun == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await remoteService.WebJobRun(jobRun);
        }, cancellationToken);
    }

    public async Task UpdateError(Guid jobRunId, String errorMsg, CancellationToken cancellationToken)
    {
        await jobRunRepository.UpdateError(jobRunId, errorMsg, cancellationToken);
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTime deleteDate, CancellationToken cancellationToken)
    {
        return await jobRunRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
    }
}