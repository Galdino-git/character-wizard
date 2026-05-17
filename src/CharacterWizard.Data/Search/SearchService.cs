using CharacterWizard.Data.Repositories;

namespace CharacterWizard.Data.Search;

/// <summary>
/// One row in a search result list. Category is the human-readable label
/// ("Race", "Spell", ...) and also doubles as the filter key used by the UI.
/// </summary>
public sealed record SearchEntry(
    string Category,
    string Name,
    string Source,
    int? Page);

public sealed record SearchResultPage(
    List<SearchEntry> Items,
    int TotalCount,
    int PageIndex,
    int PageSize);

/// <summary>
/// In-memory linear-scan search over the loaded catalog. Lives in Data so it's
/// testable without booting MAUI; the App layer just consumes it via DI.
/// </summary>
public sealed class SearchService
{
    private readonly RaceRepository _races;
    private readonly ClassRepository _classes;
    private readonly BackgroundRepository _backgrounds;
    private readonly SpellRepository _spells;
    private readonly ItemRepository _items;
    private readonly FeatRepository _feats;

    public SearchService(
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

    public SearchResultPage Search(
        string query,
        IReadOnlyCollection<string>? categories = null,
        IReadOnlyCollection<string>? sources = null,
        int pageSize = 50,
        int pageIndex = 0)
    {
        var q = (query ?? "").Trim();

        // No query AND no filters → empty; otherwise filters alone still return results.
        if (q.Length == 0 && (categories is null || categories.Count == 0))
            return new SearchResultPage(new(), 0, pageIndex, pageSize);

        var all = EnumerateAll(categories);

        var ranked = all
            .Where(e => q.Length == 0 || e.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Where(e => sources is null || sources.Count == 0 ||
                        sources.Contains(e.Source, StringComparer.OrdinalIgnoreCase))
            .Select(e => new { e, Rank = ComputeRank(e.Name, q) })
            .OrderBy(x => x.Rank)
            .ThenBy(x => x.e.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.e)
            .ToList();

        var paged = ranked
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return new SearchResultPage(paged, ranked.Count, pageIndex, pageSize);
    }

    private IEnumerable<SearchEntry> EnumerateAll(IReadOnlyCollection<string>? categories)
    {
        bool include(string cat) =>
            categories is null || categories.Count == 0 ||
            categories.Contains(cat, StringComparer.OrdinalIgnoreCase);

        if (include("Race"))
            foreach (var r in _races.All()) yield return new("Race", r.Name, r.Source, r.Page);
        if (include("Class"))
            foreach (var c in _classes.All()) yield return new("Class", c.Name, c.Source, c.Page);
        if (include("Background"))
            foreach (var b in _backgrounds.All()) yield return new("Background", b.Name, b.Source, b.Page);
        if (include("Spell"))
            foreach (var s in _spells.All()) yield return new("Spell", s.Name, s.Source, s.Page);
        if (include("Item"))
            foreach (var i in _items.All()) yield return new("Item", i.Name, i.Source, i.Page);
        if (include("Feat"))
            foreach (var f in _feats.All()) yield return new("Feat", f.Name, f.Source, f.Page);
    }

    // Rank 0 = prefix match (best), 1 = substring match (worst), -1 reserved for future exact match.
    private static int ComputeRank(string name, string query)
    {
        if (query.Length == 0) return 1;
        return name.StartsWith(query, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
    }
}
