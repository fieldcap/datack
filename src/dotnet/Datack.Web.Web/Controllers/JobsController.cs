using System;
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
        private readonly JobRunner _jobRunner;

        public JobsController(Jobs jobs, JobRunner jobRunner)
        {
            _jobs = jobs;
            _jobRunner = jobRunner;
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

        [Route("ParseCron")]
        [HttpPost]
        public ActionResult ParseCron([FromBody] JobsParseCronRequest request)
        {
            var description = CronHelper.ParseCron(request.Cron);

            var next = CronHelper.GetNextOccurrences(request.Cron, TimeSpan.FromDays(7));
            
            return Ok(new
            {
                resultFull = description, next
            });
        }

        [Route("Run")]
        [HttpPost]
        public async Task<ActionResult> Run([FromBody] JobRunRequest request, CancellationToken cancellationToken)
        {
            await _jobRunner.Run(request.JobId, cancellationToken);

            return Ok();
        }
    }

    public class JobsParseCronRequest
    {
        public String Cron { get; set; }
    }

    public class JobRunRequest
    {
        public Guid ServerId { get; set; }
        public Guid JobId { get; set; }
    }
}
