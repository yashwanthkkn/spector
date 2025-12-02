using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Spector.Handlers;

public class SpectorHttpHandlerFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly IServiceProvider _serviceProvider;

    public SpectorHttpHandlerFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            // run the rest of the builders first
            next(builder);

            // add our handler instance from DI (transient)
            var handler = _serviceProvider.GetRequiredService<SpectorHttpHandler>();
            if (handler != null)
            {
                // wrapping: if there is already a primary handler assign its InnerHandler accordingly
                // We put tracingHandler at the end of the pipeline so it is the outermost delegating handler.
                handler.InnerHandler = builder.PrimaryHandler ?? new HttpClientHandler();
                builder.PrimaryHandler = handler;
            }
        };
    }
}