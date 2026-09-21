namespace Bizcord.ChannelManagement.Domain;

public sealed class Channel
{
    private Channel(
        Guid id,
        Guid guildId,
        string name,
        ChannelType type,
        string? topic,
        ChannelSettings settings,
        ChannelStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        GuildId = guildId;
        Name = name;
        Type = type;
        Topic = topic;
        Settings = settings;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; }

    public Guid GuildId { get; }

    public string Name { get; private set; }

    public ChannelType Type { get; }

    public string? Topic { get; private set; }

    public ChannelSettings Settings { get; private set; }

    public ChannelStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsActive => Status == ChannelStatus.Active;

    public static Channel Create(
        Guid guildId,
        string name,
        ChannelType type,
        string? topic,
        ChannelSettings? settings,
        DateTimeOffset now)
    {
        if (guildId == Guid.Empty)
        {
            throw new ArgumentException("Guild id is required.", nameof(guildId));
        }

        var normalizedName = NormalizeName(name);
        var normalizedTopic = NormalizeTopic(topic);
        var channelSettings = settings ?? ChannelSettings.DefaultFor(type);

        return new Channel(
            Guid.NewGuid(),
            guildId,
            normalizedName,
            type,
            normalizedTopic,
            channelSettings,
            ChannelStatus.Active,
            now,
            now);
    }

    public void Update(string name, string? topic, ChannelSettings settings, DateTimeOffset now)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Archived channels cannot be updated.");
        }

        Name = NormalizeName(name);
        Topic = NormalizeTopic(topic);
        Settings = settings;
        UpdatedAt = now;
    }

    public void Archive(DateTimeOffset now)
    {
        Status = ChannelStatus.Archived;
        UpdatedAt = now;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Channel name is required.", nameof(name));
        }

        var normalized = name.Trim().ToLowerInvariant().Replace(' ', '-');
        if (normalized.Length > 100)
        {
            throw new ArgumentException("Channel name cannot exceed 100 characters.", nameof(name));
        }

        return normalized;
    }

    private static string? NormalizeTopic(string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return null;
        }

        var normalized = topic.Trim();
        if (normalized.Length > 250)
        {
            throw new ArgumentException("Channel topic cannot exceed 250 characters.", nameof(topic));
        }

        return normalized;
    }
}