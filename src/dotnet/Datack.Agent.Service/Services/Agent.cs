using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Datack.Agent.Models;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datack.Agent.Services;

public class AgentHostedService : IHostedService
{
    private readonly AppSettings _appSettings;
    private readonly DatabaseAdapter _databaseAdapter;
    private readonly StorageAdapter _storageAdapter;
    private readonly JobRunner _jobRunner;
    private readonly DataProtector _dataProtector;
    private readonly ILogger _logger;
    private readonly RpcService _rpcService;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly String _version;

    private CancellationToken _cancellationToken;

    public AgentHostedService(ILogger<AgentHostedService> logger,
                              AppSettings appSettings,
                              DatabaseAdapter databaseAdapter,
                              StorageAdapter storageAdapter,
                              RpcService rpcService,
                              JobRunner jobRunner,
                              DataProtector dataProtector)
    {
        _logger = logger;
        _appSettings = appSettings;
        _databaseAdapter = databaseAdapter;
        _storageAdapter = storageAdapter;
        _rpcService = rpcService;
        _jobRunner = jobRunner;
        _dataProtector = dataProtector;

        _version = VersionHelper.GetVersion();

        _rpcService.Encrypt = Encrypt;
        _rpcService.GetDatabaseList = GetDatabaseList;
        _rpcService.GetFileList = GetFileList;
        _rpcService.GetLogs = GetLogs;
        _rpcService.GetRunningTasks = GetRunningTasks;
        _rpcService.Run = Run;
        _rpcService.Stop = Stop;
        _rpcService.TestDatabaseConnection = TestDatabaseConnection;
        _rpcService.UpgradeAgent = UpgradeAgent;

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

            var appSettingsSerialized = JsonSerializer.Serialize(_appSettings, JsonSerializerOptions);
                
            await File.WriteAllTextAsync(filePath, appSettingsSerialized, cancellationToken);
        }

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

    private async Task<IList<Database>> GetDatabaseList(String databaseType, String connectionString, String password, Boolean decryptPassword)
    {
        _logger.LogTrace("GetDatabaseList");

        var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, decryptPassword);

        return await _databaseAdapter.GetDatabaseList(databaseType, fullConnectionString, CancellationToken.None);
    }

    private async Task<IList<BackupFile>> GetFileList(String storageType, String connectionString, String containerName, String rootPath, String? path)
    {
        _logger.LogTrace("GetFileList");

        return await _storageAdapter.GetFileList(storageType, connectionString, containerName, rootPath, path, CancellationToken.None);
    }

    private async Task<String> GetLogs()
    {
        _logger.LogTrace("GetLogs");

        if (String.IsNullOrWhiteSpace(_appSettings.Logging?.File?.Path))
        {
            return "Logging is disabled";
        }

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
            var line = await reader.ReadLineAsync(_cancellationToken);

            if (queue.Count >= 100)
            {
                queue.Dequeue();
            }

            queue.Enqueue(line ?? "");
        }

        return String.Join(Environment.NewLine, queue.ToList());
    }

    private Task<List<Guid>> GetRunningTasks()
    {
        _logger.LogDebug("GetRunningTasks");

        var jobRunnerTasks = JobRunner.RunningTasks.Select(m => m.Key).ToList();

        var progressEvents = _rpcService.GetProgressEvents();

        var result = new List<Guid>();
        result.AddRange(jobRunnerTasks);
        result.AddRange(progressEvents);

        result = [.. result.Distinct()];

        if (result.Count > 0)
        {
            var tasks = String.Join(", ", result);
            _logger.LogDebug("Sending RunningTasks: {tasks}", tasks);
        }
        else
        {
            _logger.LogDebug("No RunningTasks to send.");
        }

        return Task.FromResult(result);
    }

    private Task<String> Run(JobRunTask jobRunTask, JobRunTask? previousTask)
    {
        _logger.LogDebug("Run {jobRunTaskId}", jobRunTask.JobRunTaskId);

        _ = Task.Run(async () =>
        {
            await _jobRunner.ExecuteJobRunTask(jobRunTask, previousTask, _cancellationToken);
        }, _cancellationToken);

        return Task.FromResult("Success");
    }

    private Task<String> Stop(Guid jobRunTaskId)
    {
        _logger.LogDebug("Stop {jobRunTaskId}", jobRunTaskId);

        _ = Task.Run(() =>
        {
            _jobRunner.StopTask(jobRunTaskId);
        }, _cancellationToken);

        return Task.FromResult("Success");
    }

    private async Task<String> TestDatabaseConnection(String databaseType, String connectionString, String password, Boolean decryptPassword)
    {
        _logger.LogDebug("Received TestDatabaseConnection Command");

        var fullConnectionString = _databaseAdapter.CreateConnectionString(connectionString, password, decryptPassword);

        return await _databaseAdapter.TestConnection(databaseType, fullConnectionString, CancellationToken.None);
    }

    private async Task<String> UpgradeAgent()
    {
        await Task.Delay(1, _cancellationToken);

        var entryAssembly = Assembly.GetEntryAssembly();

        if (entryAssembly == null)
        {
            throw new($"Cannot find EntryAssembly");
        }

        var root = Path.GetDirectoryName(entryAssembly.Location);

        if (root == null)
        {
            throw new($"Cannot create root path from {entryAssembly.Location}");
        }

        var updatePath = Path.Combine(root, "Update.ps1");

        if (!File.Exists(updatePath))
        {
            throw new($"Cannot find update file at {updatePath}");
        }

        _logger.LogDebug("Starting upgrade {updatePath}", updatePath);

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = root,
                FileName = @"C:\windows\system32\windowspowershell\v1.0\powershell.exe",
                Arguments = "-File Update.ps1"
            };

            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upgrading agent: {message}", ex.Message);
        }

        return "Success";
    }
}