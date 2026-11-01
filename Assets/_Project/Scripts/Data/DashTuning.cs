using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>Dash parameters (GDD section 3). Extra charges come from the ExtraDashCharges stat.</summary>
    [Serializable]
    public sealed class DashTuning
    {
        [Tooltip("Horizontal speed during the dash, in m/s.")]
        [SerializeField] private float _speed;

        [Tooltip("Seconds the dash lasts; gravity is off meanwhile.")]
        [SerializeField] private float _duration;

        [Tooltip("Seconds to recharge one charge.")]
        [SerializeField] private float _cooldown;

        [Tooltip("Seconds of invulnerability from the start of the dash.")]
        [SerializeField] private float _invulnerability;

        [Tooltip("Charges every character has before stat bonuses.")]
        [SerializeField] private int _baseCharges;

        public DashTuning()
        {
        }

        public DashTuning(float speed, float duration, float cooldown, float invulnerability, int baseCharges)
        {
            _speed = speed;
            _duration = duration;
            _cooldown = cooldown;
            _invulnerability = invulnerability;
            _baseCharges = baseCharges;
        }

        public float Speed => _speed;

        public float Duration => _duration;

        public float Cooldown => _cooldown;

        public float Invulnerability => _invulnerability;

        public int BaseCharges => _baseCharges;

        public void Validate(ValidationReport report, UnityEngine.Object owner)
        {
            DataChecks.Positive(report, owner, "dash." + nameof(_speed), _speed);
            DataChecks.Positive(report, owner, "dash." + nameof(_duration), _duration);
            DataChecks.Positive(report, owner, "dash." + nameof(_cooldown), _cooldown);
            DataChecks.NotNegative(report, owner, "dash." + nameof(_invulnerability), _invulnerability);

            if (_baseCharges < 0)
            {
                report.AddError(owner.name, $"dash.{nameof(_baseCharges)} must be >= 0, is {_baseCharges}", owner);
            }
        }
    }
}
