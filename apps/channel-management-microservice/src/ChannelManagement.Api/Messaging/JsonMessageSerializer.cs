using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ChannelManagement.Api.Messaging;

public class JsonMessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public byte[] Serialize<T>(T message) =>
    JsonSerializer.SerializeToUtf8Bytes(message, Options);

    public T Deserialize<T>(byte[] data) =>
    JsonSerializer.Deserialize<T>(data, Options)
    ?? throw new InvalidOperationException(
        $"Could not deserialize message as {typeof(T).Name}");
}