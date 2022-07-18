using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class JobRuns
{
    private readonly Emails _emails;
    private readonly JobRunRepository _jobRunRepository;
    private readonly RemoteService _remoteService;

    public JobRuns(JobRunRepository jobRunRepository,
                   Emails emails,
                   RemoteService remoteService)
    {
        _jobRunRepository = jobRunRepository;
        _emails = emails;
        _remoteService = remoteService;
    }

    public async Task<List<JobRun>> GetAll(Guid? jobId, CancellationToken cancellationToken)
    {
        return await _jobRunRepository.GetAll(jobId, cancellationToken);
    }

    public async Task<List<JobRun>> GetRunning(CancellationToken cancellationToken)
    {
        return await _jobRunRepository.GetRunning(cancellationToken);
    }
        
    public async Task<JobRun> GetById(Guid jobRunId, CancellationToken cancellationToken)
    {
        return await _jobRunRepository.GetById(jobRunId, cancellationToken);
    }

    public async Task Create(JobRun jobRun, CancellationToken cancellationToken)
    {
        await _jobRunRepository.Create(jobRun, cancellationToken);
    }

    public async Task Update(JobRun jobRun, CancellationToken cancellationToken)
    {
        await _jobRunRepository.Update(jobRun, cancellationToken);
    }

    public async Task UpdateComplete(Guid jobRunId, CancellationToken cancellationToken)
    {
        await _jobRunRepository.UpdateComplete(jobRunId, cancellationToken);

        var jobRun = await GetById(jobRunId, cancellationToken);

        try
        {
            await _emails.SendComplete(jobRun, cancellationToken);
        }
        catch (Exception ex)
        {
            await _jobRunRepository.UpdateError(jobRunId, ex.Message, cancellationToken);
        }

        _ = Task.Run(async () =>
        {
            await _remoteService.WebJobRun(jobRun);
        }, cancellationToken);
    }

    public async Task UpdateStop(Guid jobRunId, CancellationToken cancellationToken)
    {
        await _jobRunRepository.UpdateStop(jobRunId, cancellationToken);

        var jobRun = await GetById(jobRunId, CancellationToken.None);

        _ = Task.Run(async () =>
        {
            await _remoteService.WebJobRun(jobRun);
        }, cancellationToken);
    }

    public async Task UpdateError(Guid jobRunId, String errorMsg, CancellationToken cancellationToken)
    {
        await _jobRunRepository.UpdateError(jobRunId, errorMsg, cancellationToken);
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTime deleteDate, CancellationToken cancellationToken)
    {
        return await _jobRunRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
    }
}