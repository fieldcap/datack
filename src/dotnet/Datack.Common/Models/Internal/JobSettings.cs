using System.Text.Json.Serialization;

namespace Datack.Common.Models.Internal;

public class JobSettings
{
    [JsonPropertyName("emailOnError")]
    public Boolean EmailOnError { get; set; }

    [JsonPropertyName("emailOnSuccess")]
    public Boolean EmailOnSuccess { get; set; }

    [JsonPropertyName("emailTo")]
    public String? EmailTo { get; set; }
}