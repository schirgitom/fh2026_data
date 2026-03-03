using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AquariumData2026.Api.HealthChecks;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqOptions _options;

    public RabbitMqHealthCheck(IOptions<RabbitMqOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.Username,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                AutomaticRecoveryEnabled = false
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            if (!connection.IsOpen || !channel.IsOpen)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ is not reachable."));
            }

            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is reachable."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ is not reachable.", ex));
        }
    }
}
