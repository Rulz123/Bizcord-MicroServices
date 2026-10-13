namespace ChannelManagement.Api.Messaging.Events;

public record ChannelUpdated(
    Guid ChannelId,
    Guid ServerId,
    string Title,
    string Description,
    string Categories,
    string Type,
    string UserRoles,
    string WriteRights,
    bool Visibility);