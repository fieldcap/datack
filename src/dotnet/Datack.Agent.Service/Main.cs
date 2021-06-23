using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Service.Helpers;
using Datack.Common.Models.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Datack.Agent.Service
{
    public class Main
    {
        private readonly CancellationToken _cancellationToken;
        private RpcService _rpcService;

        private ServerDbSettings _serverDbSettings;

        public Main(ILogger logger, CancellationToken cancellationToken)
        {
            Log.Logger = logger;
            _cancellationToken = cancellationToken;
        }

        public async Task Start(String token)
        {
            if (String.IsNullOrWhiteSpace(token))
            {
                throw new Exception($"Token cannot be null");
            }

            _rpcService = new RpcService(_cancellationToken, token, this);

            await _rpcService.StartConnection();
        }

        // ReSharper disable once UnusedMember.Local, used in RpcService
        private void RpcConnect(ServerDbSettings serverDbSettings)
        {
            _serverDbSettings = serverDbSettings;
        }
        
        // ReSharper disable once UnusedMember.Local, used in RpcService
        private async Task<String> TestSqlServer(ServerDbSettings serverDbSettings)
        {
            var result = await SqlHelper.TestDatabaseConnection(serverDbSettings);

            return result;
        }
        
        // ReSharper disable once UnusedMember.Local, used in RpcService
        private async Task<IList<String>> GetDatabaseList()
        {
            if (_serverDbSettings == null || String.IsNullOrWhiteSpace(_serverDbSettings.Server))
            {
                throw new Exception($"Server SQL connection not configured");
            }

            var result = await SqlHelper.GetDatabaseList(_serverDbSettings);

            return result;
        }
    }
}
