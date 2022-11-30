namespace Datack.Web.Service.Models;

public class ClientConnectEvent
{
    public required String AgentKey { get; set; }
}

public class ClientDisconnectEvent
{
    public required String AgentKey { get; set; }
}