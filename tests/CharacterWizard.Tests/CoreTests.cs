using CharacterWizard.Core.Models;
using CharacterWizard.Core.Persistence;
using CharacterWizard.Core.Rules;

namespace CharacterWizard.Tests;

public class AbilityScoreCalculatorTests
{
    [Theory]
    [InlineData(1, -5)] [InlineData(8, -1)] [InlineData(10, 0)]
    [InlineData(11, 0)] [InlineData(12, 1)] [InlineData(20, 5)]
    public void Modifier_matches_phb(int score, int expected) =>
        Assert.Equal(expected, AbilityScoreCalculator.Modifier(score));

    [Fact]
    public void Final_score_applies_asi_and_overrides()
    {
        var c = new Character
        {
            BaseAbilityScores = new AbilityScores { Int = 16 },
            ChoicesByLevel = new()
            {
                [4]  = new LevelChoices { AsiOrFeat = "asi", AsiAllocation = new() { [Ability.Int] = 2 } },
                [8]  = new LevelChoices { AsiOrFeat = "asi", AsiAllocation = new() { [Ability.Int] = 2 } },
            },
            AbilityOverrides =
            {
                new AbilityOverride { Ability = Ability.Cha, Delta = 2, Reason = "buff" },
            },
        };

        var final = AbilityScoreCalculator.ComputeFinal(c);
        Assert.Equal(20, final.Int);  // 16 + 2 + 2
        Assert.Equal(12, final.Cha);  // default 10 + override 2
    }
}

public class ProficiencyBonusTests
{
    [Theory]
    [InlineData(1, 2)] [InlineData(4, 2)] [InlineData(5, 3)] [InlineData(8, 3)]
    [InlineData(9, 4)] [InlineData(12, 4)] [InlineData(13, 5)] [InlineData(16, 5)]
    [InlineData(17, 6)] [InlineData(20, 6)]
    public void Proficiency_bonus_matches_phb(int level, int expected) =>
        Assert.Equal(expected, ProficiencyBonus.ForLevel(level));
}

public class PointBuyTests
{
    [Theory]
    [InlineData(8, 0)] [InlineData(9, 1)] [InlineData(10, 2)] [InlineData(11, 3)]
    [InlineData(12, 4)] [InlineData(13, 5)] [InlineData(14, 7)] [InlineData(15, 9)]
    public void Cost_matches_phb(int score, int expected) =>
        Assert.Equal(expected, PointBuy.Cost(score));

    [Theory]
    [InlineData(7)] [InlineData(16)] [InlineData(0)]
    public void Cost_out_of_range_returns_null(int score) =>
        Assert.Null(PointBuy.Cost(score));

    [Fact]
    public void Total_for_15_15_15_8_8_8_is_27()
    {
        var scores = new AbilityScores { Str = 15, Dex = 15, Con = 15, Int = 8, Wis = 8, Cha = 8 };
        Assert.Equal(27, PointBuy.TotalCost(scores));
    }

    [Fact]
    public void Total_for_standard_array_is_27()
    {
        var scores = new AbilityScores { Str = 15, Dex = 14, Con = 13, Int = 12, Wis = 10, Cha = 8 };
        Assert.Equal(27, PointBuy.TotalCost(scores));
    }
}

public class AbilityRollerTests
{
    [Fact]
    public void Roll4d6DropLowest_in_range_3_to_18()
    {
        var rng = new Random(42);
        for (int i = 0; i < 1000; i++)
        {
            var v = AbilityRoller.Roll4d6DropLowest(rng);
            Assert.InRange(v, 3, 18);
        }
    }

    [Fact]
    public void RollSix_returns_six_values()
    {
        var values = AbilityRoller.RollSix(new Random(0));
        Assert.Equal(6, values.Length);
        Assert.All(values, v => Assert.InRange(v, 3, 18));
    }

    [Fact]
    public void Roll_is_deterministic_with_same_seed()
    {
        var a = AbilityRoller.RollSix(new Random(123));
        var b = AbilityRoller.RollSix(new Random(123));
        Assert.Equal(a, b);
    }
}

