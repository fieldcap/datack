using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Service.Services
{
    public class RemoteService
    {
        private readonly IHubContext<DatackHub> _hub;

        public RemoteService(IHubContext<DatackHub> hub)
        {
            _hub = hub;
        }

        public async Task<T> Send<T>(String key, String method, Object payload, CancellationToken cancellationToken)
        {
            var hasConnection = DatackHub.Users.TryGetValue(key, out var connectionId);

            if (!hasConnection)
            {
                throw new Exception($"No connection found for server {key}");
            }

            return await SendWithConnection<T>(connectionId, method, payload, cancellationToken);
        }

        private async Task<T> SendWithConnection<T>(String connectionId, String method, Object payload, CancellationToken cancellationToken)
        {
            var request = new RpcRequest
            {
                TransactionId = Guid.NewGuid(),
                Request = method,
                Payload = JsonSerializer.Serialize(payload)
            };

            var sendArgs = new[]
            {
                request
            };

            await _hub.Clients.Client(connectionId).SendCoreAsync("request", sendArgs, cancellationToken);

            var timeout = DateTime.UtcNow.AddSeconds(30);

            while (true)
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new Exception($"No response received within timeout");
                }

                if (DatackHub.Transactions.TryGetValue(request.TransactionId, out var rpcResult))
                {
                    return JsonSerializer.Deserialize<T>(rpcResult.Result);
                }

                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
