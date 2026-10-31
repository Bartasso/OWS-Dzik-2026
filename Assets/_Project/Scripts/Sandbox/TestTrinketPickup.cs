using System;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using UnityEngine;

namespace Game.Sandbox
{
    /// <summary>Walk into it to get one copy of a trinket; it comes back after a delay so stacks can be tested.</summary>
    public sealed class TestTrinketPickup : MonoBehaviour
    {
        [SerializeField] private GameBootstrap _bootstrap;
        [SerializeField] private SandboxSettings _settings;
        [SerializeField] private TrinketDef _trinket;
        [SerializeField] private Transform _player;
        [SerializeField] private GameObject _visual;

        private RunContext _context;
        private float _respawnTimer;

        public TrinketDef Trinket => _trinket;

        private void Awake()
        {
            if (_bootstrap == null || _settings == null || _trinket == null || _player == null || _visual == null)
            {
                throw new InvalidOperationException($"{name}: missing GameBootstrap, SandboxSettings, TrinketDef, player or visual reference");
            }
        }

        private void Start()
        {
            _context = _bootstrap.Context ?? throw new InvalidOperationException($"{name}: bootstrap has no run context");
            var report = new ValidationReport();
            _trinket.Validate(report);
            if (report.HasErrors)
            {
                ValidationLog.LogErrors(report);
                throw new InvalidOperationException($"{name}: trinket {_trinket.name} is invalid, see the console");
            }
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            if (!_visual.activeSelf)
            {
                _respawnTimer -= Time.deltaTime;
                if (_respawnTimer <= 0f)
                {
                    _visual.SetActive(true);
                }

                return;
            }

            Vector3 offset = _player.position - transform.position;
            offset.y = 0f;
            float radius = _settings.PickupRadius;
            if (offset.sqrMagnitude <= radius * radius)
            {
                _context.Loadout.Add(_trinket);
                _visual.SetActive(false);
                _respawnTimer = _settings.PickupRespawnSeconds;
            }
        }
    }
}
