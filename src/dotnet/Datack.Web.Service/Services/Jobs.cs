using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services;

public class Jobs(JobRepository jobRepository)
{
    public async Task<IList<Job>> GetList(CancellationToken cancellationToken)
    {
        return await jobRepository.GetList(cancellationToken);
    }

    public async Task<IList<Job>> GetForAgent(Guid agentId, CancellationToken cancellationToken)
    {
        return await jobRepository.GetForAgent(agentId, cancellationToken);
    }

    public async Task<Job?> GetById(Guid jobId, CancellationToken cancellationToken)
    {
        return await jobRepository.GetById(jobId, cancellationToken);
    }

    public async Task<Guid> Add(Job job, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(job.Name))
        {
            throw new($"Name cannot be empty");
        }

        var allJobs = await jobRepository.GetAll(cancellationToken);
        var sameNameJobs = allJobs.Any(m => String.Equals(m.Name, job.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameJobs)
        {
            throw new($"A job with this name already exists");
        }

        return await jobRepository.Add(job, cancellationToken);
    }

    public async Task Update(Job job, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(job.Name))
        {
            throw new($"Name cannot be empty");
        }

        var allJobs = await jobRepository.GetAll(cancellationToken);
        var sameNameJobs = allJobs.Any(m => m.JobId != job.JobId && String.Equals(m.Name, job.Name, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameJobs)
        {
            throw new($"A job with this name already exists");
        }

        await jobRepository.Update(job, cancellationToken);
    }

    public async Task<Job> Duplicate(Guid jobId, CancellationToken cancellationToken)
    {
        return await jobRepository.Duplicate(jobId, cancellationToken);
    }

    public async Task Delete(Guid jobId, CancellationToken cancellationToken)
    {
        await jobRepository.Delete(jobId, cancellationToken);
    }
}