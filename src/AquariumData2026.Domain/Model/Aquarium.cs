namespace AquariumData2026.Domain.Model;

/// <summary>
/// Aquarium aggregate root.
/// </summary>
public sealed record Aquarium
{
    public AquariumId Id { get; }
    public string Name { get; }

    public Aquarium(AquariumId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Aquarium name must be provided.", nameof(name));
        }

        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name;
    }
}
