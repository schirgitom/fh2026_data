using AquariumData2026.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MQTTnet;

namespace AquariumData2026.Api.HealthChecks;

public sealed class MqttBrokerHealthCheck : IHealthCheck
{
    private readonly MqttOptions _options;

    public MqttBrokerHealthCheck(IOptions<MqttOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            using var client = new MqttClientFactory().CreateMqttClient();
            var builder = new MqttClientOptionsBuilder()
                .WithClientId($"{_options.ClientId}-health")
                .WithTcpServer(_options.Host, _options.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveSeconds));

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                builder = builder.WithCredentials(_options.Username, _options.Password);
            }

            if (_options.UseTls)
            {
                builder = builder.WithTlsOptions(tls => tls.UseTls());
            }

            await client.ConnectAsync(builder.Build(), timeoutCts.Token).ConfigureAwait(false);
            if (client.IsConnected)
            {
                await client.DisconnectAsync().ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy("MQTT broker is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MQTT broker is not reachable.", ex);
        }
    }
}
