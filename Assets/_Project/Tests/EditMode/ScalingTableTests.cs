using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ScalingTableTests
    {
        private static readonly ModifierSourceId SourceA = new ModifierSourceId(1);
        private static readonly ModifierSourceId SourceB = new ModifierSourceId(2);

        [Test]
        public void WithoutOverride_UsesWeaponAbility()
        {
            var table = new ScalingTable();

            Assert.That(table.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Int));
        }

        [Test]
        public void Override_ChangesOnlyItsCategory()
        {
            var table = new ScalingTable();
            table.SetOverride(WeaponCategory.Arcane, Ability.Str, SourceA);

            Assert.That(table.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Str));
            Assert.That(table.Resolve(WeaponCategory.Dexterity, Ability.Dex), Is.EqualTo(Ability.Dex));
        }

        [Test]
        public void LatestOverride_Wins_AndRemovingItRestoresPrevious()
        {
            var table = new ScalingTable();
            table.SetOverride(WeaponCategory.Arcane, Ability.Str, SourceA);
            table.SetOverride(WeaponCategory.Arcane, Ability.Cha, SourceB);

            Assert.That(table.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Cha));

            Assert.That(table.RemoveSource(SourceB), Is.EqualTo(1));
            Assert.That(table.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Str));

            table.RemoveSource(SourceA);
            Assert.That(table.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Int));
        }
    }
}
