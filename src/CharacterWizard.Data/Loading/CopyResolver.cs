using System.Text.Json;
using System.Text.Json.Nodes;

namespace CharacterWizard.Data.Loading;

/// <summary>
/// Merges a 5etools "_copy" child entry into its parent: child fields override
/// parent fields shallowly (arrays/objects are replaced, not deep-merged).
/// The `_copy` key itself is stripped from the result.
///
/// 5etools supports `_mod` / `_preserve` operators for finer control; the MVP
/// ignores those — practical effect is that copied entries which would patch
/// only a portion of a parent array will instead inherit the parent's array
/// when the child doesn't supply one, and replace it fully when it does.
/// </summary>
public static class CopyResolver
{
    public static JsonElement Merge(JsonElement parent, JsonElement child)
    {
        var merged = new JsonObject();

        // Start from parent fields…
        if (parent.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in parent.EnumerateObject())
            {
                if (prop.NameEquals("_copy")) continue;
                merged[prop.Name] = JsonNode.Parse(prop.Value.GetRawText());
            }
        }

        // …then overlay child fields (replacing).
        if (child.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in child.EnumerateObject())
            {
                if (prop.NameEquals("_copy")) continue;
                merged[prop.Name] = JsonNode.Parse(prop.Value.GetRawText());
            }
        }

        // Round-trip to a fresh JsonElement so callers can re-deserialize freely.
        return JsonDocument.Parse(merged.ToJsonString()).RootElement.Clone();
    }

    /// <summary>
    /// Reads the `_copy` reference of an entry (returns null if no _copy field).
    /// Format: { "_copy": { "name": "...", "source": "..." } }.
    /// </summary>
    public static (string Name, string Source)? GetCopyRef(JsonElement entry)
    {
        if (entry.ValueKind != JsonValueKind.Object) return null;
        if (!entry.TryGetProperty("_copy", out var c) || c.ValueKind != JsonValueKind.Object) return null;
        if (!c.TryGetProperty("name", out var n) || n.ValueKind != JsonValueKind.String) return null;
        if (!c.TryGetProperty("source", out var s) || s.ValueKind != JsonValueKind.String) return null;
        return (n.GetString()!, s.GetString()!);
    }
}
