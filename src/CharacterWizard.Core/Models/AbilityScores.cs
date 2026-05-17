using System.Text.Json.Serialization;

namespace CharacterWizard.Core.Models;

public enum Ability { Str, Dex, Con, Int, Wis, Cha }

public sealed record AbilityScores
{
    public int Str { get; init; } = 10;
    public int Dex { get; init; } = 10;
    public int Con { get; init; } = 10;
    public int Int { get; init; } = 10;
    public int Wis { get; init; } = 10;
    public int Cha { get; init; } = 10;

    public int Get(Ability a) => a switch
    {
        Ability.Str => Str,
        Ability.Dex => Dex,
        Ability.Con => Con,
        Ability.Int => Int,
        Ability.Wis => Wis,
        Ability.Cha => Cha,
        _ => 10,
    };

    public AbilityScores With(Ability a, int value) => a switch
    {
        Ability.Str => this with { Str = value },
        Ability.Dex => this with { Dex = value },
        Ability.Con => this with { Con = value },
        Ability.Int => this with { Int = value },
        Ability.Wis => this with { Wis = value },
        Ability.Cha => this with { Cha = value },
        _ => this,
    };

    public AbilityScores Plus(Ability a, int delta) => With(a, Get(a) + delta);

    public static AbilityScores Zero => new()
    {
        Str = 0, Dex = 0, Con = 0, Int = 0, Wis = 0, Cha = 0,
    };

    public AbilityScores Plus(AbilityScores other) => new()
    {
        Str = Str + other.Str,
        Dex = Dex + other.Dex,
        Con = Con + other.Con,
        Int = Int + other.Int,
        Wis = Wis + other.Wis,
        Cha = Cha + other.Cha,
    };
}

public sealed record AbilityOverride
{
    public required Ability Ability { get; init; }
    public required int Delta { get; init; }
    public string? Reason { get; init; }
}
