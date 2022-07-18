using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/JobRuns")]
public class JobRunsController : Controller
{
    private readonly JobRuns _jobRuns;
    private readonly JobRunTasks _jobRunTasks;
    private readonly JobRunTaskLogs _jobRunTaskLogs;
    private readonly JobRunner _jobRunner;

    public JobRunsController(JobRuns jobRuns, JobRunTasks jobRunTasks, JobRunTaskLogs jobRunTaskLogs, JobRunner jobRunner)
    {
        _jobRuns = jobRuns;
        _jobRunTasks = jobRunTasks;
        _jobRunTaskLogs = jobRunTaskLogs;
        _jobRunner = jobRunner;
    }

    [HttpGet]
    [Route("GetList")]
    public async Task<ActionResult> GetList(Guid jobId, CancellationToken cancellationToken)
    {
        var jobRuns = await _jobRuns.GetAll(null, cancellationToken);
        return Ok(jobRuns);
    }

    [HttpGet]
    [Route("GetForJob/{jobId:guid}")]
    public async Task<ActionResult> GetRuns(Guid jobId, CancellationToken cancellationToken)
    {
        var jobRuns = await _jobRuns.GetAll(jobId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpGet]
    [Route("GetById/{jobRunId:guid}")]
    public async Task<ActionResult> GetById(Guid jobRunId, CancellationToken cancellationToken)
    {
        var jobRuns = await _jobRuns.GetById(jobRunId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpGet]
    [Route("GetTasks/{jobRunId:guid}")]
    public async Task<ActionResult> GetTasks(Guid jobRunId, CancellationToken cancellationToken)
    {
        var jobRuns = await _jobRunTasks.GetByJobRunId(jobRunId, cancellationToken);
        return Ok(jobRuns);
    }

    [HttpGet]
    [Route("GetTaskLogs/{jobRunTaskId:guid}")]
    public async Task<ActionResult> GetTaskLogs(Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        var jobRuns = await _jobRunTaskLogs.GetByJobRunTaskId(jobRunTaskId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpPost]
    [Route("Stop")]
    public async Task<ActionResult> Stop([FromBody] JobRunsStopRequest request, CancellationToken cancellationToken)
    {
        await _jobRunner.Stop(request.JobRunId, cancellationToken);
        return Ok();
    }
}

public class JobRunsStopRequest
{
    public Guid JobRunId { get; set; }
}