using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;

namespace Datack.Web.Data.Repositories
{
    public class JobRunTaskLogRepository
    {
        private readonly DataContext _dataContext;

        public JobRunTaskLogRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Add(JobRunTaskLog message, CancellationToken cancellationToken)
        {
            await _dataContext.JobRunTaskLogs.AddAsync(message, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
