using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Internal;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Models;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.SignalR;

namespace Datack.Web.Service.Hubs
{
    public class AgentHub : Hub
    {
        public static event EventHandler<ClientConnectEvent> OnClientConnect;
        public static event EventHandler<ClientDisconnectEvent> OnClientDisconnect;

        private readonly Agents _agents;
        private readonly JobRunner _jobRunner;

        public AgentHub(Agents agents, JobRunner jobRunner)
        {
            _agents = agents;
            _jobRunner = jobRunner;
        }

        public static readonly ConcurrentDictionary<String, AgentConnection> Agents = new();
        public static readonly ConcurrentDictionary<Guid, RpcResult> Transactions = new();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            foreach (var (key, value) in Agents)
            {
                if (value.ConnectionId == Context.ConnectionId)
                {
                    Agents.TryRemove(key, out _);
                    OnClientDisconnect?.Invoke(this, new ClientDisconnectEvent{ AgentKey = key });
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Connect(String key, String version)
        {
            var agent = await _agents.GetByKey(key, CancellationToken.None);

            if (agent == null)
            {
                throw new Exception($"Agent with key {key} was not found");
            }

            OnClientConnect?.Invoke(this, new ClientConnectEvent{ AgentKey = key });

            Agents.TryAdd(key, new AgentConnection
            {
                ConnectionId = Context.ConnectionId,
                Version = version
            });
        }

        public void Response(RpcResult rpcResult)
        {
            Transactions.TryAdd(rpcResult.TransactionId, rpcResult);
        }

        public async Task<RpcUpdate> RpcUpdate(String key)
        {
            var agent = await _agents.GetByKey(key, CancellationToken.None);

            return new RpcUpdate
            {
                Agent = agent
            };
        }

        public async Task TaskProgress(RpcProgressEvent progressEvent)
        {
            await _jobRunner.ProgressTask(progressEvent.JobRunTaskId, progressEvent.Message, progressEvent.IsError, CancellationToken.None);
        }

        public async Task TaskComplete(RpcCompleteEvent completeEvent)
        {
            await _jobRunner.ProgressTask(completeEvent.JobRunTaskId, completeEvent.Message, completeEvent.IsError, CancellationToken.None);
            await _jobRunner.CompleteTask(completeEvent.JobRunTaskId, completeEvent.Message, completeEvent.ResultArtifact, completeEvent.IsError, CancellationToken.None);
        }
    }

    public class AgentConnection
    {
        public String ConnectionId { get; set; }
        public String Version { get; set; }
    }
}
