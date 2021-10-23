using System;

namespace Datack.Agent.Models
{
    public class ProgressEvent
    {
        public Guid JobRunTaskId { get; set; }
        public Boolean IsError { get; set; }
        public String Message { get; set; }
    }
}
