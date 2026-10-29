namespace Game.Core
{
    public readonly struct TriggerArgs
    {
        public TriggerArgs(GameTrigger trigger, float amount)
        {
            Trigger = trigger;
            Amount = amount;
        }

        public GameTrigger Trigger { get; }

        /// <summary>Damage for Hit, Crit and DamageTaken; 0 for triggers without a magnitude.</summary>
        public float Amount { get; }
    }
}
