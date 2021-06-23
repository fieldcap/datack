using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
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

        public async Task<String> TestSqlServer(String key, ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            var hasConnection = DatackHub.Users.TryGetValue(key, out var connectionId);

            if (!hasConnection)
            {
                throw new Exception($"No connection found for server {key}");
            }

            return await Send<String>(connectionId, "TestSqlServer", serverDbSettings, cancellationToken);
        }

        private async Task<T> Send<T>(String connectionId, String method, Object args, CancellationToken cancellationToken)
        {
            var request = new RpcRequest
            {
                TransactionId = Guid.NewGuid(),
                Request = method,
                Payload = JsonSerializer.Serialize(args)
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
