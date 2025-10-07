using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;
using Datack.Web.Service.Hubs;

namespace Datack.Web.Service.Services;

public class Agents(AgentRepository agentRepository, JobTaskRepository jobTaskRepository, JobRunTaskRepository jobRunTaskRepository, RemoteService remoteService)
{
    public async Task<IList<Agent>> GetAll(CancellationToken cancellationToken)
    {
        var agents = await agentRepository.GetAll(cancellationToken);

        foreach (var agent in agents)
        {
            if (AgentHub.Agents.TryGetValue(agent.Key, out var agentConnection))
            {
                agent.Status = "online";

                if (agentConnection.Version != VersionHelper.GetVersion())
                {
                    agent.Status = "versionmismatch";
                }
            }
            else
            {
                agent.Status = "offline";
            }
        }

        return agents;
    }

    public async Task<Agent?> GetById(Guid agentId, CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetById(agentId, cancellationToken);

        if (agent == null)
        {
            return null;
        }

        if (AgentHub.Agents.TryGetValue(agent.Key, out var agentConnection))
        {
            agent.Version = agentConnection.Version;

            agent.Status = "online";

            if (agentConnection.Version != VersionHelper.GetVersion())
            {
                agent.Status = "versionmismatch";
            }
        }
        else
        {
            agent.Status = "offline";
        }

        return agent;
    }

    public async Task<Agent?> GetByKey(String key, CancellationToken cancellationToken)
    {
        return await agentRepository.GetByKey(key, cancellationToken);
    }
        
    public async Task<Guid> Add(Agent agent, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(agent.Name))
        {
            throw new("Name cannot be empty");
        }

        if (String.IsNullOrWhiteSpace(agent.Key))
        {
            throw new("Key cannot be empty");
        }

        var allAgents = await agentRepository.GetAll(cancellationToken);
        var sameNameAgents = allAgents.Any(m => String.Equals(m.Name, agent.Name, StringComparison.CurrentCultureIgnoreCase));
        var sameKeyAgents = allAgents.Any(m => String.Equals(m.Key, agent.Key, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameAgents)
        {
            throw new($"An agent with this name already exists");
        }

        if (sameKeyAgents)
        {
            throw new($"An agent with this key already exists");
        }

        return await agentRepository.Add(agent, cancellationToken);
    }

    public async Task Update(Agent agent, CancellationToken cancellationToken)
    {
        if (String.IsNullOrWhiteSpace(agent.Name))
        {
            throw new("Name cannot be empty");
        }

        if (String.IsNullOrWhiteSpace(agent.Key))
        {
            throw new("Key cannot be empty");
        }

        var allAgents = await agentRepository.GetAll(cancellationToken);
        var sameNameAgents = allAgents.Any(m => m.AgentId != agent.AgentId && String.Equals(m.Name, agent.Name, StringComparison.CurrentCultureIgnoreCase));
        var sameKeyAgents = allAgents.Any(m => m.AgentId != agent.AgentId && String.Equals(m.Key, agent.Key, StringComparison.CurrentCultureIgnoreCase));

        if (sameNameAgents)
        {
            throw new($"An agent with this name already exists");
        }

        if (sameKeyAgents)
        {
            throw new($"An agent with this key already exists");
        }

        await agentRepository.Update(agent, cancellationToken);
    }

    public async Task Delete(Guid agentId, CancellationToken cancellationToken)
    {
        var jobTasks = await jobTaskRepository.GetByAgentId(agentId, cancellationToken);

        if (jobTasks.Count > 0)
        {
            var errors = jobTasks.Select(m => $"{m.Name} on job {m.Job.Name}");

            throw new($"This agent is still attached to the following tasks: {Environment.NewLine}{errors}");
        }

        await agentRepository.Delete(agentId, cancellationToken);
    }

    public async Task<String> GetLogs(Guid agentId, CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetById(agentId, cancellationToken);

        if (agent == null)
        {
            throw new($"Agent with ID {agentId} not found");
        }

        var result = await remoteService.GetLogs(agent, cancellationToken);

        return result;
    }

    public async Task UpgradeAgent(Guid agentId, CancellationToken cancellationToken)
    {
        var agent = await agentRepository.GetById(agentId, cancellationToken);

        if (agent == null)
        {
            throw new($"Agent with ID {agentId} not found");
        }

        var jobRunTasks = await jobRunTaskRepository.GetByAgentId(agentId, cancellationToken);

        var jobRunsRunning = jobRunTasks.Any(m => m.JobRun.Completed == null);

        if (jobRunsRunning)
        {
            throw new($"Cannot upgrade agent, some jobs are running");
        }

        var result = await remoteService.UpgradeAgent(agent, cancellationToken);

        if (result != "Success")
        {
            throw new("Failed to start upgrade");
        }
    }
}