using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Services.Tasks;
using Datack.Common.Enums;
using Datack.Common.Helpers;
using Datack.Common.Models.Data;
using Microsoft.Extensions.Logging;

namespace Datack.Agent.Services
{
    public class JobScheduler
    {
        private readonly ILogger<JobScheduler> _logger;
        private readonly Jobs _jobs;
        private readonly JobRuns _jobRuns;
        private readonly JobTasks _jobTasks;
        private readonly JobRunTasks _jobRunTasks;
        private readonly JobRunTaskLogs _jobRunTaskLogs;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private readonly Dictionary<String, BaseTask> _tasks;

        public JobScheduler(ILogger<JobScheduler> logger, 
                            Jobs jobs,
                            JobRuns jobRuns,
                            JobTasks jobTasks,
                            JobRunTasks jobRunTasks,
                            JobRunTaskLogs jobRunTaskLogs,
                            CreateBackupTask createBackupTask)
        {
            _logger = logger;
            _jobs = jobs;
            _jobRuns = jobRuns;
            _jobTasks = jobTasks;
            _jobRunTasks = jobRunTasks;
            _jobRunTaskLogs = jobRunTaskLogs;

            _logger.LogTrace("Constructor");

            _tasks = new Dictionary<String, BaseTask>
            {
                { "create_backup", createBackupTask }
            };
        }

        public void Start()
        {
            _logger.LogTrace("Constructor");

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            Task.Run(Trigger, _cancellationToken);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task Run(Guid jobId, BackupType backupType)
        {
            var job = await _jobs.GetById(jobId);

            if (job == null)
            {
                throw new Exception($"Job with ID {jobId} not found");
            }

            await SetupJobRun(job, backupType);
        }

        private async Task Trigger()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

                var jobs = await _jobs.GetJobs();

                foreach (var job in jobs)
                {
                    var backupType = CronHelper.GetNextOccurrence(job.Settings.CronFull, job.Settings.CronDiff, job.Settings.CronLog, now);

                    if (backupType != null)
                    {
                        await SetupJobRun(job, backupType.Value);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(60), _cancellationToken);
            }
        }

        private readonly SemaphoreSlim _runSetupLock = new SemaphoreSlim(1, 1);
        private async Task SetupJobRun(Job job, BackupType backupType)
        {
            var receivedLockSuccesfully = await _runSetupLock.WaitAsync(TimeSpan.FromSeconds(30), _cancellationToken);

            if (!receivedLockSuccesfully)
            {
                // Lock timed out
                return;
            }

            try
            {
                var jobRun = new JobRun
                {
                    JobRunId = Guid.NewGuid(),
                    JobId = job.JobId,
                    BackupType = backupType,
                    Started = DateTimeOffset.Now,
                    IsError = false,
                    Result = null
                };

                await _jobRuns.Create(jobRun);

                try
                {
                    var runningTasks = await _jobRuns.GetRunning(job.JobId);

                    runningTasks = runningTasks.Where(m => m.JobRunId != jobRun.JobRunId).ToList();

                    if (runningTasks.Count > 0)
                    {
                        throw new Exception($"Cannot start job {job.Name}, there is already another job still running ({runningTasks})");
                    }

                    var jobTasks = await _jobTasks.GetForJob(job.JobId);

                    var allJobRunTasks = new List<JobRunTask>();
                    foreach (var jobTask in jobTasks)
                    {
                        var task = GetTask(jobTask.Type);

                        var jobRunTasks = await task.Setup(job, jobTask, backupType, jobRun.JobRunId, _cancellationToken);

                        allJobRunTasks.AddRange(jobRunTasks);
                    }

                    foreach (var jobRunTask in allJobRunTasks)
                    {
                        var jobTask = jobTasks.First(m => m.JobTaskId == jobRunTask.JobTaskId);

                        jobRunTask.TaskOrder = jobTask.Order;
                    }

                    await _jobRunTasks.Create(allJobRunTasks);
                }
                catch (Exception ex)
                {
                    jobRun.Result = ex.Message;
                    jobRun.IsError = true;
                    jobRun.Completed = DateTimeOffset.Now;

                    await ExecuteJobRun(jobRun.JobRunId);
                }
                finally
                {
                    await _jobRuns.Update(jobRun);
                }
            }
            finally
            {
                _runSetupLock.Release();
            }
        }

        private async Task ExecuteJobRun(Guid jobRunId)
        {
            var jobRun = await _jobRuns.GetById(jobRunId);

            if (jobRun == null)
            {
                return;
            }

            if (jobRun.Completed.HasValue)
            {
                return;
            }

            var jobRunTasks = await _jobRunTasks.GetByJobRunTaskId(jobRun.JobRunId);

            var pendingJobRunTasks = jobRunTasks.Where(m => m.Completed == null)
                                                .OrderBy(m => m.TaskOrder)
                                                .ThenBy(m => m.ItemOrder)
                                                .ToList();

            if (!pendingJobRunTasks.Any())
            {
                await _jobRuns.UpdateComplete(jobRunId);
                return;
            }

            var nextJobRunTask = pendingJobRunTasks.First();

            var task = GetTask(nextJobRunTask.Type);

            task.OnProgressEvent += (_, args) => AddTaskMessage(args.JobRunTaskId, args.Message, args.IsError);
            task.OnStartEvent += (_, args) => StartTask(args.JobRunTaskId);
            task.OnCompleteEvent += (_, args) => CompleteTask(args.JobRunTaskId, args.JobRunId, args.Message, args.IsError);

            _ = Task.Run(() => task.Run(nextJobRunTask, _cancellationToken), _cancellationToken);
        }

        private async void AddTaskMessage(Guid jobRunTaskId, String message, Boolean isError)
        {
            if (isError)
            {
                _logger.LogError($"{jobRunTaskId}: {message}");
            }
            else
            {
                _logger.LogInformation($"{jobRunTaskId}: {message}");
            }

            await _jobRunTaskLogs.Add(new JobRunTaskLog
            {
                JobRunTaskId = jobRunTaskId,
                DateTime = DateTimeOffset.Now,
                Message = message,
                IsError = isError
            });
        }

        private async void StartTask(Guid jobRunTaskId)
        {
            _logger.LogInformation($"{jobRunTaskId}: Started");
            
            await _jobRunTasks.UpdateStarted(jobRunTaskId);
        }

        private async void CompleteTask(Guid jobRunTaskId, Guid jobRunId, String message, Boolean isError)
        {
            if (isError)
            {
                _logger.LogError($"{jobRunTaskId}: {message}");
            }
            else
            {
                _logger.LogInformation($"{jobRunTaskId}: {message}");
            }

            await _jobRunTasks.UpdateCompleted(jobRunTaskId, message, isError);

            ExecuteJobRun(jobRunTaskId);
        }

        private BaseTask GetTask(String type)
        {
            if (type == null)
            {
                throw new ArgumentException("Type cannot be null");
            }

            if (!_tasks.TryGetValue(type, out var task))
            {
                throw new Exception($"Unknown task type {type}");
            }

            return task;
        }
    }
}
