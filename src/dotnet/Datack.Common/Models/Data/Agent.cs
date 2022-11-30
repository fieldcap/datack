using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datack.Common.Models.Internal;

namespace Datack.Common.Models.Data;

#nullable disable

public class Agent
{
    [Key]
    public Guid AgentId { get; set; }

    public String Name { get; set; }

    public String Description { get; set; }

    public String Key { get; set; }
        
    public AgentSettings Settings { get; set; }

    [NotMapped]
    public String Status { get; set; }

    [NotMapped]
    public String Version { get; set; }
}