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
        private readonly AppSettings _appSettings;
        private readonly DatabaseAdapter _databaseAdapter;
        private readonly JobRunner _jobRunner;
        private readonly DataProtector _dataProtector;
        private readonly ILogger _logger;
        private readonly RpcService _rpcService;

        private CancellationToken _cancellationToken;

        public AgentHostedService(ILogger<AgentHostedService> logger,
                                  AppSettings appSettings,
                                  DatabaseAdapter databaseAdapter,
                                  RpcService rpcService,
                                  JobRunner jobRunner,
                                  DataProtector dataProtector)
        {
            _logger = logger;
            _appSettings = appSettings;
            _databaseAdapter = databaseAdapter;
            _rpcService = rpcService;
            _jobRunner = jobRunner;
            _dataProtector = dataProtector;

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

            _rpcService.Subscribe<String>("Encrypt", value => Encrypt(value));
            _rpcService.Subscribe<String, String>("GetDatabaseList", (connectionString, password) => GetDatabaseList(connectionString, password));
            _rpcService.Subscribe<JobRunTask, JobRunTask>("Run", (jobRunTask, previousTask) => Run(jobRunTask, previousTask));
            _rpcService.Subscribe<Guid>("Stop", jobRunTaskId => Stop(jobRunTaskId));
            _rpcService.Subscribe<String, String>("TestDatabaseConnection", (connectionString, password) => TestDatabaseConnection(connectionString, password));

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
        }

        private async Task<String> Encrypt(String input)
        {
            _logger.LogTrace("Encrypt");

            await Task.Delay(1, _cancellationToken);

            return _dataProtector.Encrypt(input);
        }

        private async Task<IList<Database>> GetDatabaseList(String connectionString, String password)
        {
            _logger.LogTrace("GetDatabaseList");

            var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, true);

            return await _databaseAdapter.GetDatabaseList(fullConnectionString, CancellationToken.None);
        }

        private async Task<String> TestDatabaseConnection(String connectionString, String password)
        {
            _logger.LogTrace("TestDatabaseConnection");

            var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, false);

            return await _databaseAdapter.TestConnection(fullConnectionString, CancellationToken.None);
        }

        private async Task<String> Run(JobRunTask jobRunTask, JobRunTask previousTask)
        {
            await _jobRunner.ExecuteJobRunTask(jobRunTask, previousTask, _cancellationToken);

            return "Success";
        }

        private Task<String> Stop(Guid jobRunTaskId)
        {
            _jobRunner.StopTask(jobRunTaskId);

            return Task.FromResult("Success");
        }
    }
}
