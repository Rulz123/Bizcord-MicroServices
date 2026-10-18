namespace ChannelManagement.Api.Messaging;

public interface IMessageClient
{
    Task PublishAsync<TMessage>(
                    TMessage message,
                    CancellationToken cancellationToken = default)
                    where TMessage : class;

    Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> handler,
                    CancellationToken ct = default) where T : class;

    Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
        where TRequest : class where TResponse : class;

    Task RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler, CancellationToken ct = default)
        where TRequest : class where TResponse : class;
}