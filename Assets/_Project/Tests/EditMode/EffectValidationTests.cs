using Game.Core;
using Game.Data;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class EffectValidationTests
    {
        private readonly TestAssets _assets = new TestAssets();

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void ValidTrinket_HasNoErrors()
        {
            TrinketDef trinket = _assets.Trinket("trinket.energy_drink",
                new StatModifierEffect(StatId.AttackSpeed, ModifierOp.PercentAdd, 0.25f, StackCurve.Linear),
                new StatModifierEffect(StatId.Lck, ModifierOp.Flat, 1f, StackCurve.Linear),
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.Flat, 1f),
                new ScalingOverrideEffect(WeaponCategory.Arcane, Ability.Str));

            Assert.That(Validate(trinket).Errors, Is.Empty);
        }

        [Test]
        public void NoEffects_IsError()
        {
            Assert.That(Validate(_assets.Trinket("trinket.empty")).HasErrors, Is.True);
        }

        [Test]
        public void MissingEffect_IsError()
        {
            Assert.That(Validate(_assets.Trinket("trinket.broken", (Effect)null)).HasErrors, Is.True);
        }

        [Test]
        public void SharedEffectInstance_IsError()
        {
            var effect = new StatModifierEffect(StatId.MaxHp, ModifierOp.Flat, 10f, StackCurve.Linear);

            Assert.That(Validate(_assets.Trinket("trinket.twice", effect, effect)).HasErrors, Is.True);
        }

        [Test]
        public void MissingDisplayName_IsError()
        {
            TrinketDef trinket = _assets.Trinket("trinket.nameless",
                new StatModifierEffect(StatId.MaxHp, ModifierOp.Flat, 10f, StackCurve.Linear));
            PrivateField.Set(trinket, "_displayName", "");

            Assert.That(Validate(trinket).HasErrors, Is.True);
        }

        [TestCase(StatId.Str, ModifierOp.PercentAdd, 1f, StackCurve.Linear)]
        [TestCase(StatId.Lck, ModifierOp.Flat, 0.5f, StackCurve.Linear)]
        [TestCase(StatId.Lck, ModifierOp.Flat, 1f, StackCurve.Hyperbolic)]
        [TestCase(StatId.SkillCheckBonus, ModifierOp.PercentMult, 1f, StackCurve.Linear)]
        [TestCase(StatId.MaxHp, ModifierOp.Flat, 0f, StackCurve.Linear)]
        [TestCase(StatId.CritChance, ModifierOp.Flat, -0.5f, StackCurve.Hyperbolic)]
        [TestCase((StatId)99, ModifierOp.Flat, 1f, StackCurve.Linear)]
        [TestCase(StatId.MaxHp, (ModifierOp)99, 1f, StackCurve.Linear)]
        [TestCase(StatId.MaxHp, ModifierOp.Flat, 1f, (StackCurve)99)]
        public void InvalidStatModifier_IsError(StatId stat, ModifierOp op, float value, StackCurve curve)
        {
            TrinketDef trinket = _assets.Trinket("trinket.invalid", new StatModifierEffect(stat, op, value, curve));

            Assert.That(Validate(trinket).HasErrors, Is.True);
        }

        [Test]
        public void TriggerEffect_WithPercentOnAbility_IsError()
        {
            TalentDef talent = _assets.Talent("talent.invalid",
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.PercentAdd, 0.1f));

            Assert.That(Validate(talent).HasErrors, Is.True);
        }

        [Test]
        public void TriggerEffect_WithUnknownTrigger_IsError()
        {
            TalentDef talent = _assets.Talent("talent.invalid",
                new StatOnTriggerEffect((GameTrigger)99, StatId.Lck, ModifierOp.Flat, 1f));

            Assert.That(Validate(talent).HasErrors, Is.True);
        }

        private static ValidationReport Validate(IValidatable validatable)
        {
            var report = new ValidationReport();
            validatable.Validate(report);
            return report;
        }
    }
}
