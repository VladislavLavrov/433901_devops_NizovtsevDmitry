using System.Text.Json;
using System.Text.Json.Serialization;

namespace Calculator.Services;

public static class KafkaJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
