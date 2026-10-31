using Game.Core;
using Game.Data;
using Game.Gameplay.Combat;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class DamageCalculatorTests
    {
        private const float Tolerance = 1e-4f;

        private readonly TestAssets _assets = new TestAssets();
        private GameBalance _balance;
        private WeaponDef _staff;
        private StatSheet _stats;
        private ScalingTable _scaling;

        [SetUp]
        public void SetUp()
        {
            _balance = _assets.Balance();
            _staff = _assets.Weapon("test.staff", WeaponCategory.Arcane, Ability.Int, 10f);
            _stats = new StatSheet(StatIds.UnboundedLimits());
            _scaling = new ScalingTable();
        }

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void BaseDamage_ScalesFivePercentPerPointOfWeaponAbility()
        {
            _stats.SetBase(StatId.Int, 2f);

            Assert.That(DamageCalculator.BaseDamage(_staff, _stats, _scaling, _balance), Is.EqualTo(11f).Within(Tolerance));
        }

        [Test]
        public void BaseDamage_UsesScalingOverride()
        {
            _stats.SetBase(StatId.Int, 2f);
            _stats.SetBase(StatId.Str, 6f);
            _scaling.SetOverride(WeaponCategory.Arcane, Ability.Str, new ModifierSourceId(1));

            Assert.That(DamageCalculator.BaseDamage(_staff, _stats, _scaling, _balance), Is.EqualTo(13f).Within(Tolerance));
        }

        [Test]
        public void BaseDamage_NeverNegative()
        {
            _stats.SetBase(StatId.Int, -30f);

            Assert.That(DamageCalculator.BaseDamage(_staff, _stats, _scaling, _balance), Is.EqualTo(0f));
        }

        [Test]
        public void Roll_BelowCritChance_Crits()
        {
            // 5% base + LCK 0; roll 0.01 → crit, ×2.
            HitResult hit = DamageCalculator.Roll(_staff, _stats, _scaling, _balance, new ScriptedRandomSource(new int[0], new[] { 0.01f }));

            Assert.That(hit.IsCrit, Is.True);
            Assert.That(hit.Damage, Is.EqualTo(20f).Within(Tolerance));
        }

        [Test]
        public void Roll_AboveCritChance_DoesNotCrit()
        {
            HitResult hit = DamageCalculator.Roll(_staff, _stats, _scaling, _balance, new ScriptedRandomSource(new int[0], new[] { 0.5f }));

            Assert.That(hit.IsCrit, Is.False);
            Assert.That(hit.Damage, Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void Roll_CritBonusesFromStats_Apply()
        {
            var source = new ModifierSourceId(1);
            _stats.AddModifier(new StatModifier(StatId.CritChance, ModifierOp.Flat, 1f, source));
            _stats.AddModifier(new StatModifier(StatId.CritMultiplier, ModifierOp.Flat, 0.5f, source));

            // Certain crit: no float scripted, drawing one would throw.
            HitResult hit = DamageCalculator.Roll(_staff, _stats, _scaling, _balance, new ScriptedRandomSource(new int[0]));

            Assert.That(hit.IsCrit, Is.True);
            Assert.That(hit.Damage, Is.EqualTo(25f).Within(Tolerance));
        }
    }
}
