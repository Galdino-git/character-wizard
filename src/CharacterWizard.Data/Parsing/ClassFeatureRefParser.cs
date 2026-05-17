namespace CharacterWizard.Data.Parsing;

/// <summary>
/// Parses the 5etools pipe-separated feature reference strings used in
/// class/subclass JSON files.
///
/// Class feature format:    "FeatureName|ClassName|ClassSource|Level|OptionalSource"
/// Subclass feature format: "FeatureName|ClassName|ClassSource|SubclassShortName|SubclassSource|Level|OptionalSource"
///
/// Notes:
/// - Empty segments mean "same as the parent class/subclass source".
/// - The optional trailing source segment overrides the feature source itself
///   (used when a feature is reprinted in a different book from its class).
/// </summary>
public static class ClassFeatureRefParser
{
    public static ClassFeatureRef ParseClassFeature(string raw, string defaultClassSource)
    {
        var parts = raw.Split('|');
        if (parts.Length < 4)
            throw new FormatException($"Class feature ref must have at least 4 segments: '{raw}'");

        var name        = parts[0].Trim();
        var className   = parts[1].Trim();
        var classSource = string.IsNullOrEmpty(parts[2]) ? defaultClassSource : parts[2].Trim();
        if (!int.TryParse(parts[3], out var level))
            throw new FormatException($"Class feature ref level segment is not an integer: '{raw}'");
        var featureSource = parts.Length >= 5 && !string.IsNullOrEmpty(parts[4]) ? parts[4].Trim() : classSource;

        return new ClassFeatureRef(name, className, classSource, level, featureSource);
    }

    public static SubclassFeatureRef ParseSubclassFeature(string raw, string defaultClassSource)
    {
        var parts = raw.Split('|');
        if (parts.Length < 6)
            throw new FormatException($"Subclass feature ref must have at least 6 segments: '{raw}'");

        var name          = parts[0].Trim();
        var className     = parts[1].Trim();
        var classSource   = string.IsNullOrEmpty(parts[2]) ? defaultClassSource : parts[2].Trim();
        var subclassShort = parts[3].Trim();
        var subclassSrc   = string.IsNullOrEmpty(parts[4]) ? classSource : parts[4].Trim();
        if (!int.TryParse(parts[5], out var level))
            throw new FormatException($"Subclass feature ref level segment is not an integer: '{raw}'");
        var featureSource = parts.Length >= 7 && !string.IsNullOrEmpty(parts[6]) ? parts[6].Trim() : subclassSrc;

        return new SubclassFeatureRef(name, className, classSource, subclassShort, subclassSrc, level, featureSource);
    }
}

public readonly record struct ClassFeatureRef(
    string Name,
    string ClassName,
    string ClassSource,
    int    Level,
    string FeatureSource);

public readonly record struct SubclassFeatureRef(
    string Name,
    string ClassName,
    string ClassSource,
    string SubclassShortName,
    string SubclassSource,
    int    Level,
    string FeatureSource);
