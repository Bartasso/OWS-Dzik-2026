using System;
using Game.Core;
using Game.Gameplay.Commands;
using Game.Gameplay.Movement;
using nickmaltbie.OpenKCC.Character;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>
    /// Turns the player's commands into a locomotion tick in FixedUpdate. Run speed and extra jumps
    /// come from the stats, so trinkets change them through modifiers.
    /// </summary>
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private KCCMovementEngine _engine;

        private RunContext _context;
        private PlayerAim _aim;
        private CharacterLocomotion _locomotion;
        private Vector2 _input;
        private bool _jumpQueued;

        public LocomotionSimulator Locomotion => _locomotion.Simulator;

        public void Init(RunContext context, PlayerAim aim)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _aim = aim != null ? aim : throw new ArgumentNullException(nameof(aim));
            _locomotion = new CharacterLocomotion(_engine, context.Balance.Movement);
            enabled = true;
        }

        public void Apply(in MoveCommand command)
        {
            _input = Vector2.ClampMagnitude(command.Direction, 1f);
        }

        /// <summary>Latched until the next tick, so a press between two ticks is not lost.</summary>
        public void Apply(in JumpCommand command)
        {
            _jumpQueued = true;
        }

        private void Awake()
        {
            if (_engine == null)
            {
                throw new InvalidOperationException($"{name}: missing KCCMovementEngine reference");
            }
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            // The capsule is symmetric, so facing can follow the aim every frame without affecting collisions.
            transform.rotation = _aim.YawRotation;
        }

        private void FixedUpdate()
        {
            if (_context.Pause.IsPaused)
            {
                _jumpQueued = false;
                return;
            }

            Vector3 wish = _aim.YawRotation * new Vector3(_input.x, 0f, _input.y);
            _locomotion.Tick(
                wish,
                _jumpQueued,
                _context.Stats.Get(StatId.MoveSpeed),
                _context.Stats.GetInt(StatId.ExtraJumps),
                Time.deltaTime);
            _jumpQueued = false;
        }
    }
}
