namespace ChannelManagement.Api.Messaging;

public interface IMessageClient
{
        Task PublishAsync<TMessage>(
                        TMessage message,
                        CancellationToken cancellationToken = default)
                        where TMessage : class;

        Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> handler,
                        CancellationToken ct = default) where T : class;
}