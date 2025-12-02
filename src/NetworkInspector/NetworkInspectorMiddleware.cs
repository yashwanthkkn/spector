using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NetworkInspector.Models;
using NetworkInspector.Storage;
using NetworkInspector.Services;

namespace NetworkInspector
{
    public class NetworkInspectorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRequestStorage _storage;
        private readonly ActivityTracker _activityTracker;

        public NetworkInspectorMiddleware(RequestDelegate next, IRequestStorage storage, ActivityTracker activityTracker)
        {
            _next = next;
            _storage = storage;
            _activityTracker = activityTracker;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/_inspector"))
            {
                await HandleInspectorRequest(context);
                return;
            }

            var requestModel = new RequestModel
            {
                Method = context.Request.Method,
                Url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                Timestamp = DateTime.UtcNow,
                TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty
            };

            foreach (var header in context.Request.Headers)
            {
                requestModel.RequestHeaders[header.Key] = header.Value.ToString();
            }

            context.Request.EnableBuffering();

            if (context.Request.Body.CanRead)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                requestModel.RequestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                requestModel.DurationMs = stopwatch.ElapsedMilliseconds;
                requestModel.StatusCode = context.Response.StatusCode;

                foreach (var header in context.Response.Headers)
                {
                    requestModel.ResponseHeaders[header.Key] = header.Value.ToString();
                }

                responseBody.Position = 0;
                var responseString = await new StreamReader(responseBody).ReadToEndAsync();
                requestModel.ResponseBody = responseString;

                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalBodyStream);

                // Enrich with child activities (DB queries, HTTP calls, etc.)
                if (!string.IsNullOrEmpty(requestModel.TraceId))
                {
                    var activities = _activityTracker.GetActivitiesForTrace(requestModel.TraceId);
                    foreach (var activity in activities)
                    {
                        // Skip the main ASP.NET Core request activity
                        if (activity.OperationName == "Microsoft.AspNetCore.Hosting.HttpRequestIn")
                            continue;

                        var childEvent = new Dictionary<string, object>
                        {
                            ["Type"] = GetActivityType(activity.OperationName),
                            ["OperationName"] = activity.OperationName,
                            ["DisplayName"] = activity.DisplayName,
                            ["DurationMs"] = activity.Duration.TotalMilliseconds,
                            ["Tags"] = activity.Tags
                        };

                        // Extract HTTP-specific data from tags
                        if (activity.OperationName.Contains("Http"))
                        {
                            if (activity.Tags.TryGetValue("http.url", out var url))
                                childEvent["Url"] = url;
                            if (activity.Tags.TryGetValue("http.method", out var method))
                                childEvent["Method"] = method;
                            if (activity.Tags.TryGetValue("http.status_code", out var statusCode))
                                childEvent["StatusCode"] = statusCode;
                            if (activity.Tags.TryGetValue("http.request.method", out var reqMethod))
                                childEvent["Method"] = reqMethod;
                            if (activity.Tags.TryGetValue("url.full", out var fullUrl))
                                childEvent["Url"] = fullUrl;
                            if (activity.Tags.TryGetValue("http.response.status_code", out var resStatus))
                                childEvent["StatusCode"] = resStatus;
                        }

                        // Extract DB-specific data from tags
                        if (activity.OperationName.Contains("EntityFrameworkCore") || activity.OperationName.Contains("Database"))
                        {
                            if (activity.Tags.TryGetValue("db.statement", out var statement))
                                childEvent["CommandText"] = statement;
                            if (activity.Tags.TryGetValue("db.system", out var dbSystem))
                                childEvent["DatabaseSystem"] = dbSystem;
                        }

                        requestModel.ChildEvents.Add(childEvent);
                    }
                }

                _storage.Add(requestModel);
            }
        }

        private string GetActivityType(string operationName)
        {
            if (operationName.Contains("EntityFrameworkCore") || operationName.Contains("Database"))
                return "Database";
            if (operationName.Contains("Http"))
                return "OutgoingHttp";
            return "Other";
        }

        private async Task HandleInspectorRequest(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/_inspector/api/requests"))
            {
                if (context.Request.Path.Value?.EndsWith("/details") == true)
                {
                     var id = context.Request.Query["id"].ToString();
                     var request = _storage.Get(id);
                     if (request != null)
                     {
                         context.Response.ContentType = "application/json";
                         await context.Response.WriteAsync(JsonSerializer.Serialize(request));
                     }
                     else
                     {
                         context.Response.StatusCode = 404;
                     }
                     return;
                }

                var requests = _storage.GetAll();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(requests));
                return;
            }

            if (context.Request.Path.Value == "/_inspector" || context.Request.Path.Value == "/_inspector/")
            {
                context.Response.ContentType = "text/html";
                
                var assembly = typeof(NetworkInspectorMiddleware).Assembly;
                var resourceName = "NetworkInspector.UI.index.html";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    var html = await reader.ReadToEndAsync();
                    await context.Response.WriteAsync(html);
                }
                else
                {
                    await context.Response.WriteAsync("<h1>Error: UI Resource not found</h1>");
                }
                return;
            }
            
            context.Response.StatusCode = 404;
        }
    }
}
