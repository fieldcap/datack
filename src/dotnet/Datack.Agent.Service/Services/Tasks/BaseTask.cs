using System;
using System.Threading;
using System.Threading.Tasks;
using Datack.Agent.Models;
using Datack.Common.Models.Data;

namespace Datack.Agent.Services.Tasks
{
    public abstract class BaseTask
    {
        public event EventHandler<ProgressEvent> OnProgressEvent;
        public event EventHandler<CompleteEvent> OnCompleteEvent;
        
        public abstract Task Run(JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken);
        
        private DateTime _lastProgressUpdate = DateTime.UtcNow;
        private String _lastProgressMessage;

        protected void OnProgress(Guid jobRunTaskId, String message, Boolean verbose = false)
        {
            if (verbose)
            {
                var diff = DateTime.UtcNow - _lastProgressUpdate;

                if (diff.TotalMilliseconds < 5000 || _lastProgressMessage == message)
                {
                    return;
                }

                _lastProgressUpdate = DateTime.UtcNow;
                _lastProgressMessage = message;
            }

            OnProgressEvent?.Invoke(this, new ProgressEvent
            {
                JobRunTaskId = jobRunTaskId,
                Message = message,
                IsError = false
            });
        }
        
        protected void OnComplete(Guid jobRunTaskId, String message, String resultArtifact, Boolean isError)
        {
            OnCompleteEvent?.Invoke(this, new CompleteEvent
            {
                JobRunTaskId = jobRunTaskId,
                Message = message,
                ResultArtifact = resultArtifact,
                IsError = isError
            });
        }
    }
}
