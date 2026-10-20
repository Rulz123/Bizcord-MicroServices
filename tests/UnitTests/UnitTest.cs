using ChannelManagement.Api.Context;
using ChannelManagement.Api.Messaging;
using ChannelManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace UnitTests;

public class Tests
{
    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {
        var options = new DbContextOptionsBuilder<ChannelDbContext>()
            .UseSqlServer("Server=tcp:127.0.0.1,1433;Database=ChannelManagementDB;User Id=sa;Password=Password123!;TrustServerCertificate=True")
            .Options;

        await using var dbContext = new ChannelDbContext(options);
        var messageClient = new Mock<IMessageClient>();
        messageClient.Setup(client => client.PublishAsync(
                It.IsAny<MessagePostedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var channel = new Channels
        {
            id = Guid.NewGuid(),
            serverId = Guid.NewGuid(),
            newMessageCount = 0,
            categories = "TestCategory",
            description = "TestDescription",
            title = "TestTitle",
            type = "Text",
            userroles = "Member",
            visibility = true,
            writerights = "Member"
        };
        dbContext.Channels.Add(channel);
        await dbContext.SaveChangesAsync();

        var message = new MessagePostedEvent
        {
            MessageId = Guid.NewGuid(),
            ChannelId = channel.id,
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        };
        var handler = new MessagePostedHandler(messageClient.Object, dbContext);

        await handler.Handle(message, CancellationToken.None);

        dbContext.ChangeTracker.Clear();
        var saved = await dbContext.Channels.SingleAsync(c => c.id == channel.id);
        Assert.Equal(1, saved.newMessageCount);
        messageClient.Verify(client => client.PublishAsync(
                It.Is<MessagePostedEvent>(published =>
                    published.MessageId == message.MessageId &&
                    published.ChannelId == message.ChannelId &&
                    published.AuthorId == message.AuthorId &&
                    published.Content == message.Content &&
                    published.PostedAt == message.PostedAt &&
                    published.ProcessedAt.HasValue),
                It.IsAny<CancellationToken>()),
            Times.Once);       
    }
}
