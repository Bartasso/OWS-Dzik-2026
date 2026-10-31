using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A funny situation on the map ("Ściągnij kota z drzewa"): one d20 attempt, reward on success.</summary>
    [CreateAssetMenu(menuName = "Game/Skill Check Site", fileName = "SkillCheckSite")]
    public sealed class SkillCheckSiteDef : Definition
    {
        [SerializeField] private string _displayName;
        [SerializeField] private SkillDef _skill;

        [Tooltip("Base DC. Scaling with the arena number comes later.")]
        [SerializeField] private int _dc;

        [SerializeField] private EffectDefinition _reward;

        public string DisplayName => _displayName;

        public SkillDef Skill => _skill;

        public int Dc => _dc;

        public EffectDefinition Reward => _reward;

        protected override void ValidateDefinition(ValidationReport report)
        {
            DataChecks.DisplayName(report, this, _displayName);
            DataChecks.Required(report, this, nameof(_skill), _skill);
            DataChecks.Required(report, this, nameof(_reward), _reward);

            if (_dc < 1)
            {
                report.AddError(name, $"{nameof(_dc)} must be >= 1, is {_dc}", this);
            }
        }
    }
}
