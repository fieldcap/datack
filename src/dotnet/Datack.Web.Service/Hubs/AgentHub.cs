using System.Collections.Concurrent;
using Datack.Common.Models.RPC;
using Datack.Web.Service.Models;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Hubs;

public class AgentHub(ILogger<AgentHub> logger, Agents agents) : Hub
{
    public static event EventHandler<ClientConnectEvent>? OnClientConnect;
    public static event EventHandler<ClientDisconnectEvent>? OnClientDisconnect;
    public static event EventHandler<IList<RpcProgressEvent>>? OnProgressTasks;
    public static event EventHandler<IList<RpcCompleteEvent>>? OnCompleteTasks;

    public static readonly ConcurrentDictionary<String, AgentConnection> Agents = new();

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            logger.LogError(exception, "Exception when disconnecting: {message}", exception.Message);
        }

        foreach (var (key, value) in Agents)
        {
            if (value.ConnectionId == Context.ConnectionId)
            {
                logger.LogDebug("Agent with key {key} disconnected", key);

                Agents.TryRemove(key, out _);
                OnClientDisconnect?.Invoke(this, new()
                                               { AgentKey = key });
                break;
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task Connect(String key, String version)
    {
        logger.LogDebug("Agent with key {key} (v{version}) connecting", key, version);

        var agent = await agents.GetByKey(key, CancellationToken.None);

        if (agent == null)
        {
            throw new($"Agent with key {key} was not found");
        }

        if (Agents.TryRemove(key, out var agentConnection))
        {
            logger.LogDebug("Force disconnect agent with key {key} {connectionId}", key, agentConnection.ConnectionId);
        }

        Agents.TryAdd(key, new()
        {
            ConnectionId = Context.ConnectionId,
            Version = version
        });

        OnClientConnect?.Invoke(this, new()
                                    { AgentKey = key });
    }
    
    public void UpdateProgress(List<RpcProgressEvent> progressEvents)
    {
        OnProgressTasks?.Invoke(null, progressEvents);
    }
        
    public void UpdateComplete(List<RpcCompleteEvent> completedEvents)
    {
        OnCompleteTasks?.Invoke(null, completedEvents);
    }
}

public class AgentConnection
{
    public required String ConnectionId { get; set; }
    public required String Version { get; set; }
}