namespace ChannelManagement.Api.Messaging.Requests;

public record ChannelDto(
    Guid Id,
    string Title,
    string Description,
    string Type,
    bool Visibility
);