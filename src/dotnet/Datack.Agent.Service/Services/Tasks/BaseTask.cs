using System;
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
        
        public abstract Task<IList<StepLog>> Setup(Job job, Step step, BackupType backupType, Guid jobLogId, CancellationToken cancellationToken);

        public abstract Task Run(List<StepLog> queue, CancellationToken cancellationToken);
        
        protected void OnStart(Guid stepLogId)
        {
            OnStartEvent?.Invoke(this, new StartEvent
            {
                StepLogId = stepLogId
            });
        }

        protected void OnProgress(Guid stepLogId, Int32 queue, String message)
        {
            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                StepLogId = stepLogId,
                Queue = queue,
                Message = message,
                IsError = false
            });
        }
        
        protected void OnComplete(Guid stepLogId, Guid jobLogId, String message, Boolean isError)
        {
            OnCompleteEvent?.Invoke(this, new CompleteEvent
            {
                StepLogId = stepLogId,
                JobLogId = jobLogId,
                Message = message,
                IsError = isError
            });
        }
    }
}
