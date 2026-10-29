using System;
using System.Collections.Generic;
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

        [Tooltip("Weapon damage bonus per point of its scaling ability: 0.05 = +5%.")]
        [SerializeField] private float _damagePerAbilityPoint;

        [Header("Characters")]
        [Tooltip("Bonus to each of the class's two primary abilities.")]
        [SerializeField] private int _primaryAbilityBonus;

        [Tooltip("Meters within which the player can interact (E).")]
        [SerializeField] private float _interactRadius;

        [Header("Effects")]
        [Tooltip("Proc chance multiplier per LCK point: chance × (1 + LCK × value).")]
        [SerializeField] private float _procChancePerLuck;

        [Header("Skill checks")]
        [SerializeField] private int _proficiencyBonus;
        [SerializeField] private float _nat1RerollChancePerLuck;

        [Tooltip("Copies of the reward granted on a natural 20.")]
        [SerializeField] private int _nat20RewardCopies;

        [Tooltip("Seconds the game stays paused to show the roll.")]
        [SerializeField] private float _skillCheckPauseSeconds;

        [Header("Stats")]
        [Tooltip("Clamp on final stat values. Stats without an entry are unbounded.")]
        [SerializeField] private List<StatLimit> _statLimits = new List<StatLimit>();

        public float BaseCritChance => _baseCritChance;

        public float CritChancePerLuck => _critChancePerLuck;

        public float BaseCritMultiplier => _baseCritMultiplier;

        public float ProcChancePerLuck => _procChancePerLuck;

        public float DamagePerAbilityPoint => _damagePerAbilityPoint;

        public int PrimaryAbilityBonus => _primaryAbilityBonus;

        public float InteractRadius => _interactRadius;

        public int Nat20RewardCopies => _nat20RewardCopies;

        public float SkillCheckPauseSeconds => _skillCheckPauseSeconds;

        public SkillCheckRules ToSkillCheckRules()
        {
            return new SkillCheckRules(_proficiencyBonus, _nat1RerollChancePerLuck);
        }

        public StatRange[] BuildStatLimits()
        {
            StatRange[] limits = StatIds.UnboundedLimits();
            foreach (StatLimit limit in _statLimits)
            {
                limits[(int)limit.Stat] = new StatRange(limit.Min, limit.Max);
            }

            return limits;
        }

        public void Validate(ValidationReport report)
        {
            CheckRange(report, nameof(_baseCritChance), _baseCritChance, 0f, 1f);
            CheckRange(report, nameof(_critChancePerLuck), _critChancePerLuck, 0f, 1f);
            CheckRange(report, nameof(_procChancePerLuck), _procChancePerLuck, 0f, 1f);
            CheckRange(report, nameof(_nat1RerollChancePerLuck), _nat1RerollChancePerLuck, 0f, 1f);

            if (_baseCritMultiplier < 1f)
            {
                report.AddError(name, $"{nameof(_baseCritMultiplier)} must be >= 1, is {_baseCritMultiplier}", this);
            }

            if (_proficiencyBonus < 0)
            {
                report.AddError(name, $"{nameof(_proficiencyBonus)} must be >= 0, is {_proficiencyBonus}", this);
            }

            if (_primaryAbilityBonus < 0)
            {
                report.AddError(name, $"{nameof(_primaryAbilityBonus)} must be >= 0, is {_primaryAbilityBonus}", this);
            }

            if (_nat20RewardCopies < 1)
            {
                report.AddError(name, $"{nameof(_nat20RewardCopies)} must be >= 1, is {_nat20RewardCopies}", this);
            }

            DataChecks.NotNegative(report, this, nameof(_damagePerAbilityPoint), _damagePerAbilityPoint);
            DataChecks.Positive(report, this, nameof(_interactRadius), _interactRadius);
            DataChecks.NotNegative(report, this, nameof(_skillCheckPauseSeconds), _skillCheckPauseSeconds);

            ValidateStatLimits(report);
        }

        private void OnValidate()
        {
            ValidationLog.ValidateAndLog(this);
        }

        private void ValidateStatLimits(ValidationReport report)
        {
            var seen = new bool[StatIds.Count];
            foreach (StatLimit limit in _statLimits)
            {
                if (!Enum.IsDefined(typeof(StatId), limit.Stat))
                {
                    report.AddError(name, $"stat limit for unknown stat {limit.Stat}", this);
                    continue;
                }

                if (seen[(int)limit.Stat])
                {
                    report.AddError(name, $"duplicate stat limit for {limit.Stat}", this);
                }

                seen[(int)limit.Stat] = true;

                if (float.IsNaN(limit.Min) || float.IsNaN(limit.Max) || limit.Min > limit.Max)
                {
                    report.AddError(name, $"stat limit for {limit.Stat} has min {limit.Min} > max {limit.Max}", this);
                }
            }
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
