namespace Bizcord.ChannelManagement.Contracts.Events;

public sealed record ChannelCreatedV1(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid ChannelId,
    Guid GuildId,
    string Name,
    string ChannelType);