using System;
using System.Text.Json.Serialization;

namespace Datack.Agent.Models.Internal
{
    public class QueueSettings
    {
        [JsonPropertyName("createBackup")]
        public String CreateBackup { get;set; }
    }
}
