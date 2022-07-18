namespace Datack.Common.Models.RPC;

public class RpcResult
{
    public RpcResult(Guid transactionId)
    {
        TransactionId = transactionId;
    }

    public Guid TransactionId { get; set; }
    public String Error { get; set; }
    public String Result { get; set; }
}