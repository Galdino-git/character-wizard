using CharacterWizard.Core.Models;

namespace CharacterWizard.Core.Rules;

/// <summary>
/// Computes derived save bonuses for a character. Saving throw proficiencies
/// come from the first class taken (RAW multiclass rule); the modifier is
/// ability mod + proficiency bonus when proficient, otherwise just ability mod.
/// </summary>
public static class ProficiencyResolver
{
    public delegate IReadOnlyList<string>? SaveProficienciesLookup(EntityRef classRef);

    public static SaveTable ComputeSaves(Character character, SaveProficienciesLookup lookup)
    {
        var final = AbilityScoreCalculator.ComputeFinal(character);
        var pb = ProficiencyBonus.ForLevel(character.TotalLevel);

        // RAW: when multiclassing, you don't gain new save proficiencies — only the
        // ones from your first class. We honor that by looking up the first class.
        var firstClass = character.Classes.FirstOrDefault();
        var profSet = firstClass is null
            ? new HashSet<string>()
            : new HashSet<string>(
                (lookup(firstClass.ClassRef) ?? Array.Empty<string>())
                    .Select(s => s.ToLowerInvariant()),
                StringComparer.OrdinalIgnoreCase);

        return new SaveTable
        {
            Str = MakeSave(Ability.Str, final, pb, profSet, "str"),
            Dex = MakeSave(Ability.Dex, final, pb, profSet, "dex"),
            Con = MakeSave(Ability.Con, final, pb, profSet, "con"),
            Int = MakeSave(Ability.Int, final, pb, profSet, "int"),
            Wis = MakeSave(Ability.Wis, final, pb, profSet, "wis"),
            Cha = MakeSave(Ability.Cha, final, pb, profSet, "cha"),
        };
    }

    private static SaveValue MakeSave(Ability a, AbilityScores final, int pb, HashSet<string> profSet, string key)
    {
        var mod = AbilityScoreCalculator.Modifier(final.Get(a));
        var proficient = profSet.Contains(key);
        return new SaveValue(mod + (proficient ? pb : 0), proficient);
    }
}

public sealed record SaveValue(int Modifier, bool Proficient);

public sealed record SaveTable
{
    public required SaveValue Str { get; init; }
    public required SaveValue Dex { get; init; }
    public required SaveValue Con { get; init; }
    public required SaveValue Int { get; init; }
    public required SaveValue Wis { get; init; }
    public required SaveValue Cha { get; init; }

    public SaveValue Get(Ability a) => a switch
    {
        Ability.Str => Str,
        Ability.Dex => Dex,
        Ability.Con => Con,
        Ability.Int => Int,
        Ability.Wis => Wis,
        Ability.Cha => Cha,
        _ => throw new ArgumentOutOfRangeException(nameof(a)),
    };
}
