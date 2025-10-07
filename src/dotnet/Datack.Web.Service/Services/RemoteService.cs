using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Datack.Web.Service.Services;

public class RemoteService(ILogger<RemoteService> logger, IHubContext<AgentHub> agentHub, IHubContext<WebHub> webHub)
{
    public async Task<String> TestDatabaseConnection(Agent agent, String databaseType, String connectionString, String? password, Boolean decryptPassword, CancellationToken cancellationToken)
    {
        logger.LogDebug("TestDatabaseConnection {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("TestDatabaseConnection", databaseType, connectionString, password, decryptPassword, cancellationToken);
    }
        
    public async Task<IList<Database>> GetDatabaseList(Agent agent, String databaseType, String connectionString, String? password, Boolean decryptPassword, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDatabaseList {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<IList<Database>>("GetDatabaseList", databaseType, connectionString, password, decryptPassword, cancellationToken);
    }

    public async Task<IList<BackupFile>> GetFileList(Agent agent, String storageType, String connectionString, String containerName, String rootPath, String? path, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetFileList {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<IList<BackupFile>>("GetFileList", storageType, connectionString, containerName, rootPath, path, cancellationToken);
    }
        
    public async Task<String> Run(Agent agent, JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        logger.LogDebug("Run {name} {agentId} {jobRunTaskId}", jobRunTask.JobTask.Agent.Name, jobRunTask.JobTask.Agent.AgentId, jobRunTask.JobRunTaskId);

        return await GetConnection(agent).InvokeAsync<String>("Run", jobRunTask, previousTask, cancellationToken);
    }

    public async Task<String> Stop(Agent agent, Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Stop {name} {agentId} {jobRunTaskId}", agent.Name, agent.AgentId, jobRunTaskId);

        return await GetConnection(agent).InvokeAsync<String>("Stop", jobRunTaskId, cancellationToken);
    }

    public async Task<String> Encrypt(Agent agent, String input, CancellationToken cancellationToken)
    {
        logger.LogDebug("Encrypt {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("Encrypt", input, cancellationToken);
    }

    public async Task<String> GetLogs(Agent agent, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetLogs {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("GetLogs", cancellationToken);
    }

    public async Task<IList<Guid>> GetRunningTasks(Agent agent, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRunningTasks {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<List<Guid>>("GetRunningTasks", cancellationToken);
    }

    public async Task<String> UpgradeAgent(Agent agent, CancellationToken cancellationToken)
    {
        logger.LogDebug("UpgradeAgent {name} {agentId}", agent.Name, agent.AgentId);

        return await GetConnection(agent).InvokeAsync<String>("UpgradeAgent", cancellationToken);
    }

    public async Task WebJobRun(JobRun jobRun)
    {
        await webHub.Clients.All.SendAsync("JobRun", jobRun);
    }

    public async Task WebJobRunTask(JobRunTask jobRunTask)
    {
        await webHub.Clients.All.SendAsync("JobRunTask", jobRunTask);
    }

    public async Task WebJobRunTaskLog(JobRunTaskLog jobRunTaskLog)
    {
        await webHub.Clients.All.SendAsync("JobRunTaskLog", jobRunTaskLog);
    }

    private ISingleClientProxy GetConnection(Agent agent)
    {
        var hasConnection = AgentHub.Agents.TryGetValue(agent.Key, out var connection);

        if (!hasConnection || connection == null)
        {
            throw new($"No connection found for agent with key {agent.Key}");
        }

        return agentHub.Clients.Client(connection.ConnectionId);
    }
}