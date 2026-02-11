namespace AquariumData2026.Domain.Model;

/// <summary>
/// Strongly typed identifier for aquariums.
/// </summary>
public sealed record AquariumId
{
    public string Value { get; }

    public AquariumId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Aquarium id must be provided.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
