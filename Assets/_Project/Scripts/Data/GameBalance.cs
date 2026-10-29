using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Global balance constants. One asset in the project, referenced by the bootstrap.
    /// Values live in the asset, not in code.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Game Balance", fileName = "GameBalance")]
    public sealed class GameBalance : ScriptableObject, IValidatable
    {
        [Header("Combat")]
        [SerializeField] private float _baseCritChance;
        [SerializeField] private float _critChancePerLuck;
        [SerializeField] private float _baseCritMultiplier;

        [Header("Skill checks")]
        [SerializeField] private int _proficiencyBonus;
        [SerializeField] private float _nat1RerollChancePerLuck;

        public float BaseCritChance => _baseCritChance;

        public float CritChancePerLuck => _critChancePerLuck;

        public float BaseCritMultiplier => _baseCritMultiplier;

        public SkillCheckRules ToSkillCheckRules()
        {
            return new SkillCheckRules(_proficiencyBonus, _nat1RerollChancePerLuck);
        }

        public void Validate(ValidationReport report)
        {
            CheckRange(report, nameof(_baseCritChance), _baseCritChance, 0f, 1f);
            CheckRange(report, nameof(_critChancePerLuck), _critChancePerLuck, 0f, 1f);
            CheckRange(report, nameof(_nat1RerollChancePerLuck), _nat1RerollChancePerLuck, 0f, 1f);

            if (_baseCritMultiplier < 1f)
            {
                report.AddError(name, $"{nameof(_baseCritMultiplier)} must be >= 1, is {_baseCritMultiplier}", this);
            }

            if (_proficiencyBonus < 0)
            {
                report.AddError(name, $"{nameof(_proficiencyBonus)} must be >= 0, is {_proficiencyBonus}", this);
            }
        }

        private void OnValidate()
        {
            ValidationLog.ValidateAndLog(this);
        }

        private void CheckRange(ValidationReport report, string field, float value, float min, float max)
        {
            if (value < min || value > max)
            {
                report.AddError(name, $"{field} must be in [{min}, {max}], is {value}", this);
            }
        }
    }
}
