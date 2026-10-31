using System;
using Game.Core;
using Game.Data;
using Game.Gameplay.Player;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    /// <summary>Spawns regular enemies on a ring around the player, and a boss every N kills.</summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        private RunContext _context;
        private EnemyRegistry _registry;
        private PrefabPool<Enemy> _pool;
        private EnemySpawnSettings _settings;
        private PlayerHealth _player;
        private float _timer;
        private int _regularAlive;
        private int _killsSinceBoss;

        public int Kills { get; private set; }

        public int Alive => _registry.Count;

        public void Init(RunContext context, EnemyRegistry registry, PrefabPool<Enemy> pool, EnemySpawnSettings settings, PlayerHealth player)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _player = player != null ? player : throw new ArgumentNullException(nameof(player));
            enabled = true;
        }

        public void Spawn(EnemyDef definition)
        {
            if (!_pool.TryGet(out Enemy enemy))
            {
                return;
            }

            enemy.Spawn(definition, RandomPositionAroundPlayer(), _context.Pause, _player);
            _registry.Add(enemy);
            if (definition.Kind != EnemyKind.Boss)
            {
                _regularAlive++;
            }
        }

        public void SpawnBoss()
        {
            Spawn(_settings.Boss);
        }

        public void OnKilled(Enemy enemy)
        {
            Kills++;
            bool wasBoss = enemy.Definition.Kind == EnemyKind.Boss;
            Despawn(enemy);

            if (wasBoss || _settings.BossEveryKills <= 0)
            {
                return;
            }

            _killsSinceBoss++;
            if (_killsSinceBoss >= _settings.BossEveryKills)
            {
                _killsSinceBoss = 0;
                SpawnBoss();
            }
        }

        private void Despawn(Enemy enemy)
        {
            if (enemy.Definition.Kind != EnemyKind.Boss)
            {
                _regularAlive--;
            }

            _registry.Remove(enemy);
            _pool.Release(enemy);
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            _timer = _settings.SpawnInterval;
            if (_regularAlive < _settings.MaxAlive)
            {
                Spawn(_settings.RegularEnemy);
            }
        }

        private Vector3 RandomPositionAroundPlayer()
        {
            RngStream spawns = _context.Rng.Spawns;
            float angle = spawns.NextFloat() * Mathf.PI * 2f;
            float distance = Mathf.Lerp(_settings.SpawnRadiusMin, _settings.SpawnRadiusMax, spawns.NextFloat());
            Vector3 center = _player.transform.position;
            return new Vector3(center.x + Mathf.Cos(angle) * distance, 0f, center.z + Mathf.Sin(angle) * distance);
        }
    }
}
