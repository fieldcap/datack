using System.Text.Json;

namespace Datack.Common.Helpers
{
    public static class ObjectHelper
    {
        public static T DeepCopy<T>(this T self)
            where T : class
        {
            var serialized = JsonSerializer.Serialize(self);
            return JsonSerializer.Deserialize<T>(serialized);
        }
    }
}
