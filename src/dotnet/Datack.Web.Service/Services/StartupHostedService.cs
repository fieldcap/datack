using Datack.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Datack.Web.Service.Services;

public class StartupHostedService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
            await dbContext.Seed();

            var logLevelSettingDb = await dbContext.Settings.FirstOrDefaultAsync(m => m.SettingId == "LogLevel", cancellationToken);
                    
            var logLevelSetting = "Information";

            if (logLevelSettingDb != null)
            {
                logLevelSetting = logLevelSettingDb.Value;
            }

            if (!Enum.TryParse<LogEventLevel>(logLevelSetting, out var logLevel))
            {
                logLevel = LogEventLevel.Warning;
            }

            Settings.LoggingLevelSwitch.MinimumLevel = logLevel;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}