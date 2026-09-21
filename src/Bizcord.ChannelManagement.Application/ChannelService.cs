using Bizcord.ChannelManagement.Application.Abstractions;
using Bizcord.ChannelManagement.Domain;

namespace Bizcord.ChannelManagement.Application;

public sealed class ChannelService
{
    private readonly IChannelRepository repository;
    private readonly IMessageClient messageClient;
    private readonly TimeProvider timeProvider;

    public ChannelService(IChannelRepository repository, IMessageClient messageClient, TimeProvider timeProvider)
    {
        this.repository = repository;
        this.messageClient = messageClient;
        this.timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<ChannelResponse>> GetChannelsAsync(Guid? guildId, CancellationToken cancellationToken)
    {
        var channels = await repository.ListAsync(guildId, cancellationToken);
        return channels.Select(ChannelResponse.FromChannel).ToArray();
    }

    public async Task<ChannelResponse?> GetChannelAsync(Guid id, CancellationToken cancellationToken)
    {
        var channel = await repository.GetAsync(id, cancellationToken);
        return channel is null ? null : ChannelResponse.FromChannel(channel);
    }

    public async Task<ChannelResponse> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var channel = Channel.Create(request.GuildId, request.Name, request.Type, request.Topic, request.Settings, now);

        await repository.SaveAsync(channel, cancellationToken);
        var integrationEvent = ChannelContractMapper.ToChannelCreatedEvent(channel, Guid.NewGuid(), timeProvider.GetUtcNow());
        await messageClient.PublishAsync(integrationEvent, cancellationToken);

        return ChannelResponse.FromChannel(channel);
    }

    public async Task<ChannelResponse?> UpdateChannelAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken)
    {
        var channel = await repository.GetAsync(id, cancellationToken);
        if (channel is null)
        {
            return null;
        }

        channel.Update(request.Name, request.Topic, request.Settings, timeProvider.GetUtcNow());
        await repository.SaveAsync(channel, cancellationToken);

        return ChannelResponse.FromChannel(channel);
    }

    public async Task<bool> ArchiveChannelAsync(Guid id, CancellationToken cancellationToken)
    {
        var channel = await repository.GetAsync(id, cancellationToken);
        if (channel is null)
        {
            return false;
        }

        channel.Archive(timeProvider.GetUtcNow());
        await repository.SaveAsync(channel, cancellationToken);

        return true;
    }
}