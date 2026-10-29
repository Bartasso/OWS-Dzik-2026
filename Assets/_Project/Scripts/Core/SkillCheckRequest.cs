namespace Game.Core
{
    public readonly struct SkillCheckRequest
    {
        public SkillCheckRequest(
            int dc,
            int abilityModifier,
            bool isProficient,
            int luck,
            bool hasAdvantage,
            bool hasDisadvantage,
            int bonus = 0)
        {
            Dc = dc;
            AbilityModifier = abilityModifier;
            IsProficient = isProficient;
            Luck = luck;
            HasAdvantage = hasAdvantage;
            HasDisadvantage = hasDisadvantage;
            Bonus = bonus;
        }

        public int Dc { get; }

        public int AbilityModifier { get; }

        public bool IsProficient { get; }

        public int Luck { get; }

        public bool HasAdvantage { get; }

        public bool HasDisadvantage { get; }

        /// <summary>Flat bonus from trinkets and talents (SkillCheckBonus stat).</summary>
        public int Bonus { get; }

        public static SkillCheckRequest FromStats(StatSheet stats, Ability ability, bool isProficient, int dc)
        {
            return new SkillCheckRequest(
                dc,
                stats.GetAbility(ability),
                isProficient,
                stats.GetAbility(Ability.Lck),
                stats.GetInt(StatId.SkillCheckAdvantage) > 0,
                stats.GetInt(StatId.SkillCheckDisadvantage) > 0,
                stats.GetInt(StatId.SkillCheckBonus));
        }
    }
}
