namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// Endpoint configuration for the aquarium registry API.
/// </summary>
public sealed class RegistryApiOptions
{
    public const string SectionName = "RegistryApi";

    public string BaseUrl { get; init; } = "https://registry.invalid";
    public string AquariumsPath { get; init; } = "/api/aquariums";
    public int TimeoutSeconds { get; init; } = 10;
}
