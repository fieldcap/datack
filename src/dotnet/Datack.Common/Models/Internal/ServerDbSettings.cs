using System;
using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal
{
    public class ServerDbSettings
    {
        [JsonPropertyName("server")]
        public String Server { get;set; }

        [JsonPropertyName("userName")]
        public String UserName { get;set; }

        [JsonPropertyName("password")]
        public String Password { get;set; }
    }
}
