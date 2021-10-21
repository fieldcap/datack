using System;
using System.Linq;
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();

                return BadRequest(errors);
            }

            var result = await _agents.Add(agent, cancellationToken);

            return Ok(result);
        }
        
        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Agent agent, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();

                return BadRequest(errors);
            }

            await _agents.Update(agent, cancellationToken);

            return Ok();
        }

        [HttpDelete]
        [Route("Delete/{agentId:guid}")]
        public async Task<ActionResult> Delete(Guid agentId, CancellationToken cancellationToken)
        {
            await _agents.Delete(agentId, cancellationToken);

            return Ok();
        }
    }
}
