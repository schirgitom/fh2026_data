using System.Net;
using System.Net.Http.Json;
using AquariumData2026.Infrastructure.Options;
using AquariumData2026.Infrastructure.Registry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AquariumData2026.Tests.Registry;

public sealed class AquariumRegistryClientTests
{
    [Fact]
    public async Task GetAquariumsAsync_LoadsFreshAndSeaWaterAquariums_AndMergesById()
    {
        var handler = new StubHttpMessageHandler(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/api/FreshWaterAquarium")
            {
                return JsonResponse(new[]
                {
                    new { id = "fresh-1", name = "Fresh One" },
                    new { id = "dup-1", name = "Fresh Duplicate" }
                });
            }

            if (request.RequestUri?.AbsolutePath == "/api/SeaWaterAquarium")
            {
                return JsonResponse(new[]
                {
                    new { id = "sea-1", name = "Sea One" },
                    new { id = "dup-1", name = "Sea Duplicate" }
                });
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5011/")
        };

        var options = Options.Create(new RegistryApiOptions
        {
            BaseUrl = "http://localhost:5011/",
            FreshWaterAquariumsPath = "/api/FreshWaterAquarium",
            SeaWaterAquariumsPath = "/api/SeaWaterAquarium"
        });
        var logger = new Mock<ILogger<AquariumRegistryClient>>();
        var client = new AquariumRegistryClient(httpClient, options, logger.Object);

        var result = await client.GetAquariumsAsync(CancellationToken.None);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, aquarium => aquarium.Id == "fresh-1");
        Assert.Contains(result, aquarium => aquarium.Id == "sea-1");
        Assert.Single(result, aquarium => aquarium.Id == "dup-1");
    }

    private static HttpResponseMessage JsonResponse<T>(T payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(payload)
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
        }
    }
}
