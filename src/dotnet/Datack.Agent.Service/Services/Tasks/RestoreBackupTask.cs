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
        
    public override async Task Run(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        try
        {
            if (previousTask == null)
            {
                throw new("No previous task found");
            }

            if (jobRunTask.Settings.RestoreBackup == null)
            {
                throw new("No settings set");
            }

            var sw = new Stopwatch();
            sw.Start();

            OnProgress(jobRunTask.JobRunTaskId, $"Starting restore task");

            var sourceFileName = previousTask.ResultArtifact;

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new($"No filename given");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new($"File {sourceFileName} does not exist");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started,
                FileName = Path.GetFileName(jobRunTask.ItemName),
                FileNameWithoutExtension = Path.GetFileNameWithoutExtension(jobRunTask.ItemName)
            };

            var rawDatabaseName = Path.GetFileName(jobRunTask.Settings.RestoreBackup.DatabaseName);

            if (String.IsNullOrWhiteSpace(rawDatabaseName))
            {
                throw new($"Invalid database name '{jobRunTask.Settings.RestoreBackup.DatabaseName}'");
            }

            var databaseName = rawDatabaseName.FormatFromObject(tokenValues);
            var databaseLocation = jobRunTask.Settings.RestoreBackup.DatabaseLocation?.FormatFromObject(tokenValues);
            
            OnProgress(jobRunTask.JobRunTaskId, $"Restoring backup of database {jobRunTask.ItemName} to {databaseName}");

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.RestoreBackup.DatabaseType))
            {
                throw new($"Job Task does not have a database type selected");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.RestoreBackup.ConnectionString))
            {
                throw new($"Job Task does not have a connection string configured");
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
                                                databaseLocation,
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
                
            var message = $"Completed restore of database of {databaseName} ({ByteSize.FromBytes(fileSize):0.00} in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";

            OnComplete(jobRunTask.JobRunTaskId, message, databaseName, false);
        }
        catch (Exception ex)
        {
            var message = $"Creation of restoring of database resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}