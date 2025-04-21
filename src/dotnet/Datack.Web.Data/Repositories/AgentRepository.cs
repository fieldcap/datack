using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class AgentRepository
{
    private readonly DataContext _dataContext;

    public AgentRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IList<Agent>> GetAll(CancellationToken cancellationToken)
    {
        return await _dataContext.Agents
                                 .AsNoTracking()
                                 .OrderBy(m => m.Name)
                                 .ToListAsync(cancellationToken);
    }

    public async Task<Agent?> GetById(Guid agentId, CancellationToken cancellationToken)
    {
        return await _dataContext.Agents
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(m => m.AgentId == agentId, cancellationToken);
    }

    public async Task<Agent?> GetByKey(String key, CancellationToken cancellationToken)
    {
        return await _dataContext.Agents
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(m => m.Key == key, cancellationToken);
    }
        
    public async Task<Guid> Add(Agent agent, CancellationToken cancellationToken)
    {
        agent.AgentId = Guid.NewGuid();

        await _dataContext.Agents.AddAsync(agent, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);

        return agent.AgentId;
    }

    public async Task Update(Agent agent, CancellationToken cancellationToken)
    {
        var dbAgent = await _dataContext.Agents.FirstOrDefaultAsync(m => m.AgentId == agent.AgentId, cancellationToken);

        if (dbAgent == null)
        {
            throw new($"Agent with ID {agent.AgentId} not found");
        }

        dbAgent.Name = agent.Name;
        dbAgent.Description = agent.Description;
        dbAgent.Settings = agent.Settings;

        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(Guid agentId, CancellationToken cancellationToken)
    {
        var dbAgent = await _dataContext.Agents.FirstOrDefaultAsync(m => m.AgentId == agentId, cancellationToken);

        if (dbAgent == null)
        {
            throw new($"Agent with ID {agentId} not found");
        }

        _dataContext.Agents.Remove(dbAgent);

        await _dataContext.SaveChangesAsync(cancellationToken);
    }
}