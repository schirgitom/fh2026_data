using System.Net.Http.Json;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquariumData2026.Infrastructure.Registry;

/// <summary>
/// Retrieves aquariums from an external registry API.
/// </summary>
public sealed class AquariumRegistryClient : IAquariumRegistryClient
{
    private readonly HttpClient _httpClient;
    private readonly RegistryApiOptions _options;
    private readonly ILogger<AquariumRegistryClient> _logger;

    public AquariumRegistryClient(
        HttpClient httpClient,
        IOptions<RegistryApiOptions> options,
        ILogger<AquariumRegistryClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyCollection<AquariumDto>> GetAquariumsAsync(CancellationToken cancellationToken)
    {
        var freshWaterAquariums = await GetAquariumsByPathAsync(_options.FreshWaterAquariumsPath, cancellationToken)
            .ConfigureAwait(false);
        var seaWaterAquariums = await GetAquariumsByPathAsync(_options.SeaWaterAquariumsPath, cancellationToken)
            .ConfigureAwait(false);

        var mergedAquariums = freshWaterAquariums
            .Concat(seaWaterAquariums)
            .Where(aquarium => !string.IsNullOrWhiteSpace(aquarium.Id))
            .GroupBy(aquarium => aquarium.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        _logger.LogInformation(
            "Loaded {AquariumCount} aquariums from registry API.",
            mergedAquariums.Length);

        return mergedAquariums;
    }

    private async Task<IReadOnlyCollection<AquariumDto>> GetAquariumsByPathAsync(
        string path,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Requesting aquariums from registry path {Path}.", path);

        using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Registry API returned status {StatusCode} for path {Path}.", response.StatusCode, path);
            return Array.Empty<AquariumDto>();
        }

        var aquariums = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<AquariumDto>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var result = aquariums ?? Array.Empty<AquariumDto>();
        _logger.LogDebug("Registry path {Path} returned {AquariumCount} aquariums.", path, result.Count);
        return result;
    }
}
