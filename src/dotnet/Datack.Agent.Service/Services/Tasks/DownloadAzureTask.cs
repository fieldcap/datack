using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task downloads a file from an Azure Blob.
/// </summary>
public class DownloadAzureTask(DataProtector dataProtector) : BaseTask
{
    public override async Task Run(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            if (jobRunTask.Settings.DownloadAzure == null)
            {
                throw new("No settings set");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.DownloadAzure.ConnectionString))
            {
                throw new("No connection string set");
            }

            String sourceKey;
            if (previousTask != null)
            {
                sourceKey = previousTask.ResultArtifact;
            }
            else
            {
                sourceKey = jobRunTask.ItemName;
            }

            if (String.IsNullOrWhiteSpace(sourceKey))
            {
                throw new($"Source key cannot be null");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started,
                FileName = Path.GetFileName(jobRunTask.ItemName),
                FileNameWithoutExtension = Path.GetFileNameWithoutExtension(jobRunTask.ItemName)
            };

            var destinationFileName = jobRunTask.Settings.DownloadAzure.FileName;

            if (String.IsNullOrWhiteSpace(destinationFileName))
            {
                throw new($"Destination filename cannot be null");
            }

            destinationFileName = destinationFileName.FormatFromObject(tokenValues);

            OnProgress(jobRunTask.JobRunTaskId, $"Starting downloading of file {jobRunTask.Settings.DownloadAzure.ContainerName}://{sourceKey} from Azure to file {destinationFileName}");

            var connectionString = dataProtector.Decrypt(jobRunTask.Settings.DownloadAzure.ConnectionString);

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(jobRunTask.Settings.DownloadAzure.ContainerName);
            var blobClient = containerClient.GetBlobClient(sourceKey);

            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            var fileSize = properties.Value.ContentLength;

            var blobDownloadOptions = new BlobDownloadToOptions
            {
                ProgressHandler = new Progress<Int64>(transferred =>
                {
                    var progress = (Int32)(transferred / (Double)fileSize * 100.0);
                    OnProgress(jobRunTask.JobRunTaskId, $"Downloaded {progress}%", true);
                })
            };

            await blobClient.DownloadToAsync(destinationFileName, blobDownloadOptions, cancellationToken);

            sw.Stop();

            var message = $"Completed downloading of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00}) to Azure in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
            
            OnComplete(jobRunTask.JobRunTaskId, message, destinationFileName, false);
        }
        catch (Exception ex)
        {
            var message = $"Downloading of {jobRunTask.ItemName} to Azure resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}