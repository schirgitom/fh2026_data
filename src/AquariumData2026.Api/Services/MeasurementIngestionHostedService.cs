using AquariumData2026.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AquariumData2026.Api.Services;

/// <summary>
/// Background service that starts and stops the ingestion pipeline.
/// </summary>
public sealed class MeasurementIngestionHostedService : BackgroundService
{
    private readonly IMeasurementIngestionService _ingestionService;
    private readonly ILogger<MeasurementIngestionHostedService> _logger;

    public MeasurementIngestionHostedService(
        IMeasurementIngestionService ingestionService,
        ILogger<MeasurementIngestionHostedService> logger)
    {
        _ingestionService = ingestionService ?? throw new ArgumentNullException(nameof(ingestionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Measurement ingestion hosted service is starting.");
        await _ingestionService.StartAsync(stoppingToken).ConfigureAwait(false);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Measurement ingestion hosted service is stopping.");
        }
        finally
        {
            await _ingestionService.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }
}
