using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
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
        private readonly Agents _agents;
        private readonly RemoteService _remoteService;

        public JobTasksController(JobTasks jobTasks, Agents agents, RemoteService remoteService)
        {
            _jobTasks = jobTasks;
            _agents = agents;
            _remoteService = remoteService;
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
            var agent = await _jobTasks.GetById(jobTaskId, cancellationToken);

            if (agent == null)
            {
                return NotFound();
            }

            return Ok(agent);
        }

        [HttpPost]
        [Route("Add")]
        public async Task<ActionResult<Guid>> Add([FromBody] JobTask jobTask, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
                                       .ToList();

                return BadRequest(errors);
            }

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

        [HttpDelete]
        [Route("Delete/{jobTaskId:guid}")]
        public async Task<ActionResult> Delete(Guid jobTaskId, CancellationToken cancellationToken)
        {
            await _jobTasks.Delete(jobTaskId, cancellationToken);

            return Ok();
        }

        [Route("ReOrder")]
        [HttpPost]
        public async Task<ActionResult> ReOrder([FromBody] JobTaskReOrderRequest request, CancellationToken cancellationToken)
        {
            await _jobTasks.ReOrder(request.JobId, request.JobTaskIds, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("TestDatabaseConnection")]
        public async Task<ActionResult<String>> TestDatabaseConnection([FromBody] AgentsTestDatabaseConnectionRequest request, CancellationToken cancellationToken)
        {
            var agent = await _agents.GetById(request.AgentId, cancellationToken);

            if (agent == null)
            {
                throw new Exception($"Agent with ID {request.AgentId} not found");
            }

            var password = request.ConnectionStringPassword;
            var decryptPassword = false;
            if (request.ConnectionStringPassword == "******")
            {
                var jobTask = await _jobTasks.GetById(request.JobTaskId, cancellationToken);

                password = jobTask.Settings.CreateBackup.ConnectionStringPassword;
                decryptPassword = true;
            }

            var result = await _remoteService.TestDatabaseConnection(agent, request.ConnectionString, password, decryptPassword, cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        [Route("TestDatabaseRegex")]
        public async Task<ActionResult> TestDatabaseRegex([FromBody] JobTasksTestDatabaseRegexRequest request, CancellationToken cancellationToken)
        {
            var agent = await _agents.GetById(request.AgentId, cancellationToken);

            if (agent == null)
            {
                throw new Exception($"Agent with ID {request.AgentId} not found");
            }

            if (String.IsNullOrWhiteSpace(request.ConnectionString))
            {
                return Ok(new List<DatabaseTestResult>());
            }

            var password = request.ConnectionStringPassword;
            var decryptPassword = false;
            if (request.ConnectionStringPassword == "******")
            {
                var jobTask = await _jobTasks.GetById(request.JobTaskId, cancellationToken);

                password = jobTask.Settings.CreateBackup.ConnectionStringPassword;
                decryptPassword = true;
            }

            var databases = await _remoteService.GetDatabaseList(agent, request.ConnectionString, password, decryptPassword, cancellationToken);

            var result = DatabaseHelper.FilterDatabases(databases,
                                                        request.BackupDefaultExclude,
                                                        request.BackupExcludeSystemDatabases,
                                                        request.BackupIncludeRegex,
                                                        request.BackupExcludeRegex,
                                                        request.BackupIncludeManual,
                                                        request.BackupExcludeManual);

            return Ok(result);
        }
    }

    public class AgentsTestDatabaseConnectionRequest
    {
        public Guid AgentId { get; set; }
        public Guid JobTaskId { get; set; }
        public String ConnectionString { get; set; }
        public String ConnectionStringPassword { get; set; }
    }

    public class JobTasksTestDatabaseRegexRequest
    {
        public Boolean BackupDefaultExclude { get; set; }
        public Boolean BackupExcludeSystemDatabases { get; set; }
        public String BackupIncludeRegex { get; set; }
        public String BackupExcludeRegex { get; set; }
        public String BackupIncludeManual { get; set; }
        public String BackupExcludeManual { get; set; }
        public Guid AgentId { get; set; }
        public Guid JobTaskId { get; set; }
        public String ConnectionString { get; set; }
        public String ConnectionStringPassword { get; set; }
    }

    public class JobTaskReOrderRequest
    {
        public Guid JobId { get; set; }
        public IList<Guid> JobTaskIds { get; set; }
    }
}
