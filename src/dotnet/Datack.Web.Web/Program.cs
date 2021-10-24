using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Web.Service.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Exceptions;

namespace Datack.Web.Web
{
    public class Program
    {
        public static LoggingLevelSwitch LoggingLevelSwitch;

        public static async Task Main(String[] args)
        {
            try
            {
                Log.Information($"Starting host version {VersionHelper.GetVersion()}");
                var host = CreateHostBuilder(args).Build();
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static IHostBuilder CreateHostBuilder(String[] args)
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

            if (String.IsNullOrWhiteSpace(appSettings.HostUrl))
            {
                appSettings.HostUrl = "http://0.0.0.0:6500";
            }

            LoggingLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Debug);

            Log.Logger = new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .Enrich.WithExceptionDetails()
                         .WriteTo.File(appSettings.Logging.File.Path,
                                       rollOnFileSizeLimit: true,
                                       fileSizeLimitBytes: appSettings.Logging.File.FileSizeLimitBytes,
                                       retainedFileCountLimit: appSettings.Logging.File.MaxRollingFiles)
                         .WriteTo.Console()
                         .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                         .CreateLogger();

            SelfLog.Enable(msg =>
            {
                Debug.Print(msg);
                //Debugger.Break();
                Console.WriteLine(msg);
                Debug.WriteLine(msg);
            });

            return Host.CreateDefaultBuilder(args)
                       .ConfigureLogging(logging =>
                       {
                           logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Trace);
                           logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
                       })
                       .ConfigureWebHostDefaults(webBuilder =>
                       {
                           webBuilder.UseUrls(appSettings.HostUrl)
                                     .UseSerilog()
                                     .UseKestrel()
                                     .UseStartup<Startup>();
                       });
        }
    }
}
