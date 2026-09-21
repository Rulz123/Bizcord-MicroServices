using Bizcord.ChannelManagement.Application;
using Bizcord.ChannelManagement.Domain;
using Bizcord.ChannelManagement.Infrastructure;

namespace Bizcord.ChannelManagement.Tests;

public sealed class ChannelServiceTests
{
    [Fact]
    public async Task CreateChannelAsync_NormalizesNameAndAppliesTypeDefaults()
    {
        var service = CreateService();

        var channel = await service.CreateChannelAsync(
            new CreateChannelRequest(Guid.NewGuid(), "General Chat", ChannelType.Text, "Team updates", null),
            CancellationToken.None);

        Assert.Equal("general-chat", channel.Name);
        Assert.Equal(ChannelType.Text, channel.Type);
        Assert.Equal(0, channel.Settings.SlowModeSeconds);
        Assert.Equal(ChannelStatus.Active, channel.Status);
    }

    [Fact]
    public async Task GetChannelsAsync_CanFilterByGuild()
    {
        var service = CreateService();
        var targetGuildId = Guid.NewGuid();

        await service.CreateChannelAsync(new CreateChannelRequest(targetGuildId, "Forum", ChannelType.Forum, null, null), CancellationToken.None);
        await service.CreateChannelAsync(new CreateChannelRequest(Guid.NewGuid(), "Voice", ChannelType.Voice, null, null), CancellationToken.None);

        var channels = await service.GetChannelsAsync(targetGuildId, CancellationToken.None);

        var channel = Assert.Single(channels);
        Assert.Equal(targetGuildId, channel.GuildId);
        Assert.Equal(ChannelType.Forum, channel.Type);
    }

    [Fact]
    public async Task ArchiveChannelAsync_MarksExistingChannelArchived()
    {
        var service = CreateService();
        var channel = await service.CreateChannelAsync(
            new CreateChannelRequest(Guid.NewGuid(), "Voice Room", ChannelType.Voice, null, null),
            CancellationToken.None);

        var archived = await service.ArchiveChannelAsync(channel.Id, CancellationToken.None);
        var updatedChannel = await service.GetChannelAsync(channel.Id, CancellationToken.None);

        Assert.True(archived);
        Assert.NotNull(updatedChannel);
        Assert.Equal(ChannelStatus.Archived, updatedChannel.Status);
    }

    private static ChannelService CreateService()
    {
        return new ChannelService(new InMemoryChannelRepository(), TimeProvider.System);
    }
}