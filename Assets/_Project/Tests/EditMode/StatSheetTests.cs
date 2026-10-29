using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class StatSheetTests
    {
        private const float Tolerance = 1e-5f;

        private StatSheet _sheet;
        private ModifierSourceId _source;

        [SetUp]
        public void SetUp()
        {
            _sheet = new StatSheet(StatIds.UnboundedLimits());
            _source = _sheet.CreateSource();
        }

        [Test]
        public void StatIdCount_MatchesEnum()
        {
            Assert.That(StatIds.Count, Is.EqualTo(Enum.GetValues(typeof(StatId)).Length));
        }

        [Test]
        public void AbilityStats_MatchAbilityEnum()
        {
            foreach (Ability ability in Enum.GetValues(typeof(Ability)))
            {
                StatId stat = StatIds.FromAbility(ability);
                Assert.That(stat.ToString(), Is.EqualTo(ability.ToString()));
                Assert.That(StatIds.IsAbility(stat), Is.True);
            }

            Assert.That(StatIds.IsAbility(StatId.MaxHp), Is.False);
        }

        [Test]
        public void WithoutModifiers_ReturnsBase()
        {
            _sheet.SetBase(StatId.MaxHp, 100f);

            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(100f));
        }

        [Test]
        public void FlatModifiers_Sum()
        {
            _sheet.SetBase(StatId.MaxHp, 100f);
            Add(StatId.MaxHp, ModifierOp.Flat, 20f);
            Add(StatId.MaxHp, ModifierOp.Flat, 5f);

            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(125f).Within(Tolerance));
        }

        [Test]
        public void PercentAdd_SumsBeforeMultiplying()
        {
            _sheet.SetBase(StatId.AttackSpeed, 1f);
            Add(StatId.AttackSpeed, ModifierOp.PercentAdd, 0.25f);
            Add(StatId.AttackSpeed, ModifierOp.PercentAdd, 0.25f);

            Assert.That(_sheet.Get(StatId.AttackSpeed), Is.EqualTo(1.5f).Within(Tolerance));
        }

        [Test]
        public void PercentMult_Compounds()
        {
            _sheet.SetBase(StatId.AttackSpeed, 1f);
            Add(StatId.AttackSpeed, ModifierOp.PercentMult, 0.5f);
            Add(StatId.AttackSpeed, ModifierOp.PercentMult, 0.5f);

            Assert.That(_sheet.Get(StatId.AttackSpeed), Is.EqualTo(2.25f).Within(Tolerance));
        }

        [Test]
        public void AllBuckets_FollowFormula()
        {
            // (100 + 20) × (1 + 0.5 − 0.1) × (1 + 0.2) = 120 × 1.4 × 1.2 = 201.6
            _sheet.SetBase(StatId.MaxHp, 100f);
            Add(StatId.MaxHp, ModifierOp.Flat, 20f);
            Add(StatId.MaxHp, ModifierOp.PercentAdd, 0.5f);
            Add(StatId.MaxHp, ModifierOp.PercentAdd, -0.1f);
            Add(StatId.MaxHp, ModifierOp.PercentMult, 0.2f);

            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(201.6f).Within(1e-3f));
        }

        [Test]
        public void ModifiersOnOtherStats_AreIgnored()
        {
            _sheet.SetBase(StatId.MaxHp, 100f);
            Add(StatId.MoveSpeed, ModifierOp.Flat, 3f);

            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(100f));
        }

        [Test]
        public void FinalValue_IsClampedToLimits()
        {
            StatRange[] limits = StatIds.UnboundedLimits();
            limits[(int)StatId.CooldownReduction] = new StatRange(0f, 0.8f);
            var sheet = new StatSheet(limits);
            ModifierSourceId source = sheet.CreateSource();

            sheet.AddModifier(new StatModifier(StatId.CooldownReduction, ModifierOp.Flat, 1.5f, source));
            Assert.That(sheet.Get(StatId.CooldownReduction), Is.EqualTo(0.8f));

            sheet.AddModifier(new StatModifier(StatId.CooldownReduction, ModifierOp.Flat, -5f, source));
            Assert.That(sheet.Get(StatId.CooldownReduction), Is.EqualTo(0f));
        }

        [Test]
        public void RemoveSource_RemovesOnlyThatSource()
        {
            ModifierSourceId other = _sheet.CreateSource();
            _sheet.SetBase(StatId.MaxHp, 100f);
            Add(StatId.MaxHp, ModifierOp.Flat, 10f);
            Add(StatId.MaxHp, ModifierOp.PercentAdd, 0.5f);
            _sheet.AddModifier(new StatModifier(StatId.MaxHp, ModifierOp.Flat, 1f, other));
            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(166.5f).Within(Tolerance));

            int removed = _sheet.RemoveSource(_source);

            Assert.That(removed, Is.EqualTo(2));
            Assert.That(_sheet.Get(StatId.MaxHp), Is.EqualTo(101f).Within(Tolerance));
        }

        [Test]
        public void CreateSource_GivesDistinctValidIds()
        {
            ModifierSourceId a = _sheet.CreateSource();
            ModifierSourceId b = _sheet.CreateSource();

            Assert.That(a.IsValid && b.IsValid, Is.True);
            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(default(ModifierSourceId).IsValid, Is.False);
        }

        [Test]
        public void Abilities_AreIntegers()
        {
            _sheet.SetBase(StatId.Str, 2f);
            Add(StatId.Str, ModifierOp.Flat, 3f);
            Add(StatId.Str, ModifierOp.Flat, -1f);

            Assert.That(_sheet.GetAbility(Ability.Str), Is.EqualTo(4));
        }

        [TestCase(ModifierOp.PercentAdd)]
        [TestCase(ModifierOp.PercentMult)]
        public void IntegerStat_RejectsPercentModifiers(ModifierOp op)
        {
            Assert.Throws<ArgumentException>(() => Add(StatId.Lck, op, 1f));
            Assert.Throws<ArgumentException>(() => Add(StatId.SkillCheckBonus, op, 1f));
        }

        [Test]
        public void IntegerStat_RejectsFractionalValues()
        {
            Assert.Throws<ArgumentException>(() => Add(StatId.Dex, ModifierOp.Flat, 0.5f));
            Assert.Throws<ArgumentException>(() => _sheet.SetBase(StatId.Dex, 1.5f));
        }

        [Test]
        public void GetInt_OnNonIntegerStat_Throws()
        {
            Assert.Throws<ArgumentException>(() => _sheet.GetInt(StatId.MaxHp));
        }

        [Test]
        public void ModifierWithoutSource_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                _sheet.AddModifier(new StatModifier(StatId.MaxHp, ModifierOp.Flat, 1f, default)));
        }

        [Test]
        public void WrongLimitCount_Throws()
        {
            Assert.Throws<ArgumentException>(() => new StatSheet(new StatRange[3]));
        }

        [Test]
        public void InvalidRange_Throws()
        {
            Assert.Throws<ArgumentException>(() => new StatRange(1f, 0f));
        }

        private void Add(StatId stat, ModifierOp op, float value)
        {
            _sheet.AddModifier(new StatModifier(stat, op, value, _source));
        }
    }
}
