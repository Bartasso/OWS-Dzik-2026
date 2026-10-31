using System;
using Game.Core;
using Game.Data;
using Game.Gameplay.Combat;
using Game.Gameplay.Commands;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>Fires the active weapon while the fire button is held.</summary>
    public sealed class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;

        private RunContext _context;
        private PlayerAim _aim;
        private ProjectileSystem _projectiles;
        private bool _fireHeld;
        private float _cooldown;

        public WeaponDef Weapon => _context.Character.StartingWeapon;

        /// <summary>Seconds between shots after AttackSpeed.</summary>
        public float FireInterval => Weapon.FireInterval / _context.Stats.Get(StatId.AttackSpeed);

        public void Init(RunContext context, PlayerAim aim, ProjectileSystem projectiles)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _aim = aim != null ? aim : throw new ArgumentNullException(nameof(aim));
            _projectiles = projectiles != null ? projectiles : throw new ArgumentNullException(nameof(projectiles));
            enabled = true;
        }

        public void Apply(in FireCommand command)
        {
            _fireHeld = command.IsHeld;
        }

        private void Awake()
        {
            if (_muzzle == null)
            {
                throw new InvalidOperationException($"{name}: missing muzzle Transform reference");
            }
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
            }

            if (_fireHeld && _cooldown <= 0f)
            {
                _projectiles.Launch(Weapon, _muzzle.position, _aim.Forward);
                _cooldown = FireInterval;
            }
        }
    }
}
