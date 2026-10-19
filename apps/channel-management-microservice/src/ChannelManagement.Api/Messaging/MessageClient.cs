using EasyNetQ;

namespace ChannelManagement.Api.Messaging;

public class MessageClient(IBus bus) : IMessageClient
{
    public Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class
        => bus.PubSub.PublishAsync(message, ct);

    public async Task SubscribeAsync<T>(string subscriptionId, Func<T, Task> handler,
                                        CancellationToken ct = default) where T : class
            => await bus.PubSub.SubscribeAsync<T>
            (subscriptionId,
            (msg, _) => handler(msg),
            _ => { },
            ct);

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, CancellationToken ct = default)
        where TRequest : class where TResponse : class
            => await bus.Rpc.RequestAsync<TRequest, TResponse>(request, _ => { }, ct);

    public async Task RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler, CancellationToken ct = default)
        where TRequest : class where TResponse : class
            => await bus.Rpc.RespondAsync<TRequest, TResponse>((req, _) => handler(req), _ => { }, ct);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

}
