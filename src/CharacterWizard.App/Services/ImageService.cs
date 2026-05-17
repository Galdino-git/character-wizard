using System.Collections.Concurrent;

namespace CharacterWizard.App.Services;

/// <summary>
/// Resolves image file paths and converts them to data URLs that Blazor &lt;img src&gt;
/// can consume directly (no web-host required for file:// in the WebView).
///
/// Image conventions used by 5etools (verified empirically):
///   img/classes/&lt;SOURCE&gt;/&lt;Subclass&gt; &lt;ClassName&gt;.webp   (subclass art)
///   img/classes/&lt;SOURCE&gt;/&lt;ClassName&gt;.webp                (base-class art, when present)
///   img/races/&lt;SOURCE&gt;/&lt;Subrace&gt; &lt;RaceName&gt;.webp
///   img/races/&lt;SOURCE&gt;/&lt;RaceName&gt;.webp
///   img/backgrounds/&lt;SOURCE&gt;/&lt;BackgroundName&gt;.webp
///
/// Returns null when no image is found — callers should display a placeholder.
/// </summary>
public sealed class ImageService
{
    private readonly AppPaths _paths;
    private readonly ConcurrentDictionary<string, string?> _cache = new();

    public ImageService(AppPaths paths) => _paths = paths;

    public string? GetClassImage(string className, string source) =>
        TryGetImage("classes", source, className);

    public string? GetRaceImage(string raceName, string source) =>
        TryGetImage("races", source, raceName);

    public string? GetBackgroundImage(string backgroundName, string source) =>
        TryGetImage("backgrounds", source, backgroundName);

    public string? GetItemImage(string itemName, string source) =>
        TryGetImage("items", source, itemName);

    private string? TryGetImage(string category, string source, string name)
    {
        var key = $"{category}|{source}|{name}";
        if (_cache.TryGetValue(key, out var cached)) return cached;

        var path = ResolveImagePath(category, source, name);
        var dataUrl = path is not null ? FileToDataUrl(path) : null;
        _cache[key] = dataUrl;
        return dataUrl;
    }

    private string? ResolveImagePath(string category, string source, string name)
    {
        var imgRoot = Path.Combine(_paths.DataRoot, "5etools-img", category);
        if (!Directory.Exists(imgRoot)) return null;

        var sourceFolder = Path.Combine(imgRoot, source);
        if (!Directory.Exists(sourceFolder))
            sourceFolder = imgRoot; // some sources use the root folder directly

        // Try direct match first.
        var direct = Path.Combine(sourceFolder, name + ".webp");
        if (File.Exists(direct)) return direct;

        // Fallback: case-insensitive contains lookup (handles e.g. "School of Evocation" → "Evocation Wizard.webp")
        var match = Directory.EnumerateFiles(sourceFolder, "*.webp")
            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f)
                .Contains(name, StringComparison.OrdinalIgnoreCase));
        return match;
    }

    private static string FileToDataUrl(string path)
    {
        try
        {
            var bytes = File.ReadAllBytes(path);
            var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            var mime = ext switch
            {
                "webp" => "image/webp",
                "png"  => "image/png",
                "jpg" or "jpeg" => "image/jpeg",
                "svg"  => "image/svg+xml",
                _      => "application/octet-stream",
            };
            return $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        }
        catch (IOException) { return ""; }
    }
}
