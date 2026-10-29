namespace Game.Core
{
    public readonly struct StatModifier
    {
        public StatModifier(StatId stat, ModifierOp op, float value, ModifierSourceId source)
        {
            Stat = stat;
            Op = op;
            Value = value;
            Source = source;
        }

        public StatId Stat { get; }

        public ModifierOp Op { get; }

        public float Value { get; }

        public ModifierSourceId Source { get; }
    }
}
