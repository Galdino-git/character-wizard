using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Models;

namespace CharacterWizard.Data.Repositories;

/// <summary>
/// Indirection over Catalog/SourceFilter so the app can swap the active
/// catalog at runtime (Settings → Recarregar). Tests construct repos with
/// fixed Catalog/SourceFilter; the app DI passes an <see cref="ICatalogSource"/>.
/// </summary>
public interface ICatalogSource
{
    Catalog Catalog { get; }
    SourceFilter Filter { get; }
}

internal sealed class StaticCatalogSource : ICatalogSource
{
    public Catalog Catalog { get; }
    public SourceFilter Filter { get; }
    public StaticCatalogSource(Catalog c, SourceFilter f) { Catalog = c; Filter = f; }
}

public sealed class BookRepository
{
    private readonly ICatalogSource _src;
    public BookRepository(ICatalogSource src) => _src = src;
    public BookRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IReadOnlyList<BookInfo> All => _src.Catalog.Books;

    public IEnumerable<IGrouping<string, BookInfo>> ByGroup() =>
        _src.Catalog.Books.GroupBy(b => b.Group ?? "other", StringComparer.OrdinalIgnoreCase);

    public BookInfo? FindBySource(string source) =>
        _src.Catalog.Books.FirstOrDefault(b => string.Equals(b.Source, source, StringComparison.OrdinalIgnoreCase));
}

public sealed class RaceRepository
{
    private readonly ICatalogSource _src;
    public RaceRepository(ICatalogSource src) => _src = src;
    public RaceRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<RaceData> All() => _src.Filter.Filter(_src.Catalog.Races, r => r.Source);

    public IEnumerable<SubraceData> SubracesOf(EntityRef raceRef) =>
        _src.Filter.Filter(_src.Catalog.Subraces, s => s.Source)
               .Where(s => string.Equals(s.RaceName, raceRef.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(s.RaceSource, raceRef.Source, StringComparison.OrdinalIgnoreCase));

    public RaceData? Find(EntityRef r) =>
        _src.Catalog.Races.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class ClassRepository
{
    private readonly ICatalogSource _src;
    public ClassRepository(ICatalogSource src) => _src = src;
    public ClassRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<ClassData> All() => _src.Filter.Filter(_src.Catalog.Classes, c => c.Source);

    public IEnumerable<SubclassData> SubclassesOf(EntityRef classRef) =>
        _src.Filter.Filter(_src.Catalog.Subclasses, s => s.Source)
               .Where(s => string.Equals(s.ClassName, classRef.Name, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(s.ClassSource, classRef.Source, StringComparison.OrdinalIgnoreCase));

    public ClassData? Find(EntityRef r) =>
        _src.Catalog.Classes.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class BackgroundRepository
{
    private readonly ICatalogSource _src;
    public BackgroundRepository(ICatalogSource src) => _src = src;
    public BackgroundRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<BackgroundData> All() => _src.Filter.Filter(_src.Catalog.Backgrounds, b => b.Source);

    public BackgroundData? Find(EntityRef r) =>
        _src.Catalog.Backgrounds.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class SpellRepository
{
    private readonly ICatalogSource _src;
    public SpellRepository(ICatalogSource src) => _src = src;
    public SpellRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<SpellData> All() => _src.Filter.Filter(_src.Catalog.Spells, s => s.Source);

    public IEnumerable<SpellData> AtLevel(int level) => All().Where(s => s.Level == level);

    public IEnumerable<SpellData> ForClassAtLevel(string className, string classSource, int spellLevel) =>
        AtLevel(spellLevel).Where(s =>
            _src.Catalog.SpellListIndex.ClassHasSpell(className, classSource, s.Name, s.Source));

    public SpellData? Find(EntityRef r) =>
        _src.Catalog.Spells.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class ItemRepository
{
    private readonly ICatalogSource _src;
    public ItemRepository(ICatalogSource src) => _src = src;
    public ItemRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<ItemData> All() => _src.Filter.Filter(_src.Catalog.Items, i => i.Source);

    public ItemData? Find(EntityRef r) =>
        _src.Catalog.Items.FirstOrDefault(x =>
            string.Equals(x.Name, r.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Source, r.Source, StringComparison.OrdinalIgnoreCase));
}

public sealed class FeatRepository
{
    private readonly ICatalogSource _src;
    public FeatRepository(ICatalogSource src) => _src = src;
    public FeatRepository(Catalog catalog, SourceFilter filter) : this(new StaticCatalogSource(catalog, filter)) { }

    public IEnumerable<FeatData> All() => _src.Filter.Filter(_src.Catalog.Feats, f => f.Source);
}
