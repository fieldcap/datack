using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Datack.Common.Enums;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks
{
    /// <summary>
    /// This task uploads a file to an AWS s3 bucket.
    /// </summary>
    public class UploadS3Task : BaseTask
    {
        public override async Task<IList<JobRunTask>> Setup(Job job, JobTask jobTask, IList<JobRunTask> previousJobRunTasks, BackupType backupType, Guid jobRunId, CancellationToken cancellationToken)
        {
            return previousJobRunTasks
                   .Select(m => new JobRunTask
                   {
                       JobRunTaskId = Guid.NewGuid(),
                       JobTaskId = jobTask.JobTaskId,
                       JobRunId = jobRunId,
                       Type = jobTask.Type,
                       ItemName = m.ItemName,
                       ItemOrder = m.ItemOrder,
                       IsError = false,
                       Result = null,
                       Settings = jobTask.Settings
                   })
                   .ToList();
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

                var sourceFileName = previousTask.ResultArtifact;

                OnProgress(jobRunTask.JobRunTaskId, $"Starting upload to s3 task for file {sourceFileName}");

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

                var rawFileName = Path.GetFileName(jobRunTask.Settings.UploadS3.FileName);

                if (String.IsNullOrWhiteSpace(rawFileName))
                {
                    throw new Exception($"Invalid filename '{jobRunTask.Settings.UploadS3.FileName}'");
                }

                var keyFileName = rawFileName.FormatToken(tokenValues);
                keyFileName = String.Format(keyFileName, jobRunTask.JobRun.Started);

                var rawFilePath = Path.GetDirectoryName(jobRunTask.Settings.UploadS3.FileName);

                if (String.IsNullOrWhiteSpace(rawFilePath))
                {
                    throw new Exception($"Invalid file path '{jobRunTask.Settings.UploadS3.FileName}'");
                }

                var keyPrefix = rawFilePath.FormatToken(tokenValues);
                keyPrefix = String.Format(keyPrefix, jobRunTask.JobRun.Started);

                var key = Path.Combine(keyPrefix, keyFileName);

                key = key.Replace("\\", "/");

                var resultArtifact = key;

                OnProgress(jobRunTask.JobRunTaskId, $"Starting upload file to {jobRunTask.Settings.UploadS3.Bucket}://{key}");

                var region = RegionEndpoint.GetBySystemName(jobRunTask.Settings.UploadS3.Region);

                var s3Client = new AmazonS3Client(new BasicAWSCredentials(jobRunTask.Settings.UploadS3.AccessKey, jobRunTask.Settings.UploadS3.Secret), region);

                var fileTransferUtility = new TransferUtility(s3Client);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = jobRunTask.Settings.UploadS3.Bucket,
                    FilePath = sourceFileName,
                    Key = key
                };

                uploadRequest.UploadProgressEvent += (_, args) =>
                {
                    var msg = $"Uploaded {args.PercentDone}%";

                    OnProgress(jobRunTask.JobRunTaskId, msg);
                };

                await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

                sw.Stop();
                
                var message = $"Completed uploading of {jobRunTask.ItemName} to s3 in {sw.Elapsed:g}";
                
                OnComplete(jobRunTask.JobRunTaskId, jobRunTask.JobRunId, message, resultArtifact, false);
            }
            catch (Exception ex)
            {
                var message = $"Uploading of {jobRunTask.ItemName} to s3 resulted in an error: {ex.Message}";

                OnComplete(jobRunTask.JobRunTaskId, jobRunTask.JobRunId, message, null, true);
            }
        }
    }
}
