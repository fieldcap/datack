namespace Datack.Common.Models.RPC;

public class RpcRequest
{
    public Guid TransactionId { get; set; }
    public required String Request { get; set; }
    public required String Payload { get; set; }
}