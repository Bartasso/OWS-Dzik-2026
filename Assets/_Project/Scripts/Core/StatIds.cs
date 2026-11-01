namespace Game.Core
{
    public static class StatIds
    {
        public const int Count = 20;

        public static bool IsAbility(StatId stat)
        {
            return stat >= StatId.Str && stat <= StatId.Lck;
        }

        /// <summary>
        /// Integer stats accept only whole Flat modifiers: abilities, the skill check stats and jump count.
        /// </summary>
        public static bool IsInteger(StatId stat)
        {
            return IsAbility(stat)
                || stat == StatId.SkillCheckBonus
                || stat == StatId.SkillCheckAdvantage
                || stat == StatId.SkillCheckDisadvantage
                || stat == StatId.ExtraJumps;
        }

        public static StatId FromAbility(Ability ability)
        {
            return (StatId)(int)ability;
        }

        public static StatRange[] UnboundedLimits()
        {
            var limits = new StatRange[Count];
            for (int i = 0; i < Count; i++)
            {
                limits[i] = StatRange.Unbounded;
            }

            return limits;
        }
    }
}
