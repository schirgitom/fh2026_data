namespace AquariumData2026.Application.Abstractions;

/// <summary>
/// Stores and exposes the latest timestamp reported by each device.
/// </summary>
public interface IDeviceLastSeenTracker
{
    void Record(string deviceId, DateTimeOffset timestamp);
    IReadOnlyDictionary<string, DateTimeOffset> GetSnapshot();
}
