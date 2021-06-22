using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Data.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Datack.Service.Services;

namespace Datack.Web.Controllers
{
    [Authorize]
    [Route("Api/Steps")]
    public class StepsController : Controller
    {
        private readonly Steps _steps;

        public StepsController(Steps steps)
        {
            _steps = steps;
        }

        [HttpGet]
        [Route("GetForJob/{jobId:guid}")]
        public async Task<ActionResult> List(Guid jobId, CancellationToken cancellationToken)
        {
            var result = await _steps.GetForJob(jobId, cancellationToken);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("GetById/{stepId:guid}")]
        public async Task<ActionResult> GetById(Guid stepId, CancellationToken cancellationToken)
        {
            var server = await _steps.GetById(stepId, cancellationToken);

            if (server == null)
            {
                return NotFound();
            }

            return Ok(server);
        }

        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Step step)
        {
            await _steps.Update(step);

            return Ok();
        }
    }
}
