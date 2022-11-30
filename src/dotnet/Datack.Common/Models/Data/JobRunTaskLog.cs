using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Datack.Common.Models.Data;

#nullable disable

public class JobRunTaskLog
{
    [Key]
    public Int64 JobRunTaskLogId { get; set; }

    public Guid JobRunTaskId { get; set; }
        
    [ForeignKey("JobRunTaskId")]
    public JobRunTask JobRunTask { get; set; }

    public DateTimeOffset DateTime { get; set; }

    public Boolean IsError { get; set; }

    public String Message { get; set; }
}