﻿using System.Diagnostics;
using ByteSizeLib;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks;

/// <summary>
/// This task deletes files from disk.
/// </summary>
public class DeleteFileTask : BaseTask
{
    public override async Task Run(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        try
        {
            if (previousTask == null)
            {
                throw new("No previous task found");
            }

            if (jobRunTask.Settings.DeleteFile == null)
            {
                throw new("No settings set");
            }

            if (jobRunTask.JobTask.Settings.DeleteFile == null)
            {
                throw new("No settings set");
            }

            var sourceFileName = previousTask.ResultArtifact;

            OnProgress(jobRunTask.JobRunTaskId, $"Starting delete task for file {sourceFileName}");

            if (!jobRunTask.JobTask.Settings.DeleteFile.IgnoreIfFileDoesNotExist && !File.Exists(sourceFileName))
            {
                throw new($"Source file '{sourceFileName}' not found");
            }

            if (!File.Exists(sourceFileName))
            {
                var message = $"Skipped deletion of {jobRunTask.ItemName}, file does not exist.";

                OnComplete(jobRunTask.JobRunTaskId, message, null, false);
            }
            else
            {
                var sw = new Stopwatch();
                sw.Start();

                var fileInfo = new FileInfo(sourceFileName);
                var fileSize = fileInfo.Length;

                var retryCount = 0;

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }

                    try
                    {
                        fileInfo.Delete();
                        break;
                    }
                    catch
                    {
                        retryCount++;

                        await Task.Delay(5000, cancellationToken);
                    }
                }
                    
                sw.Stop();

                var message = $"Completed deletion of {jobRunTask.ItemName} with {retryCount} retries ({ByteSize.FromBytes(fileSize):0.00}) in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";

                OnComplete(jobRunTask.JobRunTaskId, message, null, false);
            }
        }
        catch (Exception ex)
        {
            var message = $"Deletion of {jobRunTask.ItemName} resulted in an error: {ex.Message}";

            OnComplete(jobRunTask.JobRunTaskId, message, null, true);
        }
    }
}