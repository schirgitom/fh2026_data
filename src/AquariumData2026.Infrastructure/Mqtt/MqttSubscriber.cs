using System.Buffers;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace AquariumData2026.Infrastructure.Mqtt;

/// <summary>
/// MQTT subscriber based on MQTTnet client with manual reconnect handling.
/// </summary>
public sealed class MqttSubscriber : IMqttSubscriber, IAsyncDisposable
{
    private readonly IMqttClient _client;
    private readonly MqttOptions _options;
    private readonly ILogger<MqttSubscriber> _logger;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private readonly object _reconnectSync = new();
    private readonly MqttClientOptions _clientOptions;
    private Func<MqttMessage, CancellationToken, Task>? _handler;
    private bool _started;
    private IReadOnlyCollection<MqttTopic> _topics = Array.Empty<MqttTopic>();
    private CancellationTokenSource? _reconnectCts;
    private Task? _reconnectTask;

    public MqttSubscriber(IOptions<MqttOptions> options, ILogger<MqttSubscriber> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _client = new MqttClientFactory().CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        _client.ConnectedAsync += args =>
        {
            _logger.LogInformation("Connected to MQTT broker as {ClientId}.", _options.ClientId);
            return Task.CompletedTask;
        };
        _client.DisconnectedAsync += OnDisconnectedAsync;
        _clientOptions = BuildClientOptions();
    }

    public async Task SubscribeAsync(
        IReadOnlyCollection<MqttTopic> topics,
        Func<MqttMessage, CancellationToken, Task> onMessage,
        CancellationToken cancellationToken)
    {
        if (onMessage is null)
        {
            throw new ArgumentNullException(nameof(onMessage));
        }

        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _handler = onMessage;
            _topics = topics;

            if (!_started)
            {
                await _client.ConnectAsync(_clientOptions, cancellationToken).ConfigureAwait(false);
                _started = true;
            }

            if (topics.Count == 0)
            {
                _logger.LogWarning("No MQTT topics provided for subscription.");
                return;
            }

            await SubscribeInternalAsync(topics, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _sync.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_started)
            {
                return;
            }

            _started = false;
            CancelReconnect();

            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(new MqttClientDisconnectOptions()).ConfigureAwait(false);
            }
        }
        finally
        {
            _sync.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        _client.Dispose();
        _sync.Dispose();
    }

    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (_handler is null)
        {
            _logger.LogWarning("MQTT message received before handler initialization.");
            return Task.CompletedTask;
        }

        var payload = args.ApplicationMessage.Payload.ToArray();
        var message = new MqttMessage(
            args.ApplicationMessage.Topic,
            payload,
            DateTimeOffset.UtcNow);

        return _handler(message, CancellationToken.None);
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        if (!_started)
        {
            return Task.CompletedTask;
        }

        _logger.LogWarning("Disconnected from MQTT broker: {Reason}.", args.Reason);
        StartReconnectLoop();
        return Task.CompletedTask;
    }

    private async Task SubscribeInternalAsync(
        IReadOnlyCollection<MqttTopic> topics,
        CancellationToken cancellationToken)
    {
        var subscribeBuilder = new MqttClientSubscribeOptionsBuilder();
        foreach (var topic in topics)
        {
            subscribeBuilder.WithTopicFilter(filter => filter.WithTopic(topic.Value));
        }

        await _client.SubscribeAsync(subscribeBuilder.Build(), cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Subscribed to {TopicCount} MQTT topics.", topics.Count);
    }

    private void StartReconnectLoop()
    {
        lock (_reconnectSync)
        {
            if (_reconnectTask is { IsCompleted: false })
            {
                return;
            }

            _reconnectCts?.Cancel();
            _reconnectCts = new CancellationTokenSource();
            var token = _reconnectCts.Token;
            _reconnectTask = Task.Run(() => ReconnectAsync(token), token);
        }
    }

    private async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _client.ConnectAsync(_clientOptions, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Reconnected to MQTT broker.");

                if (_topics.Count > 0)
                {
                    await SubscribeInternalAsync(_topics, cancellationToken).ConfigureAwait(false);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "MQTT reconnect attempt failed. Retrying in {DelaySeconds}s.",
                    _options.ReconnectDelaySeconds);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.ReconnectDelaySeconds), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private void CancelReconnect()
    {
        lock (_reconnectSync)
        {
            _reconnectCts?.Cancel();
            _reconnectCts?.Dispose();
            _reconnectCts = null;
            _reconnectTask = null;
        }
    }

    private MqttClientOptions BuildClientOptions()
    {
        var clientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithClientId(_options.ClientId)
            .WithTcpServer(_options.Host, _options.Port)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            clientOptionsBuilder = clientOptionsBuilder.WithCredentials(_options.Username, _options.Password);
        }

        if (_options.UseTls)
        {
            clientOptionsBuilder = clientOptionsBuilder.WithTlsOptions(tls => tls.UseTls());
        }

        return clientOptionsBuilder.Build();
    }
}
