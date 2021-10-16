using System;

namespace Datack.Common.Models.RPC
{
    public class RpcProgressEvent
    {
        public Guid JobRunTaskId { get; set; }
        public Boolean IsError { get; set; }
        public String Message { get; set; }
    }
}
