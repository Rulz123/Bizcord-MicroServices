using Bizcord.ChannelManagement.Application;
using Bizcord.ChannelManagement.Application.Abstractions;
using Bizcord.ChannelManagement.Contracts.Events;
using Bizcord.ChannelManagement.Domain;
using Bizcord.ChannelManagement.Infrastructure;

namespace Bizcord.ChannelManagement.Tests;

public sealed class ChannelServiceTests
{
    [Fact]
    public async Task CreateChannelAsync_NormalizesNameAndAppliesTypeDefaults()
    {
        var service = CreateService(new RecordingMessageClient());

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
        var service = CreateService(new RecordingMessageClient());
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
        var service = CreateService(new RecordingMessageClient());
        var channel = await service.CreateChannelAsync(
            new CreateChannelRequest(Guid.NewGuid(), "Voice Room", ChannelType.Voice, null, null),
            CancellationToken.None);

        var archived = await service.ArchiveChannelAsync(channel.Id, CancellationToken.None);
        var updatedChannel = await service.GetChannelAsync(channel.Id, CancellationToken.None);

        Assert.True(archived);
        Assert.NotNull(updatedChannel);
        Assert.Equal(ChannelStatus.Archived, updatedChannel.Status);
    }

    [Fact]
    public async Task CreateChannelAsync_PublishesExactlyOneChannelCreatedEvent()
    {
        var messageClient = new RecordingMessageClient();
        var service = CreateService(messageClient);
        var guildId = Guid.NewGuid();

        var channel = await service.CreateChannelAsync(
            new CreateChannelRequest(guildId, "Announcements", ChannelType.Text, "News", null),
            CancellationToken.None);

        var integrationEvent = Assert.Single(messageClient.PublishedMessages.OfType<ChannelCreatedV1>());
        Assert.NotEqual(Guid.Empty, integrationEvent.EventId);
        Assert.Equal(channel.Id, integrationEvent.ChannelId);
        Assert.Equal(guildId, integrationEvent.GuildId);
        Assert.Equal("announcements", integrationEvent.Name);
        Assert.Equal(nameof(ChannelType.Text), integrationEvent.ChannelType);
    }

    [Fact]
    public async Task CreateChannelAsync_DoesNotPublishWhenValidationFails()
    {
        var messageClient = new RecordingMessageClient();
        var service = CreateService(messageClient);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateChannelAsync(
            new CreateChannelRequest(Guid.NewGuid(), " ", ChannelType.Text, null, null),
            CancellationToken.None));

        Assert.Empty(messageClient.PublishedMessages);
    }

    private static ChannelService CreateService(RecordingMessageClient messageClient)
    {
        return new ChannelService(new InMemoryChannelRepository(), messageClient, TimeProvider.System);
    }

    private sealed class RecordingMessageClient : IMessageClient
    {
        public List<object> PublishedMessages { get; } = new();

        public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
            where TMessage : class
        {
            PublishedMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IAsyncDisposable> SubscribeAsync<TMessage>(
            string subscriptionId,
            Func<TMessage, CancellationToken, Task> handler,
            CancellationToken cancellationToken)
            where TMessage : class
        {
            return Task.FromResult<IAsyncDisposable>(new NoopAsyncDisposable());
        }
    }

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}