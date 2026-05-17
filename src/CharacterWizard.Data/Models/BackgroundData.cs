using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record BackgroundData
{
    [JsonPropertyName("name")]    public string Name    { get; init; } = "";
    [JsonPropertyName("source")]  public string Source  { get; init; } = "";
    [JsonPropertyName("page")]    public int?   Page    { get; init; }
    [JsonPropertyName("edition")] public string? Edition { get; init; }

    [JsonPropertyName("ability")]            public JsonElement? Ability { get; init; }
    [JsonPropertyName("skillProficiencies")] public JsonElement? SkillProficiencies { get; init; }
    [JsonPropertyName("toolProficiencies")]  public JsonElement? ToolProficiencies { get; init; }
    [JsonPropertyName("languageProficiencies")] public JsonElement? LanguageProficiencies { get; init; }
    [JsonPropertyName("feats")]              public JsonElement? Feats { get; init; }
    [JsonPropertyName("startingEquipment")]  public JsonElement? StartingEquipment { get; init; }
    [JsonPropertyName("entries")]            public JsonElement? Entries { get; init; }

    [JsonPropertyName("hasFluff")]       public bool? HasFluff { get; init; }
    [JsonPropertyName("hasFluffImages")] public bool? HasFluffImages { get; init; }

    public EntityRef Ref => new(Name, Source);
}
