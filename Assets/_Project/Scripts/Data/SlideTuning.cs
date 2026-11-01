using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Data
{
    /// <summary>
    /// Slide and slide-jump parameters (GDD section 3). The slide starts at any speed, even standing,
    /// and lasts while the key is held.
    /// </summary>
    [Serializable]
    public sealed class SlideTuning
    {
        [Tooltip("Speed in m/s of crawling out from under a low ceiling after the slide key is released.")]
        [FormerlySerializedAs("_stopSpeed")]
        [SerializeField] private float _crawlSpeed;

        [Tooltip("m/s² of speed lost while sliding on flat ground.")]
        [SerializeField] private float _friction;

        [Tooltip("Share of gravity along the slope that speeds up or slows down a slide.")]
        [SerializeField] private float _slopeFactor;

        [Tooltip("m/s² of sideways steering while sliding; turns without changing speed.")]
        [SerializeField] private float _steering;

        [Tooltip("Capsule height while sliding, in meters.")]
        [SerializeField] private float _height;

        [Tooltip("Speed in m/s added by a jump made right after the slide started.")]
        [SerializeField] private float _jumpBoost;

        [Tooltip("Seconds after the slide starts during which a jump gets the boost.")]
        [SerializeField] private float _jumpBoostWindow;

        [Tooltip("Soft speed limit in m/s: boosts shrink towards it, sliding above it brakes harder.")]
        [SerializeField] private float _softSpeedCap;

        public SlideTuning()
        {
        }

        public SlideTuning(
            float crawlSpeed,
            float friction,
            float slopeFactor,
            float steering,
            float height,
            float jumpBoost,
            float jumpBoostWindow,
            float softSpeedCap)
        {
            _crawlSpeed = crawlSpeed;
            _friction = friction;
            _slopeFactor = slopeFactor;
            _steering = steering;
            _height = height;
            _jumpBoost = jumpBoost;
            _jumpBoostWindow = jumpBoostWindow;
            _softSpeedCap = softSpeedCap;
        }

        public float CrawlSpeed => _crawlSpeed;

        public float Friction => _friction;

        public float SlopeFactor => _slopeFactor;

        public float Steering => _steering;

        public float Height => _height;

        public float JumpBoost => _jumpBoost;

        public float JumpBoostWindow => _jumpBoostWindow;

        public float SoftSpeedCap => _softSpeedCap;

        public void Validate(ValidationReport report, UnityEngine.Object owner)
        {
            DataChecks.Positive(report, owner, "slide." + nameof(_crawlSpeed), _crawlSpeed);
            DataChecks.NotNegative(report, owner, "slide." + nameof(_friction), _friction);
            DataChecks.NotNegative(report, owner, "slide." + nameof(_slopeFactor), _slopeFactor);
            DataChecks.NotNegative(report, owner, "slide." + nameof(_steering), _steering);
            DataChecks.Positive(report, owner, "slide." + nameof(_height), _height);
            DataChecks.NotNegative(report, owner, "slide." + nameof(_jumpBoost), _jumpBoost);
            DataChecks.NotNegative(report, owner, "slide." + nameof(_jumpBoostWindow), _jumpBoostWindow);
            DataChecks.Positive(report, owner, "slide." + nameof(_softSpeedCap), _softSpeedCap);
        }
    }
}
