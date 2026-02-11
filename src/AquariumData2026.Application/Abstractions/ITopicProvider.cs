using AquariumData2026.Application.Models;

namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Resolves MQTT topics for aquariums.
/// </summary>
public interface ITopicProvider
{
    Task<IReadOnlyCollection<MqttTopic>> GetTopicsAsync(
        IReadOnlyCollection<AquariumDto> aquariums,
        CancellationToken cancellationToken);
}
