using Bizcord.ChannelManagement.Application.Abstractions;
using EasyNetQ;
using Microsoft.Extensions.Options;

namespace Bizcord.ChannelManagement.Infrastructure.Messaging;

public sealed class EasyNetQMessageClient : IMessageClient
{
    private readonly IBus bus;
    private readonly RabbitMqOptions options;

    public EasyNetQMessageClient(IBus bus, IOptions<RabbitMqOptions> options)
    {
        this.bus = bus;
        this.options = options.Value;
        RabbitMqOptions.Validate(this.options);
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();

    return bus.PubSub.PublishAsync(message, _ => { }, cancellationToken);
    }

    public async Task<IAsyncDisposable> SubscribeAsync<TMessage>(
        string subscriptionId,
        Func<TMessage, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
        where TMessage : class
    {
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            throw new ArgumentException("Subscription id is required.", nameof(subscriptionId));
        }

        ArgumentNullException.ThrowIfNull(handler);
        cancellationToken.ThrowIfCancellationRequested();

        return await bus.PubSub.SubscribeAsync(subscriptionId, handler, _ => { }, cancellationToken);
    }

}