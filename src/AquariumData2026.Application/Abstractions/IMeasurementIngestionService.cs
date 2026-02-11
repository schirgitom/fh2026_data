namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Coordinates MQTT ingestion and downstream publishing.
/// </summary>
public interface IMeasurementIngestionService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}
