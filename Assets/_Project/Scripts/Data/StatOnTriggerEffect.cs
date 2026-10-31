using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Adds a stat modifier for the rest of the run every time a trigger fires,
    /// e.g. "Lucky": +1 LCK per boss killed. The amount scales linearly with stacks.
    /// </summary>
    [Serializable]
    public sealed class StatOnTriggerEffect : Effect, ITriggerHandler
    {
        [SerializeField] private GameTrigger _trigger;
        [SerializeField] private StatId _stat;
        [SerializeField] private ModifierOp _op;
        [SerializeField] private float _valuePerStack;

        public StatOnTriggerEffect()
        {
        }

        public StatOnTriggerEffect(GameTrigger trigger, StatId stat, ModifierOp op, float valuePerStack)
        {
            _trigger = trigger;
            _stat = stat;
            _op = op;
            _valuePerStack = valuePerStack;
        }

        public override void Install(EffectInstance instance, IEffectContext context)
        {
            context.Subscribe(_trigger, this, instance);
        }

        public void OnTrigger(in TriggerArgs args, EffectInstance instance, IEffectContext context)
        {
            context.Stats.AddModifier(new StatModifier(_stat, _op, _valuePerStack * instance.Stacks, instance.Source));
        }

        public override void Validate(ValidationReport report, string owner, object context)
        {
            const string effect = nameof(StatOnTriggerEffect);
            EffectChecks.CheckEnum(report, owner, context, effect, _trigger);
            EffectChecks.CheckModifier(report, owner, context, effect, _stat, _op, _valuePerStack);
        }
    }
}
