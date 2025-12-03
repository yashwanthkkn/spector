# Repackaging Spector After Changes

Quick reference guide for rebuilding and redistributing the Spector NuGet package after making changes.

## Quick Steps

```bash
# 1. Update version in Spector.csproj
# 2. Build the new package
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg

# 3. (Optional) Publish to NuGet.org
dotnet nuget push nupkg/Spector.X.X.X.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

---

## Step-by-Step Guide

### 1. Update Version Number

Edit [`src/Spector/Spector.csproj`](file:///Users/yash/projects/network-inspector/src/Spector/Spector.csproj):

```xml
<PropertyGroup>
    <Version>1.0.1</Version>  <!-- Increment this -->
</PropertyGroup>
```

**Version Guidelines:**
- **Patch** (1.0.X) - Bug fixes, minor changes
- **Minor** (1.X.0) - New features, backward compatible
- **Major** (X.0.0) - Breaking changes

### 2. Build the Package

From the project root:

```bash
cd /Users/yash/projects/network-inspector

# Clean previous builds (optional but recommended)
rm -rf nupkg/*.nupkg nupkg/*.snupkg

# Build new package
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

**Expected Output:**
```
✅ Successfully created package '.../nupkg/Spector.X.X.X.nupkg'
✅ Successfully created package '.../nupkg/Spector.X.X.X.snupkg'
```

### 3. Verify Package Contents (Optional)

```bash
# List package contents
unzip -l nupkg/Spector.X.X.X.nupkg

# Check for your changes
# Ensure new files are included, old files removed, etc.
```

### 4. Test Locally

Before publishing, test the new package:

```bash
# In a test project
dotnet add package Spector --version X.X.X --source /Users/yash/projects/network-inspector/nupkg

# Run and verify your changes work
dotnet run
```

### 5. Publish (Choose One)

#### Option A: Update Local Projects

If using locally, projects will automatically pick up the new version when they restore:

```bash
# In your consuming project
dotnet restore
```

#### Option B: Publish to NuGet.org

```bash
dotnet nuget push nupkg/Spector.X.X.X.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

#### Option C: Publish to Private Feed

**GitHub Packages:**
```bash
dotnet nuget push nupkg/Spector.X.X.X.nupkg \
  --source github
```

**Azure Artifacts:**
```bash
dotnet nuget push nupkg/Spector.X.X.X.nupkg \
  --source azure
```

---

## Common Scenarios

### Changed Code Files Only

```bash
# Just rebuild - version can stay the same for local testing
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

### Added New Files

Ensure new files are included in the `.csproj`:

```xml
<!-- For embedded resources (HTML, CSS, JS) -->
<ItemGroup>
    <EmbeddedResource Include="wwwroot\newfile.js" />
</ItemGroup>

<!-- For code files - automatically included -->
```

Then rebuild:
```bash
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

### Updated README or Documentation

```bash
# README.md is automatically included via:
# <None Include="README.md" Pack="true" PackagePath="\" />

# Just rebuild
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

### Breaking Changes

1. Update version to next major (e.g., 1.0.0 → 2.0.0)
2. Update README with migration guide
3. Rebuild and publish

---

## Automation Script

Create `repackage.sh` in project root:

```bash
#!/bin/bash

# Get new version from user
read -p "Enter new version (e.g., 1.0.1): " VERSION

# Update version in csproj
sed -i '' "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" src/Spector/Spector.csproj

# Clean old packages
rm -rf nupkg/*.nupkg nupkg/*.snupkg

# Build new package
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg

echo "✅ Package built: nupkg/Spector.$VERSION.nupkg"
echo ""
echo "To publish to NuGet.org:"
echo "  dotnet nuget push nupkg/Spector.$VERSION.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json"
```

Make it executable:
```bash
chmod +x repackage.sh
./repackage.sh
```

---

## Troubleshooting

### Package Already Exists

If you get "package already exists" error when pushing to NuGet.org:
- You cannot overwrite published versions
- Increment the version number and rebuild

### Changes Not Reflected

```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Rebuild package
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg

# In consuming project, force restore
dotnet restore --force
```

### Embedded Resources Not Updating

Verify in `.csproj`:
```xml
<ItemGroup>
    <EmbeddedResource Include="wwwroot\**\*" />
</ItemGroup>
```

Then clean and rebuild:
```bash
dotnet clean
dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg
```

---

## Quick Reference

| Action | Command |
|--------|---------|
| Update version | Edit `src/Spector/Spector.csproj` |
| Build package | `dotnet pack src/Spector/Spector.csproj -c Release -o ./nupkg` |
| Verify contents | `unzip -l nupkg/Spector.X.X.X.nupkg` |
| Test locally | `dotnet add package Spector --version X.X.X --source ./nupkg` |
| Publish to NuGet | `dotnet nuget push nupkg/Spector.X.X.X.nupkg --api-key KEY --source https://api.nuget.org/v3/index.json` |
| Clear cache | `dotnet nuget locals all --clear` |

---

## See Also

- [PACKAGE_USAGE.md](file:///Users/yash/projects/network-inspector/PACKAGE_USAGE.md) - Full usage guide
- [README.md](file:///Users/yash/projects/network-inspector/src/Spector/README.md) - Package documentation
