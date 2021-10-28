using System;

namespace Datack.Web.Service.Models
{
    public class ClientConnectEvent
    {
        public String AgentKey { get; set; }
        public Boolean HasPendingEvents { get; set; }
    }

    public class ClientDisconnectEvent
    {
        public String AgentKey { get; set; }
    }
}
