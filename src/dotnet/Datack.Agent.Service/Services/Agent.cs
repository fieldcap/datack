using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Helpers;
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

        private readonly String _version;

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

            _version = VersionHelper.GetVersion();

            _logger.LogTrace("Agent Constructor");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;

            _logger.LogInformation("Starting version {_version}", _version);

            // Check if the agent has a key setting set, if not, generate a new one and save it.
            if (String.IsNullOrWhiteSpace(_appSettings.Token))
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

                _appSettings.Token = Guid.NewGuid().ToString();

                var appSettingsSerialized = JsonSerializer.Serialize(_appSettings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                await File.WriteAllTextAsync(filePath, appSettingsSerialized, cancellationToken);
            }

            _rpcService.Subscribe<String>("Encrypt", value => Encrypt(value));
            _rpcService.Subscribe<String, String, Boolean>("GetDatabaseList", (connectionString, password, decryptPassword) => GetDatabaseList(connectionString, password, decryptPassword));
            _rpcService.Subscribe("GetLogs", () => GetLogs());
            _rpcService.Subscribe<JobRunTask, JobRunTask>("Run", (jobRunTask, previousTask) => Run(jobRunTask, previousTask));
            _rpcService.Subscribe<Guid>("Stop", jobRunTaskId => Stop(jobRunTaskId));
            _rpcService.Subscribe<String, String, Boolean>("TestDatabaseConnection", (connectionString, password, decryptPassword) => TestDatabaseConnection(connectionString, password, decryptPassword));

            _rpcService.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping");

            await _rpcService.StopAsync(cancellationToken);

            _jobRunner.StopAllTasks();
        }
        
        private async Task<String> Encrypt(String input)
        {
            _logger.LogTrace("Encrypt");

            await Task.Delay(1, _cancellationToken);

            return _dataProtector.Encrypt(input);
        }

        private async Task<IList<Database>> GetDatabaseList(String connectionString, String password, Boolean decryptPassword)
        {
            _logger.LogTrace("GetDatabaseList");

            var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, decryptPassword);

            return await _databaseAdapter.GetDatabaseList(fullConnectionString, CancellationToken.None);
        }

        private async Task<String> GetLogs()
        {
            _logger.LogTrace("GetDatabaseList");

            var logFilePath = _appSettings.Logging.File.Path;

            if (!File.Exists(logFilePath))
            {
                return $"Log file at {logFilePath} cannot be found";
            }

            await using var stream = File.Open(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            using var reader = new StreamReader(stream);
            
            var queue = new Queue<String>(100);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (queue.Count >= 100)
                {
                    queue.Dequeue();
                }

                queue.Enqueue(line);
            }

            return String.Join(Environment.NewLine, queue.ToList());
        }

        private Task<String> Run(JobRunTask jobRunTask, JobRunTask previousTask)
        {
            _ = Task.Run(async () =>
            {
                await _jobRunner.ExecuteJobRunTask(jobRunTask, previousTask, _cancellationToken);
            }, _cancellationToken);

            return Task.FromResult("Success");
        }

        private Task<String> Stop(Guid jobRunTaskId)
        {
            _ = Task.Run(() =>
            {
                _jobRunner.StopTask(jobRunTaskId);
            }, _cancellationToken);

            return Task.FromResult("Success");
        }

        private async Task<String> TestDatabaseConnection(String connectionString, String password, Boolean decryptPassword)
        {
            _logger.LogTrace("TestDatabaseConnection");

            var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, decryptPassword);

            return await _databaseAdapter.TestConnection(fullConnectionString, CancellationToken.None);
        }
    }
}
