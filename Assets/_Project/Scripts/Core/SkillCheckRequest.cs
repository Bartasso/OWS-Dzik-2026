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
            bool hasDisadvantage)
        {
            Dc = dc;
            AbilityModifier = abilityModifier;
            IsProficient = isProficient;
            Luck = luck;
            HasAdvantage = hasAdvantage;
            HasDisadvantage = hasDisadvantage;
        }

        public int Dc { get; }

        public int AbilityModifier { get; }

        public bool IsProficient { get; }

        public int Luck { get; }

        public bool HasAdvantage { get; }

        public bool HasDisadvantage { get; }
    }
}
