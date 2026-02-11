namespace AquariumData2026.Infrastructure.Options;

/// <summary>
/// MQTT topic configuration used during startup.
/// </summary>
public sealed class TopicOptions
{
    public const string SectionName = "Topics";

    public bool UseHardCodedTopics { get; init; } = true;
    public string TopicTemplate { get; init; } = "aquariums/{aquariumId}/measurements";
    public string[] HardCodedTopics { get; init; } =
    [
        "aquariums/demo-reef/measurements",
        "aquariums/demo-freshwater/measurements"
    ];
}
