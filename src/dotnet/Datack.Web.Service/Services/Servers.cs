using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class Servers
    {
        private readonly ServerRepository _serverRepository;
        private readonly RemoteService _remoteService;

        public Servers(ServerRepository serverRepository, RemoteService remoteService)
        {
            _serverRepository = serverRepository;
            _remoteService = remoteService;
        }

        public async Task<IList<Server>> GetAll(CancellationToken cancellationToken)
        {
            return await _serverRepository.GetAll(cancellationToken);
        }

        public async Task<Server> GetById(Guid serverId, CancellationToken cancellationToken)
        {
            return await _serverRepository.GetById(serverId, cancellationToken);
        }

        public async Task<Server> GetByKey(String key, CancellationToken cancellationToken)
        {
            return await _serverRepository.GetByKey(key, cancellationToken);
        }
        
        public async Task<Guid> Add(Server server, CancellationToken cancellationToken)
        {
            return await _serverRepository.Add(server, cancellationToken);
        }

        public async Task Update(Server server, CancellationToken cancellationToken)
        {
            await _serverRepository.Update(server, cancellationToken);
        }

        public async Task<String> TestSqlServerConnection(Server server, CancellationToken cancellationToken)
        {
            return await _remoteService.TestSqlServerConnection(server, cancellationToken);
        }

        public async Task<IList<Database>> GetDatabaseList(Guid serverId, CancellationToken cancellationToken)
        {
            var server = await GetById(serverId, cancellationToken);

            return await _remoteService.GetDatabaseList(server, cancellationToken);
        }
    }
}
