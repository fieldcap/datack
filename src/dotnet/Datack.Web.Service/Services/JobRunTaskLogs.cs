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

        public JobRunTaskLogs(JobRunTaskLogRepository jobRunTaskLogRepository)
        {
            _jobRunTaskLogRepository = jobRunTaskLogRepository;
        }

        public async Task Add(JobRunTaskLog message, CancellationToken cancellationToken)
        {
            await _jobRunTaskLogRepository.Add(message, cancellationToken);
        }

        public async Task<IList<JobRunTaskLog>> GetByJobRunTaskId(Guid jobRunTaskId, CancellationToken cancellationToken)
        {
            return await _jobRunTaskLogRepository.GetByJobRunTaskId(jobRunTaskId, cancellationToken);
        }
    }
}
