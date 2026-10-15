using EasyNetQ;

namespace ChannelManagement.Api.Messaging;

public static class MessagingServiceExtensions
{
    public static IServiceCollection AddMessageClient(
        this IServiceCollection services, string connectionString)
    {
        services.AddEasyNetQ(connectionString);
        services.AddSingleton<IMessageClient, MessageClient>();
        return services;
    }
}