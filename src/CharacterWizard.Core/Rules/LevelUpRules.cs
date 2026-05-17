using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Rules;

/// <summary>
/// Pure helpers for the level-up flow: HP gained per level (average / max),
/// whether a class level is an ASI level, etc. Independent of UI and persistence.
/// </summary>
public static class LevelUpRules
{
    /// <summary>
    /// PHB-standard ASI levels for most classes: 4, 8, 12, 16, 19.
    /// Fighter (6, 14) and Rogue (10) get extras — those are class-specific and
    /// looked up from the class data when we wire it; for the MVP this method
    /// returns the universal set.
    /// </summary>
    public static bool IsAsiLevel(int classLevel) =>
        classLevel is 4 or 8 or 12 or 16 or 19;

    /// <summary>Average HP gained per level after 1st (rounded up, per 5e RAW).</summary>
    public static int AverageHpPerLevel(int hitDiceFaces) =>
        (hitDiceFaces / 2) + 1;

    /// <summary>HP gained at a given new level — caller supplies CON modifier.</summary>
    public static int HpGainedAtLevel(int newLevel, int hitDiceFaces, int conMod, HpMode mode, int manualRoll = 0)
    {
        var base_ = mode switch
        {
            HpMode.Average => newLevel == 1 ? hitDiceFaces : AverageHpPerLevel(hitDiceFaces),
            HpMode.Maximum => hitDiceFaces,
            HpMode.Manual  => manualRoll,
            _              => AverageHpPerLevel(hitDiceFaces),
        };
        return base_ + conMod;
    }
}

public enum HpMode { Average, Maximum, Manual }
