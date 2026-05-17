using System.Text.Json;
using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record ClassData
{
    [JsonPropertyName("name")]   public string Name   { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";
    [JsonPropertyName("page")]   public int?   Page   { get; init; }
    [JsonPropertyName("edition")] public string? Edition { get; init; }

    [JsonPropertyName("hd")]           public HitDice? HitDice { get; init; }
    [JsonPropertyName("proficiency")]  public string[]? SavingThrowProficiencies { get; init; }

    [JsonPropertyName("spellcastingAbility")] public string? SpellcastingAbility { get; init; }
    [JsonPropertyName("casterProgression")]   public string? CasterProgression { get; init; }
    [JsonPropertyName("cantripProgression")]  public int[]?  CantripProgression { get; init; }
    [JsonPropertyName("spellsKnownProgressionFixed")] public int[]? SpellsKnownProgressionFixed { get; init; }
    [JsonPropertyName("preparedSpells")] public string? PreparedSpellsFormula { get; init; }

    [JsonPropertyName("startingProficiencies")] public JsonElement? StartingProficiencies { get; init; }
    [JsonPropertyName("startingEquipment")]     public JsonElement? StartingEquipment { get; init; }
    [JsonPropertyName("multiclassing")]         public JsonElement? Multiclassing { get; init; }

    /// <summary>
    /// Raw entries. Each item is either a string (legacy short ref) or an object
    /// with shape { "classFeature": "Name|Class||Level", "gainSubclassFeature": bool }.
    /// </summary>
    [JsonPropertyName("classFeatures")] public JsonElement? ClassFeatures { get; init; }

    [JsonPropertyName("classTableGroups")] public JsonElement? ClassTableGroups { get; init; }

    [JsonPropertyName("subclassTitle")] public string? SubclassTitle { get; init; }

    [JsonPropertyName("hasFluff")]       public bool? HasFluff { get; init; }
    [JsonPropertyName("hasFluffImages")] public bool? HasFluffImages { get; init; }

    public EntityRef Ref => new(Name, Source);
}

public sealed record HitDice
{
    [JsonPropertyName("number")] public int Number { get; init; }
    [JsonPropertyName("faces")]  public int Faces  { get; init; }
}

public sealed record SubclassData
{
    [JsonPropertyName("name")]        public string Name        { get; init; } = "";
    [JsonPropertyName("shortName")]   public string? ShortName  { get; init; }
    [JsonPropertyName("source")]      public string Source      { get; init; } = "";
    [JsonPropertyName("className")]   public string ClassName   { get; init; } = "";
    [JsonPropertyName("classSource")] public string ClassSource { get; init; } = "";

    [JsonPropertyName("subclassFeatures")] public JsonElement? SubclassFeatures { get; init; }

    public EntityRef Ref => new(Name, Source);
}

public sealed record ClassFeatureData
{
    [JsonPropertyName("name")]        public string Name        { get; init; } = "";
    [JsonPropertyName("source")]      public string Source      { get; init; } = "";
    [JsonPropertyName("className")]   public string ClassName   { get; init; } = "";
    [JsonPropertyName("classSource")] public string ClassSource { get; init; } = "";
    [JsonPropertyName("level")]       public int    Level       { get; init; }
    [JsonPropertyName("entries")]     public JsonElement? Entries { get; init; }
}

public sealed record SubclassFeatureData
{
    [JsonPropertyName("name")]           public string Name           { get; init; } = "";
    [JsonPropertyName("source")]         public string Source         { get; init; } = "";
    [JsonPropertyName("className")]      public string ClassName      { get; init; } = "";
    [JsonPropertyName("classSource")]    public string ClassSource    { get; init; } = "";
    [JsonPropertyName("subclassShortName")] public string SubclassShortName { get; init; } = "";
    [JsonPropertyName("subclassSource")] public string SubclassSource { get; init; } = "";
    [JsonPropertyName("level")]          public int    Level          { get; init; }
    [JsonPropertyName("entries")]        public JsonElement? Entries  { get; init; }
}
