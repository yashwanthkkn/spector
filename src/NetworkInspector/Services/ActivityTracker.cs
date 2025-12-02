using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetworkInspector.Models;

namespace NetworkInspector.Services
{
    public class ActivityTracker
    {
        private readonly ConcurrentDictionary<string, List<ActivityData>> _activityByTrace = new();

        public void TrackActivity(Activity activity)
        {
            if (activity.TraceId == default) return;

            var traceId = activity.TraceId.ToString();
            var data = new ActivityData
            {
                Id = activity.Id ?? string.Empty,
                OperationName = activity.OperationName,
                DisplayName = activity.DisplayName,
                Duration = activity.Duration,
                StartTime = activity.StartTimeUtc,
                Tags = activity.Tags.ToDictionary(t => t.Key, t => t.Value ?? string.Empty),
                Events = activity.Events.Select(e => new ActivityEventData
                {
                    Name = e.Name,
                    Timestamp = e.Timestamp.UtcDateTime,
                    Tags = e.Tags.ToDictionary(t => t.Key, t => t.Value?.ToString() ?? string.Empty)
                }).ToList()
            };

            _activityByTrace.AddOrUpdate(
                traceId,
                _ => new List<ActivityData> { data },
                (_, list) => { list.Add(data); return list; }
            );
        }

        public List<ActivityData> GetActivitiesForTrace(string traceId)
        {
            return _activityByTrace.TryGetValue(traceId, out var activities) 
                ? activities 
                : new List<ActivityData>();
        }

        public void Clear()
        {
            _activityByTrace.Clear();
        }
    }

    public class ActivityData
    {
        public string Id { get; set; } = string.Empty;
        public string OperationName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime StartTime { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
        public List<ActivityEventData> Events { get; set; } = new();
    }

    public class ActivityEventData
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Tags { get; set; } = new();
    }
}
