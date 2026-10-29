using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A definition whose gameplay is a list of effects (trinkets, talents).</summary>
    public abstract class EffectDefinition : Definition
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Rarity _rarity;
        [SerializeReference, SubclassPicker] private List<Effect> _effects = new List<Effect>();

        public string DisplayName => _displayName;

        public Rarity Rarity => _rarity;

        public IReadOnlyList<Effect> Effects => _effects;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                report.AddError(name, "missing display name", this);
            }

            EffectChecks.CheckEnum(report, name, this, nameof(EffectDefinition), _rarity);

            if (_effects == null || _effects.Count == 0)
            {
                report.AddError(name, "has no effects", this);
                return;
            }

            for (int i = 0; i < _effects.Count; i++)
            {
                Effect effect = _effects[i];
                if (effect == null)
                {
                    report.AddError(name, $"effect #{i} is missing (empty entry, or its class was renamed/deleted)", this);
                    continue;
                }

                for (int j = 0; j < i; j++)
                {
                    // Duplicating a list entry in the Inspector can share one instance between two entries.
                    if (ReferenceEquals(_effects[j], effect))
                    {
                        report.AddError(name, $"effects #{j} and #{i} are the same instance; recreate #{i}", this);
                    }
                }

                effect.Validate(report, name, this);
            }
        }
    }
}
