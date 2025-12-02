using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace NetworkInspector.Handlers
{
    public class NetworkInspectorHandlerFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NetworkInspectorHandlerFilter> _logger;

        public NetworkInspectorHandlerFilter(IServiceProvider serviceProvider, ILogger<NetworkInspectorHandlerFilter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                _logger.LogInformation("NetworkInspectorHandlerFilter.Configure called for client: {ClientName}", builder.Name);
                
                // run the rest of the builders first
                next(builder);

                // add our handler instance from DI (transient)
                var handler = _serviceProvider.GetRequiredService<NetworkInspectorHttpHandler>();
                if (handler != null)
                {
                    // wrapping: if there is already a primary handler assign its InnerHandler accordingly
                    // We put tracingHandler at the end of the pipeline so it is the outermost delegating handler.
                    handler.InnerHandler = builder.PrimaryHandler ?? new HttpClientHandler();
                    builder.PrimaryHandler = handler;
                }
                
                _logger.LogInformation("Added NetworkInspectorHttpHandler to client: {ClientName}", builder.Name);
            };
        }
    }
}