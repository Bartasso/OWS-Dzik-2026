using System;

namespace Game.Core
{
    /// <summary>
    /// The only place where d20 is rolled (GDD section 4).
    /// </summary>
    public sealed class SkillCheck
    {
        private const int DieFaces = 20;

        private readonly SkillCheckRules _rules;
        private readonly IRandomSource _dice;

        public SkillCheck(SkillCheckRules rules, IRandomSource dice)
        {
            _rules = rules;
            _dice = dice ?? throw new ArgumentNullException(nameof(dice));
        }

        public SkillCheckResult Resolve(in SkillCheckRequest request)
        {
            // Advantage and disadvantage cancel out and never stack.
            bool advantage = request.HasAdvantage && !request.HasDisadvantage;
            bool disadvantage = request.HasDisadvantage && !request.HasAdvantage;

            int natural = RollD20();
            if (advantage || disadvantage)
            {
                int second = RollD20();
                natural = advantage ? Math.Max(natural, second) : Math.Min(natural, second);
            }

            // Only the chosen die is rerolled, with a single d20, and the reroll is final.
            bool wasRerolled = false;
            if (natural == 1 && ShouldRerollNat1(request.Luck))
            {
                natural = RollD20();
                wasRerolled = true;
            }

            int proficiency = request.IsProficient ? _rules.ProficiencyBonus : 0;
            int total = natural + request.AbilityModifier + proficiency;
            bool success = natural == DieFaces || (natural != 1 && total >= request.Dc);

            return new SkillCheckResult(natural, total, success, wasRerolled);
        }

        private bool ShouldRerollNat1(int luck)
        {
            float chance = StatFormulas.Nat1RerollChance(luck, _rules.Nat1RerollChancePerLuck);
            // Certain outcomes skip the draw, so they don't consume the stream.
            if (chance <= 0f)
            {
                return false;
            }

            return chance >= 1f || _dice.NextFloat() < chance;
        }

        private int RollD20()
        {
            return _dice.NextInt(1, DieFaces + 1);
        }
    }
}
