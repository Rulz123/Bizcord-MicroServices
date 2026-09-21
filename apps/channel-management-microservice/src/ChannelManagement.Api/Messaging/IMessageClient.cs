namespace ChannelManagement.Api.Messaging;

public interface IMessageClient
{
        Task PublishAsync<TMessage>(
        TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : class;
}