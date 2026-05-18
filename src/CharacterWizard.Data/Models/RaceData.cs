using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record RaceData
{
    [JsonPropertyName("name")]   public string Name   { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("page")]   public int?   Page   { get; init; }

    [JsonPropertyName("size")]    public string[]? Size { get; init; }
    [JsonPropertyName("speed")]   public JsonElement? Speed { get; init; }
    [JsonPropertyName("ability")] public JsonElement? Ability { get; init; }

    [JsonPropertyName("languageProficiencies")] public JsonElement? LanguageProficiencies { get; init; }
    [JsonPropertyName("traitTags")]             public string[]?     TraitTags { get; init; }
    [JsonPropertyName("entries")]               public JsonElement?  Entries { get; init; }

    [JsonPropertyName("hasFluff")]      public bool? HasFluff      { get; init; }
    [JsonPropertyName("hasFluffImages")] public bool? HasFluffImages { get; init; }

    /// <summary>True if this is a "_copy" entry whose body lives elsewhere.</summary>
    [JsonPropertyName("_copy")] public JsonElement? CopyOf { get; init; }

    /// <summary>Names of newer entries that supersede this one (e.g. XPHB version of a PHB race).</summary>
    [JsonPropertyName("reprintedAs")] public string[]? ReprintedAs { get; init; }

    public EntityRef Ref => new(Name, Source);
}

public sealed record SubraceData
{
    [JsonPropertyName("name")]      public string? Name      { get; init; }
    [JsonPropertyName("source")]    public string  Source    { get; init; } = "";
    [JsonPropertyName("raceName")]  public string  RaceName  { get; init; } = "";
    [JsonPropertyName("raceSource")] public string RaceSource { get; init; } = "";
    [JsonPropertyName("ability")]   public JsonElement? Ability { get; init; }
    [JsonPropertyName("entries")]   public JsonElement? Entries { get; init; }
    [JsonPropertyName("reprintedAs")] public string[]? ReprintedAs { get; init; }
}
