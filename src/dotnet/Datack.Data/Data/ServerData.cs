using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Models.Data;
using Datack.Data.Models.Internal;
using Microsoft.EntityFrameworkCore;

namespace Datack.Data.Data
{
    public class ServerData
    {
        private readonly DataContext _dataContext;

        public ServerData(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IList<Server>> GetAll(CancellationToken cancellationToken)
        {
            return await _dataContext.Servers.AsNoTracking().OrderBy(m => m.Name).ToListAsync(cancellationToken);
        }

        public async Task<Server> GetById(Guid serverId, CancellationToken cancellationToken)
        {
            return await _dataContext.Servers.AsNoTracking().FirstOrDefaultAsync(m => m.ServerId == serverId, cancellationToken);
        }

        public async Task UpdateDbSettings(Guid serverId, ServerDbSettings serverDbSettings, CancellationToken cancellationToken)
        {
            var dbServer = await _dataContext.Servers.FirstOrDefaultAsync(m => m.ServerId == serverId, cancellationToken);

            if (dbServer == null)
            {
                throw new Exception($"Server with ID {serverId} not found");
            }

            dbServer.DbSettings = serverDbSettings;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
