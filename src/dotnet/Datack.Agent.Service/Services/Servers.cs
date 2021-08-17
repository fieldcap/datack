using System.Linq;
using System.Threading.Tasks;
using Datack.Agent.Data;
using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Agent.Services
{
    public class Servers
    {
        private readonly DataContext _dataContext;

        private static Server _server;

        public Servers(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Server> GetServer()
        {
            if (_server == null)
            {
                _server = await _dataContext.Servers.FirstOrDefaultAsync();
            }

            return _server;
        }

        public async Task UpdateServer(Server server)
        {
            var dbServers = await _dataContext.Servers.ToListAsync();

            var otherServers = dbServers.Where(m => m.Key != server.Key).ToList();

            if (otherServers.Count > 0)
            {
                _dataContext.RemoveRange(otherServers);
                await _dataContext.SaveChangesAsync();
            }

            if (dbServers.Count == 0)
            {
                await _dataContext.Servers.AddAsync(server);
                await _dataContext.SaveChangesAsync();
            }
            else
            {
                var dbServer = dbServers.First(m => m.Key == server.Key);
                dbServer.Name = server.Name;
                dbServer.Description = server.Description;
                dbServer.DbSettings = server.DbSettings;
                dbServer.Settings = server.Settings;

                await _dataContext.SaveChangesAsync();
            }

            _server = null;
        }
    }
}
