namespace Bizcord.ChannelManagement.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string ConnectionString { get; init; } = string.Empty;

    public static void Validate(RabbitMqOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("RabbitMq:ConnectionString is required.");
        }
    }
}