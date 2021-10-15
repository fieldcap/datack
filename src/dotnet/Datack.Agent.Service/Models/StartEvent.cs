using System;

namespace Datack.Agent.Models
{
    public class StartEvent
    {
        public Guid JobRunTaskId { get; set; }
        public Boolean IsError { get; set; }
    }
}
