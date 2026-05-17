using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Search;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Repositories;

namespace CharacterWizard.Tests;

public class SearchServiceTests
{
    private static SearchService? Build()
    {
        var dataRoot = CatalogTestHelpers.FindDataRoot();
        if (dataRoot is null) return null;
        var catalog = new CatalogLoader().Load(dataRoot);
        var filter = new SourceFilter(catalog, SourceFilterSettings.AllowAll());
        return new SearchService(
            new RaceRepository(catalog, filter),
            new ClassRepository(catalog, filter),
            new BackgroundRepository(catalog, filter),
            new SpellRepository(catalog, filter),
            new ItemRepository(catalog, filter),
            new FeatRepository(catalog, filter));
    }

    [Fact]
    public void Empty_query_returns_empty_when_no_filters()
    {
        var s = Build();
        if (s is null) return;
        var page = s.Search("");
        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public void Substring_match_is_case_insensitive()
    {
        var s = Build();
        if (s is null) return;
        var page = s.Search("fireball");
        Assert.Contains(page.Items, e =>
            e.Category == "Spell" && e.Name == "Fireball" && e.Source == "PHB");
    }

    [Fact]
    public void Prefix_match_ranks_above_substring_match()
    {
        var s = Build();
        if (s is null) return;
        // "fire" — Fireball (prefix) should appear before Wall of Fire (substring)
        var page = s.Search("fire", pageSize: 200);
        var fireball = page.Items.FindIndex(e => e.Name == "Fireball" && e.Source == "PHB");
        var wallOfFire = page.Items.FindIndex(e => e.Name == "Wall of Fire" && e.Source == "PHB");
        Assert.True(fireball >= 0, "Fireball should be in results");
        Assert.True(wallOfFire >= 0, "Wall of Fire should be in results");
        Assert.True(fireball < wallOfFire,
            $"Fireball (prefix match, idx {fireball}) should rank above Wall of Fire (substring match, idx {wallOfFire})");
    }

    [Fact]
    public void Same_rank_orders_alphabetically()
    {
        var s = Build();
        if (s is null) return;
        // Find two prefix-matches: should be alphabetic
        var page = s.Search("fire", pageSize: 200);
        var prefixHits = page.Items
            .Where(e => e.Name.StartsWith("Fire", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Name)
            .ToList();
        var sorted = prefixHits.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.Equal(sorted, prefixHits);
    }
}
