using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services;

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

    public async Task<String> TestDatabaseConnection(Agent agent, String connectionString, String? password, Boolean decryptPassword, CancellationToken cancellationToken)
    {
        _logger.LogDebug("TestDatabaseConnection {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("TestDatabaseConnection", connectionString, password, decryptPassword, cancellationToken);
    }
        
    public async Task<IList<Database>> GetDatabaseList(Agent agent, String connectionString, String? password, Boolean decryptPassword, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetDatabaseList {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<IList<Database>>("GetDatabaseList", connectionString, password, decryptPassword, cancellationToken);
    }
        
    public async Task<String> Run(Agent agent, JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Run {name} {agentId} {jobRunTaskId}", jobRunTask.JobTask.Agent.Name, jobRunTask.JobTask.Agent.AgentId, jobRunTask.JobRunTaskId);

        return await GetConnection(agent).InvokeAsync<String>("Run", jobRunTask, previousTask, cancellationToken);
    }

    public async Task<String> Stop(Agent agent, Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stop {name} {agentId} {jobRunTaskId}", agent.Name, agent.AgentId, jobRunTaskId);

        return await GetConnection(agent).InvokeAsync<String>("Stop", jobRunTaskId, cancellationToken);
    }

    public async Task<String> Encrypt(Agent agent, String input, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Encrypt {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("Encrypt", input, cancellationToken);
    }

    public async Task<String> GetLogs(Agent agent, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetLogs {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("GetLogs", cancellationToken);
    }

    public async Task<IList<Guid>> GetRunningTasks(Agent agent, CancellationToken cancellationToken)
    {
        _logger.LogDebug("GetRunningTasks {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<List<Guid>>("GetRunningTasks", cancellationToken);
    }

    public async Task<String> UpgradeAgent(Agent agent, CancellationToken cancellationToken)
    {
        _logger.LogDebug("UpgradeAgent {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("UpgradeAgent", cancellationToken);
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

    private ISingleClientProxy GetConnection(Agent agent)
    {
        var hasConnection = AgentHub.Agents.TryGetValue(agent.Key, out var connection);

        if (!hasConnection || connection == null)
        {
            throw new Exception($"No connection found for agent with key {agent.Key}");
        }

        return _agentHub.Clients.Client(connection.ConnectionId);
    }
}