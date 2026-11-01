using System;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>
    /// Velocity of a character from commands, run once per fixed tick: momentum-based running, gravity,
    /// jumps with coyote time and input buffer, limited air control. Knows nothing about collisions:
    /// the caller moves the body and reports back what the world blocked.
    /// </summary>
    public sealed class LocomotionSimulator
    {
        // Below this fraction of the intended move the world is considered to have blocked it.
        private const float BlockedMoveFraction = 0.99f;
        private const float BlockedRiseFraction = 0.5f;

        private readonly MovementTuning _tuning;

        private Vector3 _horizontal;
        private float _vertical;
        private float _timeSinceGrounded = float.PositiveInfinity;
        private float _jumpBufferLeft;
        private bool _groundJumpUsed;
        private int _airJumpsUsed;

        public LocomotionSimulator(MovementTuning tuning)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        public Vector3 HorizontalVelocity => _horizontal;

        public float VerticalVelocity => _vertical;

        public Vector3 Velocity => new Vector3(_horizontal.x, _vertical, _horizontal.z);

        /// <summary>A jump (ground or air) started during the last tick.</summary>
        public bool JumpedThisTick { get; private set; }

        public int AirJumpsUsed => _airJumpsUsed;

        public void Tick(in LocomotionInput input, float deltaTime)
        {
            if (!(deltaTime > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime must be positive");
            }

            // Rising from a jump while the ground probe still touches the floor is not standing on it.
            bool grounded = input.Grounded && _vertical <= 0f;
            UpdateGroundTimers(grounded, deltaTime);
            UpdateJump(input, deltaTime);

            if (JumpedThisTick)
            {
                _vertical = _tuning.JumpSpeed;
            }
            else if (grounded)
            {
                _vertical = 0f;
            }
            else
            {
                _vertical = Mathf.Max(_vertical - _tuning.Gravity * deltaTime, -_tuning.MaxFallSpeed);
            }

            Vector3 wish = input.WishDirection;
            wish.y = 0f;
            wish = Vector3.ClampMagnitude(wish, 1f);

            if (grounded)
            {
                float rate = wish.sqrMagnitude > 0f ? _tuning.GroundAcceleration : _tuning.GroundDeceleration;
                _horizontal = Vector3.MoveTowards(_horizontal, wish * input.RunSpeed, rate * deltaTime);
            }
            else if (wish.sqrMagnitude > 0f)
            {
                // Steering only: momentum above run speed is kept, but steering cannot add more.
                float cap = Mathf.Max(_horizontal.magnitude, input.RunSpeed);
                _horizontal = Vector3.ClampMagnitude(_horizontal + wish * (_tuning.AirAcceleration * deltaTime), cap);
            }
        }

        /// <summary>
        /// Reports how far the horizontal move of this tick actually got. Speed lost against walls is
        /// dropped, so pressing into a wall does not store momentum; sliding along it keeps its speed.
        /// </summary>
        public void ResolveHorizontal(Vector3 intendedMove, Vector3 resolvedMove, float deltaTime)
        {
            float intended = intendedMove.magnitude;
            float resolved = resolvedMove.magnitude;
            if (resolved >= intended * BlockedMoveFraction)
            {
                return;
            }

            Vector3 flat = new Vector3(resolvedMove.x, 0f, resolvedMove.z);
            _horizontal = flat.sqrMagnitude > 0f ? flat.normalized * (resolved / deltaTime) : Vector3.zero;
        }

        /// <summary>Reports how far the vertical move got; a blocked rise (ceiling) ends the jump.</summary>
        public void ResolveVertical(float intendedRise, float resolvedRise)
        {
            if (intendedRise > 0f && resolvedRise < intendedRise * BlockedRiseFraction)
            {
                _vertical = 0f;
            }
        }

        /// <summary>Forgets all motion, e.g. after a teleport.</summary>
        public void Reset()
        {
            _horizontal = Vector3.zero;
            _vertical = 0f;
            _timeSinceGrounded = float.PositiveInfinity;
            _jumpBufferLeft = 0f;
            _groundJumpUsed = false;
            _airJumpsUsed = 0;
            JumpedThisTick = false;
        }

        private void UpdateGroundTimers(bool grounded, float deltaTime)
        {
            if (grounded)
            {
                _timeSinceGrounded = 0f;
                _groundJumpUsed = false;
                _airJumpsUsed = 0;
            }
            else
            {
                _timeSinceGrounded += deltaTime;
            }
        }

        private void UpdateJump(in LocomotionInput input, float deltaTime)
        {
            JumpedThisTick = false;
            if (input.JumpPressed)
            {
                _jumpBufferLeft = _tuning.JumpBufferTime;
            }

            bool canGroundJump = !_groundJumpUsed && _timeSinceGrounded <= _tuning.CoyoteTime;
            if (_jumpBufferLeft > 0f && canGroundJump)
            {
                _groundJumpUsed = true;
                JumpedThisTick = true;
            }
            else if (input.JumpPressed && _airJumpsUsed < input.ExtraJumps)
            {
                // Air jumps need a fresh press: a buffered press before landing belongs to the ground jump.
                _airJumpsUsed++;
                JumpedThisTick = true;
            }

            _jumpBufferLeft = JumpedThisTick ? 0f : Mathf.Max(0f, _jumpBufferLeft - deltaTime);
        }
    }
}
