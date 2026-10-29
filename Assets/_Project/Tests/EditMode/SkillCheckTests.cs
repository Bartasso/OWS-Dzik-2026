using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class SkillCheckTests
    {
        private const int ProficiencyBonus = 2;
        private const float RerollChancePerLuck = 0.05f;

        [Test]
        public void Total_IsNaturalPlusModifierPlusProficiency()
        {
            SkillCheckResult result = Resolve(new[] { 10 }, Request(dc: 10, modifier: 3, proficient: true));

            Assert.That(result.Natural, Is.EqualTo(10));
            Assert.That(result.Total, Is.EqualTo(15));
        }

        [Test]
        public void WithoutProficiency_NoBonus()
        {
            SkillCheckResult result = Resolve(new[] { 10 }, Request(dc: 10, modifier: 3));

            Assert.That(result.Total, Is.EqualTo(13));
        }

        [Test]
        public void NegativeModifier_LowersTotal()
        {
            SkillCheckResult result = Resolve(new[] { 10 }, Request(dc: 10, modifier: -4));

            Assert.That(result.Total, Is.EqualTo(6));
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void TotalEqualToDc_Succeeds()
        {
            Assert.That(Resolve(new[] { 12 }, Request(dc: 15, modifier: 3)).Success, Is.True);
        }

        [Test]
        public void TotalBelowDc_Fails()
        {
            Assert.That(Resolve(new[] { 11 }, Request(dc: 15, modifier: 3)).Success, Is.False);
        }

        [Test]
        public void Nat20_AlwaysSucceeds()
        {
            SkillCheckResult result = Resolve(new[] { 20 }, Request(dc: 40, modifier: -10));

            Assert.That(result.Nat20, Is.True);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void Nat1_AlwaysFails()
        {
            SkillCheckResult result = Resolve(new[] { 1 }, Request(dc: 5, modifier: 30));

            Assert.That(result.Nat1, Is.True);
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void Advantage_TakesHigherDie()
        {
            Assert.That(Resolve(new[] { 5, 17 }, Request(advantage: true)).Natural, Is.EqualTo(17));
        }

        [Test]
        public void Disadvantage_TakesLowerDie()
        {
            Assert.That(Resolve(new[] { 5, 17 }, Request(disadvantage: true)).Natural, Is.EqualTo(5));
        }

        [Test]
        public void AdvantageAndDisadvantage_CancelOut_RollingOneDie()
        {
            // Only one die is scripted: rolling a second would throw.
            Assert.That(Resolve(new[] { 8 }, Request(advantage: true, disadvantage: true)).Natural, Is.EqualTo(8));
        }

        [Test]
        public void Nat1_WithZeroLuck_IsNotRerolled()
        {
            SkillCheckResult result = Resolve(new[] { 1 }, Request(luck: 0));

            Assert.That(result.Nat1, Is.True);
            Assert.That(result.WasRerolled, Is.False);
        }

        [Test]
        public void Nat1_WithNegativeLuck_IsNotRerolled()
        {
            Assert.That(Resolve(new[] { 1 }, Request(luck: -3)).WasRerolled, Is.False);
        }

        [Test]
        public void Nat1_WithFullRerollChance_IsRerolledWithoutDrawingChance()
        {
            // 20 LCK * 5% = 100%: no float scripted, so drawing one would throw.
            SkillCheckResult result = Resolve(new[] { 1, 14 }, Request(luck: 20));

            Assert.That(result.WasRerolled, Is.True);
            Assert.That(result.Natural, Is.EqualTo(14));
        }

        [Test]
        public void Nat1_RerolledWhenChanceRollIsBelowChance()
        {
            // 2 LCK = 10% chance.
            SkillCheckResult result = Resolve(new[] { 1, 9 }, Request(luck: 2), new[] { 0.05f });

            Assert.That(result.WasRerolled, Is.True);
            Assert.That(result.Natural, Is.EqualTo(9));
        }

        [Test]
        public void Nat1_NotRerolledWhenChanceRollIsAboveChance()
        {
            SkillCheckResult result = Resolve(new[] { 1 }, Request(luck: 2), new[] { 0.5f });

            Assert.That(result.WasRerolled, Is.False);
            Assert.That(result.Nat1, Is.True);
        }

        [Test]
        public void Reroll_IsFinal_EvenIfItIsAnotherNat1()
        {
            SkillCheckResult result = Resolve(new[] { 1, 1 }, Request(dc: 2, modifier: 10, luck: 20));

            Assert.That(result.WasRerolled, Is.True);
            Assert.That(result.Nat1, Is.True);
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void Reroll_IntoTwenty_CountsAsNat20()
        {
            SkillCheckResult result = Resolve(new[] { 1, 20 }, Request(dc: 40, luck: 20));

            Assert.That(result.Nat20, Is.True);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public void Advantage_BothDiceOne_RerollsWithSingleDie()
        {
            SkillCheckResult result = Resolve(new[] { 1, 1, 12 }, Request(luck: 20, advantage: true));

            Assert.That(result.WasRerolled, Is.True);
            Assert.That(result.Natural, Is.EqualTo(12));
        }

        [Test]
        public void Advantage_ChosenDieNotOne_NoReroll()
        {
            SkillCheckResult result = Resolve(new[] { 1, 9 }, Request(luck: 20, advantage: true));

            Assert.That(result.WasRerolled, Is.False);
            Assert.That(result.Natural, Is.EqualTo(9));
        }

        [Test]
        public void Disadvantage_ChosenDieOne_IsRerolled()
        {
            SkillCheckResult result = Resolve(new[] { 1, 15, 7 }, Request(luck: 20, disadvantage: true));

            Assert.That(result.WasRerolled, Is.True);
            Assert.That(result.Natural, Is.EqualTo(7));
        }

        private static SkillCheckRequest Request(
            int dc = 10,
            int modifier = 0,
            bool proficient = false,
            int luck = 0,
            bool advantage = false,
            bool disadvantage = false)
        {
            return new SkillCheckRequest(dc, modifier, proficient, luck, advantage, disadvantage);
        }

        private static SkillCheckResult Resolve(int[] dice, SkillCheckRequest request, float[] chanceRolls = null)
        {
            var source = new ScriptedRandomSource(dice, chanceRolls);
            var skillCheck = new SkillCheck(new SkillCheckRules(ProficiencyBonus, RerollChancePerLuck), source);

            SkillCheckResult result = skillCheck.Resolve(request);

            source.AssertAllConsumed();
            return result;
        }
    }
}
