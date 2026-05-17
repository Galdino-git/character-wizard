using System.Text.Json;

namespace CharacterWizard.Core.Settings;

/// <summary>
/// User-level settings persisted at %AppData%/CharacterWizard/settings.json.
/// </summary>
public sealed class AppSettings
{
    public List<string> EnabledGroups { get; set; } =
        ["core", "supplement", "supplement-alt", "setting", "setting-alt"];
    public List<string> EnabledSources  { get; set; } = new();
    public List<string> DisabledSources { get; set; } = new();

    public Guid? LastOpenedCharacterId { get; set; }
}

public sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public string FilePath { get; }

    public AppSettingsStore(string? rootFolder = null)
    {
        rootFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CharacterWizard");
        Directory.CreateDirectory(rootFolder);
        FilePath = Path.Combine(rootFolder, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(FilePath)) return new AppSettings();
        try { return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new AppSettings(); }
        catch (JsonException) { return new AppSettings(); }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(FilePath, json);
    }
}
