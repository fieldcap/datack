using System;
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
    [Route("Api/Jobs")]
    public class JobsController : Controller
    {
        private readonly Jobs _jobs;
        private readonly JobRuns _jobRuns;
        private readonly JobRunner _jobRunner;
        private readonly JobTasks _jobTasks;

        public JobsController(Jobs jobs, JobRuns jobRuns, JobRunner jobRunner, JobTasks jobTasks)
        {
            _jobs = jobs;
            _jobRuns = jobRuns;
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
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
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
                var errors = ModelState.Select(x => x.Value.Errors)
                                       .Where(y => y.Count > 0)
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

            var jobRunId = await _jobRunner.SetupJobRun(job, cancellationToken);

            return Ok(jobRunId);
        }

        [Route("Stop")]
        [HttpPost]
        public async Task<ActionResult> Stop([FromBody] JobStopRequest request, CancellationToken cancellationToken)
        {
            await _jobRunner.Stop(request.JobRunId, cancellationToken);

            return Ok();
        }
    }

    public class JobDuplicateRequest
    {
        public Guid JobId { get; set; }
    }

    public class JobsParseCronRequest
    {
        public String Cron { get; set; }
    }

    public class JobRunRequest
    {
        public Guid JobId { get; set; }
    }

    public class JobStopRequest
    {
        public Guid JobRunId { get; set; }
    }
}
