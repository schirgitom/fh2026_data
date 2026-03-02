namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Publishes JSON payloads to downstream systems.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(string jsonPayload, CancellationToken cancellationToken);
}
