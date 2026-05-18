using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record ItemData
{
    [JsonPropertyName("name")]   public string Name   { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("page")]   public int?   Page   { get; init; }

    [JsonPropertyName("type")]     public string? Type     { get; init; }
    [JsonPropertyName("rarity")]   public string? Rarity   { get; init; }
    [JsonPropertyName("weight")]   public double? Weight   { get; init; }
    [JsonPropertyName("value")]    public double? ValueInCp { get; init; }
    [JsonPropertyName("reqAttune")] public JsonElement? RequiresAttunement { get; init; }

    [JsonPropertyName("weapon")]   public bool? IsWeapon { get; init; }
    [JsonPropertyName("armor")]    public bool? IsArmor  { get; init; }

    [JsonPropertyName("entries")]  public JsonElement? Entries { get; init; }

    [JsonPropertyName("reprintedAs")] public string[]? ReprintedAs { get; init; }

    public EntityRef Ref => new(Name, Source);
}
