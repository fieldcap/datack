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
        private readonly Emails _emails;
        private readonly JobRunRepository _jobRunRepository;
        private readonly JobRunTaskLogRepository _jobRunTaskLogRepository;
        private readonly JobRunTaskRepository _jobRunTaskRepository;
        private readonly RemoteService _remoteService;

        public JobRuns(JobRunRepository jobRunRepository,
                       JobRunTaskRepository jobRunTaskRepository,
                       JobRunTaskLogRepository jobRunTaskLogRepository,
                       Emails emails,
                       RemoteService remoteService)
        {
            _jobRunRepository = jobRunRepository;
            _jobRunTaskLogRepository = jobRunTaskLogRepository;
            _jobRunTaskRepository = jobRunTaskRepository;
            _emails = emails;
            _remoteService = remoteService;
        }

        public async Task<List<JobRun>> GetAll(Guid? jobId, CancellationToken cancellationToken)
        {
            return await _jobRunRepository.GetAll(jobId, cancellationToken);
        }

        public async Task<List<JobRun>> GetRunning(CancellationToken cancellationToken)
        {
            return await _jobRunRepository.GetRunning(cancellationToken);
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
            var jobRun = await GetById(jobRunId, cancellationToken);

            String error = null;

            try
            {
                await _emails.SendComplete(jobRun, cancellationToken);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                await _jobRunRepository.UpdateComplete(jobRunId, error, cancellationToken);
            }

            jobRun = await GetById(jobRunId, CancellationToken.None);

            _ = Task.Run(async () =>
            {
                await _remoteService.WebJobRun(jobRun);
            }, cancellationToken);
        }

        public async Task UpdateStop(Guid jobRunId, CancellationToken cancellationToken)
        {
            await _jobRunRepository.UpdateStop(jobRunId, cancellationToken);

            var jobRun = await GetById(jobRunId, CancellationToken.None);

            _ = Task.Run(async () =>
            {
                await _remoteService.WebJobRun(jobRun);
            }, cancellationToken);
        }

        public async Task DeleteForJob(Guid jobId, DateTimeOffset deleteDate, CancellationToken cancellationToken)
        {
            await _jobRunTaskLogRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
            await _jobRunTaskRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
            await _jobRunRepository.DeleteForJob(jobId, deleteDate, cancellationToken);
        }
    }
}
