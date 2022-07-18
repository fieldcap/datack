using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Datack.Common.Helpers;
using Datack.Common.Models.Internal;

namespace Datack.Web.Service.Middleware;

public class JsonProtectedConverter : JsonConverter<JobTaskSettings>
{
    public override JobTaskSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<JobTaskSettings>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, JobTaskSettings jobTaskSettings, JsonSerializerOptions options)
    {
        if (jobTaskSettings == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            var properties = typeof(JobTaskSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var property in properties)
            {
                var settings = property.GetValue(jobTaskSettings);

                if (settings == null)
                {
                    continue;
                }

                var settingKeys = settings.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var settingKey in settingKeys)
                {
                    if (!Attribute.IsDefined(settingKey, typeof(ProtectedAttribute)))
                    {
                        continue;
                    }

                    var settingValue = settingKey.GetValue(settings);

                    if (settingValue == null)
                    {
                        continue;
                    }
                        
                    settingKey.SetValue(settings, "******");
                }
            }

            JsonSerializer.Serialize(writer, jobTaskSettings);
        }
    }
}