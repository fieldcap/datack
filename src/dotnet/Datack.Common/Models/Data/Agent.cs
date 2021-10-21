using System;
using System.ComponentModel.DataAnnotations;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class Agent
    {
        [Key]
        public Guid AgentId { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public String Key { get; set; }
        
        public AgentSettings Settings { get; set; }
    }
}
