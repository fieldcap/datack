using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Helpers;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Datack.Agent.Services
{
    public class RpcService
    {
        private readonly AppSettings _appSettings;

        private HubConnection _connection;

        private readonly Dictionary<String, Expression> _requestMethods = new();
        private readonly String _version;

        private readonly SemaphoreSlim _sendLock = new(1);
        private readonly Timer _sendTimer = new Timer(1000);
        private readonly Dictionary<Guid, RpcProgressEvent> _progressEvents = new();
        private readonly Dictionary<Guid, RpcCompleteEvent> _completeEvents = new();

        public RpcService(AppSettings appSettings)
        {
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

            _connection.Closed += _ => Connect(cancellationToken);

            _connection.On<RpcRequest>("request", HandleRequest);

            _sendTimer.Elapsed += async (_, _) => await TimerTick();
            _sendTimer.Start();

            Connect(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
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

        private Task Connect(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
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

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var hasPendingEvents = _progressEvents.Any() && _completeEvents.Any();

                await _connection.SendAsync("Connect", _appSettings.Token, _version, hasPendingEvents, cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
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
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            Dictionary<Guid, RpcProgressEvent> progressEvents;
            Dictionary<Guid, RpcCompleteEvent> completeEvents;

            var receivedLock = await _sendLock.WaitAsync(500);

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
                _sendLock.Release();
            }

            try
            {
                await _connection.SendAsync("Update", progressEvents.Values.ToList(), completeEvents.Values.ToList());

                await _sendLock.WaitAsync();

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
                    _sendLock.Release();
                }
            }
            catch
            {
                // ignored
            }
        }

        public async Task QueueProgress(ProgressEvent progressEvent)
        {
            await _sendLock.WaitAsync();

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
                _sendLock.Release();
            }
        }

        public async Task QueueComplete(CompleteEvent completeEvent)
        {
            await _sendLock.WaitAsync();

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
                _sendLock.Release();
            }
        }
    }
}
