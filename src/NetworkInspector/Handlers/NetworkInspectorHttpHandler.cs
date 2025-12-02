using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NetworkInspector.Handlers
{
    public class NetworkInspectorHttpHandler : DelegatingHandler
    {
        private readonly ILogger<NetworkInspectorHttpHandler> _logger;

        public NetworkInspectorHttpHandler(ILogger<NetworkInspectorHttpHandler> logger)
        {
            _logger = logger;
        }

        public NetworkInspectorHttpHandler(HttpMessageHandler innerHandler, ILogger<NetworkInspectorHttpHandler> logger) : base(innerHandler)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("NetworkInspectorHttpHandler invoked for {Method} {Url}", request.Method, request.RequestUri);
            
            var activity = Activity.Current;
            
            if (activity != null)
            {
                _logger.LogInformation("Activity found: {ActivityId}, OperationName: {OperationName}", activity.Id, activity.OperationName);
            
                // Add request details to activity
                activity.SetTag("http.url", request.RequestUri?.ToString());
                activity.SetTag("http.method", request.Method.ToString());
                activity.SetTag("http.request.method", request.Method.ToString());
                activity.SetTag("url.full", request.RequestUri?.ToString());
                
                // Capture request body if present
                if (request.Content != null)
                {
                    var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        activity.SetTag("http.request.body", requestBody);
                    }
                    
                    // Reset the content stream so it can be read again
                    if (request.Content is StringContent || request.Content is ByteArrayContent)
                    {
                        // For StringContent and ByteArrayContent, we need to recreate it
                        var contentType = request.Content.Headers.ContentType;
                        request.Content = new StringContent(requestBody, Encoding.UTF8, contentType?.MediaType ?? "application/json");
                    }
                }
            }

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            if (activity != null)
            {
                // Add response details to activity
                activity.SetTag("http.status_code", (int)response.StatusCode);
                activity.SetTag("http.response.status_code", (int)response.StatusCode);
                
                // Capture response body
                if (response.Content != null)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        // Truncate if too long to avoid memory issues
                        var truncatedBody = responseBody.Length > 10000 
                            ? responseBody.Substring(0, 10000) + "... (truncated)"
                            : responseBody;
                        activity.SetTag("http.response.body", truncatedBody);
                    }
                    
                    // Reset the content stream so it can be read by the caller
                    var contentBytes = Encoding.UTF8.GetBytes(responseBody);
                    response.Content = new ByteArrayContent(contentBytes);
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        response.Content.Headers.ContentType?.MediaType ?? "application/json");
                }
            }

            return response;
        }
    }
}
