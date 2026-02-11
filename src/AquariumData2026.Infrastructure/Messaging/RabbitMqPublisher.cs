using System.Text.Json;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Models;
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

    public Task PublishAsync(MeasurementDto measurement, CancellationToken cancellationToken)
    {
        try
        {
            EnsureChannel();

            var payload = JsonSerializer.SerializeToUtf8Bytes(measurement);
            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = _options.Durable;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                basicProperties: properties,
                body: payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish measurement to RabbitMQ.");
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

            _connection?.Dispose();
            _connection = CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Direct,
                durable: _options.Durable,
                autoDelete: false);
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
