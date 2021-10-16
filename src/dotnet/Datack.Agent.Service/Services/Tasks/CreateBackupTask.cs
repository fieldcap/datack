using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    /// <summary>
    /// This task backs up databases based on the parameters given.
    /// </summary>
    public class CreateBackupTask : BaseTask
    {
        private readonly DatabaseAdapter _databaseAdapter;

        public CreateBackupTask(DatabaseAdapter databaseAdapter)
        {
            _databaseAdapter = databaseAdapter;
        }
        
        public override async Task Run(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

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

                var resultArtifact = storePath;

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

                await _databaseAdapter.CreateBackup(server.DbSettings,
                                                    jobRunTask.ItemName,
                                                    storePath,
                                                    evt =>
                                                    {
                                                        OnProgress(jobRunTask.JobRunTaskId, evt.Message);
                                                    },
                                                    cancellationToken);

                sw.Stop();
                
                var message = $"Completed backup of database {jobRunTask.ItemName} {sw.Elapsed:g}";
                
                OnComplete(jobRunTask.JobRunTaskId, message, resultArtifact, false);
            }
            catch (Exception ex)
            {
                var message = $"Creation of backup of database {jobRunTask.ItemName} resulted in an error: {ex.Message}";

                OnComplete(jobRunTask.JobRunTaskId, message, null, true);
            }
        }
    }
}
