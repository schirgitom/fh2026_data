using AquariumData2026.Domain.Model;

namespace AquariumData2026.Infrastructure.Decoding;

/// <summary>
/// Binary protocol mappings for metrics and units.
/// </summary>
internal static class BinaryMetricMappings
{
    public const byte ProtocolVersion = 1;
    public const int AquariumIdSize = 16;

    public static readonly IReadOnlyDictionary<byte, MetricType> MetricTypes = new Dictionary<byte, MetricType>
    {
        { 1, MetricType.WaterTemperature },
        { 2, MetricType.WaveFlow },
        { 3, MetricType.Salinity },
        { 4, MetricType.Magnesium },
        { 5, MetricType.Calcium },
        { 6, MetricType.Nitrite },
        { 7, MetricType.Nitrate },
        { 8, MetricType.Ph },
        { 9, MetricType.Alkalinity }
    };

    public static string ResolveUnit(byte unitCode) => unitCode switch
    {
        1 => MetricUnits.Celsius,
        2 => MetricUnits.LitersPerMinute,
        3 => MetricUnits.PartsPerThousand,
        4 => MetricUnits.MilligramsPerLiter,
        5 => MetricUnits.PartsPerMillion,
        6 => MetricUnits.DegreesPh,
        _ => MetricUnits.Unknown
    };
}

internal static class MetricUnits
{
    public const string Celsius = "C";
    public const string LitersPerMinute = "L/min";
    public const string PartsPerThousand = "ppt";
    public const string MilligramsPerLiter = "mg/L";
    public const string PartsPerMillion = "ppm";
    public const string DegreesPh = "pH";
    public const string Unknown = "unknown";
}
