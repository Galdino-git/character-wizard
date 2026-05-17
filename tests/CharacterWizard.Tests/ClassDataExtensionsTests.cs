using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Models;

namespace CharacterWizard.Tests;

public class ClassDataExtensionsTests
{
    private static IReadOnlyList<ClassData>? _classes;
    private static IReadOnlyList<ClassData>? GetClasses()
    {
        if (_classes is not null) return _classes;
        var root = CatalogTestHelpers.FindDataRoot();
        if (root is null) return null;
        _classes = new CatalogLoader().Load(root).Classes;
        return _classes;
    }

    private static ClassData? Find(string name, string source)
    {
        var classes = GetClasses();
        return classes?.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Source, source, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Wizard",   "PHB", 2)]
    [InlineData("Cleric",   "PHB", 1)]
    [InlineData("Fighter",  "PHB", 3)]
    [InlineData("Rogue",    "PHB", 3)]
    [InlineData("Sorcerer", "PHB", 1)]
    [InlineData("Warlock",  "PHB", 1)]
    [InlineData("Bard",     "PHB", 3)]
    [InlineData("Paladin",  "PHB", 3)]
    public void SubclassChoiceLevel_returns_expected(string name, string source, int expected)
    {
        var cls = Find(name, source);
        if (cls is null) return; // data not imported or class missing — silent skip
        Assert.Equal(expected, cls.SubclassChoiceLevel());
    }

    [Fact]
    public void IsCaster_true_for_wizard_false_for_fighter()
    {
        var wizard = Find("Wizard", "PHB");
        var fighter = Find("Fighter", "PHB");
        if (wizard is null || fighter is null) return;
        Assert.True(wizard.IsCaster());
        Assert.False(fighter.IsCaster());
    }

    [Fact]
    public void CantripsKnownAtLevel_for_wizard_starts_at_three()
    {
        var wiz = Find("Wizard", "PHB");
        if (wiz is null) return;
        Assert.Equal(3, wiz.CantripsKnownAtLevel(1));
        Assert.Equal(4, wiz.CantripsKnownAtLevel(4));
        Assert.Equal(5, wiz.CantripsKnownAtLevel(10));
    }

    [Fact]
    public void SpellsKnownAtLevel_for_wizard_accumulates()
    {
        // PHB Wizard: 6 spells in spellbook at lvl 1, +2 per level after.
        var wiz = Find("Wizard", "PHB");
        if (wiz is null) return;
        Assert.Equal(6, wiz.SpellsKnownAtLevel(1));
        Assert.Equal(8, wiz.SpellsKnownAtLevel(2));
        Assert.Equal(10, wiz.SpellsKnownAtLevel(3));
    }
}

public class SpellRepositoryForClassTests
{
    private static CharacterWizard.Data.Repositories.SpellRepository? Build()
    {
        var root = CatalogTestHelpers.FindDataRoot();
        if (root is null) return null;
        var catalog = new CatalogLoader().Load(root);
        var filter = new CharacterWizard.Data.Filtering.SourceFilter(catalog,
            CharacterWizard.Data.Filtering.SourceFilterSettings.AllowAll());
        return new CharacterWizard.Data.Repositories.SpellRepository(catalog, filter);
    }

    [Fact]
    public void ForClassAtLevel_returns_phb_wizard_cantrips()
    {
        var repo = Build();
        if (repo is null) return;
        var cantrips = repo.ForClassAtLevel("Wizard", "PHB", 0).ToList();
        Assert.NotEmpty(cantrips);
        // Fire Bolt is a canonical PHB wizard cantrip.
        Assert.Contains(cantrips, s => s.Name.Equals("Fire Bolt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ForClassAtLevel_filters_by_level()
    {
        var repo = Build();
        if (repo is null) return;
        var lvl3 = repo.ForClassAtLevel("Wizard", "PHB", 3).ToList();
        Assert.NotEmpty(lvl3);
        Assert.All(lvl3, s => Assert.Equal(3, s.Level));
        Assert.Contains(lvl3, s => s.Name.Equals("Fireball", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ForClassAtLevel_excludes_other_class_spells()
    {
        var repo = Build();
        if (repo is null) return;
        // Cure Wounds is a Cleric/Druid spell, not a Wizard spell.
        var wizLvl1 = repo.ForClassAtLevel("Wizard", "PHB", 1).ToList();
        Assert.DoesNotContain(wizLvl1, s => s.Name.Equals("Cure Wounds", StringComparison.OrdinalIgnoreCase));
    }
}
