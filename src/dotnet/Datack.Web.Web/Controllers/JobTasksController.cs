using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers
{
    [Authorize]
    [Route("Api/JobTasks")]
    public class JobTasksController : Controller
    {
        private readonly JobTasks _jobTasks;
        private readonly Servers _servers;

        public JobTasksController(JobTasks jobTasks, Servers servers)
        {
            _jobTasks = jobTasks;
            _servers = servers;
        }

        [HttpGet]
        [Route("GetForJob/{jobId:guid}")]
        public async Task<ActionResult> List(Guid jobId, CancellationToken cancellationToken)
        {
            var result = await _jobTasks.GetForJob(jobId, cancellationToken);

            return Ok(result);
        }

        [HttpGet]
        [Route("GetById/{jobTaskId:guid}")]
        public async Task<ActionResult> GetById(Guid jobTaskId, CancellationToken cancellationToken)
        {
            var server = await _jobTasks.GetById(jobTaskId, cancellationToken);

            if (server == null)
            {
                return NotFound();
            }

            return Ok(server);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] JobTask jobTask, CancellationToken cancellationToken)
        {
            var result = await _jobTasks.Add(jobTask, cancellationToken);

            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        public async Task<ActionResult> Update([FromBody] JobTask jobTask, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();

                return BadRequest(errors);
            }

            await _jobTasks.Update(jobTask, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("TestDatabaseRegex")]
        public async Task<ActionResult> TestDatabaseRegex([FromBody] JobTasksTestDatabaseRegexRequest request, CancellationToken cancellationToken)
        {
            var databases = await _servers.GetDatabaseList(request.ServerId, cancellationToken);

            var result = DatabaseHelper.FilterDatabases(databases,
                                                        request.BackupDefaultExclude,
                                                        request.BackupExcludeSystemDatabases,
                                                        request.BackupIncludeRegex,
                                                        request.BackupExcludeRegex,
                                                        request.BackupIncludeManual,
                                                        request.BackupExcludeManual);

            return Ok(result);
        }

        
        [Route("ReOrder")]
        [HttpPost]
        public async Task<ActionResult> ReOrder([FromBody] JobTaskReOrderRequest request, CancellationToken cancellationToken)
        {
            await _jobTasks.ReOrder(request.JobId, request.JobTaskIds, cancellationToken);

            return Ok();
        }
    }

    public class JobTasksTestDatabaseRegexRequest
    {
        public Boolean BackupDefaultExclude { get; set; }
        public Boolean BackupExcludeSystemDatabases { get; set; }
        public String BackupIncludeRegex { get; set; }
        public String BackupExcludeRegex { get; set; }
        public String BackupIncludeManual { get; set; }
        public String BackupExcludeManual { get; set; }
        public Guid ServerId { get; set; }
    }

    public class JobTaskReOrderRequest
    {
        public Guid JobId { get; set; }
        public IList<Guid> JobTaskIds { get; set; }
    }
}
