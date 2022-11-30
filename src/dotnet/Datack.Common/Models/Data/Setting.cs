using System.ComponentModel.DataAnnotations;

namespace Datack.Common.Models.Data;

#nullable disable

public class Setting
{
    [Key]
    public String SettingId { get; set; }

    public String Value { get; set; }

    public Boolean Secure { get; set; }
}