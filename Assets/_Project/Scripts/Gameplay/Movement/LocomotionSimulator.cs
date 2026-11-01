using System;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>
    /// Velocity of a character from commands, run once per fixed tick: momentum-based running, gravity,
    /// jumps with coyote time and input buffer, limited air control, dash, slide and slide-jump.
    /// Knows nothing about collisions: the caller moves the body and reports back what the world blocked.
    /// </summary>
    public sealed class LocomotionSimulator
    {
        // Below this fraction of the intended move the world is considered to have blocked it.
        private const float BlockedMoveFraction = 0.99f;
        private const float BlockedRiseFraction = 0.5f;

        private readonly MovementTuning _tuning;
        private readonly DashState _dash;
        private readonly SlideState _slide;

        private Vector3 _horizontal;
        private float _vertical;
        private float _timeSinceGrounded = float.PositiveInfinity;
        private float _jumpBufferLeft;
        private bool _groundJumpUsed;
        private int _airJumpsUsed;

        public LocomotionSimulator(MovementTuning tuning)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
            _dash = new DashState(tuning.Dash);
            _slide = new SlideState(tuning.Slide);
        }

        public Vector3 HorizontalVelocity => _horizontal;

        public float VerticalVelocity => _vertical;

        public Vector3 Velocity => new Vector3(_horizontal.x, _vertical, _horizontal.z);

        /// <summary>A jump (ground or air) started during the last tick.</summary>
        public bool JumpedThisTick { get; private set; }

        public int AirJumpsUsed => _airJumpsUsed;

        public DashState Dash => _dash;

        public bool Sliding => _slide.Active;

        public void Tick(in LocomotionInput input, float deltaTime)
        {
            if (!(deltaTime > 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime must be positive");
            }

            // Rising from a jump while the ground probe still touches the floor is not standing on it.
            bool grounded = input.Grounded && _vertical <= 0f;
            UpdateGroundTimers(grounded, deltaTime);
            _dash.Recharge(input.ExtraDashCharges, deltaTime);

            Vector3 wish = Flat(input.WishDirection);
            wish = Vector3.ClampMagnitude(wish, 1f);

            UpdateJump(input, deltaTime);
            if (JumpedThisTick)
            {
                StartJump();
            }
            else if (input.DashPressed && input.CanStand)
            {
                Vector3 direction = wish.sqrMagnitude > 0f ? wish.normalized : Flat(input.Facing).normalized;
                if (_dash.TryStart(direction, grounded, _horizontal.magnitude, input.RunSpeed))
                {
                    _slide.End();
                }
            }

            if (!_dash.Active)
            {
                _slide.Update(input.SlideHeld, grounded && !JumpedThisTick, input.CanStand, deltaTime);
            }

            UpdateVertical(grounded, deltaTime);
            UpdateHorizontal(input, wish, grounded, deltaTime);
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

            Vector3 flat = Flat(resolvedMove);
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
            _dash.Reset();
            _slide.End();
        }

        private static Vector3 Flat(Vector3 vector)
        {
            return new Vector3(vector.x, 0f, vector.z);
        }

        private void UpdateGroundTimers(bool grounded, float deltaTime)
        {
            if (grounded)
            {
                _timeSinceGrounded = 0f;
                _groundJumpUsed = false;
                _airJumpsUsed = 0;
                _dash.RestoreAirDash();
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

            // Under a low ceiling the jump waits in the buffer until there is room to stand.
            if (input.CanStand)
            {
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
            }

            _jumpBufferLeft = JumpedThisTick ? 0f : Mathf.Max(0f, _jumpBufferLeft - deltaTime);
        }

        private void StartJump()
        {
            _dash.RestoreAirDash();
            if (_dash.Active)
            {
                _dash.Cancel();
                _horizontal = _dash.ExitVelocity;
            }

            float boost = _slide.JumpBoost(_horizontal.magnitude);
            if (boost > 0f && _horizontal.sqrMagnitude > 0f)
            {
                _horizontal += _horizontal.normalized * boost;
            }

            _slide.End();
        }

        private void UpdateVertical(bool grounded, float deltaTime)
        {
            if (JumpedThisTick)
            {
                _vertical = _tuning.JumpSpeed;
            }
            else if (_dash.Active || grounded)
            {
                _vertical = 0f;
            }
            else
            {
                _vertical = Mathf.Max(_vertical - _tuning.Gravity * deltaTime, -_tuning.MaxFallSpeed);
            }
        }

        private void UpdateHorizontal(in LocomotionInput input, Vector3 wish, bool grounded, float deltaTime)
        {
            if (_dash.Active)
            {
                bool ended = _dash.Advance(deltaTime);
                _horizontal = ended ? _dash.ExitVelocity : _dash.Velocity;
            }
            else if (_slide.Active)
            {
                _horizontal = _slide.Slide(_horizontal, wish, input.GroundNormal, _tuning.Gravity, _tuning.OverspeedDeceleration, deltaTime);
            }
            else if (grounded)
            {
                float rate;
                if (wish.sqrMagnitude <= 0f)
                {
                    rate = _tuning.GroundDeceleration;
                }
                else if (_horizontal.magnitude > input.RunSpeed)
                {
                    // Momentum from dashes and slide-jumps bleeds off slowly instead of snapping to run speed.
                    rate = _tuning.OverspeedDeceleration;
                }
                else
                {
                    rate = _tuning.GroundAcceleration;
                }

                _horizontal = Vector3.MoveTowards(_horizontal, wish * input.RunSpeed, rate * deltaTime);
            }
            else if (wish.sqrMagnitude > 0f)
            {
                // Steering only: momentum above run speed is kept, but steering cannot add more.
                float cap = Mathf.Max(_horizontal.magnitude, input.RunSpeed);
                _horizontal = Vector3.ClampMagnitude(_horizontal + wish * (_tuning.AirAcceleration * deltaTime), cap);
            }
        }
    }
}
