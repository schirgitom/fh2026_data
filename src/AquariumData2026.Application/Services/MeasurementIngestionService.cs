using System.Threading;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using Microsoft.Extensions.Logging;

namespace AquariumData2026.Application.Services;

/// <summary>
/// Orchestrates aquarium telemetry ingestion from MQTT to RabbitMQ.
/// </summary>
public sealed class MeasurementIngestionService : IMeasurementIngestionService
{
    private readonly IAquariumRegistryClient _registryClient;
    private readonly ITopicProvider _topicProvider;
    private readonly IMqttSubscriber _mqttSubscriber;
    private readonly IMeasurementDecoder _decoder;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<MeasurementIngestionService> _logger;
    private int _started;

    public MeasurementIngestionService(
        IAquariumRegistryClient registryClient,
        ITopicProvider topicProvider,
        IMqttSubscriber mqttSubscriber,
        IMeasurementDecoder decoder,
        IMessagePublisher publisher,
        ILogger<MeasurementIngestionService> logger)
    {
        _registryClient = registryClient ?? throw new ArgumentNullException(nameof(registryClient));
        _topicProvider = topicProvider ?? throw new ArgumentNullException(nameof(topicProvider));
        _mqttSubscriber = mqttSubscriber ?? throw new ArgumentNullException(nameof(mqttSubscriber));
        _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
        {
            _logger.LogInformation("Measurement ingestion already started.");
            return;
        }

        _logger.LogInformation("Starting measurement ingestion pipeline.");

        IReadOnlyCollection<AquariumDto> aquariums;
        try
        {
            aquariums = await _registryClient.GetAquariumsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve aquariums from registry.");
            aquariums = Array.Empty<AquariumDto>();
        }

        var topics = await _topicProvider.GetTopicsAsync(aquariums, cancellationToken).ConfigureAwait(false);
        if (topics.Count == 0)
        {
            _logger.LogWarning("No MQTT topics resolved for ingestion.");
        }

        await _mqttSubscriber.SubscribeAsync(topics, HandleMessageAsync, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("MQTT subscription established.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 0) == 0)
        {
            return;
        }

        _logger.LogInformation("Stopping measurement ingestion pipeline.");
        await _mqttSubscriber.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task HandleMessageAsync(MqttMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var measurement = _decoder.Decode(message);
            await _publisher.PublishAsync(measurement, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Published measurement for aquarium {AquariumId}.", measurement.AquariumId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process MQTT message from topic {Topic}.", message.Topic);
        }
    }
}
