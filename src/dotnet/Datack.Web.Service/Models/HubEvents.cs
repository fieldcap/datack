using System;
using System.Collections.Generic;

namespace Datack.Web.Service.Models
{
    public class ClientConnectEvent
    {
        public String AgentKey { get; set; }
        public IList<Guid> RunningJobRunTaskIds { get; set; }
    }

    public class ClientDisconnectEvent
    {
        public String AgentKey { get; set; }
    }
}
