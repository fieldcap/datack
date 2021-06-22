using System;
using System.ComponentModel.DataAnnotations;

namespace Datack.Common.Models.Data
{
    public class Setting
    {
        [Key]
        public String SettingId { get; set; }

        public String Value { get; set; }

        public String Type { get; set; }
    }
}
