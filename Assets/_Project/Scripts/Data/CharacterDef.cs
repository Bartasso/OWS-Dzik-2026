using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A playable friend: base stats, two primary abilities, proficiencies and starting weapon.</summary>
    [CreateAssetMenu(menuName = "Game/Character", fileName = "Character")]
    public sealed class CharacterDef : Definition
    {
        private const int PrimaryAbilityCount = 2;

        [SerializeField] private string _displayName;

        [Tooltip("Each gets GameBalance's primary ability bonus (+2).")]
        [SerializeField] private Ability[] _primaryAbilities = new Ability[PrimaryAbilityCount];

        [Tooltip("Base values: MaxHp, MoveSpeed (m/s), AttackSpeed (1 = weapon's interval)...")]
        [SerializeField] private List<StatValue> _baseStats = new List<StatValue>();

        [SerializeField] private List<SkillDef> _proficiencies = new List<SkillDef>();
        [SerializeField] private WeaponDef _startingWeapon;

        public string DisplayName => _displayName;

        public IReadOnlyList<Ability> PrimaryAbilities => _primaryAbilities;

        public IReadOnlyList<StatValue> BaseStats => _baseStats;

        public IReadOnlyList<SkillDef> Proficiencies => _proficiencies;

        public WeaponDef StartingWeapon => _startingWeapon;

        public bool IsProficient(SkillDef skill)
        {
            foreach (SkillDef proficiency in _proficiencies)
            {
                if (proficiency == skill)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            DataChecks.DisplayName(report, this, _displayName);
            DataChecks.Required(report, this, nameof(_startingWeapon), _startingWeapon);

            if (_primaryAbilities == null || _primaryAbilities.Length != PrimaryAbilityCount)
            {
                report.AddError(name, $"needs exactly {PrimaryAbilityCount} primary abilities", this);
            }
            else if (_primaryAbilities[0] == _primaryAbilities[1])
            {
                report.AddError(name, $"primary abilities must differ, both are {_primaryAbilities[0]}", this);
            }

            var seenStats = new bool[StatIds.Count];
            foreach (StatValue value in _baseStats)
            {
                int index = (int)value.Stat;
                if ((uint)index >= StatIds.Count)
                {
                    report.AddError(name, $"base stat has unknown stat {value.Stat}", this);
                    continue;
                }

                if (seenStats[index])
                {
                    report.AddError(name, $"base stat {value.Stat} is listed twice", this);
                }

                seenStats[index] = true;
                if (StatIds.IsInteger(value.Stat) && value.Value != Mathf.Round(value.Value))
                {
                    report.AddError(name, $"base {value.Stat} must be whole, is {value.Value}", this);
                }
            }

            for (int i = 0; i < _proficiencies.Count; i++)
            {
                if (_proficiencies[i] == null)
                {
                    report.AddError(name, $"proficiency #{i} is empty", this);
                    continue;
                }

                if (_proficiencies.IndexOf(_proficiencies[i]) != i)
                {
                    report.AddError(name, $"proficiency {_proficiencies[i].name} is listed twice", this);
                }
            }
        }
    }
}
