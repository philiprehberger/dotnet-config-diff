using Xunit;
namespace Philiprehberger.ConfigDiff.Tests;

public class ConfigComparerTests
{
    [Fact]
    public void Compare_IdenticalJson_ReturnsNoChanges()
    {
        var json = """{"Key": "Value"}""";

        var result = ConfigComparer.Compare(json, json);

        Assert.Empty(result.Added);
        Assert.Empty(result.Removed);
        Assert.Empty(result.Changed);
        Assert.Single(result.Unchanged);
        Assert.Equal("Key", result.Unchanged[0]);
    }

    [Fact]
    public void Compare_AddedKey_DetectedInAddedList()
    {
        var jsonA = """{"A": "1"}""";
        var jsonB = """{"A": "1", "B": "2"}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Single(result.Added);
        Assert.Equal("B", result.Added[0]);
        Assert.Empty(result.Removed);
        Assert.Empty(result.Changed);
    }

    [Fact]
    public void Compare_RemovedKey_DetectedInRemovedList()
    {
        var jsonA = """{"A": "1", "B": "2"}""";
        var jsonB = """{"A": "1"}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Empty(result.Added);
        Assert.Single(result.Removed);
        Assert.Equal("B", result.Removed[0]);
    }

    [Fact]
    public void Compare_ChangedValue_DetectedInChangedList()
    {
        var jsonA = """{"Key": "old"}""";
        var jsonB = """{"Key": "new"}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Empty(result.Added);
        Assert.Empty(result.Removed);
        Assert.Single(result.Changed);
        Assert.Equal("Key", result.Changed[0].Path);
        Assert.Equal("old", result.Changed[0].OldValue);
        Assert.Equal("new", result.Changed[0].NewValue);
    }

    [Fact]
    public void Compare_NestedObjects_UsesDotNotation()
    {
        var jsonA = """{"Parent": {"Child": "1"}}""";
        var jsonB = """{"Parent": {"Child": "2"}}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Single(result.Changed);
        Assert.Equal("Parent.Child", result.Changed[0].Path);
    }

    [Fact]
    public void Compare_ArrayElements_UsesIndexNotation()
    {
        var jsonA = """{"Items": ["a", "b"]}""";
        var jsonB = """{"Items": ["a", "c"]}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Single(result.Changed);
        Assert.Equal("Items[1]", result.Changed[0].Path);
    }

    [Fact]
    public void Compare_WithIgnorePaths_ExcludesExactMatch()
    {
        var jsonA = """{"A": "1", "B": "old"}""";
        var jsonB = """{"A": "1", "B": "new"}""";

        var result = ConfigComparer.Compare(jsonA, jsonB, ignorePaths: ["B"]);

        Assert.Empty(result.Changed);
        Assert.Single(result.Unchanged);
    }

    [Fact]
    public void Compare_WithIgnorePathsWildcard_ExcludesNestedKeys()
    {
        var jsonA = """{"Logging": {"Level": "Info", "Provider": "Console"}, "App": "v1"}""";
        var jsonB = """{"Logging": {"Level": "Debug", "Provider": "File"}, "App": "v1"}""";

        var result = ConfigComparer.Compare(jsonA, jsonB, ignorePaths: ["Logging.*"]);

        Assert.Empty(result.Changed);
    }

    [Fact]
    public void Compare_EmptyObjects_ReturnsEmptyResult()
    {
        var result = ConfigComparer.Compare("{}", "{}");

        Assert.Empty(result.Added);
        Assert.Empty(result.Removed);
        Assert.Empty(result.Changed);
        Assert.Empty(result.Unchanged);
    }

    [Fact]
    public void Compare_NullValue_HandledCorrectly()
    {
        var jsonA = """{"Key": "value"}""";
        var jsonB = """{"Key": null}""";

        var result = ConfigComparer.Compare(jsonA, jsonB);

        Assert.Single(result.Changed);
        Assert.Equal("value", result.Changed[0].OldValue);
        Assert.Null(result.Changed[0].NewValue);
    }

    [Fact]
    public void Compare_StreamOverload_ProducesSameResult()
    {
        var jsonA = """{"Key": "old"}""";
        var jsonB = """{"Key": "new"}""";

        using var streamA = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonA));
        using var streamB = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonB));

        var result = ConfigComparer.Compare(streamA, streamB);

        Assert.Single(result.Changed);
        Assert.Equal("Key", result.Changed[0].Path);
    }
}
