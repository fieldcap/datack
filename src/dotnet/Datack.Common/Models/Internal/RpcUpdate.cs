using System.Collections.Generic;
using Datack.Common.Models.Data;

namespace Datack.Common.Models.Internal
{
    public class RpcUpdate
    {
        public Server Server { get; set; }

        public IList<Job> Jobs { get; set; }

        public IList<Step> Steps { get; set; }
    }
}
