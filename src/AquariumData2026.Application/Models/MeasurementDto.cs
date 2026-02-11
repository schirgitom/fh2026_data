namespace AquariumData2026.Application.Models;

/// <summary>
/// Measurement payload delivered to downstream systems.
/// </summary>
public sealed record MeasurementDto(
    string AquariumId,
    DateTimeOffset Timestamp,
    IReadOnlyCollection<MetricValueDto> Metrics);
