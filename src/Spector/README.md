# Spector

A lightweight network and dependency inspector for ASP.NET Core applications. Spector captures HTTP traces (incoming and outgoing requests) and provides a real-time web UI for monitoring and debugging.

## Features

- 🔍 **HTTP Request Tracing** - Automatically captures incoming and outgoing HTTP requests
- 📊 **Real-time Monitoring** - Live updates via Server-Sent Events (SSE)
- 🎨 **Embedded Web UI** - Beautiful, responsive interface for viewing traces
- 🔗 **Dependency Tracking** - Visualize request/response chains and dependencies
- 📦 **Zero Configuration** - Works out of the box with sensible defaults
- 🚀 **Lightweight** - Minimal performance overhead

## Installation

Install the Spector NuGet package:

```bash
dotnet add package Spector
```

Or via Package Manager Console:

```powershell
Install-Package Spector
```

## Quick Start

### 1. Add Spector to your services

In your `Program.cs` or `Startup.cs`:

```csharp
using Spector;

var builder = WebApplication.CreateBuilder(args);

// Add Spector services
builder.Services.AddSpector();

// ... other service registrations

var app = builder.Build();

// Use Spector middleware
app.UseSpector();

// ... other middleware

app.Run();
```

### 2. Access the UI

Run your application and navigate to:

```
http://localhost:<your-port>/local-insights
```

You'll see a real-time dashboard showing all HTTP traces captured by your application.

## Configuration

Spector works with zero configuration, but you can customize it if needed:

```csharp
builder.Services.AddSpector();
```

### Available Options

The `SpectorOptions` class provides the following configuration:

- **`UiPath`** - Custom path for the UI (default: `/local-insights`)
- **`SseEndpoint`** - Custom SSE endpoint path (default: `/local-insights/events`)
- **`ActivitySourceName`** - Activity source name for tracing (default: `Spector`)
- **`InMemoryMaxTraces`** - Maximum number of traces to keep in memory (default: 100)

To customize options, modify the `SpectorOptions` instance after registration:

```csharp
builder.Services.AddSpector();
builder.Services.Configure<SpectorOptions>(options =>
{
    options.UiPath = "/my-custom-path";
    options.InMemoryMaxTraces = 200;
});
```

## How It Works

Spector uses ASP.NET Core's built-in diagnostics features:

1. **Activity Tracing** - Leverages `System.Diagnostics.Activity` for distributed tracing
2. **Middleware** - Captures incoming HTTP requests via middleware
3. **HTTP Handler** - Intercepts outgoing HTTP calls via `IHttpMessageHandlerBuilderFilter`
4. **In-Memory Storage** - Stores recent traces in memory for quick access
5. **SSE Streaming** - Pushes updates to the UI in real-time

## What Gets Captured

For each HTTP request/response, Spector captures:

- **Request Details**
  - HTTP method (GET, POST, etc.)
  - Full URL
  - Headers
  - Request body (when available)
  - Timestamp

- **Response Details**
  - Status code
  - Headers
  - Response body (when available)
  - Duration

- **Trace Context**
  - Trace ID
  - Span ID
  - Parent-child relationships

## Use Cases

- **Development** - Debug API calls and inspect request/response data
- **Testing** - Verify HTTP interactions during integration tests
- **Troubleshooting** - Identify slow dependencies or failing requests
- **Learning** - Understand how your application communicates with external services

## Requirements

- .NET 8.0 or later
- ASP.NET Core application

## License

MIT License - see LICENSE file for details

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Support

For issues, questions, or feature requests, please open an issue on GitHub.
