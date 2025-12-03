# Using Spector NuGet Package

This guide explains how to use the Spector package in your projects and how to publish it.

## Using the Package in Other Projects

### Option 1: Install from Local Source

If you haven't published to NuGet.org yet, you can use the package locally:

#### 1. Add Local Package Source

```bash
# Add the nupkg directory as a local package source
dotnet nuget add source /Users/yash/projects/network-inspector/nupkg --name LocalSpector
```

#### 2. Install the Package

In your target project:

```bash
dotnet add package Spector --version 1.0.0
```

Or add it manually to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Spector" Version="1.0.0" />
</ItemGroup>
```

#### 3. Integrate Spector

In your `Program.cs`:

```csharp
using Spector;

var builder = WebApplication.CreateBuilder(args);

// Add Spector
builder.Services.AddSpector();

var app = builder.Build();

// Use Spector middleware
app.UseSpector();

app.MapGet("/", () => "Hello World!");

app.Run();
```

#### 4. Run and Access UI

Start your application and navigate to:
```
http://localhost:5000/local-insights
```

---

### Option 2: Direct Package Reference

You can reference the `.nupkg` file directly without adding a package source:

```bash
dotnet add package Spector --source /Users/yash/projects/network-inspector/nupkg
```

---

## Publishing the Package

### Publish to NuGet.org

#### 1. Get an API Key

1. Create an account at [nuget.org](https://www.nuget.org)
2. Go to your account settings
3. Create a new API key with "Push" permissions

#### 2. Publish the Package

```bash
cd /Users/yash/projects/network-inspector

# Publish main package
dotnet nuget push nupkg/Spector.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json

# Optionally publish symbols package for debugging
dotnet nuget push nupkg/Spector.1.0.0.snupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

#### 3. Verify Publication

After publishing (may take a few minutes to index):
- Visit https://www.nuget.org/packages/Spector
- Install in any project: `dotnet add package Spector`

---

### Publish to Private Feed

#### GitHub Packages

```bash
# Authenticate
dotnet nuget add source https://nuget.pkg.github.com/OWNER/index.json \
  --name github \
  --username YOUR_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --store-password-in-clear-text

# Push package
dotnet nuget push nupkg/Spector.1.0.0.nupkg \
  --source github
```

#### Azure Artifacts

```bash
# Add Azure Artifacts source
dotnet nuget add source https://pkgs.dev.azure.com/ORGANIZATION/_packaging/FEED/nuget/v3/index.json \
  --name azure \
  --username az \
  --password YOUR_PAT

# Push package
dotnet nuget push nupkg/Spector.1.0.0.nupkg \
  --source azure
```

---

## Updating the Package

When making changes to Spector:

### 1. Update Version

Edit `src/Spector/Spector.csproj`:

```xml
<Version>1.0.1</Version>
```

### 2. Rebuild Package

```bash
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

### 3. Republish

```bash
dotnet nuget push nupkg/Spector.1.0.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## Testing the Package

### Create a Test Project

```bash
# Create test directory
mkdir ~/spector-test
cd ~/spector-test

# Create new web API
dotnet new webapi -n TestApi
cd TestApi

# Add local package source
dotnet nuget add source /Users/yash/projects/network-inspector/nupkg --name LocalSpector

# Install Spector
dotnet add package Spector --version 1.0.0

# Edit Program.cs to add Spector
```

### Verify Integration

1. Add `builder.Services.AddSpector();` after builder creation
2. Add `app.UseSpector();` before `app.Run();`
3. Run: `dotnet run`
4. Visit: `http://localhost:5000/local-insights`
5. Make some API calls and verify they appear in the UI

---

## Package Contents

The Spector package includes:

- **Assembly**: `Spector.dll` (compiled library)
- **Documentation**: `README.md` (package documentation)
- **Embedded Resources**: Web UI files (HTML, CSS, JS)
- **Symbols**: `Spector.snupkg` (for debugging)

---

## Troubleshooting

### Package Not Found

If `dotnet add package Spector` fails:
- Verify the package source is added: `dotnet nuget list source`
- Check the package exists: `ls nupkg/`
- Try specifying the source explicitly: `dotnet add package Spector --source LocalSpector`

### UI Not Loading

- Ensure `app.UseSpector();` is called before `app.Run();`
- Check the UI path (default: `/local-insights`)
- Verify embedded resources are included in the package

### No Traces Appearing

- Ensure `builder.Services.AddSpector();` is called
- Check that HTTP requests are being made
- Verify the middleware is registered: `app.UseSpector();`

---

## Next Steps

1. ✅ Package is built and ready to use
2. 📦 Install in other projects using local source
3. 🚀 Optionally publish to NuGet.org or private feed
4. 🔄 Update version and republish as needed

For more information, see the [README.md](file:///Users/yash/projects/network-inspector/src/Spector/README.md) in the package.
