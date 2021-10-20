using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Agent.Services.Tasks;
using Datack.Common.Models.Data;
using Microsoft.Extensions.Logging;

namespace Datack.Agent.Services
{
    public class JobRunner
    {
        private readonly ILogger<JobRunner> _logger;
        private readonly RpcService _rpcService;

        private readonly SemaphoreSlim _executeJobRunLock = new(1, 1);

        private readonly Dictionary<String, BaseTask> _tasks;

        private readonly Dictionary<Guid, CancellationTokenSource> _runningTasks = new();

        public JobRunner(ILogger<JobRunner> logger,
                         RpcService rpcService,
                         CreateBackupTask createBackupTask,
                         CompressTask compressTask,
                         DeleteTask deleteTask,
                         DeleteS3Task deleteS3Task,
                         UploadAzureTask uploadAzureTask,
                         UploadS3Task uploadS3Task)
        {
            _logger = logger;
            _rpcService = rpcService;

            _tasks = new Dictionary<String, BaseTask>
            {
                {
                    "create_backup", createBackupTask
                },
                {
                    "compress", compressTask
                },
                {
                    "delete", deleteTask
                },
                {
                    "delete_s3", deleteS3Task
                },
                {
                    "upload_azure", uploadAzureTask
                },
                {
                    "upload_s3", uploadS3Task
                }
            };

            foreach (var (_, task) in _tasks)
            {
                task.OnCompleteEvent += async (_, evt) =>
                {
                    if (evt.IsError)
                    {
                        _logger.LogError("{jobRunTaskId}: {message}", evt.JobRunTaskId, evt.Message);
                    }
                    else
                    {
                        _logger.LogInformation("{jobRunTaskId}: {message}", evt.JobRunTaskId, evt.Message);
                    }

                    _runningTasks.Remove(evt.JobRunTaskId);

                    await _rpcService.SendComplete(evt);
                };
                task.OnProgressEvent += async (_, evt) =>
                {
                    if (evt.IsError)
                    {
                        _logger.LogError("{jobRunTaskId}: {message}", evt.JobRunTaskId, evt.Message);
                    }
                    else
                    {
                        _logger.LogInformation("{jobRunTaskId}: {message}", evt.JobRunTaskId, evt.Message);
                    }

                    await _rpcService.SendProgress(evt);
                };
            }
        }

        public async Task ExecuteJobRunTask(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running job run task {jobRunTaskId}", jobRunTask.JobRunTaskId);

            // Make sure only 1 process executes a job run otherwise it might run duplicate tasks.
            var receivedLockSuccesfully = await _executeJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

            try
            {
                if (!receivedLockSuccesfully)
                {
                    // Lock timed out
                    _logger.LogError("Could not obtain executeJobRunLock within 30 seconds for job run task {jobRunTaskId}", jobRunTask.JobRunTaskId);

                    return;
                }

                _logger.LogDebug("Entering lock for job run {jobRunTaskId}", jobRunTask.JobRunTaskId);

                try
                {
                    if (jobRunTask.Type == null)
                    {
                        throw new ArgumentException("Task type cannot be null");
                    }

                    if (!_tasks.TryGetValue(jobRunTask.Type, out var task))
                    {
                        throw new Exception($"Unknown task type {jobRunTask.Type}");
                    }

                    _ = Task.Run(() =>
                    {
                        CancellationTokenSource cancellationTokenSource;
                        if (jobRunTask.JobTask.Timeout > 0)
                        {
                            cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(jobRunTask.JobTask.Timeout.Value));
                        }
                        else
                        {
                            cancellationTokenSource = new CancellationTokenSource();
                        }

                        _runningTasks.Add(jobRunTask.JobRunTaskId, cancellationTokenSource);

                        return task.Run(server, jobRunTask, previousTask, cancellationTokenSource.Token);
                    }, cancellationToken);
                }
                finally
                {
                    _logger.LogDebug("Releasing lock for job run {jobRunTaskId}", jobRunTask.JobRunTaskId);
                    _executeJobRunLock.Release();
                }
            }
            catch (Exception ex)
            {
                await _rpcService.SendComplete(new CompleteEvent
                {
                    IsError = true,
                    JobRunTaskId = jobRunTask.JobRunTaskId,
                    Message = ex.Message,
                    ResultArtifact = null
                });
            }
        }

        public void StopTask(Guid jobRunTaskId)
        {
            _logger.LogDebug($"Stopping job run task {jobRunTaskId}");

            _runningTasks.TryGetValue(jobRunTaskId, out var cancellationTokenSource);

            if (cancellationTokenSource == null)
            {
                _logger.LogDebug($"Cancellation token for job task {jobRunTaskId} not found");

                return;
            }

            cancellationTokenSource.Cancel();
        }

        public void StopAllTasks()
        {
            foreach (var runningTask in _runningTasks)
            {
                runningTask.Value.Cancel();
            }
        }
    }
}
