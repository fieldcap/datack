using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
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

        public async Task<Server> GetByKey(String key, CancellationToken cancellationToken)
        {
            return await _dataContext.Servers.AsNoTracking().FirstOrDefaultAsync(m => m.Key == key, cancellationToken);
        }

        public async Task<Guid> Add(Server server, CancellationToken cancellationToken)
        {
            server.ServerId = Guid.NewGuid();

            await _dataContext.Servers.AddAsync(server, cancellationToken);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return server.ServerId;
        }

        public async Task Update(Server server, CancellationToken cancellationToken)
        {
            var dbServer = await _dataContext.Servers.FirstOrDefaultAsync(m => m.ServerId == server.ServerId, cancellationToken);

            if (dbServer == null)
            {
                throw new Exception($"Server with ID {server.ServerId} not found");
            }

            dbServer.Name = server.Name;
            dbServer.Description = server.Description;
            dbServer.Settings = server.Settings;
            dbServer.DbSettings = server.DbSettings;

            await _dataContext.SaveChangesAsync(cancellationToken);
        }
    }
}
