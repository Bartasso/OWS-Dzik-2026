using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A permanent stat modifier that grows with stacks, e.g. +25% attack speed per copy.</summary>
    [Serializable]
    public sealed class StatModifierEffect : Effect
    {
        [SerializeField] private StatId _stat;
        [SerializeField] private ModifierOp _op;
        [SerializeField] private StackCurve _curve;
        [SerializeField] private float _valuePerStack;

        public StatModifierEffect()
        {
        }

        public StatModifierEffect(StatId stat, ModifierOp op, float valuePerStack, StackCurve curve)
        {
            _stat = stat;
            _op = op;
            _valuePerStack = valuePerStack;
            _curve = curve;
        }

        public override void Install(EffectInstance instance, IEffectContext context)
        {
            Apply(instance, context);
        }

        public override void OnStacksChanged(EffectInstance instance, IEffectContext context)
        {
            context.Stats.RemoveSource(instance.Source);
            Apply(instance, context);
        }

        public override void Validate(ValidationReport report, string owner, object context)
        {
            const string effect = nameof(StatModifierEffect);
            EffectChecks.CheckModifier(report, owner, context, effect, _stat, _op, _valuePerStack);
            EffectChecks.CheckEnum(report, owner, context, effect, _curve);

            if (_curve == StackCurve.Hyperbolic)
            {
                if (StatIds.IsInteger(_stat))
                {
                    report.AddError(owner, $"{effect}: Hyperbolic curve gives fractions, {_stat} is an integer stat", context);
                }

                if (_valuePerStack < 0f)
                {
                    report.AddError(owner, $"{effect}: Hyperbolic curve needs a positive value, is {_valuePerStack}", context);
                }
            }
        }

        private void Apply(EffectInstance instance, IEffectContext context)
        {
            float value = StatFormulas.StackValue(_curve, _valuePerStack, instance.Stacks);
            context.Stats.AddModifier(new StatModifier(_stat, _op, value, instance.Source));
        }
    }
}
