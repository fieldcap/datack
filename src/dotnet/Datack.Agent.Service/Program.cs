using System.Diagnostics;
using System.Reflection;
using Datack.Agent.Models;
using Datack.Agent.Services;
using Datack.Agent.Services.DataConnections;
using Datack.Agent.Services.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace Datack.Agent;

public static class Program
{
    private static readonly LoggingLevelSwitch LoggingLevelSwitch = new();

    public static async Task Main(String[] args)
    {
        try
        {
            var builder = CreateHostBuilder(args);
                
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            Log.Warning($"Starting host on version {version}");

            if (WindowsServiceHelpers.IsWindowsService())
            {
                await builder.Build().RunAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(String[] args)
    {
        var configuration = new ConfigurationBuilder()
#if DEBUG
                            .AddJsonFile("appsettings.Development.json", true, false)
#else
                            .AddJsonFile("appsettings.json", true, false)
#endif
                            .Build();

        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        if (!String.IsNullOrWhiteSpace(appSettings.Logging?.LogLevel?.Default))
        {
            LoggingLevelSwitch.MinimumLevel = Enum.Parse<LogEventLevel>(appSettings.Logging.LogLevel.Default);
        }

        if (!String.IsNullOrWhiteSpace(appSettings.Logging?.File?.Path))
        {
            Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .Enrich.WithExceptionDetails()
                     .WriteTo.File(appSettings.Logging.File.Path,
                                   rollOnFileSizeLimit: true,
                                   fileSizeLimitBytes: appSettings.Logging.File.FileSizeLimitBytes,
                                   retainedFileCountLimit: appSettings.Logging.File.MaxRollingFiles)
                     .WriteTo.Console()
                     .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                     .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                     .CreateLogger();
        }

        Serilog.Debugging.SelfLog.Enable(msg =>
        {
            Debug.Print(msg);
            Debugger.Break();
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        });

        return Host.CreateDefaultBuilder(args)
                   .UseWindowsService()
                   .ConfigureAppConfiguration((_, config) =>
                   {
                       config.SetBasePath(Directory.GetCurrentDirectory());
                       config.AddJsonFile("appsettings.json", true);
                       config.AddCommandLine(args);
                   })
                   .ConfigureLogging((_, logging) =>
                   {
                       logging.AddFilter("Microsoft", LogLevel.Warning);
                       logging.AddSerilog();
                   })
                   .ConfigureServices((_, services) =>
                   {
                       services.AddDataProtection().SetApplicationName("Datack.Agent");

                       services.AddSingleton(appSettings);

                       services.AddSingleton<DatabaseAdapter>();
                       services.AddSingleton<DataProtector>();
                       services.AddSingleton<JobRunner>();
                       services.AddSingleton<RpcService>();
                       services.AddSingleton<StorageAdapter>();

                       services.AddSingleton<CreateBackupTask>();
                       services.AddSingleton<CompressTask>();
                       services.AddSingleton<DeleteS3Task>();
                       services.AddSingleton<DeleteFileTask>();
                       services.AddSingleton<DownloadS3Task>();
                       services.AddSingleton<DownloadAzureTask>();
                       services.AddSingleton<ExtractTask>();
                       services.AddSingleton<RestoreBackupTask>();
                       services.AddSingleton<UploadAzureTask>();
                       services.AddSingleton<UploadS3Task>();

                       services.AddSingleton<SqlServerConnection>();
                       services.AddSingleton<PostgresConnection>();
                       
                       services.AddSingleton<AwsS3Connection>();
                       services.AddSingleton<AzureBlobStorageConnection>();

                       services.AddHostedService<AgentHostedService>();
                   })
                   .UseSerilog();
    }
}