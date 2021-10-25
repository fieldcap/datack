using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class JobRunTaskLogs
    {
        private readonly JobRunTaskLogRepository _jobRunTaskLogRepository;
        private readonly RemoteService _remoteService;

        public JobRunTaskLogs(JobRunTaskLogRepository jobRunTaskLogRepository, RemoteService remoteService)
        {
            _jobRunTaskLogRepository = jobRunTaskLogRepository;
            _remoteService = remoteService;
        }

        public async Task Add(JobRunTaskLog jobRunTaskLog, CancellationToken cancellationToken)
        {
            var result = await _jobRunTaskLogRepository.Add(jobRunTaskLog, cancellationToken);

            _ = Task.Run(async () =>
            {
                await _remoteService.WebJobRunTaskLog(result);
            }, cancellationToken);
        }

        public async Task<IList<JobRunTaskLog>> GetByJobRunTaskId(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            return await _jobRunTaskLogRepository.GetByJobRunTaskId(jobRunTaskId, cancellationToken);
        }

        public async Task<Int32> DeleteForJob(Guid jobId, DateTime deleteDate, CancellationToken cancellationToken)
        {
            return await _jobRunTaskLogRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
        }
    }
}
