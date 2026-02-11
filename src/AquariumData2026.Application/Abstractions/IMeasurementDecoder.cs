using AquariumData2026.Application.Models;

namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Decodes binary MQTT payloads into measurement DTOs.
/// </summary>
public interface IMeasurementDecoder
{
    MeasurementDto Decode(MqttMessage message);
}
