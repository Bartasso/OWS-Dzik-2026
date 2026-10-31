using Game.Core;
using Game.Data;

namespace Game.Gameplay
{
    /// <summary>Services and state of the current run, created by the bootstrap and passed explicitly.</summary>
    public sealed class RunContext
    {
        public RunContext(
            GameBalance balance,
            CharacterDef character,
            Rng rng,
            StatSheet stats,
            ScalingTable scaling,
            EffectLoadout loadout,
            SkillCheck skillCheck,
            GamePause pause,
            CombatEvents events)
        {
            Balance = balance;
            Character = character;
            Rng = rng;
            Stats = stats;
            Scaling = scaling;
            Loadout = loadout;
            SkillCheck = skillCheck;
            Pause = pause;
            Events = events;
        }

        public GameBalance Balance { get; }

        public CharacterDef Character { get; }

        public Rng Rng { get; }

        public StatSheet Stats { get; }

        public ScalingTable Scaling { get; }

        public EffectLoadout Loadout { get; }

        public SkillCheck SkillCheck { get; }

        public GamePause Pause { get; }

        public CombatEvents Events { get; }
    }
}
