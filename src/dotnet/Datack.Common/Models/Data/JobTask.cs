using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class JobTask
    {
        [Key]
        public Guid JobTaskId { get; set; }

        public Guid JobId { get; set; }

        [ForeignKey("JobId")]
        public Job Job { get; set; }

        public String Type { get; set; }

        public Int32 Parallel { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public Int32 Order { get; set; }

        public JobTaskSettings Settings { get; set; }

        public Guid ServerId { get; set; }

        [ForeignKey("ServerId")]
        public Server Server { get; set; }
    }
}
