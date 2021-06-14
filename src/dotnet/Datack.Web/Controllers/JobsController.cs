using System;
using System.Threading;
using System.Threading.Tasks;
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
        [Route("GetForServer/{serverId:guid}")]
        public async Task<ActionResult> List(Guid serverId, CancellationToken cancellationToken)
        {
            var result = await _jobs.GetForServer(serverId, cancellationToken);
            return Ok(result);
        }
    }
}
