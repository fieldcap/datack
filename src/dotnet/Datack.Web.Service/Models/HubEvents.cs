using System;

namespace Datack.Web.Service.Models
{
    public class ClientConnectEvent
    {
        public String ServerKey { get; set; }
    }

    public class ClientDisconnectEvent
    {
        public String ServerKey { get; set; }
    }
}
