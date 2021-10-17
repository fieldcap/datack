using System;
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

        public String Cron { get;set; }
        
        public JobSettings Settings { get; set; }
    }
}
