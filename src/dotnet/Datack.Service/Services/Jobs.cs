using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Data.Data;

namespace Datack.Service.Services
{
    public class Jobs
    {
        private readonly JobData _jobData;

        public Jobs(JobData jobData)
        {
            _jobData = jobData;
        }

        public async Task<IList<Job>> GetList(CancellationToken cancellationToken)
        {
            return await _jobData.GetList(cancellationToken);
        }

        public async Task<IList<Job>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _jobData.GetForServer(serverId, cancellationToken);
        }

        public async Task<Job> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            return await _jobData.GetById(jobId, cancellationToken);
        }

        public async Task<Guid> Add(Job job, CancellationToken cancellationToken)
        {
            return await _jobData.Add(job, cancellationToken);
        }

        public async Task Update(Job job, CancellationToken cancellationToken)
        {
            await _jobData.Update(job, cancellationToken);
        }
    }
}
