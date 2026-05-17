using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Rules;

public static class AbilityScoreCalculator
{
    public static int Modifier(int score) => (int)Math.Floor((score - 10) / 2.0);

    /// <summary>
    /// Final ability scores = base + sum of ASI allocations across all level-ups +
    /// sum of permanent ability overrides. Race bonuses are NOT auto-applied here —
    /// they're already factored into the chosen BaseAbilityScores during creation.
    /// </summary>
    public static AbilityScores ComputeFinal(Character character)
    {
        var scores = character.BaseAbilityScores;

        foreach (var choice in character.ChoicesByLevel.Values)
        {
            if (choice.AsiOrFeat != "asi" || choice.AsiAllocation is null) continue;
            foreach (var (ability, delta) in choice.AsiAllocation)
                scores = scores.Plus(ability, delta);
        }

        foreach (var ovr in character.AbilityOverrides)
            scores = scores.Plus(ovr.Ability, ovr.Delta);

        return scores;
    }
}
