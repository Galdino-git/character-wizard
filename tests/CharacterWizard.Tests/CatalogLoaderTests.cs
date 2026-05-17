using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Parsing;
using CharacterWizard.Data.Repositories;

namespace CharacterWizard.Tests;

/// <summary>
/// Smoke tests that load the imported 5etools data from disk. Skipped if the
/// data folder hasn't been imported yet (CI-friendly).
/// </summary>
public class CatalogLoaderTests
{
    private static string? FindDataRoot()
    {
        // Walk up from the test bin folder until we find <repo>/data/5etools-json
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "data", "5etools-json");
            if (Directory.Exists(candidate)) return Path.Combine(dir.FullName, "data");
            dir = dir.Parent;
        }
        return null;
    }

    [Fact]
    public void Catalog_loads_non_empty_collections()
    {
        var dataRoot = FindDataRoot();
        if (dataRoot is null) return; // data not imported — skip silently

        var catalog = new CatalogLoader().Load(dataRoot);

        Assert.NotEmpty(catalog.Books);
        Assert.NotEmpty(catalog.Races);
        Assert.NotEmpty(catalog.Classes);
        Assert.NotEmpty(catalog.Backgrounds);
        Assert.NotEmpty(catalog.Spells);
        Assert.NotEmpty(catalog.Items);
        Assert.NotEmpty(catalog.Feats);

        // Sanity: PHB Wizard should be present (one of the most well-known anchors).
        Assert.Contains(catalog.Classes, c => c.Name == "Wizard" && c.Source == "PHB");

        // Sanity: PHB is grouped as "core".
        var phb = catalog.Books.FirstOrDefault(b => b.Source == "PHB");
        Assert.NotNull(phb);
        Assert.Equal("core", phb!.Group);
    }

    [Fact]
    public void SourceFilter_restricts_to_enabled_groups()
    {
        var dataRoot = FindDataRoot();
        if (dataRoot is null) return; // data not imported — skip silently

        var catalog = new CatalogLoader().Load(dataRoot);
        var filter = new SourceFilter(catalog, new SourceFilterSettings
        {
            EnabledGroups = ["core"],
        });

        var classes = new ClassRepository(catalog, filter).All().ToList();

        Assert.NotEmpty(classes);
        Assert.All(classes, c =>
        {
            // Every returned class must come from a source whose book.group == "core"
            // (or from an unknown source, which our filter defaults to enabled).
            var book = catalog.Books.FirstOrDefault(b => b.Source == c.Source);
            if (book is not null)
                Assert.Equal("core", book.Group);
        });
    }
}

public class ClassFeatureRefParserTests
{
    [Fact]
    public void Parses_simple_class_feature()
    {
        var r = ClassFeatureRefParser.ParseClassFeature("Arcane Recovery|Wizard||1", defaultClassSource: "PHB");
        Assert.Equal("Arcane Recovery", r.Name);
        Assert.Equal("Wizard", r.ClassName);
        Assert.Equal("PHB", r.ClassSource);
        Assert.Equal(1, r.Level);
        Assert.Equal("PHB", r.FeatureSource);
    }

    [Fact]
    public void Parses_class_feature_with_alt_source()
    {
        var r = ClassFeatureRefParser.ParseClassFeature("Cantrip Formulas|Wizard||3|TCE", "PHB");
        Assert.Equal("Cantrip Formulas", r.Name);
        Assert.Equal(3, r.Level);
        Assert.Equal("TCE", r.FeatureSource);
    }

    [Fact]
    public void Parses_subclass_feature()
    {
        var r = ClassFeatureRefParser.ParseSubclassFeature("Improved Abjuration|Wizard||Abjuration||10", "PHB");
        Assert.Equal("Improved Abjuration", r.Name);
        Assert.Equal("Abjuration", r.SubclassShortName);
        Assert.Equal(10, r.Level);
    }

    [Fact]
    public void Throws_on_invalid_level()
    {
        Assert.Throws<FormatException>(() =>
            ClassFeatureRefParser.ParseClassFeature("Bad|Wizard||notanumber", "PHB"));
    }
}
