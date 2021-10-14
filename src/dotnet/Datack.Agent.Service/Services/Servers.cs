using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class Servers
    {
        private readonly DataContextFactory _dataContextFactory;
        private static Server _server;

        public Servers(DataContextFactory dataContextFactory)
        {
            _dataContextFactory = dataContextFactory;
        }

        public async Task<Server> GetServer()
        {
            await using var context = _dataContextFactory.Create();

            if (_server == null)
            {
                _server = await context.Servers.FirstOrDefaultAsync();
            }

            return _server;
        }

        public async Task UpdateServer(Server server)
        {
            await using var context = _dataContextFactory.Create();

            var dbServers = await context.Servers.ToListAsync();

            var otherServers = dbServers.Where(m => m.Key != server.Key).ToList();

            if (otherServers.Count > 0)
            {
                context.RemoveRange(otherServers);
                await context.SaveChangesAsync();
            }

            if (dbServers.Count == 0)
            {
                await context.Servers.AddAsync(server);
                await context.SaveChangesAsync();
            }
            else
            {
                var dbServer = dbServers.First(m => m.Key == server.Key);
                dbServer.Name = server.Name;
                dbServer.Description = server.Description;
                dbServer.DbSettings = server.DbSettings;
                dbServer.Settings = server.Settings;

                await context.SaveChangesAsync();
            }

            _server = null;
        }
    }
}
