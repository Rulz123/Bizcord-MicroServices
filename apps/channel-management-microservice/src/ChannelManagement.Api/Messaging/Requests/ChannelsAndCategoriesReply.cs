namespace ChannelManagement.Api.Messaging.Requests;

public record ChannelsAndCategoriesReply(
    Guid ServerId,
    List<CategoryDto> Categories
);