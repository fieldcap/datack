using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Data.Models.Internal;

namespace Datack.Data.Models.Data
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; }

        public Guid ServerId { get; set; }

        [ForeignKey("ServerId")]
        public Server Server { get; set; }

        public String Name { get; set; }

        public JobSettings Settings { get; set; }
    }
}
