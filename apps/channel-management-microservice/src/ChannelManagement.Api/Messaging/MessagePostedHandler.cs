using ChannelManagement.Api.Messaging;
using ChannelManagement.Api.Context;

public class MessagePostedHandler : IMessageHandler<MessagePostedEvent>
{
    private readonly IMessageClient _messageClient;

    private readonly ChannelDbContext _dbContext;


    public MessagePostedHandler(IMessageClient messageClient, ChannelDbContext dbContext)
    {
        _messageClient = messageClient;
        _dbContext = dbContext;
    }

    public async Task Handle(MessagePostedEvent message, CancellationToken cancellationToken)
    {
        // Do something domain-specific
        var channel = _dbContext.Channels.FirstOrDefault(c => c.id.Equals(message.ChannelId));
        if(channel == null)
        {
            throw new Exception($"Channel with ID {message.ChannelId} not found.");
        }
        channel.newMessageCount++;
        await _dbContext.SaveChangesAsync(cancellationToken);

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

