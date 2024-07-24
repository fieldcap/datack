using System.Diagnostics;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task restores a backup of a database.
/// </summary>
public class RestoreBackupTask : BaseTask
{
    private readonly DatabaseAdapter _databaseAdapter;
    private readonly DataProtector _dataProtector;

    public RestoreBackupTask(DatabaseAdapter databaseAdapter, DataProtector dataProtector)
    {
        _databaseAdapter = databaseAdapter;
        _dataProtector = dataProtector;
    }
        
    public override async Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
    {
        try
        {
            if (jobRunTask.Settings.RestoreBackup == null)
            {
                throw new Exception("No settings set");
            }

            var sw = new Stopwatch();
            sw.Start();

            OnProgress(jobRunTask.JobRunTaskId, $"Starting backup task for database {jobRunTask.ItemName}");

            var sourceFileName = previousTask.ResultArtifact;

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new Exception($"No filename given");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new Exception($"File {sourceFileName} does not exist");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started
            };

            var rawDatabaseName = Path.GetFileName(jobRunTask.Settings.RestoreBackup.DatabaseName);

            if (String.IsNullOrWhiteSpace(rawDatabaseName))
            {
                throw new Exception($"Invalid database name '{jobRunTask.Settings.RestoreBackup.DatabaseName}'");
            }

            var databaseName = rawDatabaseName.FormatFromObject(tokenValues);
            
            var resultArtifact = databaseName;

            OnProgress(jobRunTask.JobRunTaskId, $"Restoring backup of database {jobRunTask.ItemName} to {databaseName}");

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.RestoreBackup.DatabaseType))
            {
                throw new Exception($"Job Task does not have a database type selected");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.RestoreBackup.ConnectionString))
            {
                throw new Exception($"Job Task does not have a connection string configured");
            }

            var connectionString = _databaseAdapter.CreateConnectionString(jobRunTask.Settings.RestoreBackup.ConnectionString, jobRunTask.Settings.RestoreBackup.ConnectionStringPassword, true);
            var password = jobRunTask.Settings.RestoreBackup.ConnectionStringPassword;

            if (!String.IsNullOrWhiteSpace(password))
            {
                password = _dataProtector.Decrypt(password);
            }

            await _databaseAdapter.RestoreBackup(jobRunTask.Settings.RestoreBackup.DatabaseType,
                                                connectionString,
                                                databaseName,
                                                password,
                                                jobRunTask.Settings.RestoreBackup.Options,
                                                sourceFileName,
                                                evt =>
                                                {
                                                    OnProgress(jobRunTask.JobRunTaskId, evt.Message, evt.Message.Contains('%'));
                                                },
                                                cancellationToken);

            sw.Stop();

            var fileSize = new FileInfo(sourceFileName).Length;
                
            var message = $"Completed backup of database of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00} in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";

            OnComplete(jobRunTask.JobRunTaskId, message, resultArtifact, false);
        }
        catch (Exception ex)
        {
            var message = $"Creation of backup of database {jobRunTask.ItemName} resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}