public class ProficiencyResolverTests
{
    [Fact]
    public void Wizard_proficient_in_int_and_wis()
    {
        var c = new Character
        {
            BaseAbilityScores = new() { Str = 10, Dex = 12, Con = 14, Int = 16, Wis = 12, Cha = 10 },
            Classes = { new() { ClassRef = new("Wizard", "PHB"), Levels = 5 } },
        };
        ProficiencyResolver.SaveProficienciesLookup lookup =
            (EntityRef r) => new[] { "int", "wis" };

        var saves = ProficiencyResolver.ComputeSaves(c, lookup);

        Assert.True(saves.Int.Proficient);
        Assert.True(saves.Wis.Proficient);
        Assert.False(saves.Str.Proficient);
        // Lvl 5 → PB +3, INT mod +3 → save +6
        Assert.Equal(6, saves.Int.Modifier);
        Assert.Equal(0, saves.Str.Modifier);  // STR mod 0, no PB
    }

    [Fact]
    public void Multiclass_uses_first_class_saves()
    {
        // Fighter is the first class — gets STR + CON saves regardless of secondary Wizard.
        var c = new Character
        {
            BaseAbilityScores = new() { Str = 14, Con = 14, Int = 16 },
            Classes =
            {
                new() { ClassRef = new("Fighter", "PHB"), Levels = 3 },
                new() { ClassRef = new("Wizard",  "PHB"), Levels = 2 },
            },
        };
        ProficiencyResolver.SaveProficienciesLookup lookup =
            (EntityRef r) => r.Name == "Fighter"
                ? new[] { "str", "con" }
                : new[] { "int", "wis" };

        var saves = ProficiencyResolver.ComputeSaves(c, lookup);

        Assert.True(saves.Str.Proficient);
        Assert.True(saves.Con.Proficient);
        Assert.False(saves.Int.Proficient);  // Wizard saves don't apply
    }
}

public class HitPointCalculatorTests
{
    private static int? D10(EntityRef r) => r.Name == "Fighter" ? 10 : (int?)null;
    private static int? D6(EntityRef r) => r.Name == "Wizard" ? 6 : (int?)null;
    private static int? Both(EntityRef r) => r.Name switch { "Fighter" => 10, "Wizard" => 6, _ => null };

    [Fact]
    public void Level1_d10_con2_returns_max_plus_mod()
    {
        var c = new Character
        {
            BaseAbilityScores = new() { Con = 14 },
            Classes = { new CharacterClassEntry { ClassRef = new("Fighter", "PHB"), Levels = 1 } },
        };
        Assert.Equal(12, HitPointCalculator.ComputeMaxHp(c, D10));
    }

    [Fact]
    public void Level5_with_rolls_sums_correctly()
    {
        // d10 fighter, con+2. L1 = 12. Rolls [6,5,8,7] (already include con mod) for L2-L5.
        var c = new Character
        {
            BaseAbilityScores = new() { Con = 14 },
            Classes = { new CharacterClassEntry
            {
                ClassRef = new("Fighter", "PHB"), Levels = 5,
                HitPointRolls = { 6, 5, 8, 7 },
            } },
        };
        Assert.Equal(12 + 6 + 5 + 8 + 7, HitPointCalculator.ComputeMaxHp(c, D10));
    }

    [Fact]
    public void Level_without_rolls_uses_average_plus_mod()
    {
        // d10 fighter L5 with no rolls: L1=10+2=12, L2-L5 each = (10/2+1)+2 = 8
        var c = new Character
        {
            BaseAbilityScores = new() { Con = 14 },
            Classes = { new CharacterClassEntry { ClassRef = new("Fighter", "PHB"), Levels = 5 } },
        };
        Assert.Equal(12 + 4 * 8, HitPointCalculator.ComputeMaxHp(c, D10));
    }

    [Fact]
    public void Multiclass_sums_both_classes()
    {
        // Fighter 3 + Wizard 2, con +2
        // Fighter L1=12, L2-L3 avg = 8 each → 12+16 = 28
        // Wizard L1=8 (6+2), L2 avg = (6/2+1)+2 = 6 → 8+6 = 14
        // Total: 42
        var c = new Character
        {
            BaseAbilityScores = new() { Con = 14 },
            Classes =
            {
                new() { ClassRef = new("Fighter", "PHB"), Levels = 3 },
                new() { ClassRef = new("Wizard",  "PHB"), Levels = 2 },
            },
        };
        Assert.Equal(42, HitPointCalculator.ComputeMaxHp(c, Both));
    }

