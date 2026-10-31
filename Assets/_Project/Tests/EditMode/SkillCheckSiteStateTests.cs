using System;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using Game.Gameplay.SkillChecks;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class SkillCheckSiteStateTests
    {
        private const int Nat20Copies = 2;

        private readonly TestAssets _assets = new TestAssets();
        private SkillDef _animals;
        private TrinketDef _reward;
        private StatSheet _stats;
        private EffectLoadout _loadout;

        [SetUp]
        public void SetUp()
        {
            _animals = _assets.Skill("test.skill_animals", Ability.Wis);
            _reward = _assets.Trinket("test.clover",
                new StatModifierEffect(StatId.CritChance, ModifierOp.Flat, 0.1f, StackCurve.Linear));
            _stats = new StatSheet(StatIds.UnboundedLimits());
            _stats.SetBase(StatId.Wis, 2f);
            _loadout = new EffectLoadout(_stats, new ScalingTable(), new ScriptedRandomSource(new int[0]), 0.05f);
        }

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void Success_GrantsRewardOnce_AndLocks()
        {
            var state = new SkillCheckSiteState(_assets.Site("test.cat", _animals, 12, _reward));

            SkillCheckOutcome outcome = state.Attempt(_stats, Character(), Dice(10), _loadout, Nat20Copies);

            Assert.That(outcome.Result.Total, Is.EqualTo(12));
            Assert.That(outcome.Result.Success, Is.True);
            Assert.That(outcome.RewardCopies, Is.EqualTo(1));
            Assert.That(_loadout.GetStacks(_reward), Is.EqualTo(1));
            Assert.That(state.IsLocked, Is.True);
        }

        [Test]
        public void Nat20_GrantsExtraCopies()
        {
            var state = new SkillCheckSiteState(_assets.Site("test.cat", _animals, 30, _reward));

            SkillCheckOutcome outcome = state.Attempt(_stats, Character(), Dice(20), _loadout, Nat20Copies);

            Assert.That(outcome.RewardCopies, Is.EqualTo(Nat20Copies));
            Assert.That(_loadout.GetStacks(_reward), Is.EqualTo(Nat20Copies));
        }

        [Test]
        public void Failure_GrantsNothing_ButStillLocks()
        {
            var state = new SkillCheckSiteState(_assets.Site("test.cat", _animals, 15, _reward));

            SkillCheckOutcome outcome = state.Attempt(_stats, Character(), Dice(5), _loadout, Nat20Copies);

            Assert.That(outcome.Result.Success, Is.False);
            Assert.That(outcome.RewardCopies, Is.EqualTo(0));
            Assert.That(_loadout.GetStacks(_reward), Is.EqualTo(0));
            Assert.That(state.IsLocked, Is.True);
        }

        [Test]
        public void SecondAttempt_Throws()
        {
            var state = new SkillCheckSiteState(_assets.Site("test.cat", _animals, 12, _reward));
            state.Attempt(_stats, Character(), Dice(10), _loadout, Nat20Copies);

            Assert.Throws<InvalidOperationException>(() => state.Attempt(_stats, Character(), Dice(10), _loadout, Nat20Copies));
        }

        [Test]
        public void ProficientCharacter_GetsProficiencyBonus()
        {
            var state = new SkillCheckSiteState(_assets.Site("test.cat", _animals, 14, _reward));

            SkillCheckOutcome outcome = state.Attempt(_stats, Character(_animals), Dice(10), _loadout, Nat20Copies);

            Assert.That(outcome.Request.IsProficient, Is.True);
            Assert.That(outcome.Result.Total, Is.EqualTo(14));
            Assert.That(outcome.Result.Success, Is.True);
        }

        private CharacterDef Character(params SkillDef[] proficiencies)
        {
            return _assets.Character("test.hero", new[] { Ability.Dex, Ability.Wis }, new StatValue[0], proficiencies);
        }

        private static SkillCheck Dice(int roll)
        {
            return new SkillCheck(new SkillCheckRules(2, 0.05f), new ScriptedRandomSource(new[] { roll }));
        }
    }
}
