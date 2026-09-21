using Bizcord.ChannelManagement.Application;
using Bizcord.ChannelManagement.Application.Abstractions;
using Bizcord.ChannelManagement.Infrastructure.Messaging;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bizcord.ChannelManagement.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChannelManagementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration[$"{RabbitMqOptions.SectionName}:ConnectionString"];
        if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            throw new InvalidOperationException("RabbitMq:ConnectionString is required.");
        }

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "RabbitMq:ConnectionString is required.")
            .ValidateOnStart();

        RabbitHutch.AddEasyNetQ(services, rabbitMqConnectionString);

        services.AddSingleton<IChannelRepository, InMemoryChannelRepository>();
        services.AddSingleton<IMessageClient, EasyNetQMessageClient>();

        return services;
    }
}