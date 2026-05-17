namespace CharacterWizard.Data.Loading;

/// <summary>
/// Resolves the locations of imported 5etools JSON files relative to a base data folder.
/// The data folder is the one written by the Import5eToolsData tool (it contains
/// "5etools-json/" and "5etools-img/" subfolders).
/// </summary>
public sealed class CatalogPaths
{
    public string DataRoot { get; }
    public string JsonRoot { get; }
    public string ImageRoot { get; }

    public CatalogPaths(string dataRoot)
    {
        DataRoot = dataRoot;
        JsonRoot = Path.Combine(dataRoot, "5etools-json");
        ImageRoot = Path.Combine(dataRoot, "5etools-img");
    }

    public string Books              => Path.Combine(JsonRoot, "books.json");
    public string Races              => Path.Combine(JsonRoot, "races.json");
    public string Backgrounds        => Path.Combine(JsonRoot, "backgrounds.json");
    public string Items              => Path.Combine(JsonRoot, "items.json");
    public string ItemsBase          => Path.Combine(JsonRoot, "items-base.json");
    public string Feats              => Path.Combine(JsonRoot, "feats.json");
    public string OptionalFeatures   => Path.Combine(JsonRoot, "optionalfeatures.json");
    public string ConditionsDiseases => Path.Combine(JsonRoot, "conditionsdiseases.json");

    public string ClassesFolder => Path.Combine(JsonRoot, "class");
    public string SpellsFolder  => Path.Combine(JsonRoot, "spells");

    public string SpellSourceLookup => Path.Combine(JsonRoot, "generated", "gendata-spell-source-lookup.json");

    public string ImageFolder(string category) => Path.Combine(ImageRoot, category);
}
