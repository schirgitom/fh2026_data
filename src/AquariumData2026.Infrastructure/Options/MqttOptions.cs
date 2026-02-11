namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// Connection settings for the MQTT broker.
/// </summary>
public sealed class MqttOptions
{
    public const string SectionName = "Mqtt";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1883;
    public string ClientId { get; init; } = "aquarium-data-fetcher";
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool UseTls { get; init; } = false;
    public int KeepAliveSeconds { get; init; } = 30;
    public int ReconnectDelaySeconds { get; init; } = 5;
}
