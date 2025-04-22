using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Common.Models.Internal;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

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
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
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
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
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
            throw new($"Agent with ID {request.AgentId} not found");
        }

        var password = request.ConnectionStringPassword;
        var decryptPassword = false;
        if (request.ConnectionStringPassword == "******")
        {
            var jobTask = await _jobTasks.GetById(request.JobTaskId, cancellationToken);

            if (jobTask == null)
            {
                throw new($"Cannot find job task with ID {request.JobTaskId}");
            }

            password = jobTask.Settings.CreateBackup?.ConnectionStringPassword;
            decryptPassword = true;
        }

        var result = await _remoteService.TestDatabaseConnection(agent, request.DatabaseType, request.ConnectionString, password, decryptPassword, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Route("TestDatabaseRegex")]
    public async Task<ActionResult> TestDatabaseRegex([FromBody] JobTasksTestDatabaseRegexRequest request, CancellationToken cancellationToken)
    {
        var agent = await _agents.GetById(request.AgentId, cancellationToken);

        if (agent == null)
        {
            throw new($"Agent with ID {request.AgentId} not found");
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

            if (jobTask == null)
            {
                throw new($"Cannot find job task with ID {request.JobTaskId}");
            }

            password = jobTask.Settings.CreateBackup?.ConnectionStringPassword;
            decryptPassword = true;
        }

        var databases = await _remoteService.GetDatabaseList(agent, request.DatabaseType, request.ConnectionString, password, decryptPassword, cancellationToken);

        var result = DatabaseHelper.FilterDatabases(databases,
                                                    request.BackupDefaultExclude,
                                                    request.BackupExcludeSystemDatabases,
                                                    request.BackupIncludeRegex,
                                                    request.BackupExcludeRegex,
                                                    request.BackupIncludeManual,
                                                    request.BackupExcludeManual,
                                                    request.BackupType);

        return Ok(result);
    }

    [HttpPost]
    [Route("TestFilesRegex")]
    public async Task<ActionResult> TestFilesRegex([FromBody] JobTasksTestFilesRegexRequest request, CancellationToken cancellationToken)
    {
        var agent = await _agents.GetById(request.AgentId, cancellationToken);

        if (agent == null)
        {
            throw new($"Agent with ID {request.AgentId} not found");
        }

        if (String.IsNullOrWhiteSpace(request.ConnectionString))
        {
            return Ok(new List<DatabaseTestResult>());
        }

        var connectionString = request.ConnectionString;
        if (connectionString == "******")
        {
            var jobTask = await _jobTasks.GetById(request.JobTaskId, cancellationToken);

            if (jobTask == null)
            {
                throw new($"Cannot find job task with ID {request.JobTaskId}");
            }

            connectionString = jobTask.Settings.DownloadAzure?.ConnectionString;
        }

        if (connectionString == null)
        {
            throw new($"Cannot find connection string for job task with ID {request.JobTaskId}");
        }

        var files = await _remoteService.GetFileList(agent, "azure", connectionString, request.ContainerName, request.Blob, null, cancellationToken);

        var result = FileHelper.FilterFiles(files,
                                            request.RestoreDefaultExclude,
                                            request.RestoreIncludeRegex,
                                            request.RestoreExcludeRegex,
                                            request.RestoreIncludeManual,
                                            request.RestoreExcludeManual);

        return Ok(result);
    }
}

public class AgentsTestDatabaseConnectionRequest
{
    public Guid AgentId { get; set; }
    public Guid JobTaskId { get; set; }
    public required String DatabaseType { get; set; }
    public required String ConnectionString { get; set; }
    public required String ConnectionStringPassword { get; set; }
}

public class JobTasksTestDatabaseRegexRequest
{
    public Boolean BackupDefaultExclude { get; set; }
    public Boolean BackupExcludeSystemDatabases { get; set; }
    public required String BackupIncludeRegex { get; set; }
    public required String BackupExcludeRegex { get; set; }
    public required String BackupIncludeManual { get; set; }
    public required String BackupExcludeManual { get; set; }
    public Guid AgentId { get; set; }
    public Guid JobTaskId { get; set; }
    public required String DatabaseType { get; set; }
    public required String ConnectionString { get; set; }
    public required String ConnectionStringPassword { get; set; }
    public required String BackupType { get; set; }
}

public class JobTasksTestFilesRegexRequest
{
    public Boolean RestoreDefaultExclude { get; set; }
    public required String RestoreIncludeRegex { get; set; }
    public required String RestoreExcludeRegex { get; set; }
    public required String RestoreIncludeManual { get; set; }
    public required String RestoreExcludeManual { get; set; }
    public Guid AgentId { get; set; }
    public Guid JobTaskId { get; set; }
    public required String ContainerName { get; set; }
    public required String Blob { get; set; }
    public required String ConnectionString { get; set; }
}

public class JobTaskReOrderRequest
{
    public Guid JobId { get; set; }
    public required IList<Guid> JobTaskIds { get; set; }
}