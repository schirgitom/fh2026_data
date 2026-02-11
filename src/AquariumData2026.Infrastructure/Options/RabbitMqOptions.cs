namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// Connection settings for RabbitMQ publishing.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string ExchangeName { get; init; } = "aquarium.telemetry";
    public string RoutingKey { get; init; } = "aquarium.measurements";
    public bool Durable { get; init; } = true;
}
