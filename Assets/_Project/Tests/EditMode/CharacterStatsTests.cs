using Game.Core;
using Game.Data;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class CharacterStatsTests
    {
        private readonly TestAssets _assets = new TestAssets();

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void Build_AppliesBaseStatsAndPrimaryAbilityBonus()
        {
            GameBalance balance = _assets.Balance();
            CharacterDef albert = _assets.Character("test.albert",
                new[] { Ability.Int, Ability.Wis },
                new[] { new StatValue(StatId.MaxHp, 100f), new StatValue(StatId.MoveSpeed, 6f), new StatValue(StatId.Int, 1f) });

            StatSheet stats = CharacterStats.Build(albert, balance);

            Assert.That(stats.Get(StatId.MaxHp), Is.EqualTo(100f));
            Assert.That(stats.Get(StatId.MoveSpeed), Is.EqualTo(6f));
            Assert.That(stats.GetAbility(Ability.Int), Is.EqualTo(3));
            Assert.That(stats.GetAbility(Ability.Wis), Is.EqualTo(2));
            Assert.That(stats.GetAbility(Ability.Str), Is.EqualTo(0));
        }

        [Test]
        public void Build_UsesBalanceStatLimits()
        {
            GameBalance balance = _assets.Balance();
            PrivateField.Set(balance, "_statLimits", new System.Collections.Generic.List<StatLimit> { new StatLimit(StatId.MoveSpeed, 0f, 4f) });
            CharacterDef character = _assets.Character("test.fast", new[] { Ability.Dex, Ability.Wis },
                new[] { new StatValue(StatId.MoveSpeed, 6f) });

            Assert.That(CharacterStats.Build(character, balance).Get(StatId.MoveSpeed), Is.EqualTo(4f));
        }

        [Test]
        public void CharacterValidation_RequiresTwoDifferentPrimaries()
        {
            CharacterDef character = _assets.Character("test.broken", new[] { Ability.Int, Ability.Int }, new StatValue[0]);
            PrivateField.Set(character, "_startingWeapon", _assets.Weapon("test.staff", WeaponCategory.Arcane, Ability.Int, 10f));
            var report = new ValidationReport();

            character.Validate(report);

            Assert.That(report.HasErrors, Is.True);
        }
    }
}
