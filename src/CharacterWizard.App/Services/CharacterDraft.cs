using CharacterWizard.Core.Models;

namespace CharacterWizard.App.Services;

/// <summary>
/// Mutable in-progress character being built by the wizard. Held as a scoped
/// service so the wizard steps share state without prop drilling.
/// </summary>
public sealed class CharacterDraft
{
    public string Name { get; set; } = "";
    public EntityRef? RaceRef { get; set; }
    public string? SubraceName { get; set; }
    public EntityRef? ClassRef { get; set; }
    public EntityRef? SubclassRef { get; set; }
    public EntityRef? BackgroundRef { get; set; }
    public AbilityScores BaseAbilityScores { get; set; } =
        new() { Str = 8, Dex = 13, Con = 14, Int = 15, Wis = 12, Cha = 10 }; // standard array sample

    public int InitialLevel { get; set; } = 1;

    /// <summary>
    /// Secondary classes for multiclass. The primary class lives in ClassRef +
    /// SubclassRef + InitialLevel; each entry here is a full extra class entry
    /// that will be appended to Character.Classes.
    /// </summary>
    public List<CharacterClassEntry> AdditionalClasses { get; set; } = new();

    public List<EntityRef> KnownSpells { get; set; } = new();
    public List<InventoryItem> Inventory { get; set; } = new();

    /// <summary>HP gained per class level after the 1st, in order (level 2, 3, …).</summary>
    public List<int> HitPointRolls { get; set; } = new();

    /// <summary>ASI/feat choice per level for the primary class. Key is the level (4, 8, 12, 16, 19).</summary>
    public Dictionary<int, LevelAsiChoice> LevelAsi { get; set; } = new();

    public sealed class LevelAsiChoice
    {
        public string Mode { get; set; } = "asi"; // "asi" | "feat"
        public Dictionary<Ability, int> Allocation { get; set; } = new();
        public EntityRef? FeatRef { get; set; }
    }

    public void Reset()
    {
        Name = "";
        RaceRef = null;
        SubraceName = null;
        ClassRef = null;
        SubclassRef = null;
        BackgroundRef = null;
        BaseAbilityScores = new() { Str = 8, Dex = 13, Con = 14, Int = 15, Wis = 12, Cha = 10 };
        InitialLevel = 1;
        AdditionalClasses = new();
        KnownSpells = new();
        Inventory = new();
        HitPointRolls = new();
        LevelAsi = new();
    }

    public Character ToCharacter()
    {
        var primary = ClassRef is { } cr
            ? new CharacterClassEntry
            {
                ClassRef = cr,
                Levels = InitialLevel,
                SubclassRef = SubclassRef,
                HitPointRolls = HitPointRolls.Select(v => (int?)v).ToList(),
            }
            : null;

        var classes = new List<CharacterClassEntry>();
        if (primary is not null) classes.Add(primary);
        classes.AddRange(AdditionalClasses);

        // Materialize ASI/feat choices recorded during high-level creation into
        // the character's ChoicesByLevel. Keys are the *total* level at the time
        // the choice was made — since multiclass isn't decided yet during the
        // primary-class level-up step, total == primary level.
        var choicesByLevel = LevelAsi.ToDictionary(
            kv => kv.Key,
            kv =>
            {
                var c = new LevelChoices { AsiOrFeat = kv.Value.Mode };
                if (kv.Value.Mode == "asi") c.AsiAllocation = new(kv.Value.Allocation);
                else c.FeatRef = kv.Value.FeatRef;
                return c;
            });

        return new Character
        {
            Name = string.IsNullOrWhiteSpace(Name) ? "Unnamed" : Name,
            RaceRef = RaceRef,
            SubraceName = SubraceName,
            BackgroundRef = BackgroundRef,
            BaseAbilityScores = BaseAbilityScores,
            Classes = classes,
            KnownSpells = new(KnownSpells),
            Inventory = new(Inventory),
            ChoicesByLevel = choicesByLevel,
        };
    }
}
