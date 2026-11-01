using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Movement parameters shared by every character (GDD section 3). Run speed itself is the
    /// MoveSpeed stat, so trinkets and abilities can change it.
    /// </summary>
    [Serializable]
    public sealed class MovementTuning
    {
        [Tooltip("m/s² towards the wished velocity while grounded and steering.")]
        [SerializeField] private float _groundAcceleration;

        [Tooltip("m/s² towards standstill while grounded without input.")]
        [SerializeField] private float _groundDeceleration;

        [Tooltip("m/s² of steering in the air. There is no air braking without input.")]
        [SerializeField] private float _airAcceleration;

        [Tooltip("m/s² downwards.")]
        [SerializeField] private float _gravity;

        [Tooltip("Terminal falling speed in m/s.")]
        [SerializeField] private float _maxFallSpeed;

        [Tooltip("Apex height of a jump from standstill, in meters.")]
        [SerializeField] private float _jumpHeight;

        [Tooltip("Seconds after leaving the ground during which a ground jump still works.")]
        [SerializeField] private float _coyoteTime;

        [Tooltip("Seconds a jump press is remembered before landing.")]
        [SerializeField] private float _jumpBufferTime;

        [Tooltip("Steepest walkable slope in degrees; steeper ground counts as air.")]
        [SerializeField] private float _maxWalkAngle;

        [Tooltip("Tallest step climbed without jumping, in meters.")]
        [SerializeField] private float _stepHeight;

        [Tooltip("Collision skin of the character capsule, in meters.")]
        [SerializeField] private float _skinWidth;

        public MovementTuning()
        {
        }

        public MovementTuning(
            float groundAcceleration,
            float groundDeceleration,
            float airAcceleration,
            float gravity,
            float maxFallSpeed,
            float jumpHeight,
            float coyoteTime,
            float jumpBufferTime,
            float maxWalkAngle,
            float stepHeight,
            float skinWidth)
        {
            _groundAcceleration = groundAcceleration;
            _groundDeceleration = groundDeceleration;
            _airAcceleration = airAcceleration;
            _gravity = gravity;
            _maxFallSpeed = maxFallSpeed;
            _jumpHeight = jumpHeight;
            _coyoteTime = coyoteTime;
            _jumpBufferTime = jumpBufferTime;
            _maxWalkAngle = maxWalkAngle;
            _stepHeight = stepHeight;
            _skinWidth = skinWidth;
        }

        public float GroundAcceleration => _groundAcceleration;

        public float GroundDeceleration => _groundDeceleration;

        public float AirAcceleration => _airAcceleration;

        public float Gravity => _gravity;

        public float MaxFallSpeed => _maxFallSpeed;

        public float JumpHeight => _jumpHeight;

        public float CoyoteTime => _coyoteTime;

        public float JumpBufferTime => _jumpBufferTime;

        public float MaxWalkAngle => _maxWalkAngle;

        public float StepHeight => _stepHeight;

        public float SkinWidth => _skinWidth;

        /// <summary>Launch speed that reaches <see cref="JumpHeight"/> under <see cref="Gravity"/>.</summary>
        public float JumpSpeed => Mathf.Sqrt(2f * _gravity * _jumpHeight);

        public void Validate(ValidationReport report, UnityEngine.Object owner)
        {
            DataChecks.Positive(report, owner, nameof(_groundAcceleration), _groundAcceleration);
            DataChecks.Positive(report, owner, nameof(_groundDeceleration), _groundDeceleration);
            DataChecks.NotNegative(report, owner, nameof(_airAcceleration), _airAcceleration);
            DataChecks.Positive(report, owner, nameof(_gravity), _gravity);
            DataChecks.Positive(report, owner, nameof(_maxFallSpeed), _maxFallSpeed);
            DataChecks.Positive(report, owner, nameof(_jumpHeight), _jumpHeight);
            DataChecks.NotNegative(report, owner, nameof(_coyoteTime), _coyoteTime);
            DataChecks.NotNegative(report, owner, nameof(_jumpBufferTime), _jumpBufferTime);
            DataChecks.NotNegative(report, owner, nameof(_stepHeight), _stepHeight);
            DataChecks.Positive(report, owner, nameof(_skinWidth), _skinWidth);

            if (!(_maxWalkAngle > 0f && _maxWalkAngle < 90f))
            {
                report.AddError(owner.name, $"{nameof(_maxWalkAngle)} must be in (0, 90), is {_maxWalkAngle}", owner);
            }
        }
    }
}
