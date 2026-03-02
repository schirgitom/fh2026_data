using AquariumData2026.Api.Services;
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

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<MeasurementIngestionHostedService>();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapHealthChecks("/health");
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
