using System.Text.Json;
using ChannelManagement.Api.Messaging;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace UnitTests;

public class MessagePostedContractTests
{
    private const string IncomingMessage = """
        {
          "MessageId": "11111111-1111-1111-1111-111111111111",
          "ChannelId": "22222222-2222-2222-2222-222222222222",
          "AuthorId": "33333333-3333-3333-3333-333333333333",
          "Content": "Hello from the message service",
          "PostedAt": "2026-10-20T12:00:00Z"
        }
        """;

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("\"2026-10-20T12:00:01Z\"")]
    public void IncomingMessage_DeserializesProducerContract(string? processedAtJson)
    {
        using var provider = CreateProvider();
        var serializer = provider.GetRequiredService<ISerializer>();
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(IncomingMessage)!;

        if (processedAtJson is not null)
        {
            payload["ProcessedAt"] = JsonSerializer.Deserialize<JsonElement>(processedAtJson);
        }

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var message = Assert.IsType<MessagePostedEvent>(
            serializer.BytesToMessage(typeof(MessagePostedEvent), bytes));

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), message.MessageId);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), message.ChannelId);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), message.AuthorId);
        Assert.Equal("Hello from the message service", message.Content);
        Assert.Equal(new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc), message.PostedAt);
        Assert.Equal(DateTimeKind.Utc, message.PostedAt.Kind);

        if (processedAtJson is null or "null")
        {
            Assert.Null(message.ProcessedAt);
        }
        else
        {
            Assert.NotNull(message.ProcessedAt);
            Assert.Equal(new DateTime(2026, 10, 20, 12, 0, 1, DateTimeKind.Utc), message.ProcessedAt);
            Assert.Equal(DateTimeKind.Utc, message.ProcessedAt.Value.Kind);
        }
    }

    [Fact]
    public void OutgoingMessage_SerializesConsumerContract()
    {
        using var provider = CreateProvider();
        var serializer = provider.GetRequiredService<ISerializer>();
        var message = new MessagePostedEvent
        {
            MessageId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ChannelId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            AuthorId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Content = "Hello from the message service",
            PostedAt = new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc),
            ProcessedAt = new DateTime(2026, 10, 20, 12, 0, 1, DateTimeKind.Utc)
        };

        using var bytes = serializer.MessageToBytes(typeof(MessagePostedEvent), message);
        using var document = JsonDocument.Parse(bytes.Memory);
        var payload = document.RootElement;

        Assert.Equal(
            new[] { "AuthorId", "ChannelId", "Content", "MessageId", "PostedAt", "ProcessedAt" },
            payload.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal("11111111-1111-1111-1111-111111111111", payload.GetProperty("MessageId").GetString());
        Assert.Equal("22222222-2222-2222-2222-222222222222", payload.GetProperty("ChannelId").GetString());
        Assert.Equal("33333333-3333-3333-3333-333333333333", payload.GetProperty("AuthorId").GetString());
        Assert.Equal("Hello from the message service", payload.GetProperty("Content").GetString());
        Assert.Equal("2026-10-20T12:00:00Z", payload.GetProperty("PostedAt").GetString());
        Assert.Equal("2026-10-20T12:00:01Z", payload.GetProperty("ProcessedAt").GetString());
    }

    [Fact]
    public void IncomingMessage_AllowsAdditionalProducerFields()
    {
        using var provider = CreateProvider();
        var serializer = provider.GetRequiredService<ISerializer>();
        var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(IncomingMessage)!;
        payload["FutureField"] = JsonSerializer.SerializeToElement("additional producer data");

        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var message = Assert.IsType<MessagePostedEvent>(
            serializer.BytesToMessage(typeof(MessagePostedEvent), bytes));

        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), message.MessageId);
        Assert.Equal("Hello from the message service", message.Content);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddMessageClient("host=localhost;username=guest;password=guest");
        return services.BuildServiceProvider();
    }
}