namespace CharacterWizard.Core.Rules;

public static class ProficiencyBonus
{
    /// <summary>PHB proficiency bonus by total character level (1..20).</summary>
    public static int ForLevel(int totalLevel) => totalLevel switch
    {
        <= 0 => 0,
        <= 4 => 2,
        <= 8 => 3,
        <= 12 => 4,
        <= 16 => 5,
        _ => 6,
    };
}
