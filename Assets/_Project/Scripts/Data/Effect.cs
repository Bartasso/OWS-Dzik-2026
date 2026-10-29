using System;
using Game.Core;

namespace Game.Data
{
    /// <summary>
    /// Base for effects stored with [SerializeReference] on trinkets and talents.
    /// Effects are shared between owners: keep per-owner state in EffectInstance, never in fields.
    /// Renaming or moving a subclass breaks existing assets unless it gets
    /// [UnityEngine.Scripting.APIUpdating.MovedFrom(...)] with the old name.
    /// </summary>
    [Serializable]
    public abstract class Effect : IEffect
    {
        public abstract void Install(EffectInstance instance, IEffectContext context);

        public virtual void OnStacksChanged(EffectInstance instance, IEffectContext context)
        {
        }

        public abstract void Validate(ValidationReport report, string owner, object context);
    }
}
