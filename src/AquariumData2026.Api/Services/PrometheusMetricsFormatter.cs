using System.Globalization;
using System.Text;
using AquariumData2026.Application.Abstractions;

namespace AquariumData2026.Api.Services;

internal static class PrometheusMetricsFormatter
{
    private const string DeviceLastSeenMetric = "aquarium_device_last_seen_timestamp_seconds";
    private const string DeviceLastSeenAgeMetric = "aquarium_device_last_seen_age_seconds";
    private const string DeviceCountMetric = "aquarium_devices_tracked_total";

    private const string MessagesReceivedMetric = "aquarium_ingestion_messages_received_total";
    private const string MessagesDecodedMetric = "aquarium_ingestion_messages_decoded_total";
    private const string MessagesPublishedMetric = "aquarium_ingestion_messages_published_total";
    private const string MessagesFailedMetric = "aquarium_ingestion_messages_failed_total";
    private const string LastPayloadSizeMetric = "aquarium_ingestion_last_payload_size_bytes";
    private const string LastMessageTimestampMetric = "aquarium_ingestion_last_message_timestamp_seconds";
    private const string LastPublishTimestampMetric = "aquarium_ingestion_last_publish_timestamp_seconds";

    public static string BuildMetrics(
        IReadOnlyDictionary<string, DateTimeOffset> deviceSnapshot,
        IngestionMetricsSnapshot ingestionSnapshot,
        DateTimeOffset now)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# HELP {MessagesReceivedMetric} Total number of MQTT messages received.");
        builder.AppendLine($"# TYPE {MessagesReceivedMetric} counter");
        builder.AppendLine($"{MessagesReceivedMetric} {ingestionSnapshot.MessagesReceivedTotal.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {MessagesDecodedMetric} Total number of messages decoded successfully.");
        builder.AppendLine($"# TYPE {MessagesDecodedMetric} counter");
        builder.AppendLine($"{MessagesDecodedMetric} {ingestionSnapshot.MessagesDecodedTotal.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {MessagesPublishedMetric} Total number of messages published to RabbitMQ.");
        builder.AppendLine($"# TYPE {MessagesPublishedMetric} counter");
        builder.AppendLine($"{MessagesPublishedMetric} {ingestionSnapshot.MessagesPublishedTotal.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {MessagesFailedMetric} Total number of message-processing failures.");
        builder.AppendLine($"# TYPE {MessagesFailedMetric} counter");
        builder.AppendLine($"{MessagesFailedMetric} {ingestionSnapshot.MessagesFailedTotal.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {LastPayloadSizeMetric} Payload size in bytes of the most recently received message.");
        builder.AppendLine($"# TYPE {LastPayloadSizeMetric} gauge");
        builder.AppendLine($"{LastPayloadSizeMetric} {ingestionSnapshot.LastPayloadSizeBytes.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {LastMessageTimestampMetric} Unix timestamp of the most recently received message.");
        builder.AppendLine($"# TYPE {LastMessageTimestampMetric} gauge");
        builder.AppendLine($"{LastMessageTimestampMetric} {ingestionSnapshot.LastMessageTimestampUnixSeconds.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {LastPublishTimestampMetric} Unix timestamp of the most recently published message.");
        builder.AppendLine($"# TYPE {LastPublishTimestampMetric} gauge");
        builder.AppendLine($"{LastPublishTimestampMetric} {ingestionSnapshot.LastPublishTimestampUnixSeconds.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {DeviceCountMetric} Number of devices currently tracked in memory.");
        builder.AppendLine($"# TYPE {DeviceCountMetric} gauge");
        builder.AppendLine($"{DeviceCountMetric} {deviceSnapshot.Count.ToString(CultureInfo.InvariantCulture)}");

        builder.AppendLine($"# HELP {DeviceLastSeenMetric} Unix timestamp of the last measurement sent by the device.");
        builder.AppendLine($"# TYPE {DeviceLastSeenMetric} gauge");

        builder.AppendLine($"# HELP {DeviceLastSeenAgeMetric} Number of seconds since the last measurement sent by the device.");
        builder.AppendLine($"# TYPE {DeviceLastSeenAgeMetric} gauge");

        foreach (var entry in deviceSnapshot.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            var escapedDeviceId = EscapeLabel(entry.Key);
            var unixTimestamp = entry.Value.ToUnixTimeSeconds();
            var ageSeconds = Math.Max(0, now.ToUnixTimeSeconds() - unixTimestamp);

            builder.AppendLine($"{DeviceLastSeenMetric}{{device_id=\"{escapedDeviceId}\"}} {unixTimestamp.ToString(CultureInfo.InvariantCulture)}");
            builder.AppendLine($"{DeviceLastSeenAgeMetric}{{device_id=\"{escapedDeviceId}\"}} {ageSeconds.ToString(CultureInfo.InvariantCulture)}");
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
