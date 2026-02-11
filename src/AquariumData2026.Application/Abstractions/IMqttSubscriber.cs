using AquariumData2026.Application.Models;

namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Subscribes to MQTT topics and forwards incoming messages.
/// </summary>
public interface IMqttSubscriber
{
    Task SubscribeAsync(
        IReadOnlyCollection<MqttTopic> topics,
        Func<MqttMessage, CancellationToken, Task> onMessage,
        CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
