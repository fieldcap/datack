using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class Agents
    {
        private readonly AgentRepository _agentRepository;
        private readonly JobTaskRepository _jobTaskRepository;

        public Agents(AgentRepository agentRepository, JobTaskRepository jobTaskRepository)
        {
            _agentRepository = agentRepository;
            _jobTaskRepository = jobTaskRepository;
        }

        public async Task<IList<Agent>> GetAll(CancellationToken cancellationToken)
        {
            return await _agentRepository.GetAll(cancellationToken);
        }

        public async Task<Agent> GetById(Guid agentId, CancellationToken cancellationToken)
        {
            return await _agentRepository.GetById(agentId, cancellationToken);
        }

        public async Task<Agent> GetByKey(String key, CancellationToken cancellationToken)
        {
            return await _agentRepository.GetByKey(key, cancellationToken);
        }
        
        public async Task<Guid> Add(Agent agent, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(agent.Name))
            {
                throw new Exception("Name cannot be empty");
            }

            if (String.IsNullOrWhiteSpace(agent.Key))
            {
                throw new Exception("Key cannot be empty");
            }

            var allAgents = await _agentRepository.GetAll(cancellationToken);
            var sameNameAgents = allAgents.Any(m => String.Equals(m.Name, agent.Name, StringComparison.CurrentCultureIgnoreCase));
            var sameKeyAgents = allAgents.Any(m => String.Equals(m.Key, agent.Key, StringComparison.CurrentCultureIgnoreCase));

            if (sameNameAgents)
            {
                throw new Exception($"An agent with this name already exists");
            }

            if (sameKeyAgents)
            {
                throw new Exception($"An agent with this key already exists");
            }

            return await _agentRepository.Add(agent, cancellationToken);
        }

        public async Task Update(Agent agent, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(agent.Name))
            {
                throw new Exception("Name cannot be empty");
            }

            if (String.IsNullOrWhiteSpace(agent.Key))
            {
                throw new Exception("Key cannot be empty");
            }

            var allAgents = await _agentRepository.GetAll(cancellationToken);
            var sameNameAgents = allAgents.Any(m => m.AgentId != agent.AgentId && String.Equals(m.Name, agent.Name, StringComparison.CurrentCultureIgnoreCase));
            var sameKeyAgents = allAgents.Any(m => m.AgentId != agent.AgentId && String.Equals(m.Key, agent.Key, StringComparison.CurrentCultureIgnoreCase));

            if (sameNameAgents)
            {
                throw new Exception($"An agent with this name already exists");
            }

            if (sameKeyAgents)
            {
                throw new Exception($"An agent with this key already exists");
            }

            await _agentRepository.Update(agent, cancellationToken);
        }

        public async Task Delete(Guid agentId, CancellationToken cancellationToken)
        {
            var jobTasks = await _jobTaskRepository.GetByAgentId(agentId, cancellationToken);

            if (jobTasks.Count > 0)
            {
                var errors = jobTasks.Select(m => $"{m.Name} on job {m.Job.Name}");

                throw new Exception($"This agent is still attached to the following tasks: {Environment.NewLine}{errors}");
            }

            await _agentRepository.Delete(agentId, cancellationToken);
        }
    }
}
