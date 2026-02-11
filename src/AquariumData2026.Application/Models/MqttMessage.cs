namespace AquariumData2026.Application.Models;

/// <summary>
/// Envelope for MQTT payloads received by the ingestion pipeline.
/// </summary>
public sealed record MqttMessage(string Topic, byte[] Payload, DateTimeOffset ReceivedAt);
