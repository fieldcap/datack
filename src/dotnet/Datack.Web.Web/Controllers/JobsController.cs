using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/Jobs")]
public class JobsController : Controller
{
    private readonly RemoteService _remoteService;
    private readonly Agents _agents;
    private readonly Jobs _jobs;
    private readonly JobRuns _jobRuns;
    private readonly JobRunTasks _jobRunTasks;
    private readonly JobRunTaskLogs _jobRunTaskLogs;
    private readonly JobRunner _jobRunner;
    private readonly JobTasks _jobTasks;

    public JobsController(RemoteService remoteService, Agents agents, Jobs jobs, JobRuns jobRuns, JobRunTasks jobRunTasks, JobRunTaskLogs jobRunTaskLogs, JobRunner jobRunner, JobTasks jobTasks)
    {
        _remoteService = remoteService;
        _agents = agents;
        _jobs = jobs;
        _jobRuns = jobRuns;
        _jobRunTasks = jobRunTasks;
        _jobRunTaskLogs = jobRunTaskLogs;
        _jobRunner = jobRunner;
        _jobTasks = jobTasks;
    }

    [HttpGet]
    [Route("List")]
    public async Task<ActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _jobs.GetList(cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Route("GetForAgent/{agentId:guid}")]
    public async Task<ActionResult> GetForAgent(Guid agentId, CancellationToken cancellationToken)
    {
        var result = await _jobs.GetForAgent(agentId, cancellationToken);
        return Ok(result);
    }
        
    [HttpGet]
    [Route("GetById/{jobId:guid}")]
    public async Task<ActionResult> GetById(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetById(jobId, cancellationToken);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }
        
    [HttpPost]
    [Route("Add")]
    public async Task<ActionResult<Guid>> Add([FromBody] Job job, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
                                   .ToList();

            return BadRequest(errors);
        }

        var result = await _jobs.Add(job, cancellationToken);

        return Ok(result);
    }
        
    [HttpPut]
    [Route("Update")]
    public async Task<ActionResult> Update([FromBody] Job job, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Select(x => x.Value?.Errors)
                                   .Where(x => x != null && x.Count > 0)
                                   .ToList();

            return BadRequest(errors);
        }

        await _jobs.Update(job, cancellationToken);

        return Ok();
    }
        
    [HttpPost]
    [Route("Duplicate")]
    public async Task<ActionResult<Job>> Duplicate([FromBody] JobDuplicateRequest request, CancellationToken cancellationToken)
    {
        var result = await _jobs.Duplicate(request.JobId, cancellationToken);

        return Ok(result);
    }

    [HttpDelete]
    [Route("Delete/{jobId:guid}")]
    public async Task<ActionResult<Job>> Delete(Guid jobId, CancellationToken cancellationToken)
    {
        await _jobRunTaskLogs.DeleteForJob(jobId, DateTime.UtcNow.AddYears(1), cancellationToken);
        await _jobRunTasks.DeleteForJob(jobId, DateTime.UtcNow.AddYears(1), cancellationToken);
        await _jobRuns.DeleteForJob(jobId, DateTime.UtcNow.AddYears(1), cancellationToken);
        await _jobTasks.DeleteForJob(jobId, cancellationToken);
        await _jobs.Delete(jobId, cancellationToken);

        return Ok();
    }

    [Route("ParseCron")]
    [HttpPost]
    public ActionResult ParseCron([FromBody] JobsParseCronRequest request)
    {
        var description = CronHelper.ParseCron(request.Cron);

        var next = CronHelper.GetNextOccurrences(request.Cron, TimeSpan.FromDays(7));
            
        return Ok(new
        {
            description, next
        });
    }

    [Route("Run")]
    [HttpPost]
    public async Task<ActionResult<Guid>> Run([FromBody] JobRunRequest request, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetById(request.JobId, cancellationToken);

        if (job == null)
        {
            throw new Exception($"Job with ID {request.JobId} not found");
        }

        var overrideItemList = new List<String>();
        if (!String.IsNullOrWhiteSpace(request.ItemList))
        {
            overrideItemList = request.ItemList.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        var jobRunId = await _jobRunner.SetupJobRun(job, overrideItemList, cancellationToken);

        return Ok(jobRunId);
    }

    [Route("Stop")]
    [HttpPost]
    public async Task<ActionResult> Stop([FromBody] JobStopRequest request, CancellationToken cancellationToken)
    {
        await _jobRunner.Stop(request.JobRunId, cancellationToken);

        return Ok();
    }
        
    [HttpPost]
    [Route("GetDatabaseList")]
    public async Task<ActionResult> GetDatabaseList([FromBody] JobGetDatabaseListRequest request, CancellationToken cancellationToken)
    {
        var job = await _jobs.GetById(request.JobId, cancellationToken);

        if (job == null)
        {
            throw new Exception($"Job with ID {request.JobId} not found");
        }

        var jobTasks = await _jobTasks.GetForJob(job.JobId, cancellationToken);

        var jobTask = jobTasks.FirstOrDefault(m => m.Order == 0 && m.Type == "createBackup");

        if (jobTask == null)
        {
            return Ok(new List<String>());
        }

        var agent = await _agents.GetById(jobTask.AgentId, cancellationToken);

        if (agent == null)
        {
            throw new Exception($"Agent with ID {jobTask.AgentId} not found");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.CreateBackup?.ConnectionString))
        {
            throw new Exception("Job task has no database connection string configured");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.CreateBackup?.DatabaseType))
        {
            throw new Exception($"Job task {jobTask.Name} does not have a database type set");
        }

        var databases = await _remoteService.GetDatabaseList(agent, jobTask.Settings.CreateBackup.DatabaseType, jobTask.Settings.CreateBackup.ConnectionString, jobTask.Settings.CreateBackup.ConnectionStringPassword, true, cancellationToken);

        var databaseList = databases.Where(m => m.HasAccess).Select(m => m.DatabaseName).ToList();

        return Ok(databaseList);
    }
}

public class JobDuplicateRequest
{
    public Guid JobId { get; set; }
}

public class JobsParseCronRequest
{
    public required String Cron { get; set; }
}

public class JobRunRequest
{
    public Guid JobId { get; set; }
    public String? ItemList { get; set; }
}

public class JobStopRequest
{
    public Guid JobRunId { get; set; }
}

public class JobGetDatabaseListRequest
{
    public Guid JobId { get; set; }
}