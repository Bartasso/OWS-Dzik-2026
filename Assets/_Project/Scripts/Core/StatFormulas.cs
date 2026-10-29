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
