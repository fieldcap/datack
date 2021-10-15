using System;

namespace Datack.Agent.Models
{
    public class CompleteEvent
    {
        public Guid JobRunTaskId { get; set; }
        public Guid JobRunId { get; set; }
        public String Message { get; set; }
        public Boolean IsError { get; set; }
    }
}
