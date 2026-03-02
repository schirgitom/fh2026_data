using System.Text.Json;
using Xunit;

namespace AquariumData2026.Tests.Registry;

public sealed class LiveRegistryApiTests
{
    private static readonly Uri DefaultBaseUri = new("http://localhost:5011/");

    [Fact]
    [Trait("Category", "Live")]
    public async Task RegistryApi_FreshAndSeaWaterEndpoints_AreReachableAndReturnJsonArrays()
    {
        var baseUrl = Environment.GetEnvironmentVariable("LIVE_REGISTRY_BASE_URL");
        var baseUri = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUri : new Uri(baseUrl);

        using var httpClient = new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(5)
        };

        await AssertArrayResponseAsync(httpClient, "/api/FreshWaterAquarium");
        await AssertArrayResponseAsync(httpClient, "/api/SeaWaterAquarium");
    }

    private static async Task AssertArrayResponseAsync(HttpClient httpClient, string path)
    {
        using var response = await httpClient.GetAsync(path);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.IsSuccessStatusCode,
            $"GET {path} against {httpClient.BaseAddress} failed with {(int)response.StatusCode} {response.StatusCode}. Body: {content}");

        using var document = JsonDocument.Parse(content);
        Assert.True(
            document.RootElement.ValueKind == JsonValueKind.Array,
            $"GET {path} returned JSON {document.RootElement.ValueKind}, expected Array.");
    }
}
