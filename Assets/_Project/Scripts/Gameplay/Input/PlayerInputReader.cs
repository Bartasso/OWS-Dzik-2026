using System;
using Game.Gameplay.Commands;
using Game.Gameplay.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Input
{
    /// <summary>
    /// The only place that reads the Input System: turns devices into commands.
    /// Look and fire work only while the cursor is locked, so debug UI can be clicked.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        private const string MapName = "Player";

        [SerializeField] private InputActionAsset _actions;

        [Tooltip("Degrees per pixel of mouse movement.")]
        [SerializeField] private float _lookSensitivity = 0.15f;

        private InputAction _move;
        private InputAction _look;
        private InputAction _fire;
        private InputAction _interact;
        private InputAction _jump;
        private InputAction _dash;
        private InputAction _slide;
        private PlayerAim _aim;
        private PlayerMotor _motor;
        private PlayerShooter _shooter;
        private Interactor _interactor;

        public void Init(PlayerAim aim, PlayerMotor motor, PlayerShooter shooter, Interactor interactor)
        {
            _aim = aim != null ? aim : throw new ArgumentNullException(nameof(aim));
            _motor = motor != null ? motor : throw new ArgumentNullException(nameof(motor));
            _shooter = shooter != null ? shooter : throw new ArgumentNullException(nameof(shooter));
            _interactor = interactor != null ? interactor : throw new ArgumentNullException(nameof(interactor));

            if (_actions == null)
            {
                throw new InvalidOperationException($"{name}: missing InputActionAsset reference");
            }

            InputActionMap map = _actions.FindActionMap(MapName, throwIfNotFound: true);
            _move = map.FindAction("Move", throwIfNotFound: true);
            _look = map.FindAction("Look", throwIfNotFound: true);
            _fire = map.FindAction("Fire", throwIfNotFound: true);
            _interact = map.FindAction("Interact", throwIfNotFound: true);
            _jump = map.FindAction("Jump", throwIfNotFound: true);
            _dash = map.FindAction("Dash", throwIfNotFound: true);
            _slide = map.FindAction("Slide", throwIfNotFound: true);
            map.Enable();
            enabled = true;
        }

        private void OnDestroy()
        {
            if (_move != null)
            {
                _move.actionMap.Disable();
            }
        }

        private void Update()
        {
            bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;

            _motor.Apply(new MoveCommand(_move.ReadValue<Vector2>()));

            if (_jump.WasPressedThisFrame())
            {
                _motor.Apply(new JumpCommand());
            }

            if (_dash.WasPressedThisFrame())
            {
                _motor.Apply(new DashCommand());
            }

            _motor.Apply(new SlideCommand(_slide.IsPressed()));

            if (cursorLocked)
            {
                _aim.Apply(new AimCommand(_look.ReadValue<Vector2>() * _lookSensitivity));
            }

            _shooter.Apply(new FireCommand(cursorLocked && _fire.IsPressed()));

            if (_interact.WasPressedThisFrame())
            {
                _interactor.Apply(new InteractCommand());
            }
        }
    }
}
