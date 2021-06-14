using System;
using System.Text.Json.Serialization;

namespace Datack.Data.Models.Internal
{
    public class ServerSettings
    {
        [JsonPropertyName("tempPath")]
        public String TempPath { get;set; }
    }
}
