using Bizcord.ChannelManagement.Infrastructure.Messaging;

namespace Bizcord.ChannelManagement.Tests;

public sealed class RabbitMqOptionsTests
{
    [Fact]
    public void Validate_ThrowsWhenConnectionStringIsMissing()
    {
        var options = new RabbitMqOptions { ConnectionString = " " };

        Assert.Throws<InvalidOperationException>(() => RabbitMqOptions.Validate(options));
    }

    [Fact]
    public void Validate_AllowsLocalDevelopmentConnectionString()
    {
        var options = new RabbitMqOptions { ConnectionString = "host=localhost" };

        RabbitMqOptions.Validate(options);
    }
}