using System.Text;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AquariumData2026.Infrastructure.Messaging;

/// <summary>
/// Publishes measurements to RabbitMQ.
/// </summary>
public sealed class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly object _sync = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task PublishAsync(string jsonPayload, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                _logger.LogWarning("Skipping RabbitMQ publish because JSON payload is empty.");
                return Task.CompletedTask;
            }

            EnsureChannel();

            var payload = Encoding.UTF8.GetBytes(jsonPayload);
            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = _options.Durable;
            properties.ContentType = "application/json";
            properties.ContentEncoding = "utf-8";

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                basicProperties: properties,
                body: payload);

            _logger.LogDebug(
                "Published JSON message to RabbitMQ exchange {Exchange} with routing key {RoutingKey}.",
                _options.ExchangeName,
                _options.RoutingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish JSON message to RabbitMQ.");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }

    private void EnsureChannel()
    {
        lock (_sync)
        {
            if (_channel is not null && _channel.IsOpen)
            {
                return;
            }

            _logger.LogInformation(
                "Opening RabbitMQ channel for {Host}:{Port} (vhost {VirtualHost}).",
                _options.Host,
                _options.Port,
                _options.VirtualHost);

            _connection?.Dispose();
            _connection = CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Direct,
                durable: _options.Durable,
                autoDelete: false);

            _logger.LogInformation(
                "RabbitMQ exchange declared: {Exchange} (durable: {Durable}).",
                _options.ExchangeName,
                _options.Durable);
        }
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.Username,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        return factory.CreateConnection();
    }
}
