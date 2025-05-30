﻿using System.Diagnostics;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task uploads a file to an AWS s3 bucket.
/// </summary>
public class UploadS3Task : BaseTask
{
    private readonly DataProtector _dataProtector;

    public UploadS3Task(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public override async Task Run(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        try
        {
            var sw = new Stopwatch();
            sw.Start();

            if (previousTask == null)
            {
                throw new("No previous task found");
            }

            if (jobRunTask.Settings.UploadS3 == null)
            {
                throw new("No settings set");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.UploadS3.Secret))
            {
                throw new("No S3 password set");
            }

            var sourceFileName = previousTask.ResultArtifact;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting upload to s3 task for file {sourceFileName}");

            if (String.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new($"No source file found");
            }

            if (!File.Exists(sourceFileName))
            {
                throw new($"Source file '{sourceFileName}' not found");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started,
                FileName = Path.GetFileName(jobRunTask.ItemName),
                FileNameWithoutExtension = Path.GetFileNameWithoutExtension(jobRunTask.ItemName)
            };

            var keyFileName = Path.GetFileName(jobRunTask.Settings.UploadS3.FileName);

            if (String.IsNullOrWhiteSpace(keyFileName))
            {
                throw new($"Key cannot be null");
            }

            keyFileName = keyFileName.FormatFromObject(tokenValues);

            var key = keyFileName;
                
            var keyPath = Path.GetDirectoryName(jobRunTask.Settings.UploadS3.FileName);
            if (!String.IsNullOrWhiteSpace(keyPath))
            {
                keyPath = keyPath.FormatFromObject(tokenValues);

                key = Path.Combine(keyPath, keyFileName);

                key = key.Replace("\\", "/");
            }

            var resultArtifact = key;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting upload file to {jobRunTask.Settings.UploadS3.Bucket}://{key}");

            var region = RegionEndpoint.GetBySystemName(jobRunTask.Settings.UploadS3.Region);

            var secret = _dataProtector.Decrypt(jobRunTask.Settings.UploadS3.Secret);

            var s3Client = new AmazonS3Client(new BasicAWSCredentials(jobRunTask.Settings.UploadS3.AccessKey, secret), region);

            var fileTransferUtility = new TransferUtility(s3Client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = jobRunTask.Settings.UploadS3.Bucket,
                FilePath = sourceFileName,
                Key = key,
                TagSet = new()
                {
                    new()
                    {
                        Key = "Datack:JobDate",
                        Value = jobRunTask.JobRun.Started.ToString("O")
                    }
                }
            };

            if (!String.IsNullOrWhiteSpace(jobRunTask.Settings.UploadS3.Tag))
            {
                uploadRequest.TagSet.Add(new()
                {
                    Key = "Datack:Tag",
                    Value = jobRunTask.Settings.UploadS3.Tag
                });
            }

            var fileSize = new FileInfo(sourceFileName).Length;

            uploadRequest.UploadProgressEvent += (_, args) =>
            {
                var msg = $"Uploaded {args.PercentDone}%";

                OnProgress(jobRunTask.JobRunTaskId, msg, true);
            };

            await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

            sw.Stop();
                
            var message = $"Completed uploading of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00}) to s3 in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
                
            OnComplete(jobRunTask.JobRunTaskId, message, resultArtifact, false);
        }
        catch (Exception ex)
        {
            var message = $"Uploading of {jobRunTask.ItemName} to s3 resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}