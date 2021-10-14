using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Enums;
using Datack.Common.Extensions;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    public class CreateBackupTask : BaseTask
    {
        private const Int32 Parallel = 2;
        
        private readonly DatabaseAdapter _databaseAdapter;

        public CreateBackupTask(DatabaseAdapter databaseAdapter)
        {
            _databaseAdapter = databaseAdapter;
        }

        public override async Task<IList<StepLog>> Setup(Job job, Step step, BackupType backupType, Guid jobLogId, CancellationToken cancellationToken)
        {
            var databases = new List<DatabaseStep>();

            var allDatabaseList = await _databaseAdapter.GetDatabaseList(cancellationToken);

            var filteredDatabaseList = DatabaseHelper.FilterDatabases(allDatabaseList, step.Settings.CreateBackup);

            var databaseList = filteredDatabaseList.Where(m => m.Include).ToList();

            var databaseFileSizeList = await _databaseAdapter.GetFileList(cancellationToken);

            foreach (var database in databaseList)
            {
                var files = databaseFileSizeList.Where(m => m.DatabaseName == database.DatabaseName).ToList();

                Int64 size = 0;

                if (files.Count > 0)
                {
                    size = files.Sum(m => m.Size);
                }

                databases.Add(new DatabaseStep
                {
                    DatabaseName = database.DatabaseName,
                    Size = size
                });
            }

            var batches = databases.Split(Parallel);

            var results = new List<StepLog>();

            var batchIndex = 0;
            foreach (var batch in batches)
            {
                foreach (var database in batch)
                {
                    var stepLog = new StepLog
                    {
                        StepLogId = Guid.NewGuid(),
                        StepId = step.StepId,
                        JobLogId = jobLogId,
                        DatabaseName = database.DatabaseName,
                        Queue = batchIndex,
                        Type = step.Type,
                        Settings = step.Settings
                    };

                    results.Add(stepLog);
                }

                batchIndex++;
            }

            return results;
        }

        public override async Task Run(List<StepLog> queue, CancellationToken cancellationToken)
        {
            var index = 0;
            foreach (var step in queue)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();

                    index++;

                    OnStart(step.StepLogId);

                    OnProgress(step.StepLogId, index, $"Starting backup {step.DatabaseName} {index}/{queue.Count}");

                    if (String.IsNullOrWhiteSpace(step.Settings.CreateBackup.FileName))
                    {
                        throw new Exception($"No filename given");
                    }

                    var tokenValues = new
                    {
                        DatabaseName = step.DatabaseName
                    };

                    var rawFileName = Path.GetFileName(step.Settings.CreateBackup.FileName);

                    if (String.IsNullOrWhiteSpace(rawFileName))
                    {
                        throw new Exception($"Invalid filename '{step.Settings.CreateBackup.FileName}'");
                    }

                    var fileName = rawFileName.FormatToken(tokenValues);
                    fileName = String.Format(fileName, step.JobLog.Started);

                    var rawFilePath = Path.GetDirectoryName(step.Settings.CreateBackup.FileName);

                    if (String.IsNullOrWhiteSpace(rawFilePath))
                    {
                        throw new Exception($"Invalid file path '{step.Settings.CreateBackup.FileName}'");
                    }

                    var filePath = rawFilePath.FormatToken(tokenValues);
                    filePath = String.Format(filePath, step.JobLog.Started);

                    var storePath = Path.Combine(filePath, fileName);

                    OnProgress(step.StepLogId, index, $"Testing path {storePath}");

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

                    OnProgress(step.StepLogId, index, $"Creating backup {step.DatabaseName}");

                    await _databaseAdapter.CreateBackup(step.DatabaseName,
                                                        storePath,
                                                        evt =>
                                                        {
                                                            OnProgress(step.StepLogId, index, evt.Message);
                                                        },
                                                        cancellationToken);

                    OnProgress(step.StepLogId, index, $"Completed backup {step.DatabaseName}");

                    sw.Stop();
                    
                    var message = $"Operation completed in {sw.Elapsed:g}";

                    OnComplete(step.StepLogId, step.JobLogId, message, false);
                }
                catch (Exception ex)
                {
                    var message = $"Operation resulted in an error: {ex.Message}";

                    OnComplete(step.StepLogId, step.JobLogId, message, true);
                }
            }
        }
    }

    public class DatabaseStep
    {
        public String DatabaseName { get; set; }
        public Decimal Size { get; set; }
    }
}
