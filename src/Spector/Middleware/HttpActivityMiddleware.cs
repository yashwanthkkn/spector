using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Spector.Config;

namespace Spector.Middleware;
public class HttpActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;
    private readonly SpectorOptions _opts;

    public HttpActivityMiddleware(RequestDelegate next,ActivitySource activitySource, SpectorOptions opts)
    {
        _activitySource = activitySource;
        _opts = opts;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var activity = _activitySource.StartActivity("HttpIn");

        if (activity != null)
        {
            try
            {
                await _next(context);
                activity.AddTag("test-tag", "test-value");
                activity.SetTag("http.method", context.Request.Method);
                activity.AddTag("http.path", context.Request.Path);
                activity.SetTag("http.host", context.Request.Host.Value);
                activity.SetTag("http.status_code", context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                activity.SetTag("otel.status_code", "ERROR");
                activity.SetTag("otel.status_description", ex.Message);
                throw;
            }
        }
        else
        {
            await _next(context);
        }
    }
}