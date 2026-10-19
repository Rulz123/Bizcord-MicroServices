namespace ChannelManagement.Api.Messaging.Requests;

public record CategoryDto(
    string Name,
    List<ChannelDto> Channels
);