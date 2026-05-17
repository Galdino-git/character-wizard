namespace CharacterWizard.App.Services;

/// <summary>
/// Resolves filesystem locations the app needs at runtime.
/// </summary>
public sealed class AppPaths
{
    /// <summary>Folder containing 5etools-json/ and 5etools-img/. Looked up by walking
    /// up from the exe location during dev. In a packaged release we'd point this at
    /// FileSystem.AppDataDirectory and seed it on first run.</summary>
    public string DataRoot { get; }

    public string UserDataRoot { get; }

    public AppPaths()
    {
        UserDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CharacterWizard");
        Directory.CreateDirectory(UserDataRoot);

        DataRoot = ResolveDataRoot();
    }

    private static string ResolveDataRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "data", "5etools-json");
            if (Directory.Exists(candidate)) return Path.Combine(dir.FullName, "data");
            dir = dir.Parent;
        }

        // Fallback: best-effort known dev location. App will error gracefully if missing.
        return @"C:\Prog\Projetos\CharacterWizard\CharacterWizard\data";
    }
}
