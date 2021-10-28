using System;
using System.Collections.Generic;
using System.Linq;
using Datack.Common.Models.Data;

namespace Datack.Web.Service.Exceptions
{
    public class AlreadyStartedException : Exception
    {
        public AlreadyStartedException(Job job, List<JobRun> runningTasks) : base(GetMessage(job, runningTasks))
        {
        }

        private static String GetMessage(Job job, List<JobRun> runningTasks)
        {
            var runningTasksList = String.Join(", ", runningTasks.Select(m => $"{m.JobRunId} (started {m.Started:g})"));
            return $"Cannot start job {job.Name} for group {job.Group}, there is already another job still running ({runningTasksList})";
        }
    }
}
