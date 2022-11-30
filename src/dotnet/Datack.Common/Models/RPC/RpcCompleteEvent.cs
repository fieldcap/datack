namespace Datack.Common.Models.RPC;

public class RpcCompleteEvent
{
    public Guid JobRunTaskId { get; set; }
    public required String Message { get; set; }
    public String? ResultArtifact { get; set; }
    public Boolean IsError { get; set; }
}