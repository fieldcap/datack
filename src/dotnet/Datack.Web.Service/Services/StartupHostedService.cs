using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datack.Web.Service.Services
{
    public class StartupHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public StartupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var serviceScope = _serviceProvider.CreateScope();

            var dataContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

            await dataContext.Database.MigrateAsync(cancellationToken);
            await dataContext.Seed();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
