using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Service.Helpers;
using Datack.Common.Models.Internal;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Datack.Agent.Service
{
    public class Main
    {
        private readonly CancellationToken _cancellationToken;

        private HubConnection _connection;

        private String _token;

        public Main(ILogger logger, CancellationToken cancellationToken)
        {
            Log.Logger = logger;
            _cancellationToken = cancellationToken;
        }

        public async Task Start(String token)
        {
            _token = token ?? throw new Exception($"Token cannot be null");

            await StartConnection();
        }

        private async Task StartConnection()
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

            _connection.On<ServerDbSettings>("TestSqlServer", HandleTestSqlServer);

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

        private void HandleTestSqlServer(ServerDbSettings serverDbSettings)
        {
            var result = SqlHelper.TestDatabaseConnection(serverDbSettings.Server, serverDbSettings.UserName, serverDbSettings.Password);

            _connection.SendAsync("TestSqlServerResponse", result);
        }
    }
}
