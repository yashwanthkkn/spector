using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Spector.Config;
using Spector.Handlers;
using Spector.Middleware;
using Spector.Service;
using Spector.Storage;

namespace Spector;

public static class SpectorExtensions
{
    public static IServiceCollection AddSpector(this IServiceCollection services)
    {
        var opts = new SpectorOptions();
        services.AddSingleton(opts);
        
        services.AddSingleton(sp => new InMemoryTraceStore(opts.InMemoryMaxTraces));
        services.AddHostedService<ActivityCollectorService>();
        services.AddSingleton(new ActivitySource(opts.ActivitySourceName));
        services.AddTransient<SpectorHttpHandler>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, SpectorHttpHandlerFilter>());

        return services;
    } 
    
    public static IApplicationBuilder UseSpector(this IApplicationBuilder app)
    {
        app.UseMiddleware<HttpActivityMiddleware>();
        
        var opts = app.ApplicationServices.GetRequiredService<SpectorOptions>();
        var store = app.ApplicationServices.GetRequiredService<InMemoryTraceStore>();
        
        MapSpectorSseEndpoint(app, opts, store);
        MapSpectorUiEndpoint(app, opts);
        return app;
    }

    private static void MapSpectorUiEndpoint(this IApplicationBuilder app, SpectorOptions opts)
    {
        var assembly = Assembly.GetExecutingAssembly();
        IFileProvider? embeddedProvider = null;
        var asmNs = assembly.GetName().Name ?? string.Empty;
        var tryPaths = new[] { $"{asmNs}.wwwroot", "wwwroot", $"{asmNs}" };

        foreach (var root in tryPaths)
        {
            var provider = new EmbeddedFileProvider(assembly, root);
            // quick check: see if index file exists
            var info = provider.GetFileInfo("index.html");
            if (info != null && info.Exists)
            {
                embeddedProvider = provider;
                break;
            }
        }

        if (embeddedProvider == null)
        {
            // fallback: attempt non-rooted provider
            embeddedProvider = new EmbeddedFileProvider(assembly);
        }

        // Map the UI at opts.UiPath (e.g. "/local-insights")
        var uiPath = opts.UiPath?.TrimEnd('/') ?? "/local-insights";
        
        app.Map(uiPath, branch =>
        {
            // serve default file when hitting /local-insights
            var defaultOpts = new DefaultFilesOptions { FileProvider = embeddedProvider, RequestPath = "" };
            defaultOpts.DefaultFileNames.Clear();
            defaultOpts.DefaultFileNames.Add("index.html");
            branch.UseDefaultFiles(defaultOpts);

            branch.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedProvider,
                RequestPath = ""
            });

            // If someone hits the folder root, and default files didn't pick it up, ensure index is returned
            branch.Run(async ctx =>
            {
                var file = embeddedProvider.GetFileInfo("local-insights.html");
                if (file.Exists)
                {
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                    using var stream = file.CreateReadStream();
                    await stream.CopyToAsync(ctx.Response.Body);
                }
                else
                {
                    ctx.Response.StatusCode = 404;
                    await ctx.Response.WriteAsync("Local Insights UI not found in assembly.");
                }
            });
        });
    }

    private static void MapSpectorSseEndpoint(this IApplicationBuilder app, SpectorOptions opts, InMemoryTraceStore store)
    {
        var ssePath = opts.SseEndpoint ?? "/local-insights/events";
        app.Map(ssePath, branch =>
        {
            branch.Run(async ctx =>
            {
                ctx.Response.Headers.Add("Content-Type", "text/event-stream");

                var lastIndex = -1;
                while (!ctx.RequestAborted.IsCancellationRequested)
                {
                    var items = store.GetAll();
                    if (items.Count - 1 > lastIndex)
                    {
                        for (int i = lastIndex + 1; i < items.Count; i++)
                        {
                            var json = JsonSerializer.Serialize(items[i]);
                            await ctx.Response.WriteAsync($"data: {json}\n\n");
                            await ctx.Response.Body.FlushAsync(ctx.RequestAborted);
                        }
                        lastIndex = items.Count - 1;
                    }

                    try { await Task.Delay(300, ctx.RequestAborted); } catch { break; }
                }
            });
        });
    }
}