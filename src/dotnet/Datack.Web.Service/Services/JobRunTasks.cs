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
        private readonly RemoteService _remoteService;

        public JobRunTasks(JobRunTaskRepository jobRunTaskRepository, RemoteService remoteService)
        {
            _jobRunTaskRepository = jobRunTaskRepository;
            _remoteService = remoteService;
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

        public async Task UpdateStarted(Guid jobRunTaskId, Guid jobRunId, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            await _jobRunTaskRepository.UpdateStarted(jobRunTaskId, date, cancellationToken);

            var jobRunTasks = await GetByJobRunId(jobRunId, CancellationToken.None);
            _ = Task.Run(async () =>
            {
                await _remoteService.WebJobRunTask(jobRunTasks);
            }, cancellationToken);
        }

        public async Task UpdateCompleted(Guid jobRunTaskId, Guid jobRunId, String result, String resultArtifact, Boolean isError, CancellationToken cancellationToken)
        {
            await _jobRunTaskRepository.UpdateCompleted(jobRunTaskId, result, resultArtifact, isError, cancellationToken);

            var jobRunTasks = await GetByJobRunId(jobRunId, CancellationToken.None);
            _ = Task.Run(async () =>
            {
                await _remoteService.WebJobRunTask(jobRunTasks);
            }, cancellationToken);
        }
    }
}
