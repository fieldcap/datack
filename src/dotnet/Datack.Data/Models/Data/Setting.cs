using System;
using System.ComponentModel.DataAnnotations;

namespace Datack.Data.Models.Data
{
    public class Setting
    {
        [Key]
        public String SettingId { get; set; }

        public String Value { get; set; }

        public String Type { get; set; }
    }
}
