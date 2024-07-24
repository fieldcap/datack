using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task uploads a file to a azure.
/// </summary>
public class UploadAzureTask : BaseTask
{
    private readonly DataProtector _dataProtector;

    public UploadAzureTask(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public override async Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            if (previousTask == null)
            {
                throw new Exception("No previous task found");
            }

            if (jobRunTask.Settings.UploadAzure == null)
            {
                throw new Exception("No settings set");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.UploadAzure.ConnectionString))
            {
                throw new Exception("No connection string set");
            }

            var sourceFileName = previousTask.ResultArtifact;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting upload to azure task for file {sourceFileName}");

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new Exception($"No source file found");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new Exception($"Source file '{sourceFileName}' not found");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started
            };

            var blobFileName = Path.GetFileName(jobRunTask.Settings.UploadAzure.FileName);

            if (String.IsNullOrWhiteSpace(blobFileName))
            {
                throw new Exception("Blob name cannot be null");
            }

            blobFileName = blobFileName.FormatFromObject(tokenValues);

            var blob = blobFileName;
                
            var blobPath = Path.GetDirectoryName(jobRunTask.Settings.UploadAzure.FileName);
            if (!String.IsNullOrWhiteSpace(blobPath))
            {
                blobPath = blobPath.FormatFromObject(tokenValues);

                blob = Path.Combine(blobPath, blobFileName);

                blob = blob.Replace("\\", "/");
            }

            var resultArtifact = blob;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting upload file to {jobRunTask.Settings.UploadAzure.ContainerName}://{blob}");

            var connectionString = _dataProtector.Decrypt(jobRunTask.Settings.UploadAzure.ConnectionString);

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(jobRunTask.Settings.UploadAzure.ContainerName);
            var blobClient = containerClient.GetBlobClient(blob);

            var fileSize = new FileInfo(sourceFileName).Length;
                
            var blobUploadOptions = new BlobUploadOptions
            {
                ProgressHandler = new Progress<Int64>(transferred =>
                {
                    var progress = (Int32)(transferred / (Double)fileSize * 100.0);
                    OnProgress(jobRunTask.JobRunTaskId, $"Uploading file {progress}% complete", true);
                }),
                Tags = new Dictionary<String, String>
                {
                    { "Datack:JobDate", jobRunTask.JobRun.Started.ToString("O") }
                }
            };

            if (!String.IsNullOrWhiteSpace(jobRunTask.Settings.UploadAzure.Tag))
            {
                blobUploadOptions.Tags.Add("Datack:Tag", jobRunTask.Settings.UploadAzure.Tag);
            }

            await blobClient.UploadAsync(sourceFileName, blobUploadOptions, cancellationToken);

            sw.Stop();
                
            var message = $"Completed uploading of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00}) to Azure in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
                
            OnComplete(jobRunTask.JobRunTaskId, message, resultArtifact, false);
        }
        catch (Exception ex)
        {
            var message = $"Uploading of {jobRunTask.ItemName} to Azure resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}