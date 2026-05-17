using System.Text.Json;
using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Persistence;

/// <summary>
/// JSON file persistence for characters under %AppData%/CharacterWizard/characters/.
/// One file per character; filename = "{id}.json". Backups of the previous version
/// are kept under characters/_history/{id}/{yyyymmdd-hhmmss}.json (last 5).
/// </summary>
public sealed class CharacterStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public string RootFolder { get; }
    public string CharactersFolder => Path.Combine(RootFolder, "characters");
    public string HistoryFolder => Path.Combine(CharactersFolder, "_history");

    public CharacterStore(string? rootFolder = null)
    {
        RootFolder = rootFolder ?? DefaultRootFolder();
        Directory.CreateDirectory(CharactersFolder);
    }

    public static string DefaultRootFolder() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CharacterWizard");

    public IEnumerable<Character> LoadAll()
    {
        if (!Directory.Exists(CharactersFolder)) yield break;
        foreach (var file in Directory.EnumerateFiles(CharactersFolder, "*.json"))
        {
            Character? c = null;
            try { c = JsonSerializer.Deserialize<Character>(File.ReadAllText(file), JsonOptions); }
            catch (JsonException) { /* corrupt file — skip silently for MVP */ }
            if (c is not null) yield return c;
        }
    }

    public Character? Load(Guid id)
    {
        var path = FilePathFor(id);
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<Character>(File.ReadAllText(path), JsonOptions);
    }

    public void Save(Character c)
    {
        c.UpdatedUtc = DateTime.UtcNow;
        var path = FilePathFor(c.Id);

        if (File.Exists(path)) BackupExisting(c.Id, path);

        var json = JsonSerializer.Serialize(c, JsonOptions);
        File.WriteAllText(path, json);
    }

    public void Delete(Guid id)
    {
        var path = FilePathFor(id);
        if (File.Exists(path)) File.Delete(path);
    }

    private string FilePathFor(Guid id) => Path.Combine(CharactersFolder, id + ".json");

    private void BackupExisting(Guid id, string existingPath)
    {
        var folder = Path.Combine(HistoryFolder, id.ToString());
        Directory.CreateDirectory(folder);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        File.Copy(existingPath, Path.Combine(folder, $"{stamp}.json"), overwrite: true);
        TrimBackups(folder, keep: 5);
    }

    private static void TrimBackups(string folder, int keep)
    {
        var files = new DirectoryInfo(folder).GetFiles("*.json").OrderByDescending(f => f.Name).ToList();
        foreach (var f in files.Skip(keep)) f.Delete();
    }
}
