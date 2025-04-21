using System.Collections.Concurrent;
using Datack.Agent.Services.Tasks;
using Datack.Common.Models.Data;
using Microsoft.Extensions.Logging;

namespace Datack.Agent.Services;

public class JobRunner
{
    public static readonly ConcurrentDictionary<Guid, CancellationTokenSource> RunningTasks = new();

    private static readonly SemaphoreSlim ExecuteJobRunLock = new(1, 1);

    private readonly ILogger<JobRunner> _logger;
    private readonly RpcService _rpcService;

    private readonly Dictionary<String, BaseTask> _tasks;

    public JobRunner(ILogger<JobRunner> logger,
                     RpcService rpcService,
                     CreateBackupTask createBackupTask,
                     CompressTask compressTask,
                     DeleteFileTask deleteTask,
                     DeleteS3Task deleteS3Task,
                     DownloadS3Task downloadS3Task,
                     DownloadAzureTask downloadAzureTask,
                     ExtractTask extractTask,
                     RestoreBackupTask restoreBackupTask,
                     UploadAzureTask uploadAzureTask,
                     UploadS3Task uploadS3Task)
    {
        _logger = logger;
        _rpcService = rpcService;

        _tasks = new()
        {
            {
                "createBackup", createBackupTask
            },
            {
                "compress", compressTask
            },
            {
                "deleteFile", deleteTask
            },
            {
                "deleteS3", deleteS3Task
            },
            {
                "downloadS3", downloadS3Task
            },
            {
                "downloadAzure", downloadAzureTask
            },
            {
                "extract", extractTask
            },
            {
                "restoreBackup", restoreBackupTask
            },
            {
                "uploadAzure", uploadAzureTask
            },
            {
                "uploadS3", uploadS3Task
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

                RunningTasks.TryRemove(evt.JobRunTaskId, out var _);

                var runningTasks = String.Join(", ", RunningTasks.Select(m => m.Key));
                _logger.LogDebug("Running tasks: {runningTasks}", runningTasks);

                await _rpcService.QueueComplete(evt);
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

                await _rpcService.QueueProgress(evt);
            };
        }
    }

    public async Task ExecuteJobRunTask(JobRunTask jobRunTask, JobRunTask? previousTask, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Running job run task {jobRunTaskId}", jobRunTask.JobRunTaskId);

        // Make sure only 1 process executes a job run otherwise it might run duplicate tasks.
        var receivedLockSuccesfully = await ExecuteJobRunLock.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

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
                    throw new($"Unknown task type {jobRunTask.Type}");
                }
                    
                if (RunningTasks.TryGetValue(jobRunTask.JobRunTaskId, out _))
                {
                    _logger.LogDebug("Task {jobRunTaskId} is already running ", jobRunTask.JobRunTaskId);

                    var runningTasks = String.Join(", ", RunningTasks.Select(m => m.Key));
                    _logger.LogDebug("Running tasks: {runningTasks}", runningTasks);

                    return;
                }

                _ = Task.Run(async () =>
                {
                    var timeout = jobRunTask.JobTask.Timeout ?? 3600;

                    if (jobRunTask.JobTask.Timeout <= 0)
                    {
                        timeout = 3600;
                    }

                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));

                    var runningTasks = String.Join(", ", RunningTasks.Select(m => m.Key));
                    _logger.LogDebug("Running tasks: {runningTasks}", runningTasks);

                    if (!RunningTasks.TryAdd(jobRunTask.JobRunTaskId, cancellationTokenSource))
                    {
                        _logger.LogDebug("Task {jobRunTaskId} cannot be added", jobRunTask.JobRunTaskId);
                        return;
                    }

                    await task.Run(jobRunTask, previousTask, cancellationTokenSource.Token);
                }, cancellationToken);
            }
            finally
            {
                _logger.LogDebug("Releasing lock for job run {jobRunTaskId}", jobRunTask.JobRunTaskId);
                ExecuteJobRunLock.Release();
            }
        }
        catch (Exception ex)
        {
            await _rpcService.QueueComplete(new()
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
        _logger.LogDebug("Stopping job run task {jobRunTaskId}", jobRunTaskId);

        RunningTasks.TryGetValue(jobRunTaskId, out var cancellationTokenSource);

        if (cancellationTokenSource == null)
        {
            _logger.LogDebug("Cancellation token for job task {jobRunTaskId} not found", jobRunTaskId);

            return;
        }

        cancellationTokenSource.Cancel();
    }

    public void StopAllTasks()
    {
        _logger.LogDebug("Stopping all tasks");

        foreach (var runningTask in RunningTasks)
        {
            runningTask.Value.Cancel();
        }
    }
}