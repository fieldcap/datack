﻿using Datack.Common.Models.Data;

namespace Datack.Web.Service.Tasks;

public interface IBaseTask
{
    Task<List<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> initialJobRunTasks, Guid jobRunId, CancellationToken cancellationToken);
}