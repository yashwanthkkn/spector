using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetworkInspector.Services
{
    public class NetworkInspectorActivityListener : IHostedService
    {
        private readonly ActivityTracker _tracker;
        private readonly ILogger<NetworkInspectorActivityListener> _logger;
        private ActivityListener? _listener;

        public NetworkInspectorActivityListener(ActivityTracker tracker, ILogger<NetworkInspectorActivityListener> logger)
        {
            _tracker = tracker;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new ActivityListener
            {
                ShouldListenTo = source =>
                {
                    // Listen to ASP.NET Core, HttpClient, and EF Core activities
                    var shouldListen = source.Name == "Microsoft.AspNetCore" ||
                           source.Name == "System.Net.Http" ||
                           source.Name == "Microsoft.EntityFrameworkCore";
                    
                    if (shouldListen)
                    {
                        _logger.LogDebug("Listening to ActivitySource: {SourceName}", source.Name);
                    }
                    
                    return shouldListen;
                },
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity =>
                {
                    // Only track activities that have completed
                    if (activity != null)
                    {
                        _logger.LogDebug("Activity stopped: {OperationName}, TraceId: {TraceId}, Tags: {TagCount}", 
                            activity.OperationName, activity.TraceId, activity.Tags.Count());
                        _tracker.TrackActivity(activity);
                    }
                }
            };

            ActivitySource.AddActivityListener(_listener);
            _logger.LogInformation("NetworkInspectorActivityListener started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _listener?.Dispose();
            _logger.LogInformation("NetworkInspectorActivityListener stopped");
            return Task.CompletedTask;
        }
    }
}
