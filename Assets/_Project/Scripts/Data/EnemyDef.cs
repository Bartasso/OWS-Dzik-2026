using Game.Core;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Enemy", fileName = "Enemy")]
    public sealed class EnemyDef : Definition
    {
        [SerializeField] private string _displayName;
        [SerializeField] private EnemyKind _kind;
        [SerializeField] private float _maxHp;

        [Tooltip("Meters per second.")]
        [SerializeField] private float _moveSpeed;

        [Tooltip("Body radius in meters: hit size and visual size.")]
        [SerializeField] private float _radius;

        [Header("Contact attack")]
        [SerializeField] private float _contactDamage;

        [Tooltip("Seconds between contact hits.")]
        [SerializeField] private float _contactInterval;

        public string DisplayName => _displayName;

        public EnemyKind Kind => _kind;

        public float MaxHp => _maxHp;

        public float MoveSpeed => _moveSpeed;

        public float Radius => _radius;

        public float ContactDamage => _contactDamage;

        public float ContactInterval => _contactInterval;

        protected override void ValidateDefinition(ValidationReport report)
        {
            DataChecks.DisplayName(report, this, _displayName);
            EffectChecks.CheckEnum(report, name, this, nameof(EnemyDef), _kind);
            DataChecks.Positive(report, this, nameof(_maxHp), _maxHp);
            DataChecks.NotNegative(report, this, nameof(_moveSpeed), _moveSpeed);
            DataChecks.Positive(report, this, nameof(_radius), _radius);
            DataChecks.NotNegative(report, this, nameof(_contactDamage), _contactDamage);
            DataChecks.Positive(report, this, nameof(_contactInterval), _contactInterval);
        }
    }
}
