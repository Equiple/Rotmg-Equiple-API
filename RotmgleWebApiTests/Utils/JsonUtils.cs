using System.Text.Json;
using System.Text.Json.Serialization;

namespace RotmgleWebApiTests
{
    internal static class JsonUtils
    {
        public static readonly JsonSerializerOptions DefaultOptions;

        static JsonUtils()
        {
            DefaultOptions = new(JsonSerializerDefaults.Web);
            DefaultOptions.Converters.Add(new JsonStringEnumConverter());
        }
    }
}
