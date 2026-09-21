namespace Bizcord.ChannelManagement.Domain;

public sealed record ChannelSettings(
    bool IsPrivate,
    int? UserLimit,
    int? SlowModeSeconds)
{
    public static ChannelSettings DefaultFor(ChannelType type)
    {
        return type switch
        {
            ChannelType.Voice => new ChannelSettings(false, 25, null),
            ChannelType.Text => new ChannelSettings(false, null, 0),
            ChannelType.Forum => new ChannelSettings(false, null, 30),
            _ => new ChannelSettings(false, null, null)
        };
    }
}