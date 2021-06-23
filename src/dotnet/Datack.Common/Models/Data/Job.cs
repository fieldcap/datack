using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }
        
        public JobSettings Settings { get; set; }

        public ICollection<Step> Steps { get; set; }
    }
}
