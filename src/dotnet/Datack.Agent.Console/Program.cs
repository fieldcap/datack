using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Service;

namespace Datack.Agent.Console
{
    public class Program
    {
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        private static async Task Main()
        {
            System.Console.WriteLine("Application has started. Ctrl-C to end");

            System.Console.CancelKeyPress += (_, eventArgs) =>
            {
                System.Console.WriteLine("Cancel event triggered");
                CancellationToken.Cancel();
                eventArgs.Cancel = true;
            };

            var m = new Main();
            await m.Start(CancellationToken.Token);
        }
    }
}
