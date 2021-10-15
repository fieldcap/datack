﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Enums;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks
{
    public abstract class BaseTask
    {
        public event EventHandler<StartEvent> OnStartEvent;
        public event EventHandler<ProgressEvent> OnProgressEvent;
        public event EventHandler<CompleteEvent> OnCompleteEvent;
        
        public abstract Task<IList<JobRunTask>> Setup(Job job, JobTask jobTask, BackupType backupType, Guid jobRunId, CancellationToken cancellationToken);

        public abstract Task Run(JobRunTask jobRunTask, CancellationToken cancellationToken);
        
        protected void OnStart(Guid jobRunTaskId)
        {
            OnStartEvent?.Invoke(this, new StartEvent
            {
                JobRunTaskId = jobRunTaskId
            });
        }

        protected void OnProgress(Guid jobRunTaskId, String message)
        {
            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                JobRunTaskId = jobRunTaskId,
                Message = message,
                IsError = false
            });
        }
        
        protected void OnComplete(Guid jobRunTaskId, Guid jobRunId, String message, Boolean isError)
        {
            OnCompleteEvent?.Invoke(this, new CompleteEvent
            {
                JobRunTaskId = jobRunTaskId,
                JobRunId = jobRunId,
                Message = message,
                IsError = isError
            });
        }
    }
}
