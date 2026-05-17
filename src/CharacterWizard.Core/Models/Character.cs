using System.Text.Json.Serialization;

namespace CharacterWizard.Core.Models;

/// <summary>
/// Persisted character. References to game rules (races, classes, etc.) are kept
/// by (Name, Source) — never duplicate the rules themselves. Derived stats
/// (HP total, AC, spell slots) are computed at read time from refs + choices.
/// </summary>
public sealed record Character
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "Unnamed";
    public string? PlayerName { get; set; }

    public EntityRef? RaceRef { get; set; }
    public string? SubraceName { get; set; }
    public EntityRef? BackgroundRef { get; set; }

    /// <summary>Class entries, ordered by the order they were taken (relevant for multiclass).</summary>
    public List<CharacterClassEntry> Classes { get; set; } = new();

    public AbilityScores BaseAbilityScores { get; set; } = new();
    public List<AbilityOverride> AbilityOverrides { get; set; } = new();

    public List<InventoryItem> Inventory { get; set; } = new();
    public List<EntityRef> KnownSpells { get; set; } = new();
    public List<EntityRef> PreparedSpells { get; set; } = new();

    /// <summary>Choices made at each level-up: ASI vs feat, fighting style, etc. Key is the level.</summary>
    public Dictionary<int, LevelChoices> ChoicesByLevel { get; set; } = new();

    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public int TotalLevel => Classes.Sum(c => c.Levels);
}

public sealed record CharacterClassEntry
{
    public required EntityRef ClassRef { get; init; }
    public EntityRef? SubclassRef { get; set; }
    public int Levels { get; set; } = 1;

    /// <summary>HP rolls for each level after the first (which always takes the max). Null entries = use average.</summary>
    public List<int?> HitPointRolls { get; set; } = new();
}

public sealed record InventoryItem
{
    public required EntityRef ItemRef { get; init; }
    public int Quantity { get; set; } = 1;
    public bool Equipped { get; set; }

    /// <summary>If set, displayed instead of the base item name (e.g. "Espada Élfica +2").</summary>
    public string? CustomName { get; set; }
    public int BonusAttack { get; set; }
    public int BonusDamage { get; set; }
    public int BonusAc { get; set; }
    public string? Notes { get; set; }
}

public sealed record LevelChoices
{
    /// <summary>"asi" | "feat" | null.</summary>
    public string? AsiOrFeat { get; set; }

    /// <summary>
    /// Allocation of ability score increases when AsiOrFeat = "asi".
    /// Keyed by ability; value is the number of points added (typically 2 to one
    /// stat, or 1+1 to two stats — total 2 per ASI per PHB rules).
    /// </summary>
    public Dictionary<Ability, int>? AsiAllocation { get; set; }

    /// <summary>Feat picked when AsiOrFeat = "feat".</summary>
    public EntityRef? FeatRef { get; set; }

    public string? FightingStyle { get; set; }

    /// <summary>Spells learned at this level (by Name|Source).</summary>
    public List<EntityRef> SpellsLearned { get; set; } = new();

    /// <summary>Free-form notes about other choices (e.g., expertise, maneuvers).</summary>
    public string? Notes { get; set; }
}
