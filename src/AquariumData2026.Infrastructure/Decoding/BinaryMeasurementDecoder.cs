using System.Text;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquariumData2026.Infrastructure.Decoding;

/// <summary>
/// Decodes binary payloads into measurement DTOs.
/// </summary>
public sealed class BinaryMeasurementDecoder : IMeasurementDecoder
{
    private readonly BinaryPayloadOptions _options;
    private readonly ILogger<BinaryMeasurementDecoder> _logger;

    public BinaryMeasurementDecoder(
        IOptions<BinaryPayloadOptions> options,
        ILogger<BinaryMeasurementDecoder> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public MeasurementDto Decode(MqttMessage message)
    {
        if (message.Payload.Length == 0)
        {
            throw new InvalidDataException("MQTT payload is empty.");
        }

        using var stream = new MemoryStream(message.Payload, writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);

        var version = reader.ReadByte();
        if (version != _options.ExpectedVersion)
        {
            throw new InvalidDataException($"Unsupported payload version {version}.");
        }

        var aquariumIdBytes = reader.ReadBytes(BinaryMetricMappings.AquariumIdSize);
        if (aquariumIdBytes.Length != BinaryMetricMappings.AquariumIdSize)
        {
            throw new InvalidDataException("Payload ended before aquarium id could be read.");
        }

        var aquariumId = new Guid(aquariumIdBytes).ToString("D");

        var ticks = reader.ReadInt64();
        var timestamp = new DateTimeOffset(ticks, TimeSpan.Zero);

        var metricCount = reader.ReadByte();
        var metrics = new List<MetricValueDto>(metricCount);

        for (var index = 0; index < metricCount; index++)
        {
            var metricTypeId = reader.ReadByte();
            if (!BinaryMetricMappings.MetricTypes.TryGetValue(metricTypeId, out var metricType))
            {
                throw new InvalidDataException($"Unsupported metric type id {metricTypeId}.");
            }

            var valueBytes = reader.ReadBytes(sizeof(double));
            if (valueBytes.Length != sizeof(double))
            {
                throw new InvalidDataException("Payload ended before metric value could be read.");
            }

            var value = BitConverter.ToDouble(valueBytes, 0);
            var unitCode = reader.ReadByte();
            var unit = BinaryMetricMappings.ResolveUnit(unitCode);

            metrics.Add(new MetricValueDto(metricType, (decimal)value, unit));
        }

        _logger.LogDebug(
            "Decoded binary payload for aquarium {AquariumId} with {MetricCount} metrics.",
            aquariumId,
            metrics.Count);

        return new MeasurementDto(aquariumId, timestamp, metrics);
    }
}
