using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Datack.Data.Data;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Service.Services
{
    public class DatackHub : Hub
    {
        private readonly ServerData _serverData;
        private readonly JobData _jobData;

        public DatackHub(ServerData serverData, JobData jobData)
        {
            _serverData = serverData;
            _jobData = jobData;
        }

        public static readonly ConcurrentDictionary<String, String> Users = new ConcurrentDictionary<String, String>();
        public static readonly ConcurrentDictionary<Guid, RpcResult> Transactions = new ConcurrentDictionary<Guid, RpcResult>();

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
            var server = await _serverData.GetByKey(key, CancellationToken.None);

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
            var server = await _serverData.GetByKey(key, CancellationToken.None);
            var jobs = await _jobData.GetForServer(server.ServerId, CancellationToken.None);

            return new RpcUpdate
            {
                Server = server,
                Jobs = jobs
            };
        }
    }
}
