using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A skill check category ("Zwierzęta", "Siłowanie") and the ability it rolls with.</summary>
    [CreateAssetMenu(menuName = "Game/Skill", fileName = "Skill")]
    public sealed class SkillDef : Definition
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Ability _ability;

        public string DisplayName => _displayName;

        public Ability Ability => _ability;

        protected override void ValidateDefinition(ValidationReport report)
        {
            DataChecks.DisplayName(report, this, _displayName);
            EffectChecks.CheckEnum(report, name, this, nameof(SkillDef), _ability);
        }
    }
}
