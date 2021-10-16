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
        
        public abstract Task Run(Server server, JobRunTask jobRunTask, JobRunTask previousTask, CancellationToken cancellationToken);
        
        protected void OnProgress(Guid jobRunTaskId, String message)
        {
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
