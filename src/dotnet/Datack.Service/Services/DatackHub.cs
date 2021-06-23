using System;
using System.Collections.Concurrent;
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

        public DatackHub(ServerData serverData)
        {
            _serverData = serverData;
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

        public async Task<ServerDbSettings> Connect(String key)
        {
            var server = await _serverData.GetByKey(key);

            if (server == null)
            {
                throw new Exception($"Server with key {key} was not found");
            }

            Users.TryAdd(key, Context.ConnectionId);

            return server.DbSettings;
        }

        public void Response(RpcResult rpcResult)
        {
            Transactions.TryAdd(rpcResult.TransactionId, rpcResult);
        }
    }
}
