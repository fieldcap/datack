using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
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

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _jobRepository.GetForServer(serverId, cancellationToken);
        }

        public async Task<Job> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            return await _jobRepository.GetById(jobId, cancellationToken);
        }

        public async Task<Guid> Add(Job job, CancellationToken cancellationToken)
        {
            return await _jobRepository.Add(job, cancellationToken);
        }

        public async Task Update(Job job, CancellationToken cancellationToken)
        {
            await _jobRepository.Update(job, cancellationToken);
        }
    }
}
