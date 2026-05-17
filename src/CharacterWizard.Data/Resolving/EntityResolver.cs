using CharacterWizard.Data.Models;
using CharacterWizard.Data.Repositories;

namespace CharacterWizard.Data.Resolving;

/// <summary>
/// Resolves a (category, name, source) tuple — emitted by EntryRenderer tags —
/// to a concrete entity from one of the repositories. Returns null when the
/// category is unknown or the entity is missing.
/// </summary>
public sealed class EntityResolver
{
    private readonly RaceRepository _races;
    private readonly ClassRepository _classes;
    private readonly BackgroundRepository _backgrounds;
    private readonly SpellRepository _spells;
    private readonly ItemRepository _items;
    private readonly FeatRepository _feats;

    public EntityResolver(
        RaceRepository races,
        ClassRepository classes,
        BackgroundRepository backgrounds,
        SpellRepository spells,
        ItemRepository items,
        FeatRepository feats)
    {
        _races = races;
        _classes = classes;
        _backgrounds = backgrounds;
        _spells = spells;
        _items = items;
        _feats = feats;
    }

    public ResolvedEntity? Resolve(string category, string name, string source)
    {
        var key = new EntityRef(name, source);

        return category.ToLowerInvariant() switch
        {
            "race"       => _races.Find(key)       is { } r ? new("Race",       r.Name, r.Source, r.Page, r.Entries)       : null,
            "class"      => _classes.Find(key)     is { } c ? new("Class",      c.Name, c.Source, c.Page, null)            : null,
            "background" => _backgrounds.Find(key) is { } b ? new("Background", b.Name, b.Source, b.Page, b.Entries)       : null,
            "spell"      => _spells.Find(key)      is { } s ? new("Spell",      s.Name, s.Source, s.Page, s.Entries)       : null,
            "item"       => _items.Find(key)       is { } i ? new("Item",       i.Name, i.Source, i.Page, i.Entries)       : null,
            "feat"       => _feats.All().FirstOrDefault(f => Eq(f.Name, name) && Eq(f.Source, source))
                            is { } f ? new("Feat", f.Name, f.Source, f.Page, f.Entries) : null,
            _ => null,
        };
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
