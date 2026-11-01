namespace Game.Core
{
    // Explicit values: stats are serialized in effect data. Abilities must stay first and match Ability.
    public enum StatId
    {
        Str = 0,
        Dex = 1,
        Con = 2,
        Int = 3,
        Wis = 4,
        Cha = 5,
        Lck = 6,
        MaxHp = 7,
        HpRegen = 8,
        AttackSpeed = 9,
        MoveSpeed = 10,
        CritChance = 11,
        CritMultiplier = 12,
        CooldownReduction = 13,
        PickupRange = 14,
        BoostDuration = 15,
        SkillCheckBonus = 16,
        SkillCheckAdvantage = 17,
        SkillCheckDisadvantage = 18,
        ExtraJumps = 19,
        ExtraDashCharges = 20,
    }
}
