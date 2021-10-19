using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    /// <summary>
    /// This task uploads a file to a azure.
    /// </summary>
    public class UploadAzureTask : BaseTask
    {
        public override async Task Run(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                if (previousTask == null)
                {
                    throw new Exception("No previous task found");
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
                    DatabaseName = jobRunTask.ItemName
                };

                var blobFileName = Path.GetFileName(jobRunTask.Settings.UploadAzure.FileName);

                if (String.IsNullOrWhiteSpace(blobFileName))
                {
                    throw new Exception("Blob name cannot be null");
                }

                blobFileName = blobFileName.FormatToken(tokenValues);
                blobFileName = String.Format(blobFileName, jobRunTask.JobRun.Started);

                var blob = blobFileName;
                
                var blobPath = Path.GetDirectoryName(jobRunTask.Settings.UploadAzure.FileName);
                if (!String.IsNullOrWhiteSpace(blobPath))
                {
                    blobPath = blobPath.FormatToken(tokenValues);
                    blobPath = String.Format(blobPath, jobRunTask.JobRun.Started);

                    blob = Path.Combine(blobPath, blobFileName);

                    blob = blob.Replace("\\", "/");
                }

                var resultArtifact = blob;

                OnProgress(jobRunTask.JobRunTaskId, $"Starting upload file to {jobRunTask.Settings.UploadAzure.ContainerName}://{blob}");

                var blobServiceClient = new BlobServiceClient(jobRunTask.Settings.UploadAzure.ConnectionString);
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
                        { "Datack:BackupDate", jobRunTask.Started.Value.ToString("O") },
                        { "Datack:BackupType", jobRunTask.Settings.CreateBackup.BackupType }
                    }
                };

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
}
