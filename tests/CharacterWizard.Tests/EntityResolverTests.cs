using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Repositories;
using CharacterWizard.Data.Resolving;

namespace CharacterWizard.Tests;

public class EntityResolverTests
{
    private static EntityResolver? BuildResolver()
    {
        var dataRoot = CatalogTestHelpers.FindDataRoot();
        if (dataRoot is null) return null;
        var catalog = new CatalogLoader().Load(dataRoot);
        var filter = new SourceFilter(catalog, SourceFilterSettings.AllowAll());
        // EntityResolver now also needs ICatalogSource — use the explicit ctor
        // to wrap our test-mode catalog+filter.
        var src = new TestCatalogSource(catalog, filter);
        return new EntityResolver(
            new RaceRepository(src),
            new ClassRepository(src),
            new BackgroundRepository(src),
            new SpellRepository(src),
            new ItemRepository(src),
            new FeatRepository(src),
            src);
    }

    private sealed class TestCatalogSource : CharacterWizard.Data.Repositories.ICatalogSource
    {
        public Catalog Catalog { get; }
        public CharacterWizard.Data.Filtering.SourceFilter Filter { get; }
        public TestCatalogSource(Catalog c, CharacterWizard.Data.Filtering.SourceFilter f)
        { Catalog = c; Filter = f; }
    }

    [Fact]
    public void Resolve_spell_returns_spell_data()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("spell", "Fireball", "PHB");
        Assert.NotNull(hit);
        Assert.Equal("Fireball", hit!.Name);
        Assert.Equal("PHB", hit.Source);
        Assert.Equal("Spell", hit.Category);
    }

    [Fact]
    public void Resolve_race_returns_race_data()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("race", "Elf", "PHB");
        Assert.NotNull(hit);
        Assert.Equal("Race", hit!.Category);
    }

    [Fact]
    public void Resolve_unknown_category_returns_null()
    {
        var r = BuildResolver();
        if (r is null) return;

        Assert.Null(r.Resolve("nonsense", "Anything", "PHB"));
    }

    [Fact]
    public void Resolve_missing_entity_returns_null()
    {
        var r = BuildResolver();
        if (r is null) return;

        Assert.Null(r.Resolve("spell", "DefinitelyNotASpell", "PHB"));
    }

    [Fact]
    public void Resolve_is_case_insensitive_on_category()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("SPELL", "Fireball", "PHB");
        Assert.NotNull(hit);
    }

    [Fact]
    public void Resolve_skill_returns_entry()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("skill", "Perception", "PHB");
        Assert.NotNull(hit);
        Assert.Equal("Skill", hit!.Category);
    }

    [Fact]
    public void Resolve_condition_returns_entry()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("condition", "Prone", "PHB");
        Assert.NotNull(hit);
    }

    [Fact]
    public void Resolve_class_returns_synthesized_entries()
    {
        var r = BuildResolver();
        if (r is null) return;

        var hit = r.Resolve("class", "Wizard", "PHB");
        Assert.NotNull(hit);
        Assert.Equal("Class", hit!.Category);
        Assert.NotNull(hit.Entries);
        Assert.Equal(System.Text.Json.JsonValueKind.Array, hit.Entries!.Value.ValueKind);
        Assert.True(hit.Entries.Value.GetArrayLength() > 0, "Class entries should not be empty");
    }
}

internal static class CatalogTestHelpers
{
    public static string? FindDataRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "data", "5etools-json");
            if (Directory.Exists(candidate)) return Path.Combine(dir.FullName, "data");
            dir = dir.Parent;
        }
        return null;
    }
}
