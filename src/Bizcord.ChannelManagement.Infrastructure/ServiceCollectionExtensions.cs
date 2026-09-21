using Bizcord.ChannelManagement.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Bizcord.ChannelManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChannelManagementInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IChannelRepository, InMemoryChannelRepository>();
        return services;
    }
}