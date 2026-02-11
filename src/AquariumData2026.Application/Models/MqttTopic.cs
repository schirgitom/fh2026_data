namespace AquariumData2026.Application.Models;

/// <summary>
/// MQTT topic wrapper to prevent raw string usage.
/// </summary>
public sealed record MqttTopic(string Value);
