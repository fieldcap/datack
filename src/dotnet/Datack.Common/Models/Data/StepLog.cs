using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class StepLog
    {
        [Key]
        public Guid StepLogId { get; set; }

        public Guid StepId { get; set; }

        [ForeignKey("StepId")]
        public Step Step { get; set; }

        public Guid JobLogId { get; set; }

        [ForeignKey("JobLogId")]
        public JobLog JobLog { get; set; }

        public DateTimeOffset Started { get; set; }

        public DateTimeOffset? Completed { get; set; }

        public String DatabaseName { get; set; }

        public Int32 Order { get; set; }

        public Int32 Queue { get; set; }

        public String Type { get; set; }

        public Boolean IsError { get; set; }

        public String Result { get; set; }

        public StepSettings Settings { get; set; }
    }
}
