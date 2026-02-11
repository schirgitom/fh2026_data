using AquariumData2026.Application.Abstractions;
using AquariumData2026.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AquariumData2026.Application.DependencyInjection;

/// <summary>
/// Dependency injection registrations for the application layer.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMeasurementIngestionService, MeasurementIngestionService>();
        return services;
    }
}
