

using ChannelManagement.Api.Messaging;
using FluentAssertions;

namespace UnitTests;

public class Tests
{
    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {
        var messageId = Guid.NewGuid();
        var client = new FakeMessageClient();
        var handler = new MessagePostedHandler(client);

        await handler.Handle(new MessagePostedEvent
        {
            MessageId = messageId,
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var published = client.SinglePublished<MessagePostedEvent>();
        published.Should().NotBeNull();
        published!.MessageId.Should().Be(messageId);
    }

    [Fact]
    public async Task Handler_CanConsume_MinimumValidContract()
    {
        var client = new FakeMessageClient();
        var handler = new MessagePostedHandler(client);

        // The minimum fields your handler depends on
        Func<Task> act = () => handler.Handle(new MessagePostedEvent
        {
            MessageId = Guid.NewGuid(),
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        }, CancellationToken.None);

        await act.Should().NotThrowAsync();
        client.Published.Should().HaveCount(1);
    }

    [Fact]
    public async Task MessagePostedEvent_IsConsumed_AndResultEventPublished()
    {
        var messageId = Guid.NewGuid();
        var capture = new MessageCapture<MessagePostedEvent>(_factory.MessageClient);

        // Publish directly to the broker: no HTTP, tests the message path only
        await _factory.MessageClient.PublishAsync(new MessagePostedEvent
        {
            MessageId = messageId,
            ChannelId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        });

        var result = await capture.WaitForMessageAsync(TimeSpan.FromSeconds(5));
        result.MessageId.Should().Be(messageId);
    }
}