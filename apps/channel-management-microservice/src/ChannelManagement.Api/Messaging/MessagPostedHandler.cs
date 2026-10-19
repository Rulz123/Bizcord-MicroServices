using ChannelManagement.Api.Messaging;

public class MessagePostedHandler : IMessageHandler<MessagePostedEvent>
{
    private readonly IMessageClient _messageClient;

    public MessagePostedHandler(IMessageClient messageClient)
        => _messageClient = messageClient;

    public async Task Handle(MessagePostedEvent message, CancellationToken cancellationToken)
    {
        // Do something domain-specific
        

        // Then publish your result, include MessageId for traceability
        await _messageClient.PublishAsync(new MessagePostedEvent
        {
            MessageId = message.MessageId,
            ChannelId = message.ChannelId,
            AuthorId = message.AuthorId,
            Content = message.Content,
            PostedAt = message.PostedAt,
            ProcessedAt = DateTime.UtcNow
        });
    }
}

