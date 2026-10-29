using System;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class EffectLoadoutTests
    {
        private const float Tolerance = 1e-5f;
        private const float ProcChancePerLuck = 0.05f;

        private readonly TestAssets _assets = new TestAssets();
        private StatSheet _stats;
        private ScalingTable _scaling;

        [SetUp]
        public void SetUp()
        {
            _stats = new StatSheet(StatIds.UnboundedLimits());
            _stats.SetBase(StatId.MaxHp, 100f);
            _stats.SetBase(StatId.AttackSpeed, 1f);
            _scaling = new ScalingTable();
        }

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void TradeOffTrinket_AppliesBothSides()
        {
            // "Energetyk z Nocnego": +25% attack speed, −10% max HP.
            TrinketDef energyDrink = EnergyDrink();
            EffectLoadout loadout = Loadout();

            loadout.Add(energyDrink);

            Assert.That(_stats.Get(StatId.AttackSpeed), Is.EqualTo(1.25f).Within(Tolerance));
            Assert.That(_stats.Get(StatId.MaxHp), Is.EqualTo(90f).Within(Tolerance));
        }

        [Test]
        public void SecondCopy_RaisesStacks_AndReappliesModifiers()
        {
            TrinketDef energyDrink = EnergyDrink();
            EffectLoadout loadout = Loadout();

            loadout.Add(energyDrink);
            loadout.Add(energyDrink);

            Assert.That(loadout.GetStacks(energyDrink), Is.EqualTo(2));
            Assert.That(_stats.Get(StatId.AttackSpeed), Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(_stats.Get(StatId.MaxHp), Is.EqualTo(80f).Within(Tolerance));
        }

        [Test]
        public void HyperbolicTrinket_HasDiminishingReturns()
        {
            TrinketDef trinket = _assets.Trinket("trinket.lucky_clover",
                new StatModifierEffect(StatId.CritChance, ModifierOp.Flat, 1f, StackCurve.Hyperbolic));
            EffectLoadout loadout = Loadout();

            loadout.Add(trinket);
            Assert.That(_stats.Get(StatId.CritChance), Is.EqualTo(0.5f).Within(Tolerance));

            loadout.Add(trinket);
            loadout.Add(trinket);
            Assert.That(_stats.Get(StatId.CritChance), Is.EqualTo(0.75f).Within(Tolerance));
        }

        [Test]
        public void Remove_RestoresBaseValues()
        {
            TrinketDef energyDrink = EnergyDrink();
            EffectLoadout loadout = Loadout();
            loadout.Add(energyDrink);
            loadout.Add(energyDrink);

            loadout.Remove(energyDrink);

            Assert.That(loadout.GetStacks(energyDrink), Is.EqualTo(0));
            Assert.That(_stats.Get(StatId.AttackSpeed), Is.EqualTo(1f));
            Assert.That(_stats.Get(StatId.MaxHp), Is.EqualTo(100f));
        }

        [Test]
        public void RemovingNotOwned_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => Loadout().Remove(EnergyDrink()));
        }

        [Test]
        public void SkillCheckTrinket_AddsBonusPerCopy()
        {
            // "Kostka z Kieszeni Dziadka": +1 to skill checks per copy.
            TrinketDef die = _assets.Trinket("trinket.grandpas_die",
                new StatModifierEffect(StatId.SkillCheckBonus, ModifierOp.Flat, 1f, StackCurve.Linear));
            EffectLoadout loadout = Loadout();

            loadout.Add(die);
            loadout.Add(die);
            loadout.Add(die);

            Assert.That(SkillCheckRequest.FromStats(_stats, Ability.Wis, false, 10).Bonus, Is.EqualTo(3));
        }

        [Test]
        public void LuckyTalent_AddsLuckPerBossKilled()
        {
            // "Szczęściarz": +1 LCK for every boss killed in the run.
            TalentDef lucky = _assets.Talent("talent.lucky",
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.Flat, 1f));
            EffectLoadout loadout = Loadout();
            loadout.Add(lucky);

            loadout.Raise(new TriggerArgs(GameTrigger.Kill, 0f));
            Assert.That(_stats.GetAbility(Ability.Lck), Is.EqualTo(0));

            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));
            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));
            Assert.That(_stats.GetAbility(Ability.Lck), Is.EqualTo(2));
        }

        [Test]
        public void TriggerGains_SurviveExtraCopies_AndScaleWithThem()
        {
            TrinketDef trinket = _assets.Trinket("trinket.boss_trophy",
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.Flat, 1f));
            EffectLoadout loadout = Loadout();

            loadout.Add(trinket);
            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));
            loadout.Add(trinket);
            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));

            Assert.That(_stats.GetAbility(Ability.Lck), Is.EqualTo(3));
        }

        [Test]
        public void RemovedTalent_StopsReactingAndLosesGains()
        {
            TalentDef lucky = _assets.Talent("talent.lucky",
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.Flat, 1f));
            EffectLoadout loadout = Loadout();
            loadout.Add(lucky);
            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));

            loadout.Remove(lucky);
            loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));

            Assert.That(_stats.GetAbility(Ability.Lck), Is.EqualTo(0));
        }

        [Test]
        public void ScalingTalent_ChangesWeaponAbility_UntilRemoved()
        {
            // "Pięść Mędrca": spells scale with STR instead of INT.
            TalentDef sageFist = _assets.Talent("talent.wizard.sage_fist",
                new ScalingOverrideEffect(WeaponCategory.Arcane, Ability.Str));
            EffectLoadout loadout = Loadout();

            loadout.Add(sageFist);
            Assert.That(_scaling.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Str));

            loadout.Remove(sageFist);
            Assert.That(_scaling.Resolve(WeaponCategory.Arcane, Ability.Int), Is.EqualTo(Ability.Int));
        }

        [Test]
        public void TwoOwners_ShareDefinitionButNotState()
        {
            TrinketDef energyDrink = EnergyDrink();
            var otherStats = new StatSheet(StatIds.UnboundedLimits());
            otherStats.SetBase(StatId.AttackSpeed, 1f);
            EffectLoadout first = Loadout();
            var second = new EffectLoadout(otherStats, new ScalingTable(), new ScriptedRandomSource(new int[0]), ProcChancePerLuck);

            first.Add(energyDrink);
            first.Add(energyDrink);
            second.Add(energyDrink);

            Assert.That(_stats.Get(StatId.AttackSpeed), Is.EqualTo(1.5f).Within(Tolerance));
            Assert.That(otherStats.Get(StatId.AttackSpeed), Is.EqualTo(1.25f).Within(Tolerance));
        }

        [Test]
        public void RollProc_AppliesLuck()
        {
            // 10% base, LCK 4 → 10% × 1.2 = 12%.
            _stats.SetBase(StatId.Lck, 4f);
            EffectLoadout below = Loadout(0.119f);
            EffectLoadout above = Loadout(0.121f);

            Assert.That(below.RollProc(0.10f), Is.True);
            Assert.That(above.RollProc(0.10f), Is.False);
        }

        [Test]
        public void RollProc_CertainOutcomes_DoNotDraw()
        {
            // No floats scripted: drawing one would throw.
            EffectLoadout loadout = Loadout();

            Assert.That(loadout.RollProc(0f), Is.False);
            Assert.That(loadout.RollProc(1f), Is.True);
        }

        private TrinketDef EnergyDrink()
        {
            return _assets.Trinket("trinket.energy_drink",
                new StatModifierEffect(StatId.AttackSpeed, ModifierOp.PercentAdd, 0.25f, StackCurve.Linear),
                new StatModifierEffect(StatId.MaxHp, ModifierOp.PercentAdd, -0.10f, StackCurve.Linear));
        }

        private EffectLoadout Loadout(params float[] procRolls)
        {
            return new EffectLoadout(_stats, _scaling, new ScriptedRandomSource(new int[0], procRolls), ProcChancePerLuck);
        }
    }
}
