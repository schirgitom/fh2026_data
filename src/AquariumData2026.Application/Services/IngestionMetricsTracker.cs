using System.Threading;
using AquariumData2026.Application.Abstractions;

namespace AquariumData2026.Application.Services;

/// <summary>
/// Thread-safe in-memory tracker for ingestion pipeline metrics.
/// </summary>
public sealed class IngestionMetricsTracker : IIngestionMetricsTracker
{
    private long _messagesReceivedTotal;
    private long _messagesDecodedTotal;
    private long _messagesPublishedTotal;
    private long _messagesFailedTotal;
    private long _lastPayloadSizeBytes;
    private long _lastMessageTimestampUnixSeconds;
    private long _lastPublishTimestampUnixSeconds;

    public void RecordMessageReceived(int payloadSizeBytes, DateTimeOffset receivedAt)
    {
        Interlocked.Increment(ref _messagesReceivedTotal);
        Interlocked.Exchange(ref _lastPayloadSizeBytes, Math.Max(0, payloadSizeBytes));
        SetMax(ref _lastMessageTimestampUnixSeconds, receivedAt.ToUnixTimeSeconds());
    }

    public void RecordMessageDecoded()
    {
        Interlocked.Increment(ref _messagesDecodedTotal);
    }

    public void RecordMessagePublished(DateTimeOffset publishedAt)
    {
        Interlocked.Increment(ref _messagesPublishedTotal);
        SetMax(ref _lastPublishTimestampUnixSeconds, publishedAt.ToUnixTimeSeconds());
    }

    public void RecordMessageFailed()
    {
        Interlocked.Increment(ref _messagesFailedTotal);
    }

    public IngestionMetricsSnapshot GetSnapshot()
    {
        return new IngestionMetricsSnapshot(
            Interlocked.Read(ref _messagesReceivedTotal),
            Interlocked.Read(ref _messagesDecodedTotal),
            Interlocked.Read(ref _messagesPublishedTotal),
            Interlocked.Read(ref _messagesFailedTotal),
            Interlocked.Read(ref _lastPayloadSizeBytes),
            Interlocked.Read(ref _lastMessageTimestampUnixSeconds),
            Interlocked.Read(ref _lastPublishTimestampUnixSeconds));
    }

    private static void SetMax(ref long target, long value)
    {
        while (true)
        {
            var current = Interlocked.Read(ref target);
            if (value <= current)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref target, value, current) == current)
            {
                return;
            }
        }
    }
}
