namespace Game.Core
{
    /// <summary>
    /// Per-character state of one installed effect. Effect definitions are shared data,
    /// so anything that differs per owner (source, stacks) lives here.
    /// </summary>
    public sealed class EffectInstance
    {
        public EffectInstance(ModifierSourceId source, int stacks)
        {
            Source = source;
            Stacks = stacks;
        }

        public ModifierSourceId Source { get; }

        public int Stacks { get; set; }
    }
}
