using System.Collections.Concurrent;
using AquariumData2026.Application.Abstractions;

namespace AquariumData2026.Application.Services;

/// <summary>
/// In-memory tracker for last seen timestamps per aquarium/device.
/// </summary>
public sealed class DeviceLastSeenTracker : IDeviceLastSeenTracker
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastSeenByDevice = new(StringComparer.OrdinalIgnoreCase);

    public void Record(string deviceId, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        _lastSeenByDevice.AddOrUpdate(
            deviceId,
            timestamp,
            (_, current) => timestamp > current ? timestamp : current);
    }

    public IReadOnlyDictionary<string, DateTimeOffset> GetSnapshot()
    {
        return new Dictionary<string, DateTimeOffset>(_lastSeenByDevice, StringComparer.OrdinalIgnoreCase);
    }
}
