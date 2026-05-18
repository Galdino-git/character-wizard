using CharacterWizard.Core.Settings;
using CharacterWizard.Data.Filtering;
using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Repositories;

namespace CharacterWizard.App.Services;

/// <summary>
/// Loads the 5etools catalog once at startup and exposes shared instances of
/// catalog + filter so repositories can be created cheaply elsewhere.
/// Implements <see cref="ICatalogSource"/> so repos can be reloaded with new
/// data without rebuilding DI (Settings → Recarregar).
/// </summary>
public sealed class CatalogProvider : ICatalogSource
{
    private readonly AppPaths _paths;

    public Catalog Catalog { get; private set; }
    public SourceFilter Filter { get; private set; }

    public CatalogProvider(AppPaths paths, AppSettings settings)
    {
        _paths = paths;
        Catalog = new CatalogLoader().Load(paths.DataRoot);
        Filter  = BuildFilter(settings);
    }

    public void ApplySettings(AppSettings settings) =>
        Filter = BuildFilter(settings);

    /// <summary>
    /// Re-runs the JSON loader against the same dataRoot. Used when the user
    /// manually re-imports data via the CLI (or edits files on disk) and wants
    /// to see changes without restarting the app.
    /// </summary>
    public void Reload(AppSettings settings)
    {
        Catalog = new CatalogLoader().Load(_paths.DataRoot);
        Filter = BuildFilter(settings);
    }

    private SourceFilter BuildFilter(AppSettings settings) => new(
        Catalog,
        new SourceFilterSettings
        {
            EnabledGroups   = settings.EnabledGroups,
            EnabledSources  = settings.EnabledSources,
            DisabledSources = settings.DisabledSources,
        });
}
