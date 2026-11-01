using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Gameplay.Combat;
using Game.Gameplay.Enemies;
using Game.Gameplay.Input;
using Game.Gameplay.Player;
using Game.Gameplay.SkillChecks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Gameplay
{
    /// <summary>
    /// Composition root of a run: validates data, creates services and passes them to scene objects.
    /// Scene components start disabled and are enabled by their Init, so nothing runs with broken data.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private GameBalance _balance;
        [SerializeField] private CharacterDef _character;
        [SerializeField] private EnemySpawnSettings _spawnSettings;

        [Header("Run")]
        [SerializeField] private bool _useRandomSeed = true;
        [SerializeField] private long _seed;

        [Header("Player")]
        [SerializeField] private PlayerAim _playerAim;
        [SerializeField] private PlayerMotor _playerMotor;
        [SerializeField] private PlayerShooter _playerShooter;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Interactor _interactor;
        [SerializeField] private PlayerInputReader _inputReader;

        [Header("Systems")]
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private ProjectileSystem _projectileSystem;

        [Header("Pools")]
        [SerializeField] private Transform _projectilePrefab;
        [SerializeField] private int _projectilePoolSize;
        [SerializeField] private Enemy _enemyPrefab;

        [Tooltip("Must exceed the spawn settings' MaxAlive to leave room for bosses.")]
        [SerializeField] private int _enemyPoolSize;

        [Header("Map")]
        [SerializeField] private SkillCheckSite[] _skillCheckSites = new SkillCheckSite[0];

        public RunContext Context { get; private set; }

        private void Awake()
        {
            CheckReferences();
            ValidateData();

            ulong seed = _useRandomSeed ? unchecked((ulong)DateTime.UtcNow.Ticks) : unchecked((ulong)_seed);
            _seed = unchecked((long)seed);
            var rng = new Rng(seed);
            StatSheet stats = CharacterStats.Build(_character, _balance);
            var scaling = new ScalingTable();
            var loadout = new EffectLoadout(stats, scaling, rng.Combat, _balance.ProcChancePerLuck);
            var skillCheck = new SkillCheck(_balance.ToSkillCheckRules(), rng.SkillChecks);
            Context = new RunContext(_balance, _character, rng, stats, scaling, loadout, skillCheck, new GamePause(), new CombatEvents());

            Transform poolRoot = new GameObject("Pools").transform;
            var enemies = new EnemyRegistry();
            var enemyPool = new PrefabPool<Enemy>(_enemyPrefab, _enemyPoolSize, poolRoot);
            var projectilePool = new PrefabPool<Transform>(_projectilePrefab, _projectilePoolSize, poolRoot);

            _playerAim.Init();
            _playerHealth.Init(Context);
            _playerMotor.Init(Context, _playerAim, _playerHealth);
            _enemySpawner.Init(Context, enemies, enemyPool, _spawnSettings, _playerHealth);
            _projectileSystem.Init(Context, projectilePool, enemies, new HitResolver(Context, _enemySpawner));
            _playerShooter.Init(Context, _playerAim, _projectileSystem);

            foreach (SkillCheckSite site in _skillCheckSites)
            {
                site.Init(Context);
            }

            _interactor.Init(Context, _skillCheckSites);
            _inputReader.Init(_playerAim, _playerMotor, _playerShooter, _interactor);
        }

        private void CheckReferences()
        {
            Require(_balance, nameof(_balance));
            Require(_character, nameof(_character));
            Require(_spawnSettings, nameof(_spawnSettings));
            Require(_playerAim, nameof(_playerAim));
            Require(_playerMotor, nameof(_playerMotor));
            Require(_playerShooter, nameof(_playerShooter));
            Require(_playerHealth, nameof(_playerHealth));
            Require(_interactor, nameof(_interactor));
            Require(_inputReader, nameof(_inputReader));
            Require(_enemySpawner, nameof(_enemySpawner));
            Require(_projectileSystem, nameof(_projectileSystem));
            Require(_projectilePrefab, nameof(_projectilePrefab));
            Require(_enemyPrefab, nameof(_enemyPrefab));

            for (int i = 0; i < _skillCheckSites.Length; i++)
            {
                Require(_skillCheckSites[i], $"{nameof(_skillCheckSites)}[{i}]");
            }

            if (_projectilePoolSize <= 0)
            {
                throw new InvalidOperationException($"{name}: {nameof(_projectilePoolSize)} must be > 0");
            }

            if (_enemyPoolSize <= _spawnSettings.MaxAlive)
            {
                throw new InvalidOperationException(
                    $"{name}: {nameof(_enemyPoolSize)} ({_enemyPoolSize}) must exceed {_spawnSettings.name}.MaxAlive ({_spawnSettings.MaxAlive})");
            }
        }

        private void Require(Object reference, string field)
        {
            if (reference == null)
            {
                throw new InvalidOperationException($"{name}: missing {field} reference");
            }
        }

        private void ValidateData()
        {
            var report = new ValidationReport();
            var definitions = new List<Definition>();

            Validate(report, definitions, _balance);
            Validate(report, definitions, _character);
            Validate(report, definitions, _character.StartingWeapon);
            foreach (SkillDef skill in _character.Proficiencies)
            {
                Validate(report, definitions, skill);
            }

            Validate(report, definitions, _spawnSettings);
            Validate(report, definitions, _spawnSettings.RegularEnemy);
            Validate(report, definitions, _spawnSettings.Boss);

            foreach (SkillCheckSite site in _skillCheckSites)
            {
                SkillCheckSiteDef definition = site.Definition;
                if (definition == null)
                {
                    report.AddError(site.name, "missing SkillCheckSiteDef reference", site);
                    continue;
                }

                Validate(report, definitions, definition);
                Validate(report, definitions, definition.Skill);
                Validate(report, definitions, definition.Reward);
            }

            DefinitionIdChecks.CheckUnique(definitions, report);

            if (!report.HasErrors)
            {
                return;
            }

            ValidationLog.LogErrors(report);
            string message = $"{name}: data validation failed with {report.Errors.Count} error(s), see the console";
            // Editor and development builds refuse to start; release logs and carries on (open question).
            if (Debug.isDebugBuild)
            {
                throw new InvalidOperationException(message);
            }

            Debug.LogError(message);
        }

        private static void Validate(ValidationReport report, List<Definition> definitions, IValidatable validatable)
        {
            // Missing references are reported by the owner's own validation.
            if (validatable == null || (validatable is Object unityObject && unityObject == null))
            {
                return;
            }

            if (validatable is Definition definition)
            {
                if (definitions.Contains(definition))
                {
                    return;
                }

                definitions.Add(definition);
            }

            validatable.Validate(report);
        }
    }
}
