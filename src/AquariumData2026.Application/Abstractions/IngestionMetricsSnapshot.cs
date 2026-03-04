namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Immutable export shape for ingestion pipeline metrics.
/// </summary>
public sealed record IngestionMetricsSnapshot(
    long MessagesReceivedTotal,
    long MessagesDecodedTotal,
    long MessagesPublishedTotal,
    long MessagesFailedTotal,
    long LastPayloadSizeBytes,
    long LastMessageTimestampUnixSeconds,
    long LastPublishTimestampUnixSeconds);
