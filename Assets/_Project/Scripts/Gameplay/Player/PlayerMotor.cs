using System;
using Game.Core;
using Game.Gameplay.Commands;
using UnityEngine;

namespace Game.Gameplay.Player
{
    /// <summary>Moves the player from MoveCommands. Speed comes from the MoveSpeed stat.</summary>
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private CharacterController _controller;

        private RunContext _context;
        private PlayerAim _aim;
        private Vector2 _input;
        private float _verticalSpeed;

        public void Init(RunContext context, PlayerAim aim)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _aim = aim != null ? aim : throw new ArgumentNullException(nameof(aim));
            enabled = true;
        }

        public void Apply(in MoveCommand command)
        {
            _input = Vector2.ClampMagnitude(command.Direction, 1f);
        }

        private void Awake()
        {
            if (_controller == null)
            {
                throw new InvalidOperationException($"{name}: missing CharacterController reference");
            }
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            Quaternion facing = _aim.YawRotation;
            transform.rotation = facing;

            if (_controller.isGrounded && _verticalSpeed < 0f)
            {
                _verticalSpeed = 0f;
            }

            _verticalSpeed += Physics.gravity.y * Time.deltaTime;

            Vector3 velocity = facing * new Vector3(_input.x, 0f, _input.y) * _context.Stats.Get(StatId.MoveSpeed);
            velocity.y = _verticalSpeed;
            _controller.Move(velocity * Time.deltaTime);
        }
    }
}
