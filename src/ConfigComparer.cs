using System.Text.Json;

namespace Philiprehberger.ConfigDiff;

/// <summary>
/// Represents a single key that changed between two configurations.
/// </summary>
public sealed record ConfigChange(string Key, string? OldValue, string? NewValue);

/// <summary>
/// The full result of comparing two JSON configuration documents.
/// </summary>
public sealed record ConfigDiffResult(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed,
    IReadOnlyList<ConfigChange> Changed,
    IReadOnlyList<string> Unchanged
);

/// <summary>
/// Compares two JSON configuration documents and returns a <see cref="ConfigDiffResult"/>.
/// </summary>
public static class ConfigComparer
{
    /// <summary>
    /// Compares two JSON strings and returns the differences.
    /// </summary>
    public static ConfigDiffResult Compare(string jsonA, string jsonB)
    {
        var flatA = Flatten(jsonA);
        var flatB = Flatten(jsonB);

        var added = new List<string>();
        var removed = new List<string>();
        var changed = new List<ConfigChange>();
        var unchanged = new List<string>();

        foreach (var key in flatB.Keys)
        {
            if (!flatA.TryGetValue(key, out var oldVal))
                added.Add(key);
            else if (oldVal != flatB[key])
                changed.Add(new ConfigChange(key, oldVal, flatB[key]));
            else
                unchanged.Add(key);
        }

        foreach (var key in flatA.Keys)
        {
            if (!flatB.ContainsKey(key))
                removed.Add(key);
        }

        added.Sort(StringComparer.Ordinal);
        removed.Sort(StringComparer.Ordinal);
        unchanged.Sort(StringComparer.Ordinal);

        return new ConfigDiffResult(added, removed, changed, unchanged);
    }

    /// <summary>
    /// Reads two JSON files from disk and compares them.
    /// </summary>
    public static ConfigDiffResult CompareFiles(string pathA, string pathB)
    {
        var jsonA = File.ReadAllText(pathA);
        var jsonB = File.ReadAllText(pathB);
        return Compare(jsonA, jsonB);
    }

    // Recursively flattens a JSON document to dot-notation key/value pairs.
    private static Dictionary<string, string?> Flatten(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);
        FlattenElement(doc.RootElement, string.Empty, result);
        return result;
    }

    private static void FlattenElement(
        JsonElement element,
        string prefix,
        Dictionary<string, string?> result)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    FlattenElement(prop.Value, key, result);
                }
                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    var key = $"{prefix}[{index}]";
                    FlattenElement(item, key, result);
                    index++;
                }
                break;

            case JsonValueKind.Null:
                result[prefix] = null;
                break;

            default:
                result[prefix] = element.ToString();
                break;
        }
    }
}
