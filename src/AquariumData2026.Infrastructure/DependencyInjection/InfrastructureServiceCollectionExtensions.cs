using AquariumData2026.Application.Abstractions;
using AquariumData2026.Infrastructure.Decoding;
using AquariumData2026.Infrastructure.Messaging;
using AquariumData2026.Infrastructure.Mqtt;
using AquariumData2026.Infrastructure.Options;
using AquariumData2026.Infrastructure.Registry;
using AquariumData2026.Infrastructure.Topics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AquariumData2026.Infrastructure.DependencyInjection;

/// <summary>
/// Dependency injection registrations for the infrastructure layer.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MqttOptions>(configuration.GetSection(MqttOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<RegistryApiOptions>(configuration.GetSection(RegistryApiOptions.SectionName));
        services.Configure<TopicOptions>(configuration.GetSection(TopicOptions.SectionName));
        services.Configure<BinaryPayloadOptions>(configuration.GetSection(BinaryPayloadOptions.SectionName));

        services.AddHttpClient<IAquariumRegistryClient, AquariumRegistryClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RegistryApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddSingleton<ITopicProvider, TopicProvider>();
        services.AddSingleton<IMqttSubscriber, MqttSubscriber>();
        services.AddSingleton<IMeasurementDecoder, NumericMeasurementDecoder>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
