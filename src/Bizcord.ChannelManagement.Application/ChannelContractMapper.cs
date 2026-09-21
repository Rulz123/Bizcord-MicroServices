using Bizcord.ChannelManagement.Contracts;
using Bizcord.ChannelManagement.Contracts.Events;
using Bizcord.ChannelManagement.Domain;

namespace Bizcord.ChannelManagement.Application;

public static class ChannelContractMapper
{
    public static ChannelSummaryV1 ToContract(ChannelResponse channel)
    {
        return new ChannelSummaryV1(
            channel.Id,
            channel.GuildId,
            channel.Name,
            channel.Topic,
            channel.Type.ToString(),
            channel.CreatedAt,
            channel.Status == ChannelStatus.Archived);
    }

    public static ChannelCreatedV1 ToChannelCreatedEvent(Channel channel, Guid eventId, DateTimeOffset occurredAtUtc)
    {
        return new ChannelCreatedV1(
            eventId,
            occurredAtUtc,
            channel.Id,
            channel.GuildId,
            channel.Name,
            channel.Type.ToString());
    }
}