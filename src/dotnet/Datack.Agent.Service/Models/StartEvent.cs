using System;

namespace Datack.Agent.Models
{
    public class StartEvent
    {
        public Guid StepLogId { get; set; }
        public Boolean IsError { get; set; }
    }
}
