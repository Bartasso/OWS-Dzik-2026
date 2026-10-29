namespace Game.Core
{
    // Explicit values: triggers are serialized in effect data.
    public enum GameTrigger
    {
        Hit = 0,
        Crit = 1,
        Kill = 2,
        EliteKilled = 3,
        BossKilled = 4,
        DamageTaken = 5,
        BoostPicked = 6,
        SkillCheckResolved = 7,
        LevelUp = 8,
        ArenaStarted = 9,
    }
}
