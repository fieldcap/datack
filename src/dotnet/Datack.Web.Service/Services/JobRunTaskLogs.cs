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
    }
}
