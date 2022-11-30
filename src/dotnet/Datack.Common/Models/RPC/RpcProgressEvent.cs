namespace Datack.Common.Models.RPC;

public class RpcProgressEvent
{
    public Guid JobRunTaskId { get; set; }
    public Boolean IsError { get; set; }
    public required String Message { get; set; }
}