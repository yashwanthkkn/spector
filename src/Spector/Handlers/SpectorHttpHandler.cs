using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Spector.Config;

namespace Spector.Handlers;

public class SpectorHttpHandler : DelegatingHandler
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<SpectorHttpHandler> _logger;

    public SpectorHttpHandler(ActivitySource activitySource, SpectorOptions opts, ILogger<SpectorHttpHandler> logger)
    {
        _activitySource = activitySource;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("HttpOut");

        if (activity != null)
        {
            activity.AddTag("spector.type", "http");
            activity.AddTag("spector.url", request.RequestUri?.ToString());
            activity.AddTag("spector.method", request.Method.ToString());

            // Capture request body if present
            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(requestBody))
                {
                    activity.AddTag("spector.requestBody", requestBody);
                }

                // Reset the content stream so it can be read again
                if (request.Content is StringContent || request.Content is ByteArrayContent)
                {
                    // For StringContent and ByteArrayContent, we need to recreate it
                    var contentType = request.Content.Headers.ContentType;
                    request.Content = new StringContent(requestBody, Encoding.UTF8,
                        contentType?.MediaType ?? "application/json");
                }
            }
        }

        HttpResponseMessage response;
        
        try
        {
            // Send the request
            response = await base.SendAsync(request, cancellationToken);

            if (activity != null)
            {
                // Add response details to activity
                activity.AddTag("spector.status", ((int)response.StatusCode).ToString());

                // Capture response body
                if (response.Content != null)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        activity.AddTag("spector.responseBody", responseBody);
                    }

                    // Reset the content stream so it can be read by the caller
                    var contentBytes = Encoding.UTF8.GetBytes(responseBody);
                    response.Content = new ByteArrayContent(contentBytes);
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        response.Content.Headers.ContentType?.MediaType ?? "application/json");
                }
            }
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.AddTag("spector.status", "500");
                activity.AddTag("spector.responseBody", JsonSerializer.Serialize(new { message = ex.Message }));
            }
            
            throw;
        }

        return response;
    }
}