using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spector.Config;
using Spector.Models;
using Spector.Storage;

namespace Spector.Service;

public class ActivityCollectorService : BackgroundService
{
    private readonly Channel<Activity> _channel;
    private readonly ActivityListener _listener;
    private readonly InMemoryTraceStore _store;
    private readonly ILogger<ActivityCollectorService> _logger;
    private readonly SpectorOptions _opts;

    public ActivityCollectorService(InMemoryTraceStore store, ILogger<ActivityCollectorService> logger, SpectorOptions opts)
    {
        _store = store;
        _logger = logger;
        _opts = opts;

        var options = new BoundedChannelOptions(opts.CollectorChannelCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropOldest
        };

        _channel = Channel.CreateBounded<Activity>(options);

        _listener = new ActivityListener
        {
            ShouldListenTo = src => src.Name == opts.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => { /* cheap */ },
            ActivityStopped = activity =>
            {
                // Best practice: map needed fields immediately rather than holding Activity instance.
                // But we will enqueue the Activity reference for quick mapping in the consumer.
                if (!_channel.Writer.TryWrite(activity))
                {
                    // drop — keep diagnostic info limited
                }
            }
        };

        ActivitySource.AddActivityListener(_listener);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var activity in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                if (activity.OperationName == "Microsoft.AspNetCore.Hosting.HttpRequestIn")
                        continue;
                var dto = MapActivity(activity);
                _store.Add(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to map or store activity");
            }
        }
    }

    private TraceDto MapActivity(Activity a)
    {
        var tags = new Dictionary<string, string>();
        foreach (var t in a.Tags)
        {
            if(t.Key.Contains("spector"))
                tags[t.Key] = t.Value;
        }

        var eventsList = new List<TraceEventDto>();
        foreach (var ev in a.Events)
        {
            var et = new Dictionary<string,string>();
            foreach (var kv in ev.Tags) et[kv.Key] = kv.Value?.ToString() ?? "";
            eventsList.Add(new TraceEventDto { Name = ev.Name, Timestamp = ev.Timestamp, Tags = et });
        }

        return new TraceDto
        {
            Name = a.DisplayName ?? string.Empty,
            TraceId = a.TraceId.ToString(),
            SpanId = a.SpanId.ToString(),
            ParentSpanId = a.ParentSpanId.ToString(),
            StartTimeUtc = a.StartTimeUtc,
            Duration = a.Duration,
            Kind = a.Kind.ToString(),
            Tags = tags,
            Events = eventsList
        };
    }
}