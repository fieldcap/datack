using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Service.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Service.Services
{
    public class Servers
    {
        private readonly DataContext _dataContext;
        private readonly RemoteService _remoteService;

        public Servers(DataContext dataContext, RemoteService remoteService)
        {
            _dataContext = dataContext;
            _remoteService = remoteService;
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
