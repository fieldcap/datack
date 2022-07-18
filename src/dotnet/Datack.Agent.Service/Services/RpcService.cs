using System.Linq.Expressions;
using System.Text.Json;
using Datack.Agent.Models;
using Datack.Common.Helpers;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Datack.Agent.Services;

public class RpcService
{
    private static readonly SemaphoreSlim TimerLock = new(1, 1);
    private static readonly SemaphoreSlim SendLock = new(1, 1);

    private readonly ILogger<RpcService> _logger;
    private readonly AppSettings _appSettings;

    private HubConnection _connection;

    private readonly Dictionary<String, Expression> _requestMethods = new();
    private readonly String _version;

    private readonly Dictionary<Guid, RpcProgressEvent> _progressEvents = new();
    private readonly Dictionary<Guid, RpcCompleteEvent> _completeEvents = new();

    private readonly Timer _sendTimer = new(1000);

    public RpcService(ILogger<RpcService> logger, AppSettings appSettings)
    {
        _logger = logger;
        _appSettings = appSettings;
        _version = VersionHelper.GetVersion();
    }

    public void StartAsync(CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(_appSettings.ServerUrl))
        {
            throw new Exception($"No server URL set. Please update appsettings.json");
        }

        var url = $"{_appSettings.ServerUrl.TrimEnd('/')}/hubs/agent";

        _logger.LogDebug("Connecting to {url}", url);

        _connection = new HubConnectionBuilder()
                      .WithUrl(url)
                      .ConfigureLogging(logging => 
                      {
                          logging.AddProvider(new SerilogLoggerProvider());
                          logging.SetMinimumLevel(LogLevel.Debug);
                          logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Trace);
                          logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Warning);
                      })
                      .Build();

        _connection.ServerTimeout = TimeSpan.FromMinutes(30);
        _connection.HandshakeTimeout = TimeSpan.FromMinutes(2);

        _connection.Closed += exception =>
        {
            if (exception == null)
            {
                _logger.LogDebug("Connection Closed");
            }
            else
            {
                _logger.LogError(exception, "Connection Closed with error: {message}", exception.Message);
            }

            Connect(cancellationToken);

            return Task.CompletedTask;
        };

        _connection.On<RpcRequest>("request", HandleRequest);

        _sendTimer.Elapsed += async (_, _) => await TimerTick();
        _sendTimer.Start();

        _logger.LogDebug("Starting timer");

        Connect(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopping");

        if (_connection != null)
        {
            await _connection.StopAsync(cancellationToken);
        }
    }
        
    public void Subscribe(String methodName, Expression<Func<Task>> method)
    {
        _requestMethods.Add(methodName, method);
    }

    public void Subscribe<T>(String methodName, Expression<Func<T, Task>> method)
    {
        _requestMethods.Add(methodName, method);
    }

    public void Subscribe<T1, T2>(String methodName, Expression<Func<T1, T2, Task>> method)
    {
        _requestMethods.Add(methodName, method);
    }

    public void Subscribe<T1, T2, T3>(String methodName, Expression<Func<T1, T2, T3, Task>> method)
    {
        _requestMethods.Add(methodName, method);
    }

    private void Connect(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Connecting...");

        _ = Task.Run(async () =>
        {
            while (true)
            {
                _logger.LogDebug("Trying to connect");

                try
                {
                    await _connection.StartAsync(cancellationToken);

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

            _logger.LogDebug("Connected");

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
                
            _logger.LogDebug("Connect {token} v{_version}", _appSettings.Token, _version);

            await _connection.SendAsync("Connect", _appSettings.Token, _version, cancellationToken);
        }, cancellationToken);
    }

    private async Task HandleRequest(RpcRequest rpcRequest)
    {
        var result = new RpcResult(rpcRequest.TransactionId);

        try
        {
            if (!_requestMethods.ContainsKey(rpcRequest.Request))
            {
                return;
            }

            var methodInfo = _requestMethods[rpcRequest.Request];

            Task methodResult = null;

            if (methodInfo is Expression<Func<Task>> methodInfo2)
            {
                methodResult = methodInfo2.Compile().Invoke();
            }
            else if (methodInfo is LambdaExpression lambdaExpression)
            {
                var payloadParameters = JsonSerializer.Deserialize<JsonElement[]>(rpcRequest.Payload);

                var parameters = lambdaExpression.Parameters.ToList();

                if (payloadParameters == null || payloadParameters.Length != parameters.Count)
                {
                    throw new Exception($"Parameter count mismatch for {rpcRequest.Request}. Received {payloadParameters?.Length}, expected {parameters.Count}");
                }

                var invokationParameters = new Object[parameters.Count];

                for (var i = 0; i < parameters.Count; i++)
                {
                    var payloadParameterRaw = payloadParameters[i].GetRawText();
                    var payloadParameterObject = JsonSerializer.Deserialize(payloadParameterRaw, parameters[i].Type);
                    invokationParameters[i] = payloadParameterObject;
                }

                methodResult = lambdaExpression.Compile().DynamicInvoke(invokationParameters) as Task;
            }

            if (methodResult == null)
            {
                throw new Exception($"Unable to invoke request {rpcRequest.Request}");
            }

            var taskResult = methodResult.GetType().GetProperty("Result");

            if (taskResult == null)
            {
                throw new Exception($"Task returned NULL value and cannot be awaited");
            }

            var invokationResult = taskResult.GetValue(methodResult);

            result.Result = JsonSerializer.Serialize(invokationResult);
        }
        catch (Exception ex)
        {
            var rpcException = ex.ToRpcException();
            result.Error = JsonSerializer.Serialize(rpcException);
        }
        finally
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.SendAsync("response", result);
            }
        }
    }

    private async Task TimerTick()
    {
        var hasLock = await TimerLock.WaitAsync(100);

        if (!hasLock)
        {
            _logger.LogDebug("Failed lock");
            return;
        }

        try
        {
            if (_connection.State != HubConnectionState.Connected)
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
                if (!_progressEvents.Any() && !_completeEvents.Any())
                {
                    return;
                }

                progressEvents = new Dictionary<Guid, RpcProgressEvent>(_progressEvents);
                completeEvents = new Dictionary<Guid, RpcCompleteEvent>(_completeEvents);
            }
            finally
            {
                SendLock.Release();
            }

            try
            {
                _logger.LogDebug("Sending {count} progress events, {count} complete events", progressEvents.Count, completeEvents.Count);

                var progressEventsChunks = progressEvents.ChunkBy(100);
                var completeEventsChunks = completeEvents.ChunkBy(100);

                foreach (var progressEventsChunk in progressEventsChunks)
                {
                    var innerCancellationToken = new CancellationTokenSource(500).Token;
                    await _connection.SendAsync("UpdateProgress", progressEventsChunk.Select(m => m.Value).ToList(), innerCancellationToken);
                }

                foreach (var completeEventsChunk in completeEventsChunks)
                {
                    var innerCancellationToken = new CancellationTokenSource(500).Token;
                    await _connection.SendAsync("UpdateComplete", completeEventsChunk.Select(m => m.Value).ToList(), innerCancellationToken);
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
                _logger.LogError(ex, "Error sending updates: {message}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in timer loop: {message}", ex.Message);
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
            _progressEvents.Add(Guid.NewGuid(), new RpcProgressEvent
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
            _completeEvents.Add(Guid.NewGuid(), new RpcCompleteEvent
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

        return result.Distinct().ToList();
    }
}