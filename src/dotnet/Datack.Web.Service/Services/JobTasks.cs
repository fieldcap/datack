using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class JobTasks
    {
        private readonly JobTaskRepository _jobTaskRepository;

        public JobTasks(JobTaskRepository jobTaskRepository)
        {
            _jobTaskRepository = jobTaskRepository;
        }

        public async Task<IList<JobTask>> GetForJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetForJob(jobId, cancellationToken);
        }

        public async Task<IList<JobTask>> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetForServer(serverId, cancellationToken);
        }

        public async Task<JobTask> GetById(Guid jobTaskId, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.GetById(jobTaskId, cancellationToken);
        }

        public async Task<JobTask> Add(JobTask jobTask, CancellationToken cancellationToken)
        {
            return await _jobTaskRepository.Add(jobTask, cancellationToken);
        }

        public async Task Update(JobTask jobTask, CancellationToken cancellationToken)
        {
            await _jobTaskRepository.Update(jobTask, cancellationToken);
        }

        public async Task ReOrder(Guid jobId, IList<Guid> jobTaskIds, CancellationToken cancellationToken)
        {
            await _jobTaskRepository.ReOrder(jobId, jobTaskIds, cancellationToken);
        }
    }
}
