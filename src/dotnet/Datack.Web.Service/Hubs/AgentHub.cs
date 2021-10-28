﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Models;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Hubs
{
    public class AgentHub : Hub
    {
        public static event EventHandler<ClientConnectEvent> OnClientConnect;
        public static event EventHandler<ClientDisconnectEvent> OnClientDisconnect;
        public static event EventHandler<RpcProgressEvent> OnProgressTask;
        public static event EventHandler<RpcCompleteEvent> OnCompleteTask;

        private readonly ILogger<AgentHub> _logger;
        private readonly Agents _agents;

        public AgentHub(ILogger<AgentHub> logger, Agents agents)
        {
            _logger = logger;
            _agents = agents;
        }

        public static readonly ConcurrentDictionary<String, AgentConnection> Agents = new();
        public static readonly ConcurrentDictionary<Guid, RpcResult> Transactions = new();

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "Exception when disconnecting: {message}", exception.Message);
            }

            foreach (var (key, value) in Agents)
            {
                if (value.ConnectionId == Context.ConnectionId)
                {
                    _logger.LogDebug("Agent with key {key} disconnected", key);

                    Agents.TryRemove(key, out _);
                    OnClientDisconnect?.Invoke(this, new ClientDisconnectEvent{ AgentKey = key });
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Connect(String key, String version, Boolean hasPendingEvents)
        {
            _logger.LogDebug("Agent with key {key} (v{version}) connecting", key, version);

            var agent = await _agents.GetByKey(key, CancellationToken.None);

            if (agent == null)
            {
                throw new Exception($"Agent with key {key} was not found");
            }

            if (Agents.TryRemove(key, out var agentConnection))
            {
                _logger.LogDebug("Force disconnect agent with key {key} {connectionId}", key, agentConnection.ConnectionId);
            }

            Agents.TryAdd(key, new AgentConnection
            {
                ConnectionId = Context.ConnectionId,
                Version = version
            });

            OnClientConnect?.Invoke(this, new ClientConnectEvent{ AgentKey = key, HasPendingEvents = hasPendingEvents });
        }

        public void Response(RpcResult rpcResult)
        {
            Transactions.TryAdd(rpcResult.TransactionId, rpcResult);
        }

        public void Update(List<RpcProgressEvent> progressEvents, List<RpcCompleteEvent> completedEvents)
        {
            foreach (var progressEvent in progressEvents)
            {
                OnProgressTask?.Invoke(null, progressEvent);
            }

            foreach (var completeEvent in completedEvents)
            {
                OnProgressTask?.Invoke(null, new RpcProgressEvent
                {
                    JobRunTaskId = completeEvent.JobRunTaskId,
                    Message = completeEvent.Message,
                    IsError = completeEvent.IsError
                });
                OnCompleteTask?.Invoke(null, completeEvent);
            }
        }
    }

    public class AgentConnection
    {
        public String ConnectionId { get; set; }
        public String Version { get; set; }
    }
}
