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

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

}
