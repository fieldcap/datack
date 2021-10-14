using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Enums;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Web.Service.Services
{
    public class RemoteService
    {
        private readonly IHubContext<DatackHub> _hub;

        public RemoteService(IHubContext<DatackHub> hub)
        {
            _hub = hub;
        }

        public async Task<String> TestSqlServerConnection(Server server, CancellationToken cancellationToken)
        {
            return await Send<String>(server.Key, "TestSqlServer", cancellationToken, server.DbSettings);
        }
        
        public async Task<IList<Database>> GetDatabaseList(Server server, CancellationToken cancellationToken)
        {
            return await Send<List<Database>>(server.Key, "GetDatabaseList", cancellationToken);
        }
        
        public async Task Run(Server server, Job job, BackupType backupType, CancellationToken cancellationToken)
        {
            await Send<String>(server.Key, "Run", cancellationToken, job.JobId, backupType);
        }

        private async Task<T> Send<T>(String key, String method, CancellationToken cancellationToken, params Object[] payload)
        {
            var hasConnection = DatackHub.Users.TryGetValue(key, out var connectionId);

            if (!hasConnection)
            {
                throw new Exception($"No connection found for server {key}");
            }

            return await SendWithConnection<T>(connectionId, method, cancellationToken, payload);
        }

        private async Task<T> SendWithConnection<T>(String connectionId, String method, CancellationToken cancellationToken, params Object[] payload)
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
                    if (rpcResult.Error != null)
                    {
                        var agentException = JsonSerializer.Deserialize<RpcException>(rpcResult.Error);

                        throw new Exception($"Agent threw an exception: {agentException}");
                    }
                    return JsonSerializer.Deserialize<T>(rpcResult.Result);
                }

                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
