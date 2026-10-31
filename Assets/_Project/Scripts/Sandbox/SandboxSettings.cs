using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Sandbox
{
    /// <summary>Test-only tuning for the sandbox scene.</summary>
    [CreateAssetMenu(menuName = "Game/Sandbox/Sandbox Settings", fileName = "SandboxSettings")]
    public sealed class SandboxSettings : ScriptableObject, IValidatable
    {
        [Tooltip("Meters from a pickup at which the player collects it.")]
        [SerializeField] private float _pickupRadius;

        [Tooltip("Seconds until a collected pickup comes back (to test stacking).")]
        [SerializeField] private float _pickupRespawnSeconds;

        [Tooltip("Talents offered as buttons in the debug panel.")]
        [SerializeField] private List<TalentDef> _testTalents = new List<TalentDef>();

        [Tooltip("Base chance used by the 'Roll proc' button.")]
        [SerializeField] private float _testProcChance;

        public float PickupRadius => _pickupRadius;

        public float PickupRespawnSeconds => _pickupRespawnSeconds;

        public IReadOnlyList<TalentDef> TestTalents => _testTalents;

        public float TestProcChance => _testProcChance;

        public void Validate(ValidationReport report)
        {
            if (!(_pickupRadius > 0f))
            {
                report.AddError(name, $"{nameof(_pickupRadius)} must be > 0, is {_pickupRadius}", this);
            }

            if (!(_pickupRespawnSeconds >= 0f))
            {
                report.AddError(name, $"{nameof(_pickupRespawnSeconds)} must be >= 0, is {_pickupRespawnSeconds}", this);
            }

            if (!(_testProcChance >= 0f && _testProcChance <= 1f))
            {
                report.AddError(name, $"{nameof(_testProcChance)} must be in [0, 1], is {_testProcChance}", this);
            }

            for (int i = 0; i < _testTalents.Count; i++)
            {
                if (_testTalents[i] == null)
                {
                    report.AddError(name, $"test talent #{i} is empty", this);
                }
            }
        }

        private void OnValidate()
        {
            ValidationLog.ValidateAndLog(this);
        }
    }
}
