using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Enums;
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
            var server = await _jobs.GetById(jobId, cancellationToken);

            if (server == null)
            {
                return NotFound();
            }

            return Ok(server);
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
            var descriptionFull = CronHelper.ParseCron(request.CronFull);
            var descriptionDiff = CronHelper.ParseCron(request.CronDiff);
            var descriptionLog = CronHelper.ParseCron(request.CronLog);

            var next = CronHelper.GetNextOccurrences(request.CronFull, request.CronDiff, request.CronLog, TimeSpan.FromDays(7));
            
            return Ok(new
            {
                resultFull = descriptionFull, resultDiff = descriptionDiff, resultLog = descriptionLog, next
            });
        }

        [Route("Run")]
        [HttpPost]
        public async Task<ActionResult> Run([FromBody] JobRunRequest request, CancellationToken cancellationToken)
        {
            await _jobRunner.Run(request.JobId, request.BackupType, cancellationToken);

            return Ok();
        }
    }

    public class JobsParseCronRequest
    {
        public String CronFull { get; set; }
        public String CronDiff { get; set; }
        public String CronLog { get; set; }
    }

    public class JobRunRequest
    {
        public Guid ServerId { get; set; }
        public Guid JobId { get; set; }
        public BackupType BackupType { get; set; }
    }
}
