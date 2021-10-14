using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Datack.Agent.Services
{
    public class StartupHostedService : IHostedService
    {
        private readonly DataContext _dataContext;

        public StartupHostedService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _dataContext.Database.EnsureCreatedAsync(cancellationToken);
            await _dataContext.Database.MigrateAsync(cancellationToken);
            await _dataContext.Seed();

            // Testing
            _dataContext.StepLogMessages.RemoveRange(_dataContext.StepLogMessages);
            _dataContext.StepLogs.RemoveRange(_dataContext.StepLogs);
            _dataContext.JobLogs.RemoveRange(_dataContext.JobLogs);

            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
