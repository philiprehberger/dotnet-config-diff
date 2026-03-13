using System.Text.Json;

namespace Philiprehberger.ConfigDiff;

/// <summary>
/// Represents a single key that changed between two configurations.
/// </summary>
public sealed record ConfigChange(string Path, string? OldValue, string? NewValue);

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
    /// <param name="jsonA">The base JSON document.</param>
    /// <param name="jsonB">The target JSON document.</param>
    /// <param name="ignorePaths">
    /// Optional paths to exclude from comparison. Supports exact matches
    /// (e.g., "Logging.Level") and wildcard prefix matches (e.g., "Logging.*").
    /// </param>
    public static ConfigDiffResult Compare(string jsonA, string jsonB, IEnumerable<string>? ignorePaths = null)
    {
        var flatA = Flatten(jsonA);
        var flatB = Flatten(jsonB);

        if (ignorePaths is not null)
        {
            var patterns = ignorePaths.ToList();
            FilterIgnored(flatA, patterns);
            FilterIgnored(flatB, patterns);
        }

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
    public static ConfigDiffResult CompareFiles(string pathA, string pathB, IEnumerable<string>? ignorePaths = null)
    {
        var jsonA = File.ReadAllText(pathA);
        var jsonB = File.ReadAllText(pathB);
        return Compare(jsonA, jsonB, ignorePaths);
    }

    /// <summary>
    /// Reads two JSON streams and compares them.
    /// </summary>
    public static ConfigDiffResult Compare(Stream streamA, Stream streamB, IEnumerable<string>? ignorePaths = null)
    {
        using var readerA = new StreamReader(streamA);
        using var readerB = new StreamReader(streamB);
        var jsonA = readerA.ReadToEnd();
        var jsonB = readerB.ReadToEnd();
        return Compare(jsonA, jsonB, ignorePaths);
    }

    private static void FilterIgnored(Dictionary<string, string?> flat, List<string> patterns)
    {
        var keysToRemove = flat.Keys.Where(key => IsIgnored(key, patterns)).ToList();
        foreach (var key in keysToRemove)
            flat.Remove(key);
    }

    private static bool IsIgnored(string key, List<string> patterns)
    {
        foreach (var pattern in patterns)
        {
            if (pattern.EndsWith(".*"))
            {
                var prefix = pattern[..^2];
                if (key == prefix || key.StartsWith(prefix + ".", StringComparison.Ordinal) || key.StartsWith(prefix + "[", StringComparison.Ordinal))
                    return true;
            }
            else if (key == pattern)
            {
                return true;
            }
        }

        return false;
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
