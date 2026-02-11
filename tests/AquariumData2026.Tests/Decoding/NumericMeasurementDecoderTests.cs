using System.Text;
using System.Text.Json;
using AquariumData2026.Application.Models;
using AquariumData2026.Domain.Model;
using AquariumData2026.Infrastructure.Decoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AquariumData2026.Tests.Decoding;

public sealed class NumericMeasurementDecoderTests
{
    [Fact]
    public void Decode_ValidSimulatorEnvelope_ReturnsMeasurementDto()
    {
        var logger = new Mock<ILogger<NumericMeasurementDecoder>>();
        var decoder = new NumericMeasurementDecoder(logger.Object);

        var aquariumId = Guid.NewGuid().ToString("D");
        var timestamp = new DateTimeOffset(2026, 2, 11, 15, 30, 0, TimeSpan.Zero);
        var numericPayload = new Dictionary<string, double>
        {
            ["temperature"] = 24.75,
            ["mg"] = 305.2,
            ["kh"] = 8.3,
            ["ca"] = 1315.5,
            ["ph"] = 7.9,
            ["pump"] = 1,
            ["oxygen"] = 8.6
        };

        var message = BuildSimulatorMessage(aquariumId, timestamp, numericPayload);

        var result = decoder.Decode(message);

        Assert.Equal(aquariumId, result.AquariumId);
        Assert.Equal(timestamp, result.Timestamp);
        Assert.Equal(6, result.Metrics.Count);

        var valuesByType = result.Metrics.ToDictionary(metric => metric.Type, metric => metric.Value);
        Assert.Equal(24.75m, valuesByType[MetricType.WaterTemperature]);
        Assert.Equal(305.2m, valuesByType[MetricType.Magnesium]);
        Assert.Equal(8.3m, valuesByType[MetricType.Alkalinity]);
        Assert.Equal(1315.5m, valuesByType[MetricType.Calcium]);
        Assert.Equal(7.9m, valuesByType[MetricType.Ph]);
        Assert.Equal(1m, valuesByType[MetricType.WaveFlow]);
    }

    [Fact]
    public void Decode_InvalidHexPayload_Throws()
    {
        var logger = new Mock<ILogger<NumericMeasurementDecoder>>();
        var decoder = new NumericMeasurementDecoder(logger.Object);

        var envelopeJson = JsonSerializer.Serialize(new
        {
            endDevice = new { devEui = Guid.NewGuid().ToString("D") },
            recvTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            payload = "NOT-HEX"
        });
        var message = new MqttMessage("aquariums/demo/measurements", Encoding.UTF8.GetBytes(envelopeJson), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidDataException>(() => decoder.Decode(message));
    }

    [Fact]
    public void Decode_WithoutSupportedMetrics_Throws()
    {
        var logger = new Mock<ILogger<NumericMeasurementDecoder>>();
        var decoder = new NumericMeasurementDecoder(logger.Object);

        var message = BuildSimulatorMessage(
            Guid.NewGuid().ToString("D"),
            DateTimeOffset.UtcNow,
            new Dictionary<string, double> { ["oxygen"] = 8.4 });

        Assert.Throws<InvalidDataException>(() => decoder.Decode(message));
    }

    private static MqttMessage BuildSimulatorMessage(
        string aquariumId,
        DateTimeOffset timestamp,
        IReadOnlyDictionary<string, double> numericPayload)
    {
        var innerJson = JsonSerializer.Serialize(numericPayload);
        var innerHex = Convert.ToHexString(Encoding.UTF8.GetBytes(innerJson));
        var outerJson = JsonSerializer.Serialize(new
        {
            endDevice = new { devEui = aquariumId },
            recvTime = timestamp.ToUnixTimeMilliseconds(),
            payload = innerHex,
            encodingType = "HEXA"
        });

        return new MqttMessage(
            "aquariums/demo/measurements",
            Encoding.UTF8.GetBytes(outerJson),
            DateTimeOffset.UtcNow);
    }
}
