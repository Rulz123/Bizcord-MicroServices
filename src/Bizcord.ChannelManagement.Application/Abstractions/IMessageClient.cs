namespace Bizcord.ChannelManagement.Application.Abstractions;

public interface IMessageClient
{
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class;

    Task<IAsyncDisposable> SubscribeAsync<TMessage>(
        string subscriptionId,
        Func<TMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TMessage : class;
}