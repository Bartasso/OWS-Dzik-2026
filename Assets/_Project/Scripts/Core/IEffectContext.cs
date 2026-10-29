namespace Game.Core
{
    /// <summary>What an effect may touch on its owner.</summary>
    public interface IEffectContext
    {
        StatSheet Stats { get; }

        ScalingTable Scaling { get; }

        void Subscribe(GameTrigger trigger, ITriggerHandler handler, EffectInstance instance);

        /// <summary>
        /// The only way to roll a percentage effect: applies LCK (StatFormulas.ProcChance) and draws from Rng.Combat.
        /// </summary>
        bool RollProc(float baseChance);
    }
}
