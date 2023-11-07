using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Datack.Web.Service.Services;

namespace Datack.Web.Service.Tasks;

/// <summary>
///     This task backs up databases based on the parameters given.
/// </summary>
public class CreateBackupTask : IBaseTask
{
    private readonly RemoteService _remoteService;

    public CreateBackupTask(RemoteService remoteService)
    {
        _remoteService = remoteService;
    }

    public async Task<List<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> previousJobRunTasks, Guid jobRunId, CancellationToken cancellationToken)
    {
        if (jobTask.Settings?.CreateBackup == null)
        {
            throw new Exception($"Job task {jobTask.Name} does not have a database connection string or password set");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.CreateBackup.ConnectionString))
        {
            throw new Exception($"Job task {jobTask.Name} does not have a valid database connection string or password set");
        }

        if (String.IsNullOrWhiteSpace(jobTask.Settings.CreateBackup.DatabaseType))
        {
            throw new Exception($"Job task {jobTask.Name} does not have a database type set");
        }

        var allDatabases = await _remoteService.GetDatabaseList(jobTask.Agent,
                                                                jobTask.Settings.CreateBackup.DatabaseType,
                                                                jobTask.Settings.CreateBackup.ConnectionString,
                                                                jobTask.Settings.CreateBackup.ConnectionStringPassword,
                                                                true,
                                                                cancellationToken);

        var filteredDatabases = DatabaseHelper.FilterDatabases(allDatabases,
                                                               jobTask.Settings.CreateBackup.BackupDefaultExclude,
                                                               jobTask.Settings.CreateBackup.BackupExcludeSystemDatabases,
                                                               jobTask.Settings.CreateBackup.BackupIncludeRegex,
                                                               jobTask.Settings.CreateBackup.BackupExcludeRegex,
                                                               jobTask.Settings.CreateBackup.BackupIncludeManual,
                                                               jobTask.Settings.CreateBackup.BackupExcludeManual,
                                                               jobTask.Settings.CreateBackup.BackupType);

        var index = 0;

        return filteredDatabases
               .Where(m => m.Include)
               .Select(database => new JobRunTask
               {
                   JobRunTaskId = Guid.NewGuid(),
                   JobTaskId = jobTask.JobTaskId,
                   JobRunId = jobRunId,
                   Type = jobTask.Type,
                   ItemName = database.DatabaseName,
                   ItemOrder = index++,
                   IsError = false,
                   Result = null,
                   Settings = jobTask.Settings
               })
               .ToList();
    }
}