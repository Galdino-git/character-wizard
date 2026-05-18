using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record SpellData
{
    [JsonPropertyName("name")]   public string Name   { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("page")]   public int?   Page   { get; init; }
    [JsonPropertyName("level")]  public int    Level  { get; init; }
    [JsonPropertyName("school")] public string? School { get; init; }

    [JsonPropertyName("time")]       public JsonElement? Time { get; init; }
    [JsonPropertyName("range")]      public JsonElement? Range { get; init; }
    [JsonPropertyName("components")] public JsonElement? Components { get; init; }
    [JsonPropertyName("duration")]   public JsonElement? Duration { get; init; }
    [JsonPropertyName("entries")]    public JsonElement? Entries { get; init; }
    [JsonPropertyName("entriesHigherLevel")] public JsonElement? EntriesHigherLevel { get; init; }

    [JsonPropertyName("damageInflict")] public string[]? DamageInflict { get; init; }
    [JsonPropertyName("savingThrow")]   public string[]? SavingThrow   { get; init; }

    [JsonPropertyName("classes")] public JsonElement? Classes { get; init; }

    [JsonPropertyName("reprintedAs")] public string[]? ReprintedAs { get; init; }

    public EntityRef Ref => new(Name, Source);
}
