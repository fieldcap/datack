using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Users.TryRemove(Context.ConnectionId, out _);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Connect(String key)
        {
            var server = await _serverData.GetByKey(key);

            if (server == null)
            {
                throw new Exception($"Server with key {key} was not found");
            }

            Users.TryAdd(Context.ConnectionId, key);
        }
    }
}
