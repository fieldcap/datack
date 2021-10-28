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

        public Boolean IsActive { get; set; }

        public String Type { get; set; }

        public Int32 Parallel { get; set; }

        public Int32 MaxItemsToKeep { get; set; }

        public String Name { get; set; }

        public String Description { get; set; }

        public Int32 Order { get; set; }

        public Int32? Timeout { get; set; }
        
        public Guid? UsePreviousTaskArtifactsFromJobTaskId { get; set; }

        [ForeignKey("UsePreviousTaskArtifactsFromJobTaskId")]
        public JobTask UsePreviousTaskArtifactsFromJobTask { get; set; }

        public JobTaskSettings Settings { get; set; }

        public Guid AgentId { get; set; }

        [ForeignKey("AgentId")]
        public Agent Agent { get; set; }
    }
}
