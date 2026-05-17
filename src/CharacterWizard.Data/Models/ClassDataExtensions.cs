using System.Text.Json;

namespace CharacterWizard.Data.Models;

public static class ClassDataExtensions
{
    /// <summary>
    /// Returns the level at which the class chooses a subclass, by scanning
    /// `classFeatures[]` for the entry flagged with `gainSubclassFeature: true`.
    /// Returns null when the class has no subclass mechanic at all (rare — most
    /// official classes have one) or when the JSON shape is unexpected.
    /// </summary>
    public static int? SubclassChoiceLevel(this ClassData cls)
    {
        var arr = cls.ClassFeatures;
        if (arr is not { ValueKind: JsonValueKind.Array }) return null;

        foreach (var item in arr.Value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            if (!item.TryGetProperty("gainSubclassFeature", out var flag)) continue;
            if (flag.ValueKind != JsonValueKind.True) continue;
            if (!item.TryGetProperty("classFeature", out var refEl) ||
                refEl.ValueKind != JsonValueKind.String) continue;

            // Pipe-separated: "FeatureName|ClassName|ClassSource|Level"
            var parts = refEl.GetString()!.Split('|');
            if (parts.Length >= 4 && int.TryParse(parts[3], out var lvl))
                return lvl;
        }

        return null;
    }

    /// <summary>True when the class has a spellcasting ability declared.</summary>
    public static bool IsCaster(this ClassData cls) =>
        !string.IsNullOrEmpty(cls.SpellcastingAbility);

    /// <summary>
    /// Number of cantrips known at the given total class level, from
    /// `cantripProgression[]`. Returns 0 when the class has no cantrips.
    /// </summary>
    public static int CantripsKnownAtLevel(this ClassData cls, int level)
    {
        var prog = cls.CantripProgression;
        if (prog is null || prog.Length == 0) return 0;
        var idx = Math.Clamp(level - 1, 0, prog.Length - 1);
        return prog[idx];
    }

    /// <summary>
    /// Number of leveled spells known/in spellbook at the given class level,
    /// from `spellsKnownProgressionFixed[]`. Note this is "delta per level" not
    /// cumulative — sum the entries up to `level` to get the running total.
    /// Returns 0 when not applicable.
    /// </summary>
    public static int SpellsKnownAtLevel(this ClassData cls, int level)
    {
        var prog = cls.SpellsKnownProgressionFixed;
        if (prog is null || prog.Length == 0) return 0;
        // Sum first `level` entries (delta-per-level table).
        var take = Math.Min(level, prog.Length);
        var sum = 0;
        for (int i = 0; i < take; i++) sum += prog[i];
        return sum;
    }

    /// <summary>
    /// Names of class features gained at the given level. Reads `classFeatures[]`
    /// which mixes string entries ("Name|Class||Level") and object entries
    /// ({ classFeature: "...", gainSubclassFeature: bool }).
    /// </summary>
    public static IEnumerable<string> FeaturesAtLevel(this ClassData cls, int level)
    {
        if (cls.ClassFeatures is not { ValueKind: JsonValueKind.Array } arr) yield break;
        foreach (var item in arr.EnumerateArray())
        {
            string? raw = item.ValueKind switch
            {
                JsonValueKind.String => item.GetString(),
                JsonValueKind.Object when item.TryGetProperty("classFeature", out var f) &&
                                          f.ValueKind == JsonValueKind.String => f.GetString(),
                _ => null,
            };
            if (raw is null) continue;
            var parts = raw.Split('|');
            if (parts.Length < 4) continue;
            if (int.TryParse(parts[3], out var lvl) && lvl == level)
                yield return parts[0];
        }
    }

    /// <summary>
    /// All class features up to and including the given level, with their level
    /// of acquisition. Useful for the character sheet "features so far" list.
    /// </summary>
    public static IEnumerable<(string Name, int Level)> FeaturesUpToLevel(this ClassData cls, int level)
    {
        if (cls.ClassFeatures is not { ValueKind: JsonValueKind.Array } arr) yield break;
        foreach (var item in arr.EnumerateArray())
        {
            string? raw = item.ValueKind switch
            {
                JsonValueKind.String => item.GetString(),
                JsonValueKind.Object when item.TryGetProperty("classFeature", out var f) &&
                                          f.ValueKind == JsonValueKind.String => f.GetString(),
                _ => null,
            };
            if (raw is null) continue;
            var parts = raw.Split('|');
            if (parts.Length < 4) continue;
            if (int.TryParse(parts[3], out var lvl) && lvl <= level)
                yield return (parts[0], lvl);
        }
    }
}

public static class SubclassDataExtensions
{
    /// <summary>
    /// Subclass feature names gained at the given level.
    /// `subclassFeatures` format: "FeatureName|Class||SubclassShortName||Level".
    /// </summary>
    public static IEnumerable<string> FeaturesAtLevel(this SubclassData sub, int level)
    {
        if (sub.SubclassFeatures is not { ValueKind: JsonValueKind.Array } arr) yield break;
        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String) continue;
            var parts = item.GetString()!.Split('|');
            if (parts.Length < 6) continue;
            if (int.TryParse(parts[5], out var lvl) && lvl == level)
                yield return parts[0];
        }
    }
}
