namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// Endpoint configuration for the aquarium registry API.
/// </summary>
public sealed class RegistryApiOptions
{
    public const string SectionName = "RegistryApi";

    public string BaseUrl { get; init; } = "http://localhost:5011/";
    public string FreshWaterAquariumsPath { get; init; } = "/api/FreshWaterAquarium";
    public string SeaWaterAquariumsPath { get; init; } = "/api/SeaWaterAquarium";
    public int TimeoutSeconds { get; init; } = 10;
    public string ServiceKey { get; init; } = string.Empty;
}
