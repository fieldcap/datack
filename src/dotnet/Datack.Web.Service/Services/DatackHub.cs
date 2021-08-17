using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Web.Service.Services
{
    public class DatackHub : Hub
    {
        private readonly Servers _servers;
        private readonly Jobs _jobs;
        private readonly Steps _steps;

        public DatackHub(Servers servers, Jobs jobs, Steps steps)
        {
            _servers = servers;
            _jobs = jobs;
            _steps = steps;
        }

        public static readonly ConcurrentDictionary<String, String> Users = new();
        public static readonly ConcurrentDictionary<Guid, RpcResult> Transactions = new();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var (key, value) in Users)
            {
                if (value == Context.ConnectionId)
                {
                    Users.TryRemove(key, out _);
                    break;
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Connect(String key)
        {
            var server = await _servers.GetByKey(key, CancellationToken.None);

            if (server == null)
            {
                throw new Exception($"Server with key {key} was not found");
            }

            Users.TryAdd(key, Context.ConnectionId);
        }

        public void Response(RpcResult rpcResult)
        {
            Transactions.TryAdd(rpcResult.TransactionId, rpcResult);
        }

        public async Task<RpcUpdate> RpcUpdate(String key)
        {
            var server = await _servers.GetByKey(key, CancellationToken.None);
            var jobs = await _jobs.GetForServer(server.ServerId, CancellationToken.None);
            var steps = await _steps.GetForServer(server.ServerId, CancellationToken.None);

            return new RpcUpdate
            {
                Server = server,
                Jobs = jobs,
                Steps = steps
            };
        }
    }
}
