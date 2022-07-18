using System.ComponentModel.DataAnnotations;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data;

public class Job
{
    [Key]
    public Guid JobId { get; set; }

    public String Name { get; set; }

    public Boolean IsActive { get; set; }

    public String Group { get; set; }

    public Int32 Priority { get; set; }

    public String Description { get; set; }

    public String Cron { get;set; }

    public Int32?  DeleteLogsTimeSpanAmount { get; set; }

    public String DeleteLogsTimeSpanType { get; set; }
        
    public JobSettings Settings { get; set; }
}