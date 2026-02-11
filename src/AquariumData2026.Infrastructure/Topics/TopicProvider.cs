using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AquariumData2026.Infrastructure.Topics;

/// <summary>
/// Resolves MQTT topics based on configuration and registry data.
/// </summary>
public sealed class TopicProvider : ITopicProvider
{
    private const string AquariumIdToken = "{aquariumId}";

    private readonly TopicOptions _options;
    private readonly ILogger<TopicProvider> _logger;

    public TopicProvider(IOptions<TopicOptions> options, ILogger<TopicProvider> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<IReadOnlyCollection<MqttTopic>> GetTopicsAsync(
        IReadOnlyCollection<AquariumDto> aquariums,
        CancellationToken cancellationToken)
    {
        if (_options.UseHardCodedTopics)
        {
            var hardCoded = _options.HardCodedTopics
                .Where(topic => !string.IsNullOrWhiteSpace(topic))
                .Select(topic => new MqttTopic(topic))
                .ToArray();

            _logger.LogInformation("Using {TopicCount} hard-coded MQTT topics.", hardCoded.Length);
            return Task.FromResult<IReadOnlyCollection<MqttTopic>>(hardCoded);
        }

        if (!_options.TopicTemplate.Contains(AquariumIdToken, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Topic template must contain the token {AquariumIdToken}.");
        }

        var topics = aquariums
            .Where(aquarium => !string.IsNullOrWhiteSpace(aquarium.Id))
            .Select(aquarium => new MqttTopic(
                _options.TopicTemplate.Replace(AquariumIdToken, aquarium.Id, StringComparison.Ordinal)))
            .ToArray();

        _logger.LogInformation("Resolved {TopicCount} MQTT topics from registry.", topics.Length);
        return Task.FromResult<IReadOnlyCollection<MqttTopic>>(topics);
    }
}
