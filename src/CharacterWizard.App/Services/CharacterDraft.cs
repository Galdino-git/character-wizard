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
    public EntityRef? ClassRef { get; set; }
    public EntityRef? BackgroundRef { get; set; }
    public AbilityScores BaseAbilityScores { get; set; } =
        new() { Str = 8, Dex = 13, Con = 14, Int = 15, Wis = 12, Cha = 10 }; // standard array sample

    public int InitialLevel { get; set; } = 1;

    public void Reset()
    {
        Name = "";
        RaceRef = null;
        ClassRef = null;
        BackgroundRef = null;
        BaseAbilityScores = new() { Str = 8, Dex = 13, Con = 14, Int = 15, Wis = 12, Cha = 10 };
        InitialLevel = 1;
    }

    public Character ToCharacter() => new()
    {
        Name = string.IsNullOrWhiteSpace(Name) ? "Unnamed" : Name,
        RaceRef = RaceRef,
        BackgroundRef = BackgroundRef,
        BaseAbilityScores = BaseAbilityScores,
        Classes = ClassRef is { } cr
            ? new List<CharacterClassEntry> { new() { ClassRef = cr, Levels = InitialLevel } }
            : new(),
    };
}
