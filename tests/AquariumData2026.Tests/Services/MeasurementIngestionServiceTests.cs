using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AquariumData2026.Tests.Services;

public sealed class MeasurementIngestionServiceTests
{
    [Fact]
    public async Task StartAsync_SubscribesAndPublishesDecodedMessage()
    {
        var registryClient = new Mock<IAquariumRegistryClient>();
        var topicProvider = new Mock<ITopicProvider>();
        var mqttSubscriber = new Mock<IMqttSubscriber>();
        var decoder = new Mock<IMeasurementDecoder>();
        var publisher = new Mock<IMessagePublisher>();
        var logger = new Mock<ILogger<MeasurementIngestionService>>();

        var aquariums = new[] { new AquariumDto("reef-1", "Reef") };
        var topics = new[] { new MqttTopic("aquariums/reef-1/measurements") };
        var measurement = new MeasurementDto("reef-1", DateTimeOffset.UtcNow, Array.Empty<MetricValueDto>());

        registryClient.Setup(client => client.GetAquariumsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(aquariums);
        topicProvider.Setup(provider => provider.GetTopicsAsync(aquariums, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);
        decoder.Setup(d => d.Decode(It.IsAny<MqttMessage>())).Returns(measurement);

        Func<MqttMessage, CancellationToken, Task>? handler = null;
        mqttSubscriber
            .Setup(subscriber => subscriber.SubscribeAsync(
                topics,
                It.IsAny<Func<MqttMessage, CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyCollection<MqttTopic>, Func<MqttMessage, CancellationToken, Task>, CancellationToken>(
                (_, callback, _) => handler = callback)
            .Returns(Task.CompletedTask);

        var service = new MeasurementIngestionService(
            registryClient.Object,
            topicProvider.Object,
            mqttSubscriber.Object,
            decoder.Object,
            publisher.Object,
            logger.Object);

        await service.StartAsync(CancellationToken.None);

        Assert.NotNull(handler);
        await handler!(new MqttMessage("aquariums/reef-1/measurements", [1, 2, 3], DateTimeOffset.UtcNow), CancellationToken.None);

        publisher.Verify(p => p.PublishAsync(measurement, It.IsAny<CancellationToken>()), Times.Once);
    }
}
