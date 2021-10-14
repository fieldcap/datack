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
        private readonly JobLogs _jobLogs;
        private readonly Steps _steps;
        private readonly StepLogs _stepLogs;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private readonly Dictionary<String, BaseTask> _tasks;

        public JobScheduler(ILogger<JobScheduler> logger, 
                            Jobs jobs,
                            JobLogs jobLogs,
                            Steps steps,
                            StepLogs stepLogs,
                            CreateBackupTask createBackupTask)
        {
            _logger = logger;
            _jobs = jobs;
            _jobLogs = jobLogs;
            _steps = steps;
            _stepLogs = stepLogs;

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

            if (job != null)
            {
                throw new Exception($"Job with ID {jobId} not found");
            }


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
                        await RunSetup(job, backupType.Value);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(60), _cancellationToken);
            }
        }

        private async Task RunSetup(Job job, BackupType backupType)
        {
            var jobLog = new JobLog
            {
                JobLogId = Guid.NewGuid(),
                JobId = job.JobId,
                BackupType = backupType,
                Started = DateTimeOffset.Now,
                IsError = false,
                Result = null
            };

            await _jobLogs.Create(jobLog);

            try
            {
                var runningTasks = await _jobLogs.GetRunning(job.JobId);

                runningTasks = runningTasks.Where(m => m.JobLogId != jobLog.JobLogId).ToList();

                if (runningTasks.Count > 0)
                {
                    throw new Exception($"Job {job.Name} is already running");
                }

                await CreateSteps(job, backupType, jobLog);
            }
            catch (Exception ex)
            {
                jobLog.Result = ex.Message;
                jobLog.IsError = true;
                jobLog.Completed = DateTimeOffset.Now;
            }
            finally
            {
                await _jobLogs.Update(jobLog);
            }
        }

        private async Task CreateSteps(Job job, BackupType backupType, JobLog jobLog)
        {
            var steps = await _steps.GetForJob(job.JobId);

            var allStepLogs = new List<StepLog>();
            foreach (var step in steps)
            {
                var task = GetTask(step.Type);

                var stepLogs = await task.Setup(job, step, backupType, jobLog.JobLogId);

                allStepLogs.AddRange(stepLogs);
            }

            var index = 0;
            foreach (var stepLogQueue in allStepLogs.GroupBy(m => m.StepId))
            {
                foreach (var stepLog in stepLogQueue)
                {
                    stepLog.Order = index;
                }

                index++;
            }

            await _stepLogs.Create(allStepLogs);

            await RunNext(jobLog.JobLogId);
        }

        private async Task RunNext(Guid jobLogId)
        {
            var jobLog = await _jobLogs.GetById(jobLogId);

            if (jobLog == null)
            {
                throw new Exception($"JobLog with ID {jobLogId} not found");
            }

            if (jobLog.Completed.HasValue)
            {
                throw new Exception($"JobLog with ID {jobLogId} is already completed");
            }

            var stepLogs = await _stepLogs.GetByJobLogId(jobLog.JobLogId);

            var queue = stepLogs.Where(m => m.Completed == null)
                                .OrderBy(m => m.Order)
                                .ThenBy(m => m.Queue)
                                .GroupBy(m => m.Order)
                                .ToList();

            if (!queue.Any())
            {
                // Update job log as completed.
                return;
            }

            var nextStep = queue.First();

            var task = GetTask(nextStep.First().Type);

            var nextQueue = nextStep.GroupBy(m => m.Queue);

            task.OnProgressEvent += (_, args) =>
            {
                
            };

            foreach (var steps in nextQueue)
            {
                _ = Task.Run(() => task.Run(steps.ToList()), _cancellationToken);
            }
        }

        private BaseTask GetTask(String type)
        {
            if (type == null)
            {
                throw new ArgumentException("Type cannot be null");
            }

            if (!_tasks.TryGetValue(type, out var task))
            {
                throw new Exception($"Unknown step type {type}");
            }

            return task;
        }
    }
}
