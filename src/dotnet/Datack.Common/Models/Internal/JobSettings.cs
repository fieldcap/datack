using System;
using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal
{
    public class JobSettings
    {
        [JsonPropertyName("cronFull")]
        public String CronFull { get;set; }

        [JsonPropertyName("cronDiff")]
        public String CronDiff { get;set; }

        [JsonPropertyName("cronLog")]
        public String CronLog { get;set; }
    }
}
