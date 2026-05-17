using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Import5eToolsData;

internal static class Program
{
    private static readonly string[] JsonFilesToCopy =
    [
        "books.json",
        "adventures.json",
        "races.json",
        "fluff-races.json",
        "backgrounds.json",
        "fluff-backgrounds.json",
        "feats.json",
        "fluff-feats.json",
        "optionalfeatures.json",
        "items.json",
        "items-base.json",
        "fluff-items.json",
        "conditionsdiseases.json",
        "actions.json",
        "skills.json",
        "senses.json",
        "languages.json",
        "deities.json",
        "loot.json",
    ];

    private static readonly string[] JsonFolderGlobs =
    [
        "class/class-*.json",
        "spells/spells-*.json",
    ];

    private static readonly string[] ImageFoldersToCopy =
    [
        "classes",
        "races",
        "backgrounds",
        "items",
        "objects",
        "covers",
    ];

    public static async Task<int> Main(string[] args)
    {
        var sourceOption = new Option<DirectoryInfo>(
            name: "--source",
            description: "Path to the 5etools repo root (folder containing data/ and img/).")
        { IsRequired = true };

        var targetOption = new Option<DirectoryInfo>(
            name: "--target",
            description: "Output folder where JSON and images will be copied (typically <repo>/data).")
        { IsRequired = true };

        var skipImagesOption = new Option<bool>(
            name: "--skip-images",
            description: "Skip image copy (JSON only). Useful for fast iteration.");

        var verboseOption = new Option<bool>(name: "--verbose", description: "Verbose output.");

        var rootCommand = new RootCommand("Imports 5etools JSON data and a subset of images into CharacterWizard.")
        {
            sourceOption,
            targetOption,
            skipImagesOption,
            verboseOption,
        };

        rootCommand.SetHandler(
            (source, target, skipImages, verbose) => RunAsync(source, target, skipImages, verbose),
            sourceOption, targetOption, skipImagesOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task<int> RunAsync(DirectoryInfo source, DirectoryInfo target, bool skipImages, bool verbose)
    {
        var sourceData = new DirectoryInfo(Path.Combine(source.FullName, "data"));
        var sourceImg  = new DirectoryInfo(Path.Combine(source.FullName, "img"));

        if (!sourceData.Exists)
        {
            Console.Error.WriteLine($"ERROR: source data folder not found: {sourceData.FullName}");
            return 1;
        }

        var targetJson = new DirectoryInfo(Path.Combine(target.FullName, "5etools-json"));
        var targetImg  = new DirectoryInfo(Path.Combine(target.FullName, "5etools-img"));
        Directory.CreateDirectory(targetJson.FullName);
        if (!skipImages) Directory.CreateDirectory(targetImg.FullName);

        Console.WriteLine($"Source: {source.FullName}");
        Console.WriteLine($"Target: {target.FullName}");
        Console.WriteLine();

        var jsonCount = CopyJsonFiles(sourceData, targetJson, verbose);
        var (imgCount, imgBytes) = skipImages ? (0, 0L) : CopyImages(sourceImg, targetImg, verbose);

        await WriteManifestAsync(source, target, jsonCount, imgCount, imgBytes);

        Console.WriteLine();
        Console.WriteLine($"Done. {jsonCount} JSON files copied, {imgCount} images ({FormatSize(imgBytes)}).");
        return 0;
    }

    private static int CopyJsonFiles(DirectoryInfo sourceData, DirectoryInfo targetJson, bool verbose)
    {
        var count = 0;

        foreach (var file in JsonFilesToCopy)
        {
            var src = Path.Combine(sourceData.FullName, file);
            if (!File.Exists(src))
            {
                Console.WriteLine($"  [skip] not found: {file}");
                continue;
            }
            var dst = Path.Combine(targetJson.FullName, file);
            Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
            File.Copy(src, dst, overwrite: true);
            if (verbose) Console.WriteLine($"  [json] {file}");
            count++;
        }

        foreach (var glob in JsonFolderGlobs)
        {
            var folder = Path.GetDirectoryName(glob)!;
            var pattern = Path.GetFileName(glob);
            var folderPath = Path.Combine(sourceData.FullName, folder);
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"  [skip] folder not found: {folder}");
                continue;
            }

            var dstFolder = Path.Combine(targetJson.FullName, folder);
            Directory.CreateDirectory(dstFolder);

            foreach (var src in Directory.EnumerateFiles(folderPath, pattern))
            {
                var dst = Path.Combine(dstFolder, Path.GetFileName(src));
                File.Copy(src, dst, overwrite: true);
                if (verbose) Console.WriteLine($"  [json] {folder}/{Path.GetFileName(src)}");
                count++;
            }
        }

        Console.WriteLine($"JSON: {count} files");
        return count;
    }

