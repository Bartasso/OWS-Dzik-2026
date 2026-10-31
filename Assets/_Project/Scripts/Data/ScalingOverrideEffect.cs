using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>Weapons of a category scale with a different ability, e.g. "Sage's Fist": Arcane → Str.</summary>
    [Serializable]
    public sealed class ScalingOverrideEffect : Effect
    {
        [SerializeField] private WeaponCategory _category;
        [SerializeField] private Ability _ability;

        public ScalingOverrideEffect()
        {
        }

        public ScalingOverrideEffect(WeaponCategory category, Ability ability)
        {
            _category = category;
            _ability = ability;
        }

        public override void Install(EffectInstance instance, IEffectContext context)
        {
            context.Scaling.SetOverride(_category, _ability, instance.Source);
        }

        public override void Validate(ValidationReport report, string owner, object context)
        {
            const string effect = nameof(ScalingOverrideEffect);
            EffectChecks.CheckEnum(report, owner, context, effect, _category);
            EffectChecks.CheckEnum(report, owner, context, effect, _ability);
        }
    }
}
