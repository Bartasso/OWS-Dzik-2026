namespace Game.Core
{
    /// <summary>
    /// An effect of a trinket or talent. Everything it adds must be tagged with instance.Source,
    /// so the owner can uninstall it by removing that source from stats, scaling and triggers.
    /// </summary>
    public interface IEffect
    {
        void Install(EffectInstance instance, IEffectContext context);

        /// <summary>Called after instance.Stacks changed (another trinket copy picked up).</summary>
        void OnStacksChanged(EffectInstance instance, IEffectContext context);
    }
}
