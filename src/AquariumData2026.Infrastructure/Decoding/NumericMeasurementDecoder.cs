using System.Globalization;
using System.Text.Json;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Domain.Model;
using Microsoft.Extensions.Logging;

namespace AquariumData2026.Infrastructure.Decoding;

/// <summary>
/// Decodes simulator MQTT envelopes that contain HEXA-encoded numeric JSON payloads.
/// </summary>
public sealed class NumericMeasurementDecoder : IMeasurementDecoder
{
    private static readonly JsonSerializerOptions EnvelopeJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly IReadOnlyDictionary<string, (MetricType Type, string Unit)> MetricMappings =
        new Dictionary<string, (MetricType Type, string Unit)>(StringComparer.OrdinalIgnoreCase)
        {
            ["temperature"] = (MetricType.WaterTemperature, "C"),
            ["watertemperature"] = (MetricType.WaterTemperature, "C"),
            ["waveflow"] = (MetricType.WaveFlow, "L/min"),
            ["flow"] = (MetricType.WaveFlow, "L/min"),
            ["pump"] = (MetricType.WaveFlow, "L/min"),
            ["salinity"] = (MetricType.Salinity, "ppt"),
            ["mg"] = (MetricType.Magnesium, "mg/L"),
            ["magnesium"] = (MetricType.Magnesium, "mg/L"),
            ["ca"] = (MetricType.Calcium, "ppm"),
            ["calcium"] = (MetricType.Calcium, "ppm"),
            ["nitrite"] = (MetricType.Nitrite, "ppm"),
            ["nitrate"] = (MetricType.Nitrate, "ppm"),
            ["ph"] = (MetricType.Ph, "pH"),
            ["kh"] = (MetricType.Alkalinity, "dKH"),
            ["alkalinity"] = (MetricType.Alkalinity, "dKH")
        };

    private readonly ILogger<NumericMeasurementDecoder> _logger;

    public NumericMeasurementDecoder(ILogger<NumericMeasurementDecoder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public MeasurementDto Decode(MqttMessage message)
    {
        if (message.Payload.Length == 0)
        {
            throw new InvalidDataException("MQTT payload is empty.");
        }

        SimulatorEnvelope envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<SimulatorEnvelope>(message.Payload, EnvelopeJsonOptions)
                ?? throw new InvalidDataException("MQTT payload could not be deserialized.");
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("MQTT payload is not a valid simulator envelope.", ex);
        }

        if (string.IsNullOrWhiteSpace(envelope.Payload))
        {
            throw new InvalidDataException("Simulator envelope does not contain a payload field.");
        }

        if (string.IsNullOrWhiteSpace(envelope.EndDevice?.DevEui))
        {
            throw new InvalidDataException("Simulator envelope does not contain endDevice.devEui.");
        }

        byte[] numericPayloadBytes;
        try
        {
            numericPayloadBytes = Convert.FromHexString(envelope.Payload);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException("Simulator payload is not valid hexadecimal text.", ex);
        }

        JsonDocument numericPayload;
        try
        {
            numericPayload = JsonDocument.Parse(numericPayloadBytes);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Decoded simulator payload is not valid JSON.", ex);
        }

        using (numericPayload)
        {
            if (numericPayload.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException("Decoded simulator payload must be a JSON object.");
            }

            var metrics = new List<MetricValueDto>();
            foreach (var property in numericPayload.RootElement.EnumerateObject())
            {
                if (!TryReadNumericValue(property.Value, out var value))
                {
                    _logger.LogDebug("Skipping non-numeric metric field {FieldName}.", property.Name);
                    continue;
                }

                var normalizedKey = NormalizeMetricKey(property.Name);
                if (!MetricMappings.TryGetValue(normalizedKey, out var mapping))
                {
                    _logger.LogDebug("Skipping unsupported metric field {FieldName}.", property.Name);
                    continue;
                }

                metrics.Add(new MetricValueDto(mapping.Type, Convert.ToDecimal(value, CultureInfo.InvariantCulture), mapping.Unit));
            }

            if (metrics.Count == 0)
            {
                throw new InvalidDataException("No supported numeric metrics were found in simulator payload.");
            }

            var timestamp = envelope.RecvTime > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(envelope.RecvTime)
                : message.ReceivedAt;

            _logger.LogDebug(
                "Decoded numeric payload for aquarium {AquariumId} with {MetricCount} metrics.",
                envelope.EndDevice.DevEui,
                metrics.Count);

            return new MeasurementDto(envelope.EndDevice.DevEui, timestamp, metrics);
        }
    }

    private static bool TryReadNumericValue(JsonElement element, out double value)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Number:
                return element.TryGetDouble(out value);
            case JsonValueKind.String:
                return double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            default:
                value = 0;
                return false;
        }
    }

    private static string NormalizeMetricKey(string key) =>
        key.Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim();

    private sealed record SimulatorEnvelope(SimulatorEndDevice? EndDevice, string? Payload, long RecvTime);

    private sealed record SimulatorEndDevice(string? DevEui);
}
