﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Web.Service.Services
{
    public class RemoteService
    {
        private readonly IHubContext<AgentHub> _agentHub;
        private readonly IHubContext<WebHub> _webHub;

        public RemoteService(IHubContext<AgentHub> agentHub, IHubContext<WebHub> webHub)
        {
            _agentHub = agentHub;
            _webHub = webHub;
        }

        public async Task<String> TestDatabaseConnection(Agent agent, String connectionString, String password, Boolean decryptPassword, CancellationToken cancellationToken)
        {
            return await Send<String>(agent.Key, "TestDatabaseConnection", cancellationToken, connectionString, password, decryptPassword);
        }
        
        public async Task<IList<Database>> GetDatabaseList(Agent agent, String connectionString, String password, Boolean decryptPassword, CancellationToken cancellationToken)
        {
            return await Send<List<Database>>(agent.Key, "GetDatabaseList", cancellationToken, connectionString, password, decryptPassword);
        }
        
        public async Task<String> Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            return await Send<String>(jobRunTask.JobTask.Agent.Key, "Run", cancellationToken, jobRunTask, previousTask);
        }

        public async Task<String> Stop(JobRunTask jobRunTask, CancellationToken cancellationToken)
        {
            return await Send<String>(jobRunTask.JobTask.Agent.Key, "Stop", cancellationToken, jobRunTask.JobRunTaskId);
        }

        public async Task<String> Encrypt(Agent agent, String input, CancellationToken cancellationToken)
        {
            return await Send<String>(agent.Key, "Encrypt", cancellationToken, input);
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

            var sendArgs = new[]
            {
                request
            };

            await _agentHub.Clients.Client(connectionId).SendCoreAsync("request", sendArgs, cancellationToken);

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

        public async Task WebJobRunTask(IList<JobRunTask> jobRunTask)
        {
            await _webHub.Clients.All.SendAsync("JobRunTask", jobRunTask);
        }

        public async Task WebJobRunTaskLog(JobRunTaskLog jobRunTaskLog)
        {
            await _webHub.Clients.All.SendAsync("JobRunTaskLog", jobRunTaskLog);
        }
    }
}
