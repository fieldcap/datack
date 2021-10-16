using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Enums;
using Datack.Common.Models.Data;

namespace Datack.Web.Service.Tasks
{
    public interface IBaseTask
    {
        Task<List<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> initialJobRunTasks, BackupType backupType, Guid jobRunId, CancellationToken cancellationToken);
    }
}
