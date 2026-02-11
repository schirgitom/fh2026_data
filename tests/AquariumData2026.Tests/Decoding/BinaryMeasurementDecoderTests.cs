using System.Text;
using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Decoding;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AquariumData2026.Tests.Decoding;

public sealed class BinaryMeasurementDecoderTests
{
    [Fact]
    public void Decode_ValidPayload_ReturnsMeasurementDto()
    {
        var options = Options.Create(new BinaryPayloadOptions { ExpectedVersion = 1 });
        var logger = new Mock<ILogger<BinaryMeasurementDecoder>>();
        var decoder = new BinaryMeasurementDecoder(options, logger.Object);

        var aquariumId = Guid.NewGuid();
        var timestamp = new DateTimeOffset(2026, 2, 3, 12, 0, 0, TimeSpan.Zero);

        var payload = BuildPayload(
            version: 1,
            aquariumId: aquariumId,
            timestamp: timestamp,
            metrics:
            [
                (MetricTypeId: (byte)1, Value: 24.5d, UnitCode: (byte)1),
                (MetricTypeId: (byte)3, Value: 35.1d, UnitCode: (byte)3)
            ]);

        var message = new MqttMessage("aquariums/demo/measurements", payload, DateTimeOffset.UtcNow);

        var result = decoder.Decode(message);

        Assert.Equal(aquariumId.ToString("D"), result.AquariumId);
        Assert.Equal(timestamp, result.Timestamp);
        Assert.Equal(2, result.Metrics.Count);
        Assert.Equal(24.5m, result.Metrics.First().Value);
    }

    [Fact]
    public void Decode_InvalidVersion_Throws()
    {
        var options = Options.Create(new BinaryPayloadOptions { ExpectedVersion = 1 });
        var logger = new Mock<ILogger<BinaryMeasurementDecoder>>();
        var decoder = new BinaryMeasurementDecoder(options, logger.Object);

        var payload = BuildPayload(
            version: 2,
            aquariumId: Guid.NewGuid(),
            timestamp: DateTimeOffset.UtcNow,
            metrics: []);

        var message = new MqttMessage("aquariums/demo/measurements", payload, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidDataException>(() => decoder.Decode(message));
    }

    private static byte[] BuildPayload(
        byte version,
        Guid aquariumId,
        DateTimeOffset timestamp,
        IReadOnlyCollection<(byte MetricTypeId, double Value, byte UnitCode)> metrics)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write(version);
        writer.Write(aquariumId.ToByteArray());
        writer.Write(timestamp.Ticks);
        writer.Write((byte)metrics.Count);

        foreach (var metric in metrics)
        {
            writer.Write(metric.MetricTypeId);
            writer.Write(metric.Value);
            writer.Write(metric.UnitCode);
        }

        writer.Flush();
        return stream.ToArray();
    }
}
