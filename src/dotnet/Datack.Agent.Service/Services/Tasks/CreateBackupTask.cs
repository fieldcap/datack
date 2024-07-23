using System.Diagnostics;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task backs up databases based on the parameters given.
/// </summary>
public class CreateBackupTask : BaseTask
{
    private readonly DatabaseAdapter _databaseAdapter;
    private readonly DataProtector _dataProtector;

    public CreateBackupTask(DatabaseAdapter databaseAdapter, DataProtector dataProtector)
    {
        _databaseAdapter = databaseAdapter;
        _dataProtector = dataProtector;
    }
        
    public override async Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
    {
        try
        {
            if (jobRunTask.Settings.CreateBackup == null)
            {
                throw new Exception("No settings set");
            }

            var sw = new Stopwatch();
            sw.Start();

            OnProgress(jobRunTask.JobRunTaskId, $"Starting backup task for database {jobRunTask.ItemName}");

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.CreateBackup.FileName))
            {
                throw new Exception($"No filename given");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName
            };

            var rawFileName = Path.GetFileName(jobRunTask.Settings.CreateBackup.FileName);

            if (String.IsNullOrWhiteSpace(rawFileName))
            {
                throw new Exception($"Invalid filename '{jobRunTask.Settings.CreateBackup.FileName}'");
            }

            var fileName = rawFileName.FormatFromObject(tokenValues);
            fileName = String.Format(fileName, jobRunTask.JobRun.Started);

            var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.CreateBackup.FileName);

            if (String.IsNullOrWhiteSpace(rawFilePath))
            {
                throw new Exception($"Invalid file path '{jobRunTask.Settings.CreateBackup.FileName}'");
            }

            var filePath = rawFilePath.FormatFromObject(tokenValues);
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
                File.Delete(fileName);
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

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.CreateBackup.DatabaseType))
            {
                throw new Exception($"Job Task does not have a database type selected");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.CreateBackup.ConnectionString))
            {
                throw new Exception($"Job Task does not have a connection string configured");
            }

            var connectionString = _databaseAdapter.CreateConnectionString(jobRunTask.Settings.CreateBackup.ConnectionString, jobRunTask.Settings.CreateBackup.ConnectionStringPassword, true);
            var password = jobRunTask.Settings.CreateBackup.ConnectionStringPassword;

            if (!String.IsNullOrWhiteSpace(password))
            {
                password = _dataProtector.Decrypt(password);
            }

            await _databaseAdapter.CreateBackup(jobRunTask.Settings.CreateBackup.DatabaseType,
                                                connectionString,
                                                jobRunTask.ItemName,
                                                jobRunTask.Settings.CreateBackup.BackupType,
                                                password,
                                                jobRunTask.Settings.CreateBackup.Options,
                                                storePath,
                                                evt =>
                                                {
                                                    OnProgress(jobRunTask.JobRunTaskId, evt.Message, evt.Message.Contains('%'));
                                                },
                                                cancellationToken);

            sw.Stop();

            var fileSize = new FileInfo(storePath).Length;
                
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