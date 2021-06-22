using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Service;
using Serilog;
using Serilog.Exceptions;

namespace Datack.Agent.Console
{
    public class Program
    {
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        private static async Task Main()
        {
            const String token = "5026d123-0b7a-4ecc-9b97-4950324f161f";

            Log.Logger = new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .Enrich.WithExceptionDetails()
                         .WriteTo.Console()
                         .MinimumLevel.Debug()
                         .CreateLogger();

            await Task.Delay(5000);

            Log.Information("Application has started. Ctrl-C to end");

            System.Console.CancelKeyPress += (_, eventArgs) =>
            {
                CancellationToken.Cancel();
                eventArgs.Cancel = true;
            };

            var m = new Main(Log.Logger, CancellationToken.Token);
            await m.Start(token);

            while (true)
            {
                await Task.Delay(10);
            }
        }
    }
}
