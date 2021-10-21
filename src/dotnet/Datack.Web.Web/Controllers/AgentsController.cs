using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers
{
    [Authorize]
    [Route("Api/Agents")]
    public class AgentsController : Controller
    {
        private readonly Agents _agents;

        public AgentsController(Agents agents)
        {
            _agents = agents;
        }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult> List(CancellationToken cancellationToken)
        {
            var result = await _agents.GetAll(cancellationToken);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("GetById/{agentId:guid}")]
        public async Task<ActionResult> GetById(Guid agentId, CancellationToken cancellationToken)
        {
            var agent = await _agents.GetById(agentId, cancellationToken);

            if (agent == null)
            {
                return NotFound();
            }

            return Ok(agent);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] Agent agent, CancellationToken cancellationToken)
        {
            var result = await _agents.Add(agent, cancellationToken);

            return Ok(result);
        }
        
        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Agent agent, CancellationToken cancellationToken)
        {
            await _agents.Update(agent, cancellationToken);

            return Ok();
        }
        
        [HttpPost]
        [Route("TestDatabaseConnection")]
        public async Task<ActionResult<String>> TestDatabaseConnection([FromBody] AgentsTestDatabaseConnectionRequest request, CancellationToken cancellationToken)
        {
            var result = await _agents.TestDatabaseConnection(request.AgentId, request.ConnectionString, request.ConnectionStringPassword, cancellationToken);

            return Ok(result);
        }
    }

    public class AgentsTestDatabaseConnectionRequest
    {
        public Guid AgentId { get; set; }
        public String ConnectionString { get; set; }
        public String ConnectionStringPassword { get; set; }
    }
}
