using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        if (context.Request.Path.StartsWithSegments("/spector"))
        {
            await _next(context);
            return;
        }
        using var activity = _activitySource.StartActivity("HttpIn");

        if (activity != null)
        {
            string? requestBody = null;
            string? responseBody = null;

            try
            {
                // Enable request body buffering to allow multiple reads
                if (_opts.RecordRequestBodies)
                {
                    context.Request.EnableBuffering();
                    
                    // Read the request body
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    
                    // Reset the stream position for the next middleware
                    context.Request.Body.Position = 0;
                }

                // Capture response body by replacing the response stream
                Stream originalResponseBody = context.Response.Body;
                
                if (_opts.RecordResponseBodies)
                {
                    using var responseBodyStream = new MemoryStream();
                    context.Response.Body = responseBodyStream;

                    try
                    {
                        await _next(context);

                        // Read the response body
                        responseBodyStream.Position = 0;
                        using var reader = new StreamReader(responseBodyStream);
                        responseBody = await reader.ReadToEndAsync();

                        // Copy the response back to the original stream
                        responseBodyStream.Position = 0;
                        await responseBodyStream.CopyToAsync(originalResponseBody);
                    }
                    finally
                    {
                        context.Response.Body = originalResponseBody;
                    }
                }
                else
                {
                    await _next(context);
                }

                // Add tags after processing
                activity.AddTag("spector.type", "http");
                activity.AddTag("spector.url", context.Request.Path);
                activity.AddTag("spector.method", context.Request.Method);
                
                if (_opts.RecordRequestBodies && requestBody != null)
                {
                    activity.AddTag("spector.requestBody", requestBody);
                }
                
                if (_opts.RecordResponseBodies && responseBody != null)
                {
                    activity.AddTag("spector.responseBody", responseBody);
                }
                
                activity.AddTag("spector.status", context.Response.StatusCode.ToString());
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        else
        {
            await _next(context);
        }
    }
}