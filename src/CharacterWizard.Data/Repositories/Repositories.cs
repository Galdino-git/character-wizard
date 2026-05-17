using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Models;

namespace CharacterWizard.Data.Repositories;

public sealed class BookRepository
{
    private readonly Catalog _catalog;
    public BookRepository(Catalog catalog) => _catalog = catalog;

    public IReadOnlyList<BookInfo> All => _catalog.Books;

    public IEnumerable<IGrouping<string, BookInfo>> ByGroup() =>
        _catalog.Books.GroupBy(b => b.Group ?? "other", StringComparer.OrdinalIgnoreCase);

    public BookInfo? FindBySource(string source) =>
        _catalog.Books.FirstOrDefault(b => string.Equals(b.Source, source, StringComparison.OrdinalIgnoreCase));
}

public sealed class RaceRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public RaceRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<RaceData> All() => _filter.Filter(_catalog.Races, r => r.Source);

    public IEnumerable<SubraceData> SubracesOf(EntityRef raceRef) =>
        _filter.Filter(_catalog.Subraces, s => s.Source)
               .Where(s => string.Equals(s.RaceName, raceRef.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(s.RaceSource, raceRef.Source, StringComparison.OrdinalIgnoreCase));

    public RaceData? Find(EntityRef r) =>
        _catalog.Races.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class ClassRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public ClassRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<ClassData> All() => _filter.Filter(_catalog.Classes, c => c.Source);

    public IEnumerable<SubclassData> SubclassesOf(EntityRef classRef) =>
        _filter.Filter(_catalog.Subclasses, s => s.Source)
               .Where(s => string.Equals(s.ClassName, classRef.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(s.ClassSource, classRef.Source, StringComparison.OrdinalIgnoreCase));

    public ClassData? Find(EntityRef r) =>
        _catalog.Classes.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class BackgroundRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public BackgroundRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<BackgroundData> All() => _filter.Filter(_catalog.Backgrounds, b => b.Source);

    public BackgroundData? Find(EntityRef r) =>
        _catalog.Backgrounds.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class SpellRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public SpellRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<SpellData> All() => _filter.Filter(_catalog.Spells, s => s.Source);

    public IEnumerable<SpellData> AtLevel(int level) => All().Where(s => s.Level == level);

    /// <summary>
    /// All spells of the given level (0 = cantrip) that can be cast by the
    /// given class. Backed by the 5etools spell-source lookup imported under
    /// <c>generated/</c>. Results are also subject to the active SourceFilter.
    /// </summary>
    public IEnumerable<SpellData> ForClassAtLevel(string className, string classSource, int spellLevel) =>
        AtLevel(spellLevel).Where(s =>
            _catalog.SpellListIndex.ClassHasSpell(className, classSource, s.Name, s.Source));

    public SpellData? Find(EntityRef r) =>
        _catalog.Spells.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class ItemRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public ItemRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<ItemData> All() => _filter.Filter(_catalog.Items, i => i.Source);

    public ItemData? Find(EntityRef r) =>
        _catalog.Items.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class FeatRepository
{
    private readonly Catalog _catalog;
    private readonly SourceFilter _filter;
    public FeatRepository(Catalog catalog, SourceFilter filter)
    {
        _catalog = catalog;
        _filter = filter;
    }

    public IEnumerable<FeatData> All() => _filter.Filter(_catalog.Feats, f => f.Source);
}
