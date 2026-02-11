using AquariumData2026.Application.Models;

namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Retrieves aquariums from the registry service during startup.
/// </summary>
public interface IAquariumRegistryClient
{
    Task<IReadOnlyCollection<AquariumDto>> GetAquariumsAsync(CancellationToken cancellationToken);
}
