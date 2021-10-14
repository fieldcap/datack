﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Enums;
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
        private readonly Jobs _jobs;
        private readonly Servers _servers;
        private readonly Steps _steps;

        private readonly JobScheduler _jobScheduler;
        private readonly RpcService _rpcService;
        
        public AgentHostedService(ILogger<AgentHostedService> logger, AppSettings appSettings, DatabaseAdapter databaseAdapter, Jobs jobs, JobScheduler jobScheduler, Servers servers, Steps steps)
        {
            _logger = logger;
            _appSettings = appSettings;
            _databaseAdapter = databaseAdapter;
            _jobs = jobs;
            _jobScheduler = jobScheduler;
            _servers = servers;
            _steps = steps;

            _rpcService = new RpcService(_appSettings);

            _logger.LogTrace("Agent Constructor");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Starting");

            if (String.IsNullOrWhiteSpace(_appSettings.Token))
            {
                throw new Exception($"Token cannot be null");
            }

            _rpcService.OnConnect += (_, _) => Connect();

            _rpcService.Subscribe("GetDatabaseList", () => GetDatabaseList());
            _rpcService.Subscribe<Guid, BackupType>("Run", (jobId, backupType) => Run(jobId, backupType));
            _rpcService.Subscribe<ServerDbSettings>("TestSqlServer", serverDbSettings => TestSqlServer(serverDbSettings));

            _rpcService.StartAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping");

            await _rpcService.StopAsync(cancellationToken);
            _jobScheduler.Stop();
        }

        private async void Connect()
        {
            _logger.LogTrace("Connect");

            var response = await _rpcService.Send<RpcUpdate>("RpcUpdate");

            await _servers.UpdateServer(response.Server);
            await _jobs.UpdateJobs(response.Jobs);
            await _steps.UpdateSteps(response.Steps, response.Server.ServerId);

            _jobScheduler.Start();
        }

        private async Task<IList<Database>> GetDatabaseList()
        {
            _logger.LogTrace("GetDatabaseList");

            return await _databaseAdapter.GetDatabaseList();
        }

        private async Task<String> TestSqlServer(ServerDbSettings serverDbSettings)
        {
            _logger.LogTrace("TestSqlServer");

            return await _databaseAdapter.TestConnection(serverDbSettings);
        }

        private async Task<String> Run(Guid jobId, BackupType backupType)
        {
            _logger.LogTrace("Run {jobId} {backupType}", jobId, backupType);

            

            return "Success";
        }
    }
}
