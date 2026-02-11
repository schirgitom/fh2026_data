namespace AquariumData2026.Domain.Model;

/// <summary>
/// Immutable value for a metric in a measurement.
/// </summary>
public sealed record MetricValue
{
    public MetricType Type { get; }
    public decimal Value { get; }
    public string Unit { get; }

    public MetricValue(MetricType type, decimal value, string unit)
    {
        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new ArgumentException("Unit must be provided.", nameof(unit));
        }

        Type = type;
        Value = value;
        Unit = unit;
    }
}
