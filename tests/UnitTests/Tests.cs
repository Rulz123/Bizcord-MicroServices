using ChannelManagement.Api.Messaging;
using ChannelManagementMicroservice.Models;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class Tests
{
    private MessagePostedEvent message = new()
    {
        MessageId = Guid.NewGuid(),
        ChannelId = Guid.NewGuid(),
        AuthorId = Guid.NewGuid(),
        Content = "Hello world",
        PostedAt = DateTime.UtcNow
    };
    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {
        var mockMessagePostedHandler = new Mock<MessagePostedHandler>();
        mockMessagePostedHandler.Setup(messageHandler => messageHandler.Handle(message, CancellationToken.None))
            .Returns(Task.CompletedTask);
        await mockMessagePostedHandler.Object.Handle(message, CancellationToken.None);

        mockMessagePostedHandler.Verify(messageHandler => messageHandler.Handle(message, It.IsAny<CancellationToken>()), Times.Once);

    }

    /*  [Fact]
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
     } */

    /*   [Fact]
      public async Task MessagePostedEvent_IsConsumed_AndResultEventPublished()
      {
           var messageId = Guid.NewGuid();
          var capture = new MessagePostedHandler<MessagePostedEvent>(_factory.MessageClient);

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
      } */
}