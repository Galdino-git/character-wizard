using System.Text.Json;
using System.Text.Json.Nodes;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Models;
using CharacterWizard.Data.Repositories;

namespace CharacterWizard.Data.Resolving;

/// <summary>
/// Resolves a (category, name, source) tuple — emitted by EntryRenderer tags —
/// to a concrete entity from one of the repositories or the wider catalog.
/// Returns null when category/entity is unknown.
/// </summary>
public sealed class EntityResolver
{
    private readonly RaceRepository _races;
    private readonly ClassRepository _classes;
    private readonly BackgroundRepository _backgrounds;
    private readonly SpellRepository _spells;
    private readonly ItemRepository _items;
    private readonly FeatRepository _feats;
    private readonly ICatalogSource _src;

    public EntityResolver(
        RaceRepository races,
        ClassRepository classes,
        BackgroundRepository backgrounds,
        SpellRepository spells,
        ItemRepository items,
        FeatRepository feats,
        ICatalogSource catalogSource)
    {
        _races = races;
        _classes = classes;
        _backgrounds = backgrounds;
        _spells = spells;
        _items = items;
        _feats = feats;
        _src = catalogSource;
    }

    public ResolvedEntity? Resolve(string category, string name, string source)
    {
        var key = new EntityRef(name, source);
        var cat = category.ToLowerInvariant();

        return cat switch
        {
            "race"       => _races.Find(key)       is { } r ? new("Race",       r.Name, r.Source, r.Page, r.Entries)       : null,
            "class"      => _classes.Find(key)     is { } c ? SynthesizeClass(c) : null,
            "background" => _backgrounds.Find(key) is { } b ? new("Background", b.Name, b.Source, b.Page, b.Entries)       : null,
            "spell"      => _spells.Find(key)      is { } s ? new("Spell",      s.Name, s.Source, s.Page, s.Entries)       : null,
            "item"       => _items.Find(key)       is { } i ? new("Item",       i.Name, i.Source, i.Page, i.Entries)       : null,
            "feat"       => _feats.All().FirstOrDefault(f => Eq(f.Name, name) && Eq(f.Source, source))
                            is { } f ? new("Feat", f.Name, f.Source, f.Page, f.Entries) : null,

            "condition" or "disease" => Glossary("Condition", _src.Catalog.Conditions, name, source),
            "skill"  => Glossary("Skill",  _src.Catalog.Skills,  name, source),
            "action" => Glossary("Action", _src.Catalog.Actions, name, source),
            "sense"  => Glossary("Sense",  _src.Catalog.Senses,  name, source),

            "classfeature" or "feature" =>
                _src.Catalog.ClassFeatures.FirstOrDefault(cf => Eq(cf.Name, name) && Eq(cf.Source, source)) is { } cf
                    ? new("Class Feature", cf.Name, cf.Source, cf.Page, cf.Entries) : null,

            "subclassfeature" =>
                _src.Catalog.SubclassFeatures.FirstOrDefault(sf => Eq(sf.Name, name) && Eq(sf.Source, source)) is { } sf
                    ? new("Subclass Feature", sf.Name, sf.Source, sf.Page, sf.Entries) : null,

            _ => null,
        };
    }

    private static ResolvedEntity? Glossary(string label, IReadOnlyList<GlossaryEntry> source, string name, string src)
    {
        var hit = source.FirstOrDefault(x => Eq(x.Name, name) && Eq(x.Source, src))
               ?? source.FirstOrDefault(x => Eq(x.Name, name));
        return hit is null ? null : new ResolvedEntity(label, hit.Name, hit.Source, hit.Page, hit.Entries);
    }

    /// <summary>
    /// Class doesn't carry a single `entries` field — its content is the union
    /// of overview metadata + per-level features + subclasses. Build that tree
    /// at lookup time and hand back as a JsonElement so EntryRenderer renders
    /// it just like any other content.
    /// </summary>
    private ResolvedEntity SynthesizeClass(ClassData c)
    {
        var arr = new JsonArray();

        var headerLines = new JsonArray();
        if (c.HitDice is { } hd)
            headerLines.Add($"{{@b Hit Die}}: d{hd.Faces}");
        if (c.SavingThrowProficiencies is { Length: > 0 } saves)
            headerLines.Add("{@b Saving Throws}: " + string.Join(", ", saves.Select(s => s.ToUpperInvariant())));
        if (!string.IsNullOrEmpty(c.SpellcastingAbility))
            headerLines.Add($"{{@b Spellcasting}}: {c.SpellcastingAbility.ToUpperInvariant()}");
        if (headerLines.Count > 0)
            arr.Add(new JsonObject { ["type"] = "entries", ["name"] = "Overview", ["entries"] = headerLines });

        var featuresByLevel = _src.Catalog.ClassFeatures
            .Where(f => Eq(f.ClassName, c.Name) && Eq(f.ClassSource, c.Source))
            .GroupBy(f => f.Level)
            .OrderBy(g => g.Key);

        foreach (var grp in featuresByLevel)
        {
            var levelEntries = new JsonArray();
            foreach (var f in grp.OrderBy(x => x.Name))
                levelEntries.Add($"{{@classFeature {f.Name}|{c.Name}||{f.Level}|{f.Source}}}");
            arr.Add(new JsonObject
            {
                ["type"] = "entries",
                ["name"] = $"Level {grp.Key}",
                ["entries"] = levelEntries,
            });
        }

        var subclasses = _classes.SubclassesOf(c.Ref).OrderBy(s => s.Name).ToList();
        if (subclasses.Count > 0)
        {
            var subList = new JsonArray();
            foreach (var sc in subclasses)
                subList.Add($"{sc.Name} ({sc.Source})");
            arr.Add(new JsonObject
            {
                ["type"] = "entries",
                ["name"] = c.SubclassTitle ?? "Subclasses",
                ["entries"] = subList,
            });
        }

        var doc = JsonDocument.Parse(arr.ToJsonString());
        return new ResolvedEntity("Class", c.Name, c.Source, c.Page, doc.RootElement.Clone());
    }

    private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Lightweight, UI-agnostic projection of any resolved entity. Captures the
/// minimal fields needed to render a detail panel: name, source, page, and the
/// raw entries JsonElement (renderable via EntryRenderer.Render).
/// </summary>
public sealed record ResolvedEntity(
    string Category,
    string Name,
    string Source,
    int? Page,
    System.Text.Json.JsonElement? Entries);
