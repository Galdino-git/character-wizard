using CharacterWizard.Data.Loading;
using CharacterWizard.Data.Models;

namespace CharacterWizard.Data.Filtering;

/// <summary>
/// Decides whether content from a given source code (e.g., "PHB", "XGE") is
/// enabled, based on user-configured group toggles and per-source overrides.
/// </summary>
public sealed class SourceFilter
{
    private readonly Dictionary<string, string?> _sourceToGroup; // sourceCode -> group (case-insensitive key)
    private readonly HashSet<string> _enabledGroups;
    private readonly HashSet<string> _disabledSources;
    private readonly HashSet<string> _enabledSources;

    /// <summary>When true, callers should drop entries whose <c>reprintedAs</c> array
    /// is non-empty (i.e. older versions superseded by newer ones).</summary>
    public bool HideReprintedVersions { get; }

    public SourceFilter(Catalog catalog, SourceFilterSettings settings)
    {
        _sourceToGroup = catalog.Books.ToDictionary(
            b => b.Source,
            b => b.Group,
            StringComparer.OrdinalIgnoreCase);

        _enabledGroups   = new HashSet<string>(settings.EnabledGroups,   StringComparer.OrdinalIgnoreCase);
        _disabledSources = new HashSet<string>(settings.DisabledSources, StringComparer.OrdinalIgnoreCase);
        _enabledSources  = new HashSet<string>(settings.EnabledSources,  StringComparer.OrdinalIgnoreCase);
        HideReprintedVersions = settings.HideReprintedVersions;
    }

    public bool IsEnabled(string source)
    {
        if (string.IsNullOrEmpty(source)) return false;

        if (_disabledSources.Contains(source)) return false;
        if (_enabledSources.Contains(source))  return true;

        if (_sourceToGroup.TryGetValue(source, out var group) && group is not null)
            return _enabledGroups.Contains(group);

        // Unknown source (not present in books.json) — default to enabled.
        return true;
    }

    public IEnumerable<T> Filter<T>(IEnumerable<T> items, Func<T, string> sourceSelector) =>
        items.Where(i => IsEnabled(sourceSelector(i)));
}

public sealed record SourceFilterSettings
{
    /// <summary>Groups from books.json that are enabled by default. core/supplement/setting/etc.</summary>
    public IReadOnlyCollection<string> EnabledGroups { get; init; } =
        ["core", "supplement", "supplement-alt", "setting", "setting-alt"];

    /// <summary>Individual sources to force-enable even if their group is disabled.</summary>
    public IReadOnlyCollection<string> EnabledSources { get; init; } = Array.Empty<string>();

    /// <summary>Individual sources to force-disable even if their group is enabled.</summary>
    public IReadOnlyCollection<string> DisabledSources { get; init; } = Array.Empty<string>();

    /// <summary>If true, entries with non-empty reprintedAs are dropped.</summary>
    public bool HideReprintedVersions { get; init; } = true;

    public static SourceFilterSettings AllowAll() => new()
    {
        EnabledGroups =
        [
            "core", "supplement", "supplement-alt",
            "setting", "setting-alt",
            "organized-play", "screen", "homecraft", "recipe", "other",
        ],
        HideReprintedVersions = false,
    };
}
