namespace Spector.Config;

public sealed class SpectorOptions
{
    public string ActivitySourceName { get; set; } = "Spector.ActivitySource";
    public int InMemoryMaxTraces { get; set; } = 5000;
    public int CollectorChannelCapacity { get; set; } = 5000;
    public string UiPath { get; set; } = "/spector";
    public string SseEndpoint { get; set; } = "/spector/events";
    public bool RecordRequestBodies { get; set; } = true; // dev-only
    public bool RecordResponseBodies { get; set; } = true;
}