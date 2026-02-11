using AquariumData2026.Application.Models;

namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Publishes measurements to downstream systems.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync(MeasurementDto measurement, CancellationToken cancellationToken);
}
