using Datack.Agent.Models;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Datack.Agent.Services;

public class RpcService(ILogger<RpcService> logger, AppSettings appSettings)
{
#nullable disable
    public Func<String, Task<String>> Encrypt;
    public Func<String, String, String, Boolean, Task<IList<Database>>> GetDatabaseList;
    public Func<String, String, String, String, String, Task<IList<BackupFile>>> GetFileList;
    public Func<Task<String>> GetLogs;
    public Func<Task<List<Guid>>> GetRunningTasks;
    public Func<JobRunTask, JobRunTask, Task<String>> Run;
    public Func<Guid, Task<String>> Stop;
    public Func<String, String, String, Boolean, Task<String>> TestDatabaseConnection;
    public Func<Task<String>> UpgradeAgent;
#nullable restore

    private static readonly SemaphoreSlim TimerLock = new(1, 1);
    private static readonly SemaphoreSlim SendLock = new(1, 1);

    public HubConnection? Connection;

    private readonly String _version = VersionHelper.GetVersion();

    private readonly Dictionary<Guid, RpcProgressEvent> _progressEvents = [];
    private readonly Dictionary<Guid, RpcCompleteEvent> _completeEvents = [];

    private readonly Timer _sendTimer = new(1000);

    public void StartAsync(CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(appSettings.ServerUrl))
        {
            throw new($"No server URL set. Please update appsettings.json");
        }

        var url = $"{appSettings.ServerUrl.TrimEnd('/')}/hubs/agent";

        logger.LogDebug("Connecting to {url}", url);

        Connection = new HubConnectionBuilder()
                      .WithUrl(url)
                      .ConfigureLogging(logging => 
                      {
                          logging.AddProvider(new SerilogLoggerProvider());
                          logging.SetMinimumLevel(LogLevel.Debug);
                          logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Trace);
                          logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
                      })
                      .Build();

        Connection.ServerTimeout = TimeSpan.FromMinutes(30);
        Connection.HandshakeTimeout = TimeSpan.FromMinutes(2);

        Connection.Closed += exception =>
        {
            if (exception == null)
            {
                logger.LogDebug("Connection Closed");
            }
            else
            {
                logger.LogError(exception, "Connection Closed with error: {message}", exception.Message);
            }

            Connect(cancellationToken);

            return Task.CompletedTask;
        };

        Connection.On("Encrypt",Encrypt);
        Connection.On("GetDatabaseList", GetDatabaseList);
        Connection.On("GetFileList", GetFileList);
        Connection.On("GetLogs", GetLogs);
        Connection.On("GetRunningTasks", GetRunningTasks);
        Connection.On("Run", Run);
        Connection.On("Stop", Stop);
        Connection.On("TestDatabaseConnection", TestDatabaseConnection);
        Connection.On("UpgradeAgent", UpgradeAgent);
        
        _sendTimer.Elapsed += async (_, _) => await TimerTick();
        _sendTimer.Start();

        logger.LogDebug("Starting timer");

        Connect(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Stopping");

        if (Connection != null)
        {
            await Connection.StopAsync(cancellationToken);
        }
    }

    private void Connect(CancellationToken cancellationToken)
    {
        logger.LogDebug("Connecting...");

        if (Connection == null)
        {
            throw new("Cannot connect, connection is not initialized yet");
        }

        _ = Task.Run(async () =>
        {
            while (true)
            {
                logger.LogDebug("Trying to connect");

                try
                {
                    await Connection.StartAsync(cancellationToken);

                    break;
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }

            logger.LogDebug("Connected");

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
                
            logger.LogDebug("Connect {token} v{_version}", appSettings.Token, _version);

            await Connection.SendAsync("Connect", appSettings.Token, _version, cancellationToken);
        }, cancellationToken);
    }
    
    private async Task TimerTick()
    {
        var hasLock = await TimerLock.WaitAsync(100);

        if (!hasLock)
        {
            logger.LogDebug("Failed lock");
            return;
        }

        try
        {
            if (Connection?.State != HubConnectionState.Connected)
            {
                return;
            }

            Dictionary<Guid, RpcProgressEvent> progressEvents;
            Dictionary<Guid, RpcCompleteEvent> completeEvents;

            var receivedLock = await SendLock.WaitAsync(500);

            if (!receivedLock)
            {
                return;
            }

            try
            {
                if (_progressEvents.Count == 0 && _completeEvents.Count == 0)
                {
                    return;
                }

                progressEvents = new(_progressEvents);
                completeEvents = new(_completeEvents);
            }
            finally
            {
                SendLock.Release();
            }

            try
            {
                logger.LogDebug("Sending {count} progress events, {count} complete events", progressEvents.Count, completeEvents.Count);

                var progressEventsChunks = progressEvents.ChunkBy(100);
                var completeEventsChunks = completeEvents.ChunkBy(100);

                foreach (var progressEventsChunk in progressEventsChunks)
                {
                    var innerCancellationToken = new CancellationTokenSource(500).Token;
                    await Connection.SendAsync("UpdateProgress", progressEventsChunk.Select(m => m.Value).ToList(), innerCancellationToken);
                }

                foreach (var completeEventsChunk in completeEventsChunks)
                {
                    var innerCancellationToken = new CancellationTokenSource(500).Token;
                    await Connection.SendAsync("UpdateComplete", completeEventsChunk.Select(m => m.Value).ToList(), innerCancellationToken);
                }

                await SendLock.WaitAsync();

                try
                {
                    foreach (var (key, _) in progressEvents)
                    {
                        _progressEvents.Remove(key);
                    }

                    foreach (var (key, _) in completeEvents)
                    {
                        _completeEvents.Remove(key);
                    }
                }
                finally
                {
                    SendLock.Release();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending updates: {message}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in timer loop: {message}", ex.Message);
        }
        finally
        {
            TimerLock.Release();
        }
    }

    public async Task QueueProgress(ProgressEvent progressEvent)
    {
        await SendLock.WaitAsync();

        try
        {
            _progressEvents.Add(Guid.NewGuid(), new()
            {
                IsError = progressEvent.IsError,
                JobRunTaskId = progressEvent.JobRunTaskId,
                Message = progressEvent.Message
            });
        }
        finally
        {
            SendLock.Release();
        }
    }

    public async Task QueueComplete(CompleteEvent completeEvent)
    {
        await SendLock.WaitAsync();

        try
        {
            _completeEvents.Add(Guid.NewGuid(), new()
            {
                IsError = completeEvent.IsError,
                JobRunTaskId = completeEvent.JobRunTaskId,
                Message = completeEvent.Message,
                ResultArtifact = completeEvent.ResultArtifact
            });
        }
        finally
        {
            SendLock.Release();
        }
    }

    public IList<Guid> GetProgressEvents()
    {
        var progressEvents = _progressEvents.Keys.ToList();
        var completeEvents = _completeEvents.Keys.ToList();

        var result = new List<Guid>();
        result.AddRange(progressEvents);
        result.AddRange(completeEvents);

        return [.. result.Distinct()];
    }
}