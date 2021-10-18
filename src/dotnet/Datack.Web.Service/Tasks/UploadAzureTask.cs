using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;

namespace Datack.Web.Service.Tasks
{
    /// <summary>
    /// This task uploads a file to an AWS s3 bucket.
    /// </summary>
    public class UploadAzureTask : IBaseTask
    {
        public Task<List<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> previousJobRunTasks, Guid jobRunId, CancellationToken cancellationToken)
        {
            var result = previousJobRunTasks
                         .Select(m => new JobRunTask
                         {
                             JobRunTaskId = Guid.NewGuid(),
                             JobTaskId = jobTask.JobTaskId,
                             JobRunId = jobRunId,
                             Type = jobTask.Type,
                             ItemName = m.ItemName,
                             ItemOrder = m.ItemOrder,
                             IsError = false,
                             Result = null,
                             Settings = jobTask.Settings
                         })
                         .ToList();

            return Task.FromResult(result);
        }
    }
}
