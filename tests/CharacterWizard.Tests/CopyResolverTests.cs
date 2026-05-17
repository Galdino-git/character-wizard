using System.Text.Json;
using CharacterWizard.Data.Loading;

namespace CharacterWizard.Tests;

public class CopyResolverTests
{
    private static JsonElement P(string json) =>
        JsonDocument.Parse(json).RootElement.Clone();

    [Fact]
    public void Child_overrides_simple_field()
    {
        var parent = P("""{ "name":"Elf", "source":"PHB", "size":["M"] }""");
        var child  = P("""{ "_copy":{"name":"Elf","source":"PHB"}, "name":"High Elf" }""");

        var merged = CopyResolver.Merge(parent, child);

        Assert.Equal("High Elf", merged.GetProperty("name").GetString());
        Assert.Equal("PHB", merged.GetProperty("source").GetString());
        Assert.Equal(JsonValueKind.Array, merged.GetProperty("size").ValueKind);
    }

    [Fact]
    public void Child_adds_new_field()
    {
        var parent = P("""{ "name":"Elf", "source":"PHB" }""");
        var child  = P("""{ "_copy":{"name":"Elf","source":"PHB"}, "name":"High Elf", "ability":[{"int":1}] }""");

        var merged = CopyResolver.Merge(parent, child);

        Assert.True(merged.TryGetProperty("ability", out _));
    }

    [Fact]
    public void Child_array_replaces_parent_array()
    {
        var parent = P("""{ "name":"Elf", "source":"PHB", "entries":["base trait"] }""");
        var child  = P("""{ "_copy":{"name":"Elf","source":"PHB"}, "name":"High Elf", "entries":["high trait"] }""");

        var merged = CopyResolver.Merge(parent, child);
        var entries = merged.GetProperty("entries");
        Assert.Equal(1, entries.GetArrayLength());
        Assert.Equal("high trait", entries[0].GetString());
    }

    [Fact]
    public void Copy_field_is_stripped_from_result()
    {
        var parent = P("""{ "name":"Elf", "source":"PHB" }""");
        var child  = P("""{ "_copy":{"name":"Elf","source":"PHB"}, "name":"High Elf" }""");

        var merged = CopyResolver.Merge(parent, child);
        Assert.False(merged.TryGetProperty("_copy", out _));
    }
}
