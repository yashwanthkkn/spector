using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using NetworkInspector.Storage;
using NetworkInspector.Services;
using NetworkInspector.Handlers;

namespace NetworkInspector
{
    public static class NetworkInspectorExtensions
    {
        public static IServiceCollection AddNetworkInspector(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IRequestStorage, InMemoryRequestStorage>();
            services.AddSingleton<ActivityTracker>();
            services.AddHostedService<NetworkInspectorActivityListener>();
            
            // Add the handler to capture HTTP request/response details
            services.AddTransient<NetworkInspectorHttpHandler>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NetworkInspectorHandlerFilter>());

            // services.AddHttpClient("my-client")
            //     .AddHttpMessageHandler<NetworkInspectorHttpHandler>(); 
            
            //services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, NetworkInspectorHandlerFilter>());
            
            return services;
        }

        public static IApplicationBuilder UseNetworkInspector(this IApplicationBuilder app)
        {
            return app.UseMiddleware<NetworkInspectorMiddleware>();
        }
    }
}
