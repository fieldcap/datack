using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Enums;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks
{
    public abstract class BaseTask
    {
        public event EventHandler<ProgressEvent> OnProgressEvent;
        public event EventHandler<TimeSpan> OnCompleteEvent;
        public event EventHandler<Exception> OnErrorEvent;
        
        private Int32 _currentProgress;
        private Int32 _maxProgress;

        public async Task<IList<StepLog>> Setup(Job job, Step step, BackupType backupType, Guid jobLogId)
        {
            OnProgress(0, 0);
            return await Run(job, step, backupType, jobLogId);
        }

        protected abstract Task<IList<StepLog>> Run(Job job, Step step, BackupType backupType, Guid jobLogId);

        protected void OnProgress(Int32 progress, Int32 max)
        {
            _currentProgress = progress;
            _maxProgress = max;

            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                Current = _currentProgress,
                Max = _maxProgress,
                Message = null
            });
        }
        
        protected void OnProgress(Int32 progress)
        {
            _currentProgress = progress;

            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                Current = _currentProgress,
                Max = _maxProgress,
                Message = null
            });
        }

        protected void OnProgress(String message)
        {
            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                Current = _currentProgress,
                Max = _maxProgress,
                Message = message
            });
        }
    }
}
