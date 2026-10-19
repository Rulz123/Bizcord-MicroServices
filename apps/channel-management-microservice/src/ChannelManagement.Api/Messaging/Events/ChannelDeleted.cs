namespace ChannelManagement.Api.Messaging.Events;

public record ChannelDeleted(Guid ChannelId, Guid ServerId);