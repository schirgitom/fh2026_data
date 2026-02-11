namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// Options for binary payload decoding.
/// </summary>
public sealed class BinaryPayloadOptions
{
    public const string SectionName = "BinaryPayload";

    public byte ExpectedVersion { get; init; } = 1;
}
