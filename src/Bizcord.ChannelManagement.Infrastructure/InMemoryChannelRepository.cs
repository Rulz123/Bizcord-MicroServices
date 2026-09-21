using System.Collections.Concurrent;
using Bizcord.ChannelManagement.Application;
using Bizcord.ChannelManagement.Domain;

namespace Bizcord.ChannelManagement.Infrastructure;

public sealed class InMemoryChannelRepository : IChannelRepository
{
    private readonly ConcurrentDictionary<Guid, Channel> channels = new();

    public Task<IReadOnlyCollection<Channel>> ListAsync(Guid? guildId, CancellationToken cancellationToken)
    {
        var result = channels.Values
            .Where(channel => guildId is null || channel.GuildId == guildId)
            .OrderBy(channel => channel.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<Channel>>(result);
    }

    public Task<Channel?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        channels.TryGetValue(id, out var channel);
        return Task.FromResult(channel);
    }

    public Task SaveAsync(Channel channel, CancellationToken cancellationToken)
    {
        channels[channel.Id] = channel;
        return Task.CompletedTask;
    }
}