    [Fact]
    public void Unknown_class_contributes_zero()
    {
        var c = new Character
        {
            BaseAbilityScores = new() { Con = 14 },
            Classes = { new() { ClassRef = new("Homebrew", "X"), Levels = 5 } },
        };
        Assert.Equal(0, HitPointCalculator.ComputeMaxHp(c, _ => null));
    }
}

public class LevelUpRulesTests
{
    [Theory]
    [InlineData(1, false)] [InlineData(4, true)] [InlineData(8, true)] [InlineData(12, true)]
    [InlineData(16, true)] [InlineData(19, true)] [InlineData(20, false)]
    public void IsAsiLevel_matches_phb(int level, bool expected) =>
        Assert.Equal(expected, LevelUpRules.IsAsiLevel(level));

    [Theory]
    [InlineData(6, 4)] [InlineData(8, 5)] [InlineData(10, 6)] [InlineData(12, 7)]
    public void AverageHpPerLevel_rounds_up(int faces, int expected) =>
        Assert.Equal(expected, LevelUpRules.AverageHpPerLevel(faces));

    [Fact]
    public void HpGainedAtLevel1_with_d6_and_con2_is_max_plus_mod()
    {
        // First level always max HP from class hit dice + con mod.
        Assert.Equal(8, LevelUpRules.HpGainedAtLevel(1, 6, 2, HpMode.Average));
    }

    [Fact]
    public void HpGainedAtLevel_average_d10_con1()
    {
        // (10/2)+1 + 1 = 7
        Assert.Equal(7, LevelUpRules.HpGainedAtLevel(2, 10, 1, HpMode.Average));
    }

    [Fact]
    public void HpGainedAtLevel_maximum_uses_full_die()
    {
        Assert.Equal(10, LevelUpRules.HpGainedAtLevel(5, 8, 2, HpMode.Maximum));
    }

    [Fact]
    public void HpGainedAtLevel_manual_uses_provided_roll()
    {
        Assert.Equal(7, LevelUpRules.HpGainedAtLevel(3, 6, 2, HpMode.Manual, manualRoll: 5));
    }
}

public class SpellSlotTableTests
{
    [Fact]
    public void Wizard_level_5_has_correct_slots()
    {
        Assert.Equal(4, SpellSlotTable.GetSlots(5, 1));
        Assert.Equal(3, SpellSlotTable.GetSlots(5, 2));
        Assert.Equal(2, SpellSlotTable.GetSlots(5, 3));
        Assert.Equal(0, SpellSlotTable.GetSlots(5, 4));
    }

    [Fact]
    public void Level_20_has_ninth_slot()
    {
        Assert.Equal(1, SpellSlotTable.GetSlots(20, 9));
    }
}

public class CharacterStoreTests
{
    private static string MakeTempRoot() =>
        Path.Combine(Path.GetTempPath(), "cw-test-" + Guid.NewGuid());

    [Fact]
    public void Preserves_multiple_inventory_items_with_bonuses()
    {
        var tmp = MakeTempRoot();
        try
        {
            var store = new CharacterStore(tmp);
            var c = new Character
            {
                Name = "Inv Tester",
                Inventory =
                {
                    new InventoryItem { ItemRef = new EntityRef("Longsword", "PHB"),
                        CustomName = "Espada Élfica +2", BonusAttack = 2, BonusDamage = 2,
                        Equipped = true, Quantity = 1 },
                    new InventoryItem { ItemRef = new EntityRef("Healing Potion", "PHB"),
                        Quantity = 5 },
                    new InventoryItem { ItemRef = new EntityRef("Plate Armor", "PHB"),
                        BonusAc = 1, Equipped = true, Notes = "Heirloom" },
                },
            };

            store.Save(c);
            var loaded = store.Load(c.Id)!;

            Assert.Equal(3, loaded.Inventory.Count);
            var sword = loaded.Inventory[0];
            Assert.Equal("Espada Élfica +2", sword.CustomName);
            Assert.Equal(2, sword.BonusAttack);
            Assert.Equal(2, sword.BonusDamage);
            Assert.True(sword.Equipped);

            var potion = loaded.Inventory[1];
            Assert.Equal(5, potion.Quantity);

            var armor = loaded.Inventory[2];
            Assert.Equal(1, armor.BonusAc);
            Assert.Equal("Heirloom", armor.Notes);
        }
        finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
    }

