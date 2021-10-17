using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data
{
    public class JobRunTask
    {
        [Key]
        public Guid JobRunTaskId { get; set; }

        public Guid JobTaskId { get; set; }

        [ForeignKey("JobTaskId")]
        public JobTask JobTask { get; set; }

        public Guid JobRunId { get; set; }

        [ForeignKey("JobRunId")]
        public JobRun JobRun { get; set; }

        public DateTimeOffset? Started { get; set; }

        public DateTimeOffset? Completed { get; set; }

        public Int64? RunTime { get; set; }

        public String Type { get; set; }

        public String ItemName { get; set; }

        public Int32 TaskOrder { get; set; }

        public Int32 ItemOrder { get; set; }

        public Boolean IsError { get; set; }

        public String Result { get; set; }

        public String ResultArtifact { get; set; }

        public JobTaskSettings Settings { get; set; }
    }
}
