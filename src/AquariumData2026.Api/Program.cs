using AquariumData2026.Api.Services;
using AquariumData2026.Application.DependencyInjection;
using AquariumData2026.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<MeasurementIngestionHostedService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok("Aquarium data ingestion service running."));

app.Run();
