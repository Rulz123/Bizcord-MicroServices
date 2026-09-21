using Bizcord.ChannelManagement.Domain;

namespace Bizcord.ChannelManagement.Application;

public sealed record CreateChannelRequest(
    Guid GuildId,
    string Name,
    ChannelType Type,
    string? Topic,
    ChannelSettings? Settings);

public sealed record UpdateChannelRequest(
    string Name,
    string? Topic,
    ChannelSettings Settings);

public sealed record ChannelResponse(
    Guid Id,
    Guid GuildId,
    string Name,
    ChannelType Type,
    string? Topic,
    ChannelSettings Settings,
    ChannelStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static ChannelResponse FromChannel(Channel channel)
    {
        return new ChannelResponse(
            channel.Id,
            channel.GuildId,
            channel.Name,
            channel.Type,
            channel.Topic,
            channel.Settings,
            channel.Status,
            channel.CreatedAt,
            channel.UpdatedAt);
    }
}