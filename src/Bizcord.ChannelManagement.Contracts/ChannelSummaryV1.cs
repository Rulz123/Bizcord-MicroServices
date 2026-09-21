namespace Bizcord.ChannelManagement.Contracts;

public sealed record ChannelSummaryV1(
    Guid ChannelId,
    Guid GuildId,
    string Name,
    string? Description,
    string ChannelType,
    DateTimeOffset CreatedAtUtc,
    bool IsArchived);