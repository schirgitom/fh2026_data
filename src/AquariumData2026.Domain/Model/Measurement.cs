namespace AquariumData2026.Domain.Model;

/// <summary>
/// Measurement captured from the aquarium telemetry stream.
/// </summary>
public sealed record Measurement
{
    public AquariumId AquariumId { get; }
    public DateTimeOffset Timestamp { get; }
    public IReadOnlyCollection<MetricValue> Metrics { get; }

    public Measurement(AquariumId aquariumId, DateTimeOffset timestamp, IReadOnlyCollection<MetricValue> metrics)
    {
        AquariumId = aquariumId ?? throw new ArgumentNullException(nameof(aquariumId));
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        Timestamp = timestamp;
    }
}
