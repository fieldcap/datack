using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/Agents")]
public class AgentsController(Agents agents) : Controller
{
    [HttpGet]
    [Route("List")]
    public async Task<ActionResult> List(CancellationToken cancellationToken)
    {
        var result = await agents.GetAll(cancellationToken);
        return Ok(result);
    }
        
    [HttpGet]
    [Route("GetById/{agentId:guid}")]
    public async Task<ActionResult> GetById(Guid agentId, CancellationToken cancellationToken)
    {
        var agent = await agents.GetById(agentId, cancellationToken);

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
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
                                   .ToList();

            return BadRequest(errors);
        }

        var result = await agents.Add(agent, cancellationToken);

        return Ok(result);
    }
        
    [HttpPut]
    [Route("Update")]
    public async Task<ActionResult> Update([FromBody] Agent agent, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
                                   .ToList();

            return BadRequest(errors);
        }

        await agents.Update(agent, cancellationToken);

        return Ok();
    }

    [HttpDelete]
    [Route("Delete/{agentId:guid}")]
    public async Task<ActionResult> Delete(Guid agentId, CancellationToken cancellationToken)
    {
        await agents.Delete(agentId, cancellationToken);

        return Ok();
    }
        
    [HttpGet]
    [Route("Logs/{agentId:guid}")]
    public async Task<ActionResult<String>> Logs(Guid agentId, CancellationToken cancellationToken)
    {
        var logs = await agents.GetLogs(agentId, cancellationToken);

        return Ok(logs);
    }

    [HttpGet]
    [Route("UpgradeAgent/{agentId:guid}")]
    public async Task<ActionResult> UpgradeAgent(Guid agentId, CancellationToken cancellationToken)
    {
        await agents.UpgradeAgent(agentId, cancellationToken);

        return Ok();
    }
}