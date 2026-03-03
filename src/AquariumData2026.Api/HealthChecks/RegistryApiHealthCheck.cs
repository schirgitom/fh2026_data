using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace AquariumData2026.Api.HealthChecks;

public sealed class RegistryApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RegistryApiOptions _options;

    public RegistryApiHealthCheck(IHttpClientFactory httpClientFactory, IOptions<RegistryApiOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_options.BaseUrl);

            var paths = new[] { _options.FreshWaterAquariumsPath, _options.SeaWaterAquariumsPath };
            foreach (var path in paths)
            {
                using var response = await client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy(
                        $"Registry API endpoint {path} is not reachable (status {(int)response.StatusCode}).");
                }
            }

            return HealthCheckResult.Healthy("Registry API is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Registry API is not reachable.", ex);
        }
    }
}
