using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class Jobs
{
    private readonly JobRepository _jobRepository;

    public Jobs(JobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IList<Job>> GetList(CancellationToken cancellationToken)
    {
        return await _jobRepository.GetList(cancellationToken);
    }

    public async Task<IList<Job>> GetForAgent(Guid agentId, CancellationToken cancellationToken)
    {
        return await _jobRepository.GetForAgent(agentId, cancellationToken);
    }

    public async Task<Job?> GetById(Guid jobId, CancellationToken cancellationToken)
    {
        return await _jobRepository.GetById(jobId, cancellationToken);
    }

    public async Task<Guid> Add(Job job, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(job.Name))
        {
            throw new Exception($"Name cannot be empty");
        }

        var allJobs = await _jobRepository.GetAll(cancellationToken);
        var sameNameJobs = allJobs.Any(m => String.Equals(m.Name, job.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameJobs)
        {
            throw new Exception($"A job with this name already exists");
        }

        return await _jobRepository.Add(job, cancellationToken);
    }

    public async Task Update(Job job, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(job.Name))
        {
            throw new Exception($"Name cannot be empty");
        }

        var allJobs = await _jobRepository.GetAll(cancellationToken);
        var sameNameJobs = allJobs.Any(m => m.JobId != job.JobId && String.Equals(m.Name, job.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameJobs)
        {
            throw new Exception($"A job with this name already exists");
        }

        await _jobRepository.Update(job, cancellationToken);
    }

    public async Task<Job> Duplicate(Guid jobId, CancellationToken cancellationToken)
    {
        return await _jobRepository.Duplicate(jobId, cancellationToken);
    }

    public async Task Delete(Guid jobId, CancellationToken cancellationToken)
    {
        await _jobRepository.Delete(jobId, cancellationToken);
    }
}