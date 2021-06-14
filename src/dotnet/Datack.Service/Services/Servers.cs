using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Data;
using Datack.Data.Models.Data;
using Datack.Data.Models.Internal;

namespace Datack.Service.Services
{
    public class Servers
    {
        private readonly ServerData _serverData;

        public Servers(ServerData serverData)
        {
            _serverData = serverData;
        }

        public async Task<IList<Server>> GetAll(CancellationToken cancellationToken)
        {
            return await _serverData.GetAll(cancellationToken);
        }

        public async Task<Server> GetById(Guid serverId, CancellationToken cancellationToken)
        {
            return await _serverData.GetById(serverId, cancellationToken);
        }

        public async Task UpdateDbSettings(Guid serverId, ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            await _serverData.UpdateDbSettings(serverId, serverDbSettings, cancellationToken);
        }
    }
}
