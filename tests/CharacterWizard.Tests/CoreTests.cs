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
