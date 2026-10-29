using System;

namespace Game.Core
{
    /// <summary>
    /// The single place for scaling formulas. Balance numbers come in as parameters (from GameBalance),
    /// never as constants here. Every formula is covered by EditMode tests.
    /// </summary>
    public static class StatFormulas
    {
        public static float CritChance(int luck, float baseChance, float chancePerLuck, float bonusChance)
        {
            return Clamp01(baseChance + luck * chancePerLuck + bonusChance);
        }

        public static float Nat1RerollChance(int luck, float chancePerLuck)
        {
            return Clamp01(luck * chancePerLuck);
        }

        /// <summary>Weapon damage factor from its scaling ability: +X% per point, never below zero.</summary>
        public static float AbilityDamageMultiplier(int abilityModifier, float bonusPerPoint)
        {
            float multiplier = 1f + abilityModifier * bonusPerPoint;
            return multiplier < 0f ? 0f : multiplier;
        }

        /// <summary>A crit never deals less than a normal hit.</summary>
        public static float CritMultiplier(float baseMultiplier, float bonus)
        {
            float multiplier = baseMultiplier + bonus;
            return multiplier < 1f ? 1f : multiplier;
        }

        /// <summary>Chance of a percentage effect (trinket proc) after LCK.</summary>
        public static float ProcChance(float baseChance, int luck, float bonusPerLuck)
        {
            return Clamp01(baseChance * (1f + luck * bonusPerLuck));
        }

        public static float StackValue(StackCurve curve, float valuePerStack, int stacks)
        {
            if (stacks <= 0)
            {
                return 0f;
            }

            switch (curve)
            {
                case StackCurve.Linear:
                    return valuePerStack * stacks;
                case StackCurve.Hyperbolic:
                    return 1f - 1f / (1f + valuePerStack * stacks);
                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
