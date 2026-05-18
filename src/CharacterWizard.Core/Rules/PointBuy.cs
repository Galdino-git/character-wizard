using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Rules;

/// <summary>
/// PHB point-buy ability score system. Scores must be in [8..15]; total cost
/// must equal 27 in a strict allocation, though we don't enforce equality
/// (the UI shows the running total and lets the user balance).
/// </summary>
public static class PointBuy
{
    public const int Budget = 27;
    public const int MinScore = 8;
    public const int MaxScore = 15;

    private static readonly int[] CostTable =
    {
        // index = score - 8 (so index 0 = score 8)
        0, // 8
        1, // 9
        2, // 10
        3, // 11
        4, // 12
        5, // 13
        7, // 14
        9, // 15
    };

    /// <summary>Returns the point-buy cost for a single ability score, or null if out of range.</summary>
    public static int? Cost(int score)
    {
        if (score < MinScore || score > MaxScore) return null;
        return CostTable[score - MinScore];
    }

    /// <summary>Total cost across all six abilities. Returns null if any score is out of range.</summary>
    public static int? TotalCost(AbilityScores scores)
    {
        var total = 0;
        foreach (var a in (Ability[])Enum.GetValues(typeof(Ability)))
        {
            var c = Cost(scores.Get(a));
            if (c is null) return null;
            total += c.Value;
        }
        return total;
    }
}
