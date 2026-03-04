using System.Globalization;
using System.Text;

namespace AquariumData2026.Api.Services;

internal static class PrometheusMetricsFormatter
{
    private const string MetricName = "aquarium_device_last_seen_timestamp_seconds";

    public static string BuildLastSeenMetrics(IReadOnlyDictionary<string, DateTimeOffset> snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# HELP {MetricName} Unix timestamp of the last measurement sent by the device.");
        builder.AppendLine($"# TYPE {MetricName} gauge");

        foreach (var entry in snapshot.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            var escapedDeviceId = EscapeLabel(entry.Key);
            var unixTimestamp = entry.Value.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
            builder.AppendLine($"{MetricName}{{device_id=\"{escapedDeviceId}\"}} {unixTimestamp}");
        }

        return builder.ToString();
    }

    private static string EscapeLabel(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}
