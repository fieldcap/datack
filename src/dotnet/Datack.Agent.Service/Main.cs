using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Datack.Agent.Service
{
    public class Main
    {
        private HubConnection _connection;

        public async Task Start(CancellationToken cancellationToken)
        {
            await StartConnection(cancellationToken);
        }

        private async Task StartConnection(CancellationToken cancellationToken)
        {
            _connection = new HubConnectionBuilder()
                          .WithUrl("http://localhost:6500/hub")
                          .WithAutomaticReconnect()
                          .Build();

            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);
                    break;
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    if (_connection.State == HubConnectionState.Disconnected)
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}