    private static (int count, long bytes) CopyImages(DirectoryInfo sourceImg, DirectoryInfo targetImg, bool verbose)
    {
        if (!sourceImg.Exists)
        {
            Console.WriteLine($"  [skip] images source not found: {sourceImg.FullName}");
            return (0, 0);
        }

        var totalCount = 0;
        var totalBytes = 0L;

        foreach (var folder in ImageFoldersToCopy)
        {
            var srcFolder = new DirectoryInfo(Path.Combine(sourceImg.FullName, folder));
            if (!srcFolder.Exists) continue;

            var dstFolder = new DirectoryInfo(Path.Combine(targetImg.FullName, folder));
            Directory.CreateDirectory(dstFolder.FullName);

            var (c, b) = CopyDirectoryRecursive(srcFolder, dstFolder, verbose);
            Console.WriteLine($"  [img] {folder,-12} {c,6} files  {FormatSize(b)}");
            totalCount += c;
            totalBytes += b;
        }

        Console.WriteLine($"IMG : {totalCount} files, {FormatSize(totalBytes)}");
        return (totalCount, totalBytes);
    }

    private static (int count, long bytes) CopyDirectoryRecursive(DirectoryInfo src, DirectoryInfo dst, bool verbose)
    {
        var count = 0;
        var bytes = 0L;

        foreach (var file in src.GetFiles())
        {
            var dstPath = Path.Combine(dst.FullName, file.Name);
            file.CopyTo(dstPath, overwrite: true);
            count++;
            bytes += file.Length;
            if (verbose) Console.WriteLine($"    {file.FullName.Substring(src.FullName.Length).TrimStart('\\','/')}");
        }

        foreach (var subDir in src.GetDirectories())
        {
            var dstSub = new DirectoryInfo(Path.Combine(dst.FullName, subDir.Name));
            Directory.CreateDirectory(dstSub.FullName);
            var (c, b) = CopyDirectoryRecursive(subDir, dstSub, verbose);
            count += c;
            bytes += b;
        }

        return (count, bytes);
    }

    private static async Task WriteManifestAsync(
        DirectoryInfo source, DirectoryInfo target,
        int jsonCount, int imgCount, long imgBytes)
    {
        var manifestPath = Path.Combine(target.FullName, "MANIFEST.json");
        var manifest = new
        {
            importedAtUtc = DateTime.UtcNow.ToString("O"),
            sourcePath = source.FullName,
            sourceHash = await TryComputeShortHashAsync(Path.Combine(source.FullName, "data", "books.json")),
            counts = new { json = jsonCount, images = imgCount, imageBytes = imgBytes },
        };

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, json, Encoding.UTF8);
        Console.WriteLine($"Manifest: {manifestPath}");
    }

    private static async Task<string?> TryComputeShortHashAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        await using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        double v = bytes;
        string[] units = ["KB", "MB", "GB"];
        var i = -1;
        do { v /= 1024; i++; } while (v >= 1024 && i < units.Length - 1);
        return $"{v,7:F1} {units[i]}";
    }
}
