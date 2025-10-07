using Datack.Common.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Datack.Web.Data.Repositories;

public class JobRunTaskLogRepository(DataContext dataContext)
{
    public async Task<IList<JobRunTaskLog>> GetByJobRunTaskId(Guid jobRunTaskId, CancellationToken cancellationToken)
    {
        return await dataContext.JobRunTaskLogs.AsNoTracking().Where(m => m.JobRunTaskId == jobRunTaskId).OrderBy(m => m.DateTime).ToListAsync(cancellationToken);
    }

    public async Task<JobRunTaskLog> Add(JobRunTaskLog message, CancellationToken cancellationToken)
    {
        await dataContext.JobRunTaskLogs.AddAsync(message, cancellationToken);

        await dataContext.SaveChangesAsync(cancellationToken);
        
        return message;
    }

    public async Task<Int32> DeleteForJob(Guid jobId, DateTimeOffset deleteDate, CancellationToken cancellationToken)
    {
        return await dataContext.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM JobRunTaskLogs
WHERE JobRunTaskLogs.JobRunTaskLogId IN (
    SELECT JobRunTaskLogs.JobRunTaskLogId FROM JobRunTaskLogs
    INNER JOIN JobRunTasks ON JobRunTasks.JobRunTaskId = JobRunTaskLogs.JobRunTaskId
    INNER JOIN JobRuns ON JobRuns.JobRunId = JobRunTasks.JobRunId
    WHERE JobRuns.JobId = {jobId} AND JobRuns.Started < {deleteDate.Ticks}
)", cancellationToken);
    }

    public async Task DeleteForTask(Guid jobTaskId, CancellationToken cancellationToken)
    {
        await dataContext.Database.ExecuteSqlInterpolatedAsync($@"DELETE FROM JobRunTaskLogs
WHERE JobRunTaskLogs.JobRunTaskLogId IN (
    SELECT JobRunTaskLogs.JobRunTaskLogId FROM JobRunTaskLogs
    INNER JOIN JobRunTasks ON JobRunTasks.JobRunTaskId = JobRunTaskLogs.JobRunTaskId
    WHERE JobRunTasks.JobTaskId = {jobTaskId}
)", cancellationToken);
    }
}