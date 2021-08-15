using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
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
        private readonly Servers _servers;

        public StepsController(Steps steps, Servers servers)
        {
            _steps = steps;
            _servers = servers;
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

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] Step step)
        {
            var result = await _steps.Add(step);

            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] Step step)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();

                return BadRequest(errors);
            }

            await _steps.Update(step);

            return Ok();
        }

        [HttpPost]
        [Route("TestDatabaseRegex")]
        public async Task<ActionResult> TestDatabaseRegex([FromBody] StepsTestDatabaseRegexRequest request, CancellationToken cancellationToken)
        {
            var databases = await _servers.GetDatabaseList(request.ServerId, cancellationToken);

            var result = DatabaseHelper.FilterDatabases(databases,
                                                        request.Settings.BackupExcludeSystemDatabases,
                                                        request.Settings.BackupIncludeRegex,
                                                        request.Settings.BackupExcludeRegex,
                                                        request.Settings.BackupIncludeManual,
                                                        request.Settings.BackupExcludeManual,
                                                        request.Settings.BackupDefaultExclude);

            return Ok(result);
        }
    }

    public class StepsTestDatabaseRegexRequest
    {
        public StepCreateDatabaseSettings Settings { get; set; } 
        public Guid ServerId { get; set; }
    }
}
