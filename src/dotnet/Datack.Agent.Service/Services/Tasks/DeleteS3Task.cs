using System.Diagnostics;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Datack.Common.Models.Data;
using StringTokenFormatter;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task deletes files from S3.
/// </summary>
public class DeleteS3Task : BaseTask
{
    private readonly DataProtector _dataProtector;

    public DeleteS3Task(DataProtector dataProtector)
    {
        _dataProtector = dataProtector;
    }

    public override async Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
    {
        try
        {
            if (jobRunTask.Settings.DeleteS3 == null)
            {
                throw new Exception("No settings set");
            }

            OnProgress(jobRunTask.JobRunTaskId, $"Starting delete S3 task");

            var keyPath = jobRunTask.Settings.DeleteS3.FileName;

            if (String.IsNullOrWhiteSpace(keyPath))
            {
                throw new Exception($"Key path cannot be null");
            }

            if (String.IsNullOrWhiteSpace(jobRunTask.Settings.DeleteS3.Tag))
            {
                throw new Exception($"Tag cannot be null");
            }

            var sw = new Stopwatch();
            sw.Start();

            var tokenValues = new
            {
                jobRunTask.ItemName
            };

            keyPath = keyPath.FormatToken(tokenValues);
            keyPath = String.Format(keyPath, jobRunTask.JobRun.Started);

            keyPath = keyPath.Replace("\\", "/");

            var region = RegionEndpoint.GetBySystemName(jobRunTask.Settings.DeleteS3.Region);

            var secret = _dataProtector.Decrypt(jobRunTask.Settings.DeleteS3.Secret);

            var s3Client = new AmazonS3Client(new BasicAWSCredentials(jobRunTask.Settings.DeleteS3.AccessKey, secret), region);

            String nextToken = null;

            var deleteDate = DateTimeOffset.UtcNow;

            deleteDate = jobRunTask.Settings.DeleteS3.TimeSpanType switch
            {
                "Year" => deleteDate.AddYears(-jobRunTask.Settings.DeleteS3.TimeSpanAmount),
                "Month" => deleteDate.AddMonths(-jobRunTask.Settings.DeleteS3.TimeSpanAmount),
                "Day" => deleteDate.AddDays(-jobRunTask.Settings.DeleteS3.TimeSpanAmount),
                "Hour" => deleteDate.AddHours(-jobRunTask.Settings.DeleteS3.TimeSpanAmount),
                "Minute" => deleteDate.AddMinutes(-jobRunTask.Settings.DeleteS3.TimeSpanAmount),
                _ => deleteDate
            };

            OnProgress(jobRunTask.JobRunTaskId, $"Get objects in path {jobRunTask.Settings.DeleteS3.Bucket}://{keyPath} with tag {jobRunTask.Settings.DeleteS3.Tag} that have been ran before {deleteDate:R}");

            var totalCount = 0;
            var errorCount = 0;

            var progressCount = 0;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                var listObjectsResponse = await s3Client.ListObjectsV2Async(new ListObjectsV2Request
                                                                            {
                                                                                BucketName = jobRunTask.Settings.DeleteS3.Bucket,
                                                                                ContinuationToken = nextToken,
                                                                                MaxKeys = 1000,
                                                                                Prefix = keyPath
                                                                            },
                                                                            cancellationToken);

                OnProgress(jobRunTask.JobRunTaskId, $"Listing {progressCount} - {progressCount + listObjectsResponse.S3Objects.Count}");

                progressCount += listObjectsResponse.S3Objects.Count;

                nextToken = listObjectsResponse.NextContinuationToken;

                var objectsToDelete = new List<KeyVersion>();
                    
                foreach (var s3Object in listObjectsResponse.S3Objects)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    var s3Tags = await s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                    {
                        Key = s3Object.Key,
                        BucketName = s3Object.BucketName
                    }, cancellationToken);

                    var jobDateTag = s3Tags.Tagging.FirstOrDefault(m => m.Key == "Datack:JobDate");
                    var tagTag = s3Tags.Tagging.FirstOrDefault(m => m.Key == "Datack:Tag");

                    if (jobDateTag != null && tagTag?.Value == jobRunTask.Settings.DeleteS3.Tag)
                    {
                        var date = DateTime.Parse(jobDateTag.Value);

                        if (date < deleteDate)
                        {
                            objectsToDelete.Add(new KeyVersion
                            {
                                Key = s3Object.Key
                            });   
                        }
                    }
                }

                if (objectsToDelete.Count > 0)
                {
                    OnProgress(jobRunTask.JobRunTaskId, $"Deleting {objectsToDelete.Count} objects");

                    var deleteResult = await s3Client.DeleteObjectsAsync(new DeleteObjectsRequest
                                                                         {
                                                                             BucketName = jobRunTask.Settings.DeleteS3.Bucket,
                                                                             Objects = objectsToDelete
                                                                         },
                                                                         cancellationToken);

                    if (deleteResult.DeleteErrors != null)
                    {
                        errorCount += deleteResult.DeleteErrors.Count;
                        totalCount += deleteResult.DeletedObjects.Count;
                        foreach (var deleteError in deleteResult.DeleteErrors)
                        {
                            OnProgress(jobRunTask.JobRunTaskId, $"{deleteError.Code}: {deleteError.Message} ({deleteError.Key})");
                        }
                    }
                }
            }
            while (nextToken != null);

            sw.Stop();

            var message = $"Completed deletion of {totalCount - errorCount} S3 of {jobRunTask.ItemName} in {sw.Elapsed:g} with {errorCount} errors";
            OnComplete(jobRunTask.JobRunTaskId, message, null, errorCount > 0);
        }
        catch (Exception ex)
        {
            var message = $"Deletion in S3 of {jobRunTask.ItemName} resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}