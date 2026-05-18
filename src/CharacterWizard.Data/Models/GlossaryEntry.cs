using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

/// <summary>
/// Shared shape across conditions/diseases/skills/actions/senses — all of them
/// have just a name, source, page, and entries body. We use one record because
/// modeling six near-identical types adds noise without payoff.
/// </summary>
public sealed record GlossaryEntry
{
    [JsonPropertyName("name")]    public string Name   { get; init; } = "";
    [JsonPropertyName("source")]  public string Source { get; init; } = "";
    [JsonPropertyName("page")]    public int?   Page   { get; init; }
    [JsonPropertyName("entries")] public JsonElement? Entries { get; init; }

    public EntityRef Ref => new(Name, Source);
}

/// <summary>
/// Single class/subclass feature with full body (extracted from the class JSON
/// files' top-level `classFeature[]` / `subclassFeature[]` arrays). The 5etools
/// references like "Arcane Recovery|Wizard||1" resolve against this list.
/// </summary>
public sealed record ClassFeatureEntry
{
    [JsonPropertyName("name")]        public string Name        { get; init; } = "";
    [JsonPropertyName("source")]      public string Source      { get; init; } = "";
    [JsonPropertyName("page")]        public int?   Page        { get; init; }
    [JsonPropertyName("className")]   public string ClassName   { get; init; } = "";
    [JsonPropertyName("classSource")] public string ClassSource { get; init; } = "";
    [JsonPropertyName("level")]       public int    Level       { get; init; }
    [JsonPropertyName("entries")]     public JsonElement? Entries { get; init; }
}

public sealed record SubclassFeatureEntry
{
    [JsonPropertyName("name")]              public string Name             { get; init; } = "";
    [JsonPropertyName("source")]            public string Source           { get; init; } = "";
    [JsonPropertyName("page")]              public int?   Page             { get; init; }
    [JsonPropertyName("className")]         public string ClassName        { get; init; } = "";
    [JsonPropertyName("classSource")]       public string ClassSource      { get; init; } = "";
    [JsonPropertyName("subclassShortName")] public string SubclassShortName { get; init; } = "";
    [JsonPropertyName("subclassSource")]    public string SubclassSource   { get; init; } = "";
    [JsonPropertyName("level")]             public int    Level            { get; init; }
    [JsonPropertyName("entries")]           public JsonElement? Entries    { get; init; }
}
