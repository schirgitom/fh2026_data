namespace AquariumData2026.Domain.Model;

/// <summary>
/// Supported metric types in the aquarium telemetry stream.
/// </summary>
public enum MetricType : byte
{
    WaterTemperature = 1,
    WaveFlow = 2,
    Salinity = 3,
    Magnesium = 4,
    Calcium = 5,
    Nitrite = 6,
    Nitrate = 7,
    Ph = 8,
    Alkalinity = 9
}
