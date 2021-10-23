using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace Datack.Agent.Services
{
    public class RpcService
    {
        public event EventHandler OnConnect; 

        private readonly AppSettings _appSettings;

        private HubConnection _connection;

        private readonly Dictionary<String, Expression> _requestMethods = new();
        
        public RpcService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public void StartAsync(CancellationToken cancellationToken)
        {
            _connection = new HubConnectionBuilder()
                          .WithUrl("http://localhost:3001/hubs/agent")
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

            Connect(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_connection != null)
            {
                await _connection.StopAsync(cancellationToken);
            }
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
                        if (_connection.State == HubConnectionState.Disconnected)
                        {
                            await Task.Delay(1000, cancellationToken);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await _connection.SendAsync("Connect", _appSettings.Token, cancellationToken);

                OnConnect?.Invoke(this, null!);
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
                await _connection.SendAsync("response", result);   
            }
        }

        public async Task<T> Send<T>(String methodName)
        {
            var response = await _connection.InvokeAsync<T>(methodName, _appSettings.Token);

            return response;
        }

        public async Task SendProgress(ProgressEvent progressEvent)
        {
            await _connection.SendCoreAsync("TaskProgress", new Object[]{ new RpcProgressEvent
            {
                IsError = progressEvent.IsError,
                JobRunTaskId = progressEvent.JobRunTaskId,
                Message = progressEvent.Message
            }});
        }

        public async Task SendComplete(CompleteEvent completeEvent)
        {
            await _connection.SendCoreAsync("TaskComplete", new Object[]{ new RpcCompleteEvent
            {
                IsError = completeEvent.IsError,
                JobRunTaskId= completeEvent.JobRunTaskId,
                Message = completeEvent.Message,
                ResultArtifact = completeEvent.ResultArtifact
            }});
        }
    }
}
