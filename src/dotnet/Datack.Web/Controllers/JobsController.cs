using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Datack.Service.Services;

namespace Datack.Web.Controllers
{
    [Authorize]
    [Route("Api/Jobs")]
    public class JobsController : Controller
    {
        private readonly Jobs _jobs;

        public JobsController(Jobs jobs)
        {
            _jobs = jobs;
        }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult> List(CancellationToken cancellationToken)
        {
            var result = await _jobs.GetList(cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetForServer/{serverId:guid}")]
        public async Task<ActionResult> GetForServer(Guid serverId, CancellationToken cancellationToken)
        {
            var result = await _jobs.GetForServer(serverId, cancellationToken);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("GetById/{jobId:guid}")]
        public async Task<ActionResult> GetById(Guid jobId, CancellationToken cancellationToken)
        {
            var server = await _jobs.GetById(jobId, cancellationToken);

            if (server == null)
            {
                return NotFound();
            }

            return Ok(server);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] Job job, CancellationToken cancellationToken)
        {
            var result = await _jobs.Add(job, cancellationToken);

            return Ok(result);
        }
        
        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Job job, CancellationToken cancellationToken)
        {
            await _jobs.Update(job, cancellationToken);

            return Ok();
        }
    }
}
