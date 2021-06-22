using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Data.Data;

namespace Datack.Service.Services
{
    public class Servers
    {
        private readonly ServerData _serverData;
        private readonly RemoteService _remoteService;

        public Servers(ServerData serverData, RemoteService remoteService)
        {
            _serverData = serverData;
            _remoteService = remoteService;
        }

        public async Task<IList<Server>> GetAll(CancellationToken cancellationToken)
        {
            return await _serverData.GetAll(cancellationToken);
        }

        public async Task<Server> GetById(Guid serverId, CancellationToken cancellationToken)
        {
            return await _serverData.GetById(serverId, cancellationToken);
        }
        
        public async Task<Server> Add(Server server, CancellationToken cancellationToken)
        {
            var newServerId = await _serverData.Add(server, cancellationToken);

            return await GetById(newServerId, cancellationToken);
        }

        public async Task Update(Server server, CancellationToken cancellationToken)
        {
            await _serverData.Update(server, cancellationToken);
        }

        public async Task Test(Server server, CancellationToken cancellationToken)
        {
            await _remoteService.TestSqlServer(server.Key, server.DbSettings, cancellationToken);
        }
    }
}
