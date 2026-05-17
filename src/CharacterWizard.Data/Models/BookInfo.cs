using System.Text.Json.Serialization;

namespace CharacterWizard.Data.Models;

public sealed record BookInfo
{
    [JsonPropertyName("name")]   public string Name   { get; init; } = "";
    [JsonPropertyName("id")]     public string Id     { get; init; } = "";
    [JsonPropertyName("source")] public string Source { get; init; } = "";

    /// <summary>core, supplement, supplement-alt, setting, setting-alt, organized-play, screen, homecraft, recipe, other.</summary>
    [JsonPropertyName("group")]     public string? Group     { get; init; }
    [JsonPropertyName("published")] public string? Published { get; init; }
    [JsonPropertyName("author")]    public string? Author    { get; init; }

    [JsonPropertyName("cover")] public CoverInfo? Cover { get; init; }
}

public sealed record CoverInfo
{
    [JsonPropertyName("type")] public string? Type { get; init; }
    [JsonPropertyName("path")] public string? Path { get; init; }
}
