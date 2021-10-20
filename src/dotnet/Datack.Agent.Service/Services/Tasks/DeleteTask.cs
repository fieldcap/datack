using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks
{
    /// <summary>
    /// This task deletes files from disk.
    /// </summary>
    public class DeleteTask : BaseTask
    {
        public override Task Run(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            try
            {
                if (previousTask == null)
                {
                    throw new Exception("No previous task found");
                }

                if (jobRunTask.Settings.Delete == null)
                {
                    throw new Exception("No settings set");
                }

                var sourceFileName = previousTask.ResultArtifact;

                OnProgress(jobRunTask.JobRunTaskId, $"Starting delete task for file {sourceFileName}");

                if (!jobRunTask.JobTask.Settings.Delete.IgnoreIfFileDoesNotExist && !File.Exists(sourceFileName))
                {
                    throw new Exception($"Source file '{sourceFileName}' not found");
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

                    fileInfo.Delete();

                    sw.Stop();

                    var message = $"Completed deletion of {jobRunTask.ItemName} ({ByteSize.FromBytes(fileSize):0.00}) in {sw.Elapsed:g} ({ByteSize.FromBytes(fileSize / sw.Elapsed.TotalSeconds):0.00}/s)";

                    OnComplete(jobRunTask.JobRunTaskId, message, null, false);
                }
            }
            catch (Exception ex)
            {
                var message = $"Deletion of {jobRunTask.ItemName} resulted in an error: {ex.Message}";

                OnComplete(jobRunTask.JobRunTaskId, message, null, true);
            }

            return Task.CompletedTask;
        }
    }
}
