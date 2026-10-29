using Game.Core;
using Game.Data;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class GameBalanceTests
    {
        private GameBalance _balance;

        [SetUp]
        public void SetUp()
        {
            _balance = ScriptableObject.CreateInstance<GameBalance>();
            _balance.name = "GameBalance";
            Set("_baseCritChance", 0.05f);
            Set("_critChancePerLuck", 0.01f);
            Set("_baseCritMultiplier", 2f);
            Set("_proficiencyBonus", 2);
            Set("_nat1RerollChancePerLuck", 0.05f);
            Set("_procChancePerLuck", 0.05f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_balance);
        }

        [Test]
        public void ValidValues_HaveNoErrors()
        {
            Assert.That(Validate().HasErrors, Is.False);
        }

        [TestCase("_baseCritChance", -0.1f)]
        [TestCase("_baseCritChance", 1.5f)]
        [TestCase("_critChancePerLuck", -0.01f)]
        [TestCase("_nat1RerollChancePerLuck", 1.1f)]
        [TestCase("_procChancePerLuck", -0.05f)]
        [TestCase("_baseCritMultiplier", 0.5f)]
        public void OutOfRangeFloat_IsError(string field, float value)
        {
            Set(field, value);

            Assert.That(Validate().HasErrors, Is.True);
        }

        [Test]
        public void NegativeProficiency_IsError()
        {
            Set("_proficiencyBonus", -1);

            Assert.That(Validate().HasErrors, Is.True);
        }

        [Test]
        public void SkillCheckRules_CarryAssetValues()
        {
            SkillCheckRules rules = _balance.ToSkillCheckRules();

            Assert.That(rules.ProficiencyBonus, Is.EqualTo(2));
            Assert.That(rules.Nat1RerollChancePerLuck, Is.EqualTo(0.05f));
        }

        [Test]
        public void StatLimits_BuildClampsOnlyListedStats()
        {
            SetLimits(new StatLimit(StatId.CooldownReduction, 0f, 0.8f));

            StatRange[] limits = _balance.BuildStatLimits();

            Assert.That(limits.Length, Is.EqualTo(StatIds.Count));
            Assert.That(limits[(int)StatId.CooldownReduction].Max, Is.EqualTo(0.8f));
            Assert.That(limits[(int)StatId.MaxHp].Max, Is.EqualTo(float.PositiveInfinity));
            Assert.That(Validate().HasErrors, Is.False);
        }

        [Test]
        public void StatLimit_MinAboveMax_IsError()
        {
            SetLimits(new StatLimit(StatId.MoveSpeed, 2f, 1f));

            Assert.That(Validate().HasErrors, Is.True);
        }

        [Test]
        public void StatLimit_Duplicate_IsError()
        {
            SetLimits(new StatLimit(StatId.MoveSpeed, 0f, 5f), new StatLimit(StatId.MoveSpeed, 0f, 3f));

            Assert.That(Validate().HasErrors, Is.True);
        }

        private void SetLimits(params StatLimit[] limits)
        {
            Set("_statLimits", new System.Collections.Generic.List<StatLimit>(limits));
        }

        private ValidationReport Validate()
        {
            var report = new ValidationReport();
            _balance.Validate(report);
            return report;
        }

        private void Set(string field, object value)
        {
            PrivateField.Set(_balance, field, value);
        }
    }
}