    [Fact]
    public void Preserves_ability_overrides_list()
    {
        var tmp = MakeTempRoot();
        try
        {
            var store = new CharacterStore(tmp);
            var c = new Character
            {
                Name = "Buff Tester",
                AbilityOverrides =
                {
                    new AbilityOverride { Ability = Ability.Cha, Delta = 2, Reason = "Tomo" },
                    new AbilityOverride { Ability = Ability.Str, Delta = -1, Reason = "Curse" },
                },
            };

            store.Save(c);
            var loaded = store.Load(c.Id)!;

            Assert.Equal(2, loaded.AbilityOverrides.Count);
            Assert.Equal(Ability.Cha, loaded.AbilityOverrides[0].Ability);
            Assert.Equal(2, loaded.AbilityOverrides[0].Delta);
            Assert.Equal("Tomo", loaded.AbilityOverrides[0].Reason);
            Assert.Equal(-1, loaded.AbilityOverrides[1].Delta);
        }
        finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
    }

    [Fact]
    public void Save_creates_history_entry_on_overwrite()
    {
        var tmp = MakeTempRoot();
        try
        {
            var store = new CharacterStore(tmp);
            var c = new Character { Name = "v1" };
            store.Save(c);

            c.Name = "v2";
            store.Save(c);

            var historyFolder = Path.Combine(store.HistoryFolder, c.Id.ToString());
            Assert.True(Directory.Exists(historyFolder), "history folder should exist after second save");
            var snapshots = Directory.GetFiles(historyFolder, "*.json");
            Assert.Single(snapshots);
        }
        finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
    }

    [Fact]
    public void History_keeps_at_most_five_versions()
    {
        var tmp = MakeTempRoot();
        try
        {
            var store = new CharacterStore(tmp);
            var c = new Character { Name = "v" };
            store.Save(c);

            // 7 overwrites → 7 backups attempted but capped at 5
            for (int i = 0; i < 7; i++)
            {
                c.Name = $"v{i + 2}";
                store.Save(c);
                Thread.Sleep(1100); // ensure distinct timestamp at second granularity
            }

            var snapshots = Directory.GetFiles(Path.Combine(store.HistoryFolder, c.Id.ToString()), "*.json");
            Assert.True(snapshots.Length <= 5, $"expected <= 5 snapshots, got {snapshots.Length}");
        }
        finally { if (Directory.Exists(tmp)) Directory.Delete(tmp, true); }
    }

    [Fact]
    public void Save_load_roundtrip_preserves_character()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "cw-test-" + Guid.NewGuid());
        try
        {
            var store = new CharacterStore(tmp);
            var c = new Character
            {
                Name = "Elaria",
                RaceRef = new EntityRef("Elf", "PHB"),
                BackgroundRef = new EntityRef("Sage", "PHB"),
                BaseAbilityScores = new AbilityScores { Str = 8, Dex = 14, Con = 14, Int = 16, Wis = 12, Cha = 10 },
                Classes =
                {
                    new CharacterClassEntry
                    {
                        ClassRef = new EntityRef("Wizard", "PHB"),
                        Levels = 5,
                        SubclassRef = new EntityRef("School of Evocation", "PHB"),
                    },
                },
                Inventory =
                {
                    new InventoryItem
                    {
                        ItemRef = new EntityRef("Longsword", "PHB"),
                        CustomName = "Espada Élfica +2",
                        BonusAttack = 2, BonusDamage = 2,
                    },
                },
            };

            store.Save(c);
            var loaded = store.Load(c.Id);

            Assert.NotNull(loaded);
            Assert.Equal("Elaria", loaded!.Name);
            Assert.Equal(5, loaded.TotalLevel);
            Assert.Equal("Wizard", loaded.Classes[0].ClassRef.Name);
            Assert.Equal("Espada Élfica +2", loaded.Inventory[0].CustomName);
            Assert.Equal(2, loaded.Inventory[0].BonusAttack);
        }
        finally
        {
            if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
        }
    }
}
