﻿using System;
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

        private async Task Trigger()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                now = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, TimeZoneInfo.Local.GetUtcOffset(now));

                var jobs = await _jobs.GetJobs();

                foreach (var job in jobs)
                {
                    await RunSetup(job, BackupType.Full);

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
                if (!_tasks.TryGetValue(step.Type, out var task))
                {
                    throw new Exception($"Unknown step type {step.Type}");
                }

                var stepLogs = await task.Setup(job, step, backupType, jobLog.JobLogId);

                allStepLogs.AddRange(stepLogs);
            }

            var index = 0;
            foreach (var stepLogQueue in allStepLogs.GroupBy(m => m.Queue))
            {
                foreach (var stepLog in stepLogQueue)
                {
                    stepLog.Order = index;
                }

                index++;
            }

            await _stepLogs.Create(allStepLogs);
        }
    }
}