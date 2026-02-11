using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Options;
using AquariumData2026.Infrastructure.Topics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AquariumData2026.Tests.Topics;

public sealed class TopicProviderTests
{
    [Fact]
    public async Task GetTopicsAsync_UsesHardCodedTopics_WhenConfigured()
    {
        var options = Options.Create(new TopicOptions
        {
            UseHardCodedTopics = true,
            HardCodedTopics = ["aquariums/demo-reef/measurements", "aquariums/demo-fresh/measurements"]
        });
        var logger = new Mock<ILogger<TopicProvider>>();
        var provider = new TopicProvider(options, logger.Object);

        var topics = await provider.GetTopicsAsync(Array.Empty<AquariumDto>(), CancellationToken.None);

        Assert.Equal(2, topics.Count);
        Assert.Contains(topics, topic => topic.Value == "aquariums/demo-reef/measurements");
    }

    [Fact]
    public async Task GetTopicsAsync_BuildsTopics_FromTemplate()
    {
        var options = Options.Create(new TopicOptions
        {
            UseHardCodedTopics = false,
            TopicTemplate = "aquariums/{aquariumId}/measurements"
        });
        var logger = new Mock<ILogger<TopicProvider>>();
        var provider = new TopicProvider(options, logger.Object);

        var aquariums = new[]
        {
            new AquariumDto("reef-1", "Reef Tank"),
            new AquariumDto("fresh-1", "Fresh Tank")
        };

        var topics = await provider.GetTopicsAsync(aquariums, CancellationToken.None);

        Assert.Equal(2, topics.Count);
        Assert.Contains(topics, topic => topic.Value == "aquariums/reef-1/measurements");
        Assert.Contains(topics, topic => topic.Value == "aquariums/fresh-1/measurements");
    }
}
