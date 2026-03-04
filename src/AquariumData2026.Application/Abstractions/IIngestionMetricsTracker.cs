namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Tracks ingestion pipeline counters and gauges for Prometheus export.
/// </summary>
public interface IIngestionMetricsTracker
{
    void RecordMessageReceived(int payloadSizeBytes, DateTimeOffset receivedAt);
    void RecordMessageDecoded();
    void RecordMessagePublished(DateTimeOffset publishedAt);
    void RecordMessageFailed();
    IngestionMetricsSnapshot GetSnapshot();
}
