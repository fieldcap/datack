using System.Diagnostics;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using ByteSizeLib;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task downloads a file from an AWS s3 bucket.
/// </summary>
public class DownloadS3Task : BaseTask
{
    private readonly DataProtector _dataProtector;

    public DownloadS3Task(DataProtector dataProtector)
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

            if (jobRunTask.Settings.DownloadS3 == null)
            {
                throw new Exception("No settings set");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.DownloadS3.Secret))
            {
                throw new Exception("No S3 password set");
            }

            var tokenValues = new
            {
                jobRunTask.ItemName,
                jobRunTask.JobRun.Started
            };

            var sourceKey = previousTask.ResultArtifact;
            var destinationFileName = jobRunTask.Settings.DownloadS3.FileName;

            if (String.IsNullOrWhiteSpace(destinationFileName))
            {
                throw new Exception($"Destination filename cannot be null");
            }

            destinationFileName = destinationFileName.FormatFromObject(tokenValues);

            OnProgress(jobRunTask.JobRunTaskId, $"Starting downloading of file {jobRunTask.Settings.DownloadS3.Bucket}://{sourceKey} from s3 to file {destinationFileName}");
            
            var region = RegionEndpoint.GetBySystemName(jobRunTask.Settings.DownloadS3.Region);

            var secret = _dataProtector.Decrypt(jobRunTask.Settings.DownloadS3.Secret);

            var s3Client = new AmazonS3Client(new BasicAWSCredentials(jobRunTask.Settings.DownloadS3.AccessKey, secret), region);

            var fileTransferUtility = new TransferUtility(s3Client);

            var downloadRequest = new TransferUtilityDownloadRequest
            {
                BucketName = jobRunTask.Settings.DownloadS3.Bucket,
                FilePath = destinationFileName,
                Key = sourceKey
            };
            
            downloadRequest.WriteObjectProgressEvent += (_, args) =>
            {
                var msg = $"Downloaded {args.PercentDone}%";

                OnProgress(jobRunTask.JobRunTaskId, msg, true);
            };

            await fileTransferUtility.DownloadAsync(downloadRequest, cancellationToken);

            sw.Stop();

            if (!File.Exists(destinationFileName))
            {
                throw new Exception($"Downloaded file '{destinationFileName}' not found");
            }

            var fileSize = new FileInfo(destinationFileName).Length;

            var message = $"Completed downloading of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00}) to s3 in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";
                
            OnComplete(jobRunTask.JobRunTaskId, message, destinationFileName, false);
        }
        catch (Exception ex)
        {
            var message = $"Downloading of {jobRunTask.ItemName} to s3 resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}