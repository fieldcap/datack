using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.RPC;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace Datack.Agent.Service
{
    public class RpcService
    {
        private readonly CancellationToken _cancellationToken;
        private readonly String _token;
        private readonly Main _main;
        private HubConnection _connection;

        private readonly Dictionary<String, MethodInfo> _requestMethods = new Dictionary<String, MethodInfo>();

        public RpcService(CancellationToken cancellationToken, String token, Main main)
        {
            _cancellationToken = cancellationToken;
            _token = token;
            _main = main;

            var thisType = _main.GetType();

            foreach (var methodInfo in thisType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                _requestMethods.Add(methodInfo.Name, methodInfo);
            }
        }

        public async Task StartConnection()
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

            _connection.Closed += _ => Connect();

            _connection.On<RpcRequest>("request", HandleRequest);

            await Task.Run(Connect, _cancellationToken);
        }

        private async Task Connect()
        {
            while (true)
            {
                try
                {
                    await _connection.StartAsync(_cancellationToken);
                    break;
                }
                catch when (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch
                {
                    if (_connection.State == HubConnectionState.Disconnected)
                    {
                        await Task.Delay(1000, _cancellationToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await _connection.SendAsync("Connect", _token, _cancellationToken);
        }

        private async Task HandleRequest(RpcRequest rpcRequest)
        {
            var result = new RpcResult(rpcRequest.TransactionId);
            try
            {
                if (!_requestMethods.ContainsKey(rpcRequest.Request))
                {
                    throw new Exception($"Method {rpcRequest.Request} was not implemented");
                }

                var methodInfo = _requestMethods[rpcRequest.Request];

                var methodParameters = new List<Object>();

                var parameters = methodInfo.GetParameters();

                foreach (var parameter in parameters)
                {
                    var parameterValue = JsonSerializer.Deserialize(rpcRequest.Payload, parameter.ParameterType);
                    methodParameters.Add(parameterValue);
                }
                
                var methodResult = methodInfo.Invoke(_main, methodParameters.ToArray());

                Object invokationResult;
                if (methodResult is Task task)
                {
                    invokationResult = task.GetType().GetProperty("Result").GetValue(task);
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
                await _connection.SendAsync("response", result, _cancellationToken);   
            }
        }
    }
}
