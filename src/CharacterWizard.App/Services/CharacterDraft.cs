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
    }

    public Character ToCharacter()
    {
        var classes = new List<CharacterClassEntry>();
        if (ClassRef is { } cr)
            classes.Add(new() { ClassRef = cr, Levels = InitialLevel, SubclassRef = SubclassRef });
        classes.AddRange(AdditionalClasses);

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
        };
    }
}
