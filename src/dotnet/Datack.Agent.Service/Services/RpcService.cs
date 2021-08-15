using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace Datack.Agent.Services
{
    public class RpcService : IHostedService
    {
        private readonly AppSettings _appSettings;

        private HubConnection _connection;

        private readonly Dictionary<String, Func<Object, Task>> _requestMethods = new();

        public RpcService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = new HubConnectionBuilder()
                          .WithUrl("http://localhost:3001/hub")
                          .ConfigureLogging(logging => 
                          {
                              logging.AddProvider(new SerilogLoggerProvider());
                              logging.SetMinimumLevel(LogLevel.Debug);
                              logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                              logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                          })
                          .Build();

            _connection.Closed += _ => Connect(cancellationToken);

            _connection.On<RpcRequest>("request", HandleRequest);

            _ = Task.Run(() => Connect(cancellationToken), cancellationToken);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_connection != null)
            {
                await _connection.StopAsync(cancellationToken);
            }
        }

        public void Subscribe(String methodName, Func<Object, Task> method)
        {
            _requestMethods.Add(methodName, method);
        }
        
        private async Task Connect(CancellationToken cancellationToken)
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

            if (_requestMethods.TryGetValue("Connect", out var onConnect))
            {
                await onConnect(null);
            }
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

                Object parameter = null;
                if (!String.IsNullOrWhiteSpace(rpcRequest.Payload))
                {
                    parameter = JsonSerializer.Deserialize<Object>(rpcRequest.Payload);
                }
                
                var methodResult = methodInfo.Invoke(parameter);

                Object invokationResult;
                if (methodResult is Task task)
                {
                    var taskResult = task.GetType().GetProperty("Result");

                    if (taskResult == null)
                    {
                        throw new Exception($"Task returned NULL value and cannot be awaited");
                    }

                    invokationResult = taskResult.GetValue(task);
                }
                else
                {
                    invokationResult = methodResult;
                }

                result.Result = JsonSerializer.Serialize(invokationResult);
            }
            catch (Exception ex)
            {
                result.Error = JsonSerializer.Serialize(ex);
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

        public async Task<T> Send<T>(String methodName, Object arg1)
        {
            var response = await _connection.InvokeAsync<T>(methodName, _appSettings.Token, arg1);

            return response;
        }
    }
}
