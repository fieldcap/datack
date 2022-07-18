using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data;

public class JobRun
{
    [Key]
    public Guid JobRunId { get; set; }

    public Guid JobId { get; set; }

    [ForeignKey("JobId")]
    public Job Job { get; set; }

    public DateTimeOffset Started { get; set; }

    public DateTimeOffset? Completed { get; set; }

    public Int64? RunTime { get; set; }

    public Boolean IsError { get; set; }

    public String Result { get; set; }
        
    public JobSettings Settings { get; set; }
}