using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Data.Repositories;

namespace Datack.Web.Service.Services
{
    public class Agents
    {
        private readonly AgentRepository _agentRepository;
        private readonly RemoteService _remoteService;

        public Agents(AgentRepository agentRepository, RemoteService remoteService)
        {
            _agentRepository = agentRepository;
            _remoteService = remoteService;
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
            return await _agentRepository.Add(agent, cancellationToken);
        }

        public async Task Update(Agent agent, CancellationToken cancellationToken)
        {
            await _agentRepository.Update(agent, cancellationToken);
        }

        public async Task<String> TestDatabaseConnection(Guid agentId, String connectionString, String connectionStringPassword, CancellationToken cancellationToken)
        {
            var agent = await _agentRepository.GetById(agentId, cancellationToken);

            return await _remoteService.TestDatabaseConnection(agent, connectionString, connectionStringPassword, cancellationToken);
        }

        public async Task<IList<Database>> GetDatabaseList(Guid agentId, String connectionString, String connectionStringPassword, CancellationToken cancellationToken)
        {
            var agent = await GetById(agentId, cancellationToken);

            return await _remoteService.GetDatabaseList(agent, connectionString, connectionStringPassword, cancellationToken);
        }
    }
}
