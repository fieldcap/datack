using Datack.Web.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datack.Web.Web.Controllers;

[Authorize]
[Route("Api/JobRuns")]
public class JobRunsController(JobRuns runs, JobRunTasks jobRunTasks, JobRunTaskLogs jobRunTaskLogs, JobRunner jobRunner)
    : Controller
{
    [HttpGet]
    [Route("GetList")]
    public async Task<ActionResult> GetList(CancellationToken cancellationToken)
    {
        var jobRuns = await runs.GetAll(null, cancellationToken);
        return Ok(jobRuns);
    }

    [HttpGet]
    [Route("GetForJob/{jobId:guid}")]
    public async Task<ActionResult> GetRuns(Guid jobId, CancellationToken cancellationToken)
    {
        var jobRuns = await runs.GetAll(jobId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpGet]
    [Route("GetById/{jobRunId:guid}")]
    public async Task<ActionResult> GetById(Guid jobRunId, CancellationToken cancellationToken)
    {
        var jobRuns = await runs.GetById(jobRunId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpGet]
    [Route("GetTasks/{jobRunId:guid}")]
    public async Task<ActionResult> GetTasks(Guid jobRunId, CancellationToken cancellationToken)
    {
        var jobRuns = await jobRunTasks.GetByJobRunId(jobRunId, cancellationToken);
        return Ok(jobRuns);
    }

    [HttpGet]
    [Route("GetTaskLogs/{jobRunTaskId:guid}")]
    public async Task<ActionResult> GetTaskLogs(Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        var jobRuns = await jobRunTaskLogs.GetByJobRunTaskId(jobRunTaskId, cancellationToken);
        return Ok(jobRuns);
    }
        
    [HttpPost]
    [Route("Stop")]
    public async Task<ActionResult> Stop([FromBody] JobRunsStopRequest request, CancellationToken cancellationToken)
    {
        await jobRunner.Stop(request.JobRunId, cancellationToken);
        return Ok();
    }
}

public class JobRunsStopRequest
{
    public Guid JobRunId { get; set; }
}