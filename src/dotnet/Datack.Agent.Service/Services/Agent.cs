using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datack.Agent.Services
{
    public class AgentHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly DatabaseAdapter _databaseAdapter;
        private readonly RpcService _rpcService;
        private readonly JobRunner _jobRunner;

        private CancellationToken _cancellationToken;

        private Server _server;

        public AgentHostedService(ILogger<AgentHostedService> logger, AppSettings appSettings, DatabaseAdapter databaseAdapter, RpcService rpcService, JobRunner jobRunner)
        {
            _logger = logger;
            _appSettings = appSettings;
            _databaseAdapter = databaseAdapter;
            _rpcService = rpcService;
            _jobRunner = jobRunner;

            _logger.LogTrace("Agent Constructor");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _logger.LogTrace("Starting");

            if (String.IsNullOrWhiteSpace(_appSettings.Token))
            {
                throw new Exception($"Token cannot be null");
            }

            _rpcService.OnConnect += (_, _) => Connect();

            _rpcService.Subscribe("GetDatabaseList", () => GetDatabaseList());
            _rpcService.Subscribe<JobRunTask, JobRunTask>("Run", (jobRunTask, previousTask) => Run(jobRunTask, previousTask));
            _rpcService.Subscribe<Guid>("Stop", jobRunTaskId => Stop(jobRunTaskId));
            _rpcService.Subscribe<ServerDbSettings>("TestSqlServer", serverDbSettings => TestSqlServer(serverDbSettings));

            _rpcService.StartAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping");

            await _rpcService.StopAsync(cancellationToken);

            _jobRunner.StopAllTasks();
        }

        private async void Connect()
        {
            _logger.LogTrace("Connect");

            var response = await _rpcService.Send<RpcUpdate>("RpcUpdate");

            _server = response.Server;
        }

        private async Task<IList<Database>> GetDatabaseList()
        {
            _logger.LogTrace("GetDatabaseList");

            return await _databaseAdapter.GetDatabaseList(_server.DbSettings, CancellationToken.None);
        }

        private async Task<String> TestSqlServer(ServerDbSettings serverDbSettings)
        {
            _logger.LogTrace("TestSqlServer");

            return await _databaseAdapter.TestConnection(serverDbSettings, CancellationToken.None);
        }

        private async Task<String> Run(JobRunTask jobRunTask, JobRunTask previousTask)
        {
            if (_server == null)
            {
                throw new Exception($"No server settings found");
            }

            await _jobRunner.ExecuteJobRunTask(_server, jobRunTask, previousTask, _cancellationToken);

            return "Success";
        }
        
        private Task<String> Stop(Guid jobRunTaskId)
        {
            if (_server == null)
            {
                throw new Exception($"No server settings found");
            }

            _jobRunner.StopTask(jobRunTaskId);

            return Task.FromResult("Success");
        }
    }
}
