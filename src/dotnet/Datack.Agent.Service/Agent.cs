using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Helpers;
using Datack.Agent.Models;
using Datack.Agent.Services;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datack.Agent
{
    public class Agent : IHostedService
    {
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;

        private readonly JobScheduler _jobScheduler = new JobScheduler();
        private readonly RpcService _rpcService;

        private Server _server;

        public Agent(ILogger<Agent> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;

            _rpcService = new RpcService(_appSettings);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(_appSettings.Token))
            {
                throw new Exception($"Token cannot be null");
            }

            _rpcService.Subscribe("Connect", _ => OnConnect());
            _rpcService.Subscribe("GetDatabaseList", _ => GetDatabaseList());
            _rpcService.Subscribe("TestSqlServer", result => TestSqlServer(result as ServerDbSettings));

            await _rpcService.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _rpcService.StopAsync(cancellationToken);
        }

        private async Task OnConnect()
        {
            var response = await _rpcService.Send<RpcUpdate>("RpcUpdate");

            _server = response.Server;

            _jobScheduler.Update(response.Server, response.Jobs);
        }
        
        private async Task<String> TestSqlServer(ServerDbSettings serverDbSettings)
        {
            var result = await SqlHelper.TestDatabaseConnection(serverDbSettings);

            return result;
        }
        
        private async Task<IList<DatabaseList>> GetDatabaseList()
        {
            if (_server == null || String.IsNullOrWhiteSpace(_server.DbSettings.Server))
            {
                throw new Exception($"Server SQL connection not configured");
            }

            var result = await SqlHelper.GetDatabaseList(_server.DbSettings);

            return result;
        }
    }
}
