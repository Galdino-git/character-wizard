using System.Text.Json;
using CharacterWizard.Data.Models;

namespace CharacterWizard.Data.Loading;

/// <summary>
/// Loads JSON data files imported from 5etools into an in-memory <see cref="Catalog"/>.
/// </summary>
public sealed class CatalogLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public Catalog Load(string dataRoot)
    {
        var paths = new CatalogPaths(dataRoot);
        if (!Directory.Exists(paths.JsonRoot))
            throw new DirectoryNotFoundException($"5etools JSON folder not found: {paths.JsonRoot}");

        var books       = LoadCollection<BookInfo>(paths.Books, "book");
        var races       = LoadCollection<RaceData>(paths.Races, "race");
        var subraces    = LoadCollection<SubraceData>(paths.Races, "subrace");
        var backgrounds = LoadCollection<BackgroundData>(paths.Backgrounds, "background");
        var items       = LoadCollection<ItemData>(paths.Items, "item");
        var itemsBase   = LoadCollection<ItemData>(paths.ItemsBase, "baseitem");
        var feats       = LoadCollection<FeatData>(paths.Feats, "feat");

        var classes    = new List<ClassData>();
        var subclasses = new List<SubclassData>();
        if (Directory.Exists(paths.ClassesFolder))
        {
            foreach (var file in Directory.EnumerateFiles(paths.ClassesFolder, "class-*.json"))
            {
                classes.AddRange(LoadCollection<ClassData>(file, "class"));
                subclasses.AddRange(LoadCollection<SubclassData>(file, "subclass"));
            }
        }

        var spells = new List<SpellData>();
        if (Directory.Exists(paths.SpellsFolder))
        {
            foreach (var file in Directory.EnumerateFiles(paths.SpellsFolder, "spells-*.json"))
                spells.AddRange(LoadCollection<SpellData>(file, "spell"));
        }

        var mergedItems = items.Concat(itemsBase).ToList();
        var spellListIndex = ClassSpellListIndex.LoadFrom(paths.SpellSourceLookup);

        return new Catalog
        {
            Paths = paths,
            SpellListIndex = spellListIndex,
            Books = books,
            Races = races,
            Subraces = subraces,
            Classes = classes,
            Subclasses = subclasses,
            Backgrounds = backgrounds,
            Spells = spells,
            Items = mergedItems,
            Feats = feats,
        };
    }

    private static List<T> LoadCollection<T>(string filePath, string rootKey)
    {
        if (!File.Exists(filePath)) return new List<T>();

        using var stream = File.OpenRead(filePath);
        using var doc = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });

        if (!doc.RootElement.TryGetProperty(rootKey, out var arrayElement) ||
            arrayElement.ValueKind != JsonValueKind.Array)
        {
            return new List<T>();
        }

        // First pass: index every non-_copy entry by (name|source) so child
        // entries can reference them. We snapshot the JsonElement (clone) so
        // it survives the JsonDocument's lifetime.
        var parentIndex = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            if (item.TryGetProperty("_copy", out _)) continue;
            if (TryEntityKey(item, out var key)) parentIndex[key] = item.Clone();
        }

        var result = new List<T>(arrayElement.GetArrayLength());
        foreach (var item in arrayElement.EnumerateArray())
        {
            JsonElement effective;
            var copyRef = CopyResolver.GetCopyRef(item);
            if (copyRef is { } cr)
            {
                var parentKey = $"{cr.Name}|{cr.Source}";
                if (!parentIndex.TryGetValue(parentKey, out var parent))
                    continue; // parent not found — drop the child silently
                effective = CopyResolver.Merge(parent, item);
            }
            else
            {
                effective = item;
            }

            try
            {
                var parsed = effective.Deserialize<T>(JsonOptions);
                if (parsed is not null) result.Add(parsed);
            }
            catch (JsonException)
            {
                // Schema mismatch on this entry — skip rather than crash the entire catalog.
            }
        }

        return result;
    }

    private static bool TryEntityKey(JsonElement item, out string key)
    {
        key = "";
        if (!item.TryGetProperty("name", out var n) || n.ValueKind != JsonValueKind.String) return false;
        if (!item.TryGetProperty("source", out var s) || s.ValueKind != JsonValueKind.String) return false;
        key = $"{n.GetString()}|{s.GetString()}";
        return true;
    }
}
