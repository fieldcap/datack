using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class JobRunTasks
    {
        private readonly JobRunTaskRepository _jobRunTaskRepository;

        public JobRunTasks(JobRunTaskRepository jobRunTaskRepository)
        {
            _jobRunTaskRepository = jobRunTaskRepository;
        }

        public async Task<JobRunTask> GetById(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            return await _jobRunTaskRepository.GetById(jobRunTaskId, cancellationToken);
        }

        public async Task<IList<JobRunTask>> GetByJobRunId(Guid jobRunId, CancellationToken cancellationToken)
        {
            return await _jobRunTaskRepository.GetByJobRunId(jobRunId, cancellationToken);
        }

        public async Task Create(IList<JobRunTask> jobRunTasks, CancellationToken cancellationToken)
        {
            await _jobRunTaskRepository.Create(jobRunTasks, cancellationToken);
        }

        public async Task UpdateStarted(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            await _jobRunTaskRepository.UpdateStarted(jobRunTaskId, cancellationToken);
        }

        public async Task UpdateCompleted(Guid jobRunTaskId, String result, String resultArtifact, Boolean isError, CancellationToken cancellationToken)
        {
            await _jobRunTaskRepository.UpdateCompleted(jobRunTaskId, result, resultArtifact, isError, cancellationToken);
        }
    }
}
