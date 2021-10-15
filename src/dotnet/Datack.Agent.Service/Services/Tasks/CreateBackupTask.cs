using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Enums;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    public class CreateBackupTask : BaseTask
    {
        private readonly DatabaseAdapter _databaseAdapter;

        public CreateBackupTask(DatabaseAdapter databaseAdapter)
        {
            _databaseAdapter = databaseAdapter;
        }

        public override async Task<IList<JobRunTask>> Setup(Job job, JobTask jobTask, BackupType backupType, Guid jobRunId, CancellationToken cancellationToken)
        {
            var allDatabases = await _databaseAdapter.GetDatabaseList(cancellationToken);

            var filteredDatabases = DatabaseHelper.FilterDatabases(allDatabases, 
                                                                   jobTask.Settings.CreateBackup.BackupDefaultExclude,
                                                                   jobTask.Settings.CreateBackup.BackupExcludeSystemDatabases,
                                                                   jobTask.Settings.CreateBackup.BackupIncludeRegex,
                                                                   jobTask.Settings.CreateBackup.BackupExcludeRegex,
                                                                   jobTask.Settings.CreateBackup.BackupIncludeManual,
                                                                   jobTask.Settings.CreateBackup.BackupExcludeManual);

            var index = 0;

            return filteredDatabases.Select(database => new JobRunTask
                                    {
                                        JobRunTaskId = Guid.NewGuid(),
                                        JobTaskId = jobTask.JobTaskId,
                                        JobRunId = jobRunId,
                                        Type = jobTask.Type,
                                        Parallel = jobTask.Parallel,
                                        ItemName = database.DatabaseName,
                                        ItemOrder = index++,
                                        IsError = false,
                                        Result = null,
                                        Settings = jobTask.Settings
                                    })
                                    .ToList();
        }

        public override async Task Run(JobRunTask jobRunTask, CancellationToken cancellationToken)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                OnStart(jobRunTask.JobRunTaskId);

                OnProgress(jobRunTask.JobRunTaskId, $"Starting backup task for database {jobRunTask.ItemName}");

                if (String.IsNullOrWhiteSpace(jobRunTask.Settings.CreateBackup.FileName))
                {
                    throw new Exception($"No filename given");
                }

                var tokenValues = new
                {
                    DatabaseName = jobRunTask.ItemName
                };

                var rawFileName = Path.GetFileName(jobRunTask.Settings.CreateBackup.FileName);

                if (String.IsNullOrWhiteSpace(rawFileName))
                {
                    throw new Exception($"Invalid filename '{jobRunTask.Settings.CreateBackup.FileName}'");
                }

                var fileName = rawFileName.FormatToken(tokenValues);
                fileName = String.Format(fileName, jobRunTask.JobRun.Started);

                var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.CreateBackup.FileName);

                if (String.IsNullOrWhiteSpace(rawFilePath))
                {
                    throw new Exception($"Invalid file path '{jobRunTask.Settings.CreateBackup.FileName}'");
                }

                var filePath = rawFilePath.FormatToken(tokenValues);
                filePath = String.Format(filePath, jobRunTask.JobRun.Started);

                var storePath = Path.Combine(filePath, fileName);

                OnProgress(jobRunTask.JobRunTaskId, $"Testing path {storePath}");

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                if (File.Exists(fileName))
                {
                    throw new Exception($"Cannot create backup, file '{storePath}' already exists");
                }

                try
                {
                    await File.WriteAllTextAsync(storePath, "Test write backup", cancellationToken);
                }
                finally
                {
                    File.Delete(storePath);
                }

                OnProgress(jobRunTask.JobRunTaskId, $"Creating backup of database {jobRunTask.ItemName}");

                await _databaseAdapter.CreateBackup(jobRunTask.ItemName,
                                                    storePath,
                                                    evt =>
                                                    {
                                                        OnProgress(jobRunTask.JobRunTaskId, evt.Message);
                                                    },
                                                    cancellationToken);

                sw.Stop();
                
                var message = $"Completed backup of database {jobRunTask.ItemName} {sw.Elapsed:g}";

                OnComplete(jobRunTask.JobRunTaskId, jobRunTask.JobTaskId, message, false);
            }
            catch (Exception ex)
            {
                var message = $"Creation of backup of database {jobRunTask.ItemName} resulted in an error: {ex.Message}";

                OnComplete(jobRunTask.JobRunTaskId, jobRunTask.JobTaskId, message, true);
            }
        }
    }
}
