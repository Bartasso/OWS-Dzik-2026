using System;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>
    /// Slide while running: keeps speed with low friction, gains it downhill and loses it uphill, steers
    /// without changing speed. Stays crouched while there is no room to stand.
    /// </summary>
    public sealed class SlideState
    {
        private readonly SlideTuning _tuning;

        private float _timeSinceStart;

        public SlideState(SlideTuning tuning)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        public bool Active { get; private set; }

        /// <summary>Starts or ends the slide for this tick from the input and the current speed.</summary>
        public void Update(bool held, bool grounded, bool canStand, float horizontalSpeed, float deltaTime)
        {
            if (Active)
            {
                _timeSinceStart += deltaTime;
                bool wantsToStop = !held || horizontalSpeed < _tuning.StopSpeed;
                if (!grounded || (wantsToStop && canStand))
                {
                    Active = false;
                }

                return;
            }

            if (held && grounded && horizontalSpeed >= _tuning.StartSpeed)
            {
                Active = true;
                _timeSinceStart = 0f;
            }
        }

        /// <summary>Horizontal velocity after one tick of sliding.</summary>
        /// <param name="groundNormal">Unit normal of the ground under the character.</param>
        /// <param name="gravity">Gravity in m/s², positive.</param>
        public Vector3 Slide(Vector3 horizontal, Vector3 wish, Vector3 groundNormal, float gravity, float overspeedDeceleration, float deltaTime)
        {
            // Downhill direction scaled by sin(slope); its horizontal part speeds up or slows down the slide.
            Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
            downhill.y = 0f;
            horizontal += downhill * (gravity * _tuning.SlopeFactor * deltaTime);

            float speed = horizontal.magnitude;
            float braking = _tuning.Friction + (speed > _tuning.SoftSpeedCap ? overspeedDeceleration : 0f);
            float newSpeed = Mathf.Max(0f, speed - braking * deltaTime);

            Vector3 direction = speed > 0f ? horizontal / speed : Vector3.zero;
            if (wish.sqrMagnitude <= 0f)
            {
                return direction * newSpeed;
            }

            if (newSpeed < _tuning.StopSpeed)
            {
                // Too slow to slide but kept crouched by a low ceiling: crawl out at the stop speed.
                return Vector3.MoveTowards(direction * newSpeed, wish * _tuning.StopSpeed, (_tuning.Friction + _tuning.Steering) * deltaTime);
            }

            Vector3 steered = direction * newSpeed + wish * (_tuning.Steering * deltaTime);
            return (steered.sqrMagnitude > 0f ? steered.normalized : direction) * newSpeed;
        }

        /// <summary>
        /// Speed added by a jump out of this slide: only right after it started, shrinking to nothing at
        /// the soft speed cap, so well-timed chains build speed but level off.
        /// </summary>
        public float JumpBoost(float horizontalSpeed)
        {
            if (!Active || _timeSinceStart > _tuning.JumpBoostWindow)
            {
                return 0f;
            }

            return _tuning.JumpBoost * Mathf.Clamp01(1f - horizontalSpeed / _tuning.SoftSpeedCap);
        }

        public void End()
        {
            Active = false;
        }
    }
}
