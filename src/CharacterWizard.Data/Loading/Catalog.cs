using CharacterWizard.Data.Models;

namespace CharacterWizard.Data.Loading;

/// <summary>
/// In-memory snapshot of all parsed 5etools data. Built once at app startup by
/// <see cref="CatalogLoader"/>. Treat as read-only after construction.
/// </summary>
public sealed class Catalog
{
    public required IReadOnlyList<BookInfo>        Books        { get; init; }
    public required IReadOnlyList<RaceData>        Races        { get; init; }
    public required IReadOnlyList<SubraceData>     Subraces     { get; init; }
    public required IReadOnlyList<ClassData>       Classes      { get; init; }
    public required IReadOnlyList<SubclassData>    Subclasses   { get; init; }
    public required IReadOnlyList<BackgroundData>  Backgrounds  { get; init; }
    public required IReadOnlyList<SpellData>       Spells       { get; init; }
    public required IReadOnlyList<ItemData>        Items        { get; init; }
    public required IReadOnlyList<FeatData>        Feats        { get; init; }

    public required CatalogPaths Paths { get; init; }
}
