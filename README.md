# Philiprehberger.ConfigDiff

[![CI](https://github.com/philiprehberger/dotnet-config-diff/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-config-diff/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.ConfigDiff.svg)](https://www.nuget.org/packages/Philiprehberger.ConfigDiff)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-config-diff)](LICENSE)

Compare JSON configuration files (`appsettings.json`, etc.) and get a structured diff of added, removed, changed, and unchanged keys.

## Installation

```bash
dotnet add package Philiprehberger.ConfigDiff
```

## Usage

```csharp
using Philiprehberger.ConfigDiff;

var jsonA = """
{
  "Logging": { "Level": "Warning" },
  "ConnectionStrings": { "Default": "Server=old" },
  "FeatureFlag": false
}
""";

var jsonB = """
{
  "Logging": { "Level": "Information" },
  "ConnectionStrings": { "Default": "Server=old" },
  "NewSetting": "hello"
}
""";

ConfigDiffResult diff = ConfigComparer.Compare(jsonA, jsonB);

foreach (var key in diff.Added)
    Console.WriteLine($"+ {key}");

foreach (var key in diff.Removed)
    Console.WriteLine($"- {key}");

foreach (var change in diff.Changed)
    Console.WriteLine($"~ {change.Path}: '{change.OldValue}' -> '{change.NewValue}'");

// Output:
// + NewSetting
// - FeatureFlag
// ~ Logging.Level: 'Warning' -> 'Information'
```

### Compare files

```csharp
var diff = ConfigComparer.CompareFiles("appsettings.json", "appsettings.Production.json");
```

### Compare streams

```csharp
using var streamA = File.OpenRead("appsettings.json");
using var streamB = File.OpenRead("appsettings.Production.json");
var diff = ConfigComparer.Compare(streamA, streamB);
```

### Ignore paths

Use `ignorePaths` to exclude specific keys from the comparison. Supports exact matches and wildcard prefix matches with `.*`:

```csharp
var diff = ConfigComparer.Compare(jsonA, jsonB, ignorePaths: new[]
{
    "Logging.*",           // ignore everything under Logging
    "ConnectionStrings.*"  // ignore everything under ConnectionStrings
});

// Only NewSetting and FeatureFlag will appear in the result
```

### Rich change entries

Changed entries include both the old and new values via the `ConfigChange` record:

```csharp
foreach (var change in diff.Changed)
{
    Console.WriteLine($"{change.Path}: {change.OldValue} -> {change.NewValue}");
}
```

## API

### `ConfigComparer`

| Method | Description |
|--------|-------------|
| `Compare(string jsonA, string jsonB, IEnumerable<string>? ignorePaths = null)` | Diff two JSON strings |
| `CompareFiles(string pathA, string pathB, IEnumerable<string>? ignorePaths = null)` | Read files from disk and diff them |
| `Compare(Stream streamA, Stream streamB, IEnumerable<string>? ignorePaths = null)` | Read streams and diff them |

### `ConfigDiffResult`

| Property | Type | Description |
|----------|------|-------------|
| `Added` | `IReadOnlyList<string>` | Keys present in B but not in A |
| `Removed` | `IReadOnlyList<string>` | Keys present in A but not in B |
| `Changed` | `IReadOnlyList<ConfigChange>` | Keys present in both with different values |
| `Unchanged` | `IReadOnlyList<string>` | Keys with identical values |

### `ConfigChange`

| Property | Type | Description |
|----------|------|-------------|
| `Path` | `string` | Dot-notation key path |
| `OldValue` | `string?` | Value in A |
| `NewValue` | `string?` | Value in B |

## Development

```bash
dotnet build src/Philiprehberger.ConfigDiff.csproj --configuration Release
```

## License

MIT
