namespace CharacterWizard.Core.Rules;

/// <summary>
/// Full-caster spell slot progression (PHB). Indexed [level-1][spellLevel-1].
/// Use <see cref="GetSlots"/> to query. For multiclass spellcasting, sum the
/// "effective caster levels" of each casting class and look up the same table.
/// </summary>
public static class SpellSlotTable
{
    // [characterLevel-1, spellLevel-1] -> number of slots (9 spell levels)
    private static readonly int[,] FullCaster = new int[20, 9]
    {
        { 2, 0, 0, 0, 0, 0, 0, 0, 0 }, // 1
        { 3, 0, 0, 0, 0, 0, 0, 0, 0 }, // 2
        { 4, 2, 0, 0, 0, 0, 0, 0, 0 }, // 3
        { 4, 3, 0, 0, 0, 0, 0, 0, 0 }, // 4
        { 4, 3, 2, 0, 0, 0, 0, 0, 0 }, // 5
        { 4, 3, 3, 0, 0, 0, 0, 0, 0 }, // 6
        { 4, 3, 3, 1, 0, 0, 0, 0, 0 }, // 7
        { 4, 3, 3, 2, 0, 0, 0, 0, 0 }, // 8
        { 4, 3, 3, 3, 1, 0, 0, 0, 0 }, // 9
        { 4, 3, 3, 3, 2, 0, 0, 0, 0 }, // 10
        { 4, 3, 3, 3, 2, 1, 0, 0, 0 }, // 11
        { 4, 3, 3, 3, 2, 1, 0, 0, 0 }, // 12
        { 4, 3, 3, 3, 2, 1, 1, 0, 0 }, // 13
        { 4, 3, 3, 3, 2, 1, 1, 0, 0 }, // 14
        { 4, 3, 3, 3, 2, 1, 1, 1, 0 }, // 15
        { 4, 3, 3, 3, 2, 1, 1, 1, 0 }, // 16
        { 4, 3, 3, 3, 2, 1, 1, 1, 1 }, // 17
        { 4, 3, 3, 3, 3, 1, 1, 1, 1 }, // 18
        { 4, 3, 3, 3, 3, 2, 1, 1, 1 }, // 19
        { 4, 3, 3, 3, 3, 2, 2, 1, 1 }, // 20
    };

    public static int GetSlots(int casterLevel, int spellLevel)
    {
        if (casterLevel is < 1 or > 20) return 0;
        if (spellLevel is < 1 or > 9) return 0;
        return FullCaster[casterLevel - 1, spellLevel - 1];
    }

    /// <summary>Returns the full row of slots (length 9) for the given caster level.</summary>
    public static int[] GetSlotRow(int casterLevel)
    {
        var row = new int[9];
        for (int i = 0; i < 9; i++) row[i] = GetSlots(casterLevel, i + 1);
        return row;
    }
}
