namespace Game.Core
{
    /// <summary>Balance values for skill checks, supplied from GameBalance.</summary>
    public readonly struct SkillCheckRules
    {
        public SkillCheckRules(int proficiencyBonus, float nat1RerollChancePerLuck)
        {
            ProficiencyBonus = proficiencyBonus;
            Nat1RerollChancePerLuck = nat1RerollChancePerLuck;
        }

        public int ProficiencyBonus { get; }

        public float Nat1RerollChancePerLuck { get; }
    }
}
