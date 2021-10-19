using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class JobRuns
    {
        private readonly JobRunRepository _jobRunRepository;

        public JobRuns(JobRunRepository jobRunRepository)
        {
            _jobRunRepository = jobRunRepository;
        }

        public async Task<List<JobRun>> GetAll(Guid? jobId, CancellationToken cancellationToken)
        {
            return await _jobRunRepository.GetAll(jobId, cancellationToken);
        }

        public async Task<List<JobRun>> GetRunning(CancellationToken cancellationToken)
        {
            return await _jobRunRepository.GetRunning(cancellationToken);
        }

        public async Task<List<JobRun>> GetByJobId(Guid jobId, CancellationToken cancellationToken)
        {
            return await _jobRunRepository.GetByJobId(jobId, cancellationToken);
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
        }

        public async Task UpdateStop(Guid jobRunId, CancellationToken cancellationToken)
        {
            await _jobRunRepository.UpdateStop(jobRunId, cancellationToken);
        }

        public async Task Delete(Guid jobId, Int32 keepDays, CancellationToken cancellationToken)
        {
            await _jobRunRepository.Delete(jobId, keepDays, cancellationToken);
        }
    }
}
