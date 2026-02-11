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
        using var response = await _httpClient.GetAsync(_options.AquariumsPath, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Registry API returned status {StatusCode}.", response.StatusCode);
            return Array.Empty<AquariumDto>();
        }

        var aquariums = await response.Content
            .ReadFromJsonAsync<IReadOnlyCollection<AquariumDto>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return aquariums ?? Array.Empty<AquariumDto>();
    }
}
