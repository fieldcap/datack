using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services
{
    public class RemoteService
    {
        private readonly ILogger<RemoteService> _logger;
        private readonly IHubContext<AgentHub> _agentHub;
        private readonly IHubContext<WebHub> _webHub;

        public RemoteService(ILogger<RemoteService> logger, IHubContext<AgentHub> agentHub, IHubContext<WebHub> webHub)
        {
            _logger = logger;
            _agentHub = agentHub;
            _webHub = webHub;
        }

        public async Task<String> TestDatabaseConnection(Agent agent, String connectionString, String password, Boolean decryptPassword, CancellationToken cancellationToken)
        {
            _logger.LogDebug("TestDatabaseConnection {name} {agentId}", agent.Name, agent.AgentId);

            return await Send<String>(agent.Key, "TestDatabaseConnection", cancellationToken, connectionString, password, decryptPassword);
        }
        
        public async Task<IList<Database>> GetDatabaseList(Agent agent, String connectionString, String password, Boolean decryptPassword, CancellationToken cancellationToken)
        {
            _logger.LogDebug("GetDatabaseList {name} {agentId}", agent.Name, agent.AgentId);

            return await Send<List<Database>>(agent.Key, "GetDatabaseList", cancellationToken, connectionString, password, decryptPassword);
        }
        
        public async Task<String> Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            _logger.LogDebug("GetDatabaseList {name} {agentId} {jobRunTaskId}", jobRunTask.JobTask.Agent.Name, jobRunTask.JobTask.Agent.AgentId, jobRunTask.JobRunTaskId);

            return await Send<String>(jobRunTask.JobTask.Agent.Key, "Run", cancellationToken, jobRunTask, previousTask);
        }

        public async Task<String> Stop(JobRunTask jobRunTask, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stop {name} {agentId} {jobRunTaskId}", jobRunTask.JobTask.Agent.Name, jobRunTask.JobTask.Agent.AgentId, jobRunTask.JobRunTaskId);

            return await Send<String>(jobRunTask.JobTask.Agent.Key, "Stop", cancellationToken, jobRunTask.JobRunTaskId);
        }

        public async Task<String> Encrypt(Agent agent, String input, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Encrypt {name} {agentId}", agent.Name, agent.AgentId);

            return await Send<String>(agent.Key, "Encrypt", cancellationToken, input);
        }

        public async Task<String> GetLogs(Agent agent, CancellationToken cancellationToken)
        {
            _logger.LogDebug("GetLogs {name} {agentId}", agent.Name, agent.AgentId);

            return await Send<String>(agent.Key, "GetLogs", cancellationToken);
        }

        public async Task<String> UpgradeAgent(Agent agent, CancellationToken cancellationToken)
        {
            return await Send<String>(agent.Key, "UpgradeAgent", cancellationToken);
        }

        private async Task<T> Send<T>(String key, String method, CancellationToken cancellationToken, params Object[] payload)
        {
            var hasConnection = AgentHub.Agents.TryGetValue(key, out var connection);

            if (!hasConnection)
            {
                throw new Exception($"No connection found for agent with key {key}");
            }

            return await SendWithConnection<T>(connection.ConnectionId, method, cancellationToken, payload);
        }

        private async Task<T> SendWithConnection<T>(String connectionId, String method, CancellationToken cancellationToken, params Object[] payload)
        {
            var request = new RpcRequest
            {
                TransactionId = Guid.NewGuid(),
                Request = method,
                Payload = JsonSerializer.Serialize(payload)
            };
            
            await _agentHub.Clients.Client(connectionId).SendAsync("request", request, cancellationToken);

            var timeout = DateTime.UtcNow.AddSeconds(30);

            while (true)
            {
                if (DateTime.UtcNow > timeout)
                {
                    throw new Exception($"No response received within timeout");
                }

                if (AgentHub.Transactions.TryGetValue(request.TransactionId, out var rpcResult))
                {
                    if (rpcResult.Error != null)
                    {
                        var agentException = JsonSerializer.Deserialize<RpcException>(rpcResult.Error);

                        throw new Exception($"Agent threw an exception: {agentException}");
                    }
                    return JsonSerializer.Deserialize<T>(rpcResult.Result);
                }

                await Task.Delay(100, cancellationToken);
            }
        }

        public async Task WebJobRun(JobRun jobRun)
        {
            await _webHub.Clients.All.SendAsync("JobRun", jobRun);
        }

        public async Task WebJobRunTask(JobRunTask jobRunTask)
        {
            await _webHub.Clients.All.SendAsync("JobRunTask", jobRunTask);
        }

        public async Task WebJobRunTaskLog(JobRunTaskLog jobRunTaskLog)
        {
            await _webHub.Clients.All.SendAsync("JobRunTaskLog", jobRunTaskLog);
        }
    }
}
