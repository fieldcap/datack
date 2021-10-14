using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Datack.Agent.Services
{
    public class StartupHostedService : IHostedService
    {
        private readonly DataContextFactory _dataContextFactory;

        public StartupHostedService(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await using var context = _dataContextFactory.Create();
            
            await context.Database.MigrateAsync(cancellationToken);
            await context.Seed();

            // Testing
            context.StepLogMessages.RemoveRange(context.StepLogMessages);
            context.StepLogs.RemoveRange(context.StepLogs);
            context.JobLogs.RemoveRange(context.JobLogs);

            await context.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
