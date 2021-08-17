using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datack.Common.Models.Data
{
    public class StepLogMessage
    {
        [Key]
        public Int64 StepLogMessageId { get; set; }

        public Guid StepLogId { get; set; }

        [ForeignKey("StepLogId")]
        public StepLog StepLog { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public Boolean IsError { get; set; }

        public String Message { get; set; }
    }
}
