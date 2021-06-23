using System;

namespace Datack.Common.Models.RPC
{
    public class RpcRequest
    {
        public Guid TransactionId { get; set; }
        public String Request { get; set; }
        public String Payload { get; set; }
    }
}
