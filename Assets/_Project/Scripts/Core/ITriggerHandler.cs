namespace Game.Core
{
    public interface ITriggerHandler
    {
        void OnTrigger(in TriggerArgs args, EffectInstance instance, IEffectContext context);
    }
}
