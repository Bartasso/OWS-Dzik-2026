using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>Wave settings for an arena. Read every frame, so tweaking in Play Mode works live.</summary>
    [CreateAssetMenu(menuName = "Game/Enemy Spawn Settings", fileName = "EnemySpawnSettings")]
    public sealed class EnemySpawnSettings : ScriptableObject, IValidatable
    {
        [SerializeField] private EnemyDef _regularEnemy;
        [SerializeField] private EnemyDef _boss;

        [Tooltip("Seconds between regular spawns.")]
        [SerializeField] private float _spawnInterval;

        [Tooltip("Maximum regular enemies alive at once.")]
        [SerializeField] private int _maxAlive;

        [Tooltip("Spawn ring around the player, in meters.")]
        [SerializeField] private float _spawnRadiusMin;
        [SerializeField] private float _spawnRadiusMax;

        [Tooltip("A boss appears after this many regular kills. 0 = never.")]
        [SerializeField] private int _bossEveryKills;

        public EnemyDef RegularEnemy => _regularEnemy;

        public EnemyDef Boss => _boss;

        public float SpawnInterval => _spawnInterval;

        public int MaxAlive => _maxAlive;

        public float SpawnRadiusMin => _spawnRadiusMin;

        public float SpawnRadiusMax => _spawnRadiusMax;

        public int BossEveryKills => _bossEveryKills;

        public void Validate(ValidationReport report)
        {
            DataChecks.Required(report, this, nameof(_regularEnemy), _regularEnemy);
            DataChecks.Required(report, this, nameof(_boss), _boss);
            DataChecks.Positive(report, this, nameof(_spawnInterval), _spawnInterval);
            DataChecks.NotNegative(report, this, nameof(_spawnRadiusMin), _spawnRadiusMin);

            if (_maxAlive < 0)
            {
                report.AddError(name, $"{nameof(_maxAlive)} must be >= 0, is {_maxAlive}", this);
            }

            if (_bossEveryKills < 0)
            {
                report.AddError(name, $"{nameof(_bossEveryKills)} must be >= 0, is {_bossEveryKills}", this);
            }

            if (_spawnRadiusMax < _spawnRadiusMin)
            {
                report.AddError(name, $"{nameof(_spawnRadiusMax)} ({_spawnRadiusMax}) is below {nameof(_spawnRadiusMin)} ({_spawnRadiusMin})", this);
            }

            if (_regularEnemy != null && _regularEnemy.Kind == EnemyKind.Boss)
            {
                report.AddError(name, $"{nameof(_regularEnemy)} {_regularEnemy.name} is a boss", this);
            }

            if (_boss != null && _boss.Kind != EnemyKind.Boss)
            {
                report.AddError(name, $"{nameof(_boss)} {_boss.name} is not of kind Boss", this);
            }
        }

        private void OnValidate()
        {
            ValidationLog.ValidateAndLog(this);
        }
    }
}
