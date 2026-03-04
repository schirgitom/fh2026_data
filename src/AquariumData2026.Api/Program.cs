using AquariumData2026.Api.Services;
using AquariumData2026.Api.HealthChecks;
using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.DependencyInjection;
using AquariumData2026.Infrastructure.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendDev", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<MeasurementIngestionHostedService>();
    builder.Services.AddHealthChecks()
        .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
        .AddCheck<MqttBrokerHealthCheck>("mqtt", tags: ["ready"])
        .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"])
        .AddCheck<RegistryApiHealthCheck>("registry", tags: ["ready"]);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("live")
    });
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready")
    });
    app.MapGet("/metrics", (IDeviceLastSeenTracker deviceLastSeenTracker) =>
    {
        var snapshot = deviceLastSeenTracker.GetSnapshot();
        var metricsPayload = PrometheusMetricsFormatter.BuildLastSeenMetrics(snapshot);
        return Results.Text(metricsPayload, "text/plain; version=0.0.4; charset=utf-8");
    });
    app.MapGet("/", () => Results.Ok("Aquarium data ingestion service running."));

    Log.Information("Starting AquariumData2026 API.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AquariumData2026 API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
