using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Rules;

/// <summary>
/// Computes a character's maximum HP from the persisted hit point rolls plus
/// CON modifier per class level. Levels for which the user did not record a
/// roll (e.g. a character created at level 6 without going through level-up
/// prompts) fall back to the class average.
///
/// PHB rule recap (per class entry):
///     L1     → max hit die + CON mod
///     L2..N  → (rolled value OR average) + CON mod, per level
/// </summary>
public static class HitPointCalculator
{
    /// <summary>
    /// Resolves hit-dice faces by class entry; supplied by caller because Core
    /// must not depend on Data. Keys are EntityRef. Returns null when class
    /// can't be resolved — that entry contributes 0 (defensive).
    /// </summary>
    public delegate int? HitDiceFacesLookup(EntityRef classRef);

    public static int ComputeMaxHp(Character character, HitDiceFacesLookup lookup)
    {
        var conMod = AbilityScoreCalculator.Modifier(
            AbilityScoreCalculator.ComputeFinal(character).Con);

        var total = 0;
        foreach (var entry in character.Classes)
        {
            var faces = lookup(entry.ClassRef);
            if (faces is null || entry.Levels <= 0) continue;

            // L1: max die + CON mod
            total += faces.Value + conMod;

            // L2..N: rolled OR average, + CON mod
            var average = (faces.Value / 2) + 1;
            for (int lvl = 2; lvl <= entry.Levels; lvl++)
            {
                var idx = lvl - 2; // HitPointRolls[0] is the roll for level 2
                int roll;
                if (idx < entry.HitPointRolls.Count && entry.HitPointRolls[idx] is int r && r > 0)
                {
                    // Stored rolls already include CON mod (see LevelUpModal.Apply),
                    // so don't re-add it.
                    roll = r;
                }
                else
                {
                    roll = average + conMod;
                }
                total += roll;
            }
        }
        return total;
    }
}
