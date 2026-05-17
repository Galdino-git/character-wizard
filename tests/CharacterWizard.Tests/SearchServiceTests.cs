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

    [Fact]
    public void Filters_by_category()
    {
        var s = Build();
        if (s is null) return;
        var page = s.Search("fire", categories: new[] { "Item" }, pageSize: 200);
        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, e => Assert.Equal("Item", e.Category));
    }

    [Fact]
    public void Filters_by_source()
    {
        var s = Build();
        if (s is null) return;
        var page = s.Search("fire", sources: new[] { "PHB" }, pageSize: 200);
        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, e => Assert.Equal("PHB", e.Source));
    }

    [Fact]
    public void Pagination_slices_results()
    {
        var s = Build();
        if (s is null) return;
        var p0 = s.Search("fire", pageSize: 3, pageIndex: 0);
        var p1 = s.Search("fire", pageSize: 3, pageIndex: 1);

        Assert.Equal(3, p0.Items.Count);
        // p1 may have fewer items if total is small; both share the same totalCount
        Assert.Equal(p0.TotalCount, p1.TotalCount);
        // Items in different pages don't overlap
        Assert.Empty(p0.Items.Intersect(p1.Items));
    }

    [Fact]
    public void Total_count_independent_of_page()
    {
        var s = Build();
        if (s is null) return;
        var small = s.Search("fire", pageSize: 1);
        var big   = s.Search("fire", pageSize: 1000);
        Assert.Equal(big.Items.Count, big.TotalCount);
        Assert.Equal(big.TotalCount, small.TotalCount);
    }

    [Fact]
    public void Filters_only_returns_results_when_a_category_is_picked()
    {
        var s = Build();
        if (s is null) return;
        var page = s.Search("", categories: new[] { "Race" }, pageSize: 5);
        Assert.NotEmpty(page.Items);
        Assert.All(page.Items, e => Assert.Equal("Race", e.Category));
    }
}
