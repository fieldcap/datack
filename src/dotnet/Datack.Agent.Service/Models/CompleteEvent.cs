using System;

namespace Datack.Agent.Models
{
    public class ProgressEvent
    {
        public Guid StepLogId { get; set; }
        public Int32 Queue { get; set; }
        public Boolean IsError { get; set; }
        public String Message { get; set; }
    }
}
