using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record FeatData
{
    [JsonPropertyName("name")]    public string Name    { get; init; } = "";
    [JsonPropertyName("source")]  public string Source  { get; init; } = "";
    [JsonPropertyName("page")]    public int?   Page    { get; init; }

    [JsonPropertyName("prerequisite")] public JsonElement? Prerequisite { get; init; }
    [JsonPropertyName("ability")]      public JsonElement? Ability { get; init; }
    [JsonPropertyName("entries")]      public JsonElement? Entries { get; init; }

    [JsonPropertyName("reprintedAs")] public string[]? ReprintedAs { get; init; }

    public EntityRef Ref => new(Name, Source);
}
