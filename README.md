# Philiprehberger.ConfigDiff

Compare JSON configuration files (`appsettings.json`, etc.) and get a structured diff of added, removed, changed, and unchanged keys.

## Install

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
    Console.WriteLine($"~ {change.Key}: '{change.OldValue}' -> '{change.NewValue}'");

// Output:
// + NewSetting
// - FeatureFlag
// ~ Logging.Level: 'Warning' -> 'Information'
```

### Compare files

```csharp
var diff = ConfigComparer.CompareFiles("appsettings.json", "appsettings.Production.json");
```

## API

### `ConfigComparer`

| Method | Description |
|--------|-------------|
| `Compare(string jsonA, string jsonB)` | Diff two JSON strings |
| `CompareFiles(string pathA, string pathB)` | Read files from disk and diff them |

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
| `Key` | `string` | Dot-notation key path |
| `OldValue` | `string?` | Value in A |
| `NewValue` | `string?` | Value in B |

## License

MIT
