namespace CharacterWizard.Core.Models;

/// <summary>
/// Reference to a 5etools entity (race, class, background, spell, item, feat...).
/// Mirrors CharacterWizard.Data.Models.EntityRef but lives in Core so Core
/// doesn't depend on Data.
/// </summary>
public readonly record struct EntityRef(string Name, string Source)
{
    public string Key => $"{Name}|{Source}";
    public override string ToString() => Key;
}
