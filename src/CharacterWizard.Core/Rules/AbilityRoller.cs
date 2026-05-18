namespace CharacterWizard.Core.Rules;

/// <summary>
/// 4d6-drop-lowest roller — the most common rolled-stats variant for D&D 5e.
/// Pure (takes RNG seed/instance) so it's deterministic in tests.
/// </summary>
public static class AbilityRoller
{
    /// <summary>Rolls a single value: 4d6, drop the lowest, sum the rest.</summary>
    public static int Roll4d6DropLowest(Random rng)
    {
        Span<int> rolls = stackalloc int[4];
        for (int i = 0; i < 4; i++) rolls[i] = rng.Next(1, 7); // 1..6
        int min = rolls[0], sum = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += rolls[i];
            if (rolls[i] < min) min = rolls[i];
        }
        return sum - min;
    }

    /// <summary>Rolls 6 values for the player to assign to abilities.</summary>
    public static int[] RollSix(Random rng)
    {
        var result = new int[6];
        for (int i = 0; i < 6; i++) result[i] = Roll4d6DropLowest(rng);
        return result;
    }
}
