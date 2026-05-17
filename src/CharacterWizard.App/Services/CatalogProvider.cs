using CharacterWizard.Core.Settings;
using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;

namespace CharacterWizard.App.Services;

/// <summary>
/// Loads the 5etools catalog once at startup and exposes shared instances of
/// catalog + filter so repositories can be created cheaply elsewhere.
/// </summary>
public sealed class CatalogProvider
{
    public Catalog Catalog { get; }
    public SourceFilter Filter { get; private set; }

    public CatalogProvider(AppPaths paths, AppSettings settings)
    {
        Catalog = new CatalogLoader().Load(paths.DataRoot);
        Filter  = BuildFilter(settings);
    }

    public void ApplySettings(AppSettings settings) =>
        Filter = BuildFilter(settings);

    private SourceFilter BuildFilter(AppSettings settings) => new(
        Catalog,
        new SourceFilterSettings
        {
            EnabledGroups   = settings.EnabledGroups,
            EnabledSources  = settings.EnabledSources,
            DisabledSources = settings.DisabledSources,
        });
}
