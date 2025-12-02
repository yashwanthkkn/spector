namespace Spector.Models;

public record TraceEventDto
{
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public Dictionary<string,string> Tags { get; init; } = new();
}

public record TraceDto
{
    public string Name { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public string SpanId { get; init; } = string.Empty;
    public string ParentSpanId { get; init; } = string.Empty;
    public DateTime StartTimeUtc { get; init; }
    public TimeSpan Duration { get; init; }
    public string Kind { get; init; } = string.Empty;
    public Dictionary<string,string> Tags { get; init; } = new();
    public List<TraceEventDto> Events { get; init; } = new();
}