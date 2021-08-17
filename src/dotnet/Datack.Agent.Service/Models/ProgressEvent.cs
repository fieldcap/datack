using System;

namespace Datack.Agent.Models
{
    public class ProgressEvent
    {
        public Int32 Current { get; set; }
        public Int32 Max { get; set; }
        public String Message { get; set; }
    }
}
