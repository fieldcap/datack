using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
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

        public async Task TestSqlServer(String key, ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            var hasConnection = DatackHub.Users.TryGetValue(key, out var connectionId);

            if (!hasConnection)
            {
                throw new Exception($"No connection found for server {key}");
            }

            await _hub.Clients.User(connectionId).SendAsync("TestSqlServer", serverDbSettings, cancellationToken);
        }
    }
}
