using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>A projectile weapon. Values are read on every shot, so tweaking them in Play Mode works live.</summary>
    [CreateAssetMenu(menuName = "Game/Weapon", fileName = "Weapon")]
    public sealed class WeaponDef : Definition
    {
        [SerializeField] private string _displayName;
        [SerializeField] private WeaponCategory _category;
        [SerializeField] private Ability _scalingAbility;

        [Header("Damage")]
        [SerializeField] private float _baseDamage;

        [Tooltip("Seconds between shots at AttackSpeed 1.")]
        [SerializeField] private float _fireInterval;

        [Header("Projectile")]
        [Tooltip("Meters per second.")]
        [SerializeField] private float _projectileSpeed;

        [Tooltip("Radius in meters: both the hit size and the visual size.")]
        [SerializeField] private float _projectileRadius;

        [Tooltip("Meters the projectile flies before it disappears.")]
        [SerializeField] private float _range;

        public string DisplayName => _displayName;

        public WeaponCategory Category => _category;

        public Ability ScalingAbility => _scalingAbility;

        public float BaseDamage => _baseDamage;

        public float FireInterval => _fireInterval;

        public float ProjectileSpeed => _projectileSpeed;

        public float ProjectileRadius => _projectileRadius;

        public float Range => _range;

        protected override void ValidateDefinition(ValidationReport report)
        {
            DataChecks.DisplayName(report, this, _displayName);
            EffectChecks.CheckEnum(report, name, this, nameof(WeaponDef), _category);
            EffectChecks.CheckEnum(report, name, this, nameof(WeaponDef), _scalingAbility);
            DataChecks.Positive(report, this, nameof(_baseDamage), _baseDamage);
            DataChecks.Positive(report, this, nameof(_fireInterval), _fireInterval);
            DataChecks.Positive(report, this, nameof(_projectileSpeed), _projectileSpeed);
            DataChecks.Positive(report, this, nameof(_projectileRadius), _projectileRadius);
            DataChecks.Positive(report, this, nameof(_range), _range);
        }
    }
}
