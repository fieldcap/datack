using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services
{
    public class JobRunTaskLogs
    {
        private readonly DataContextFactory _dataContextFactory;

        public JobRunTaskLogs(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task Add(JobRunTaskLog message)
        {
            await using var context = _dataContextFactory.Create();

            await context.JobRunTaskLogs.AddAsync(message);
            await context.SaveChangesAsync();
        }
    }
}
