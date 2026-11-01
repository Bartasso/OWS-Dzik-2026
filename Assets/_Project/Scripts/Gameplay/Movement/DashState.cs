using System;
using Game.Data;
using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>
    /// Dash charges, their recharge and the dash in progress. One air dash per airtime: landing and
    /// every jump give it back.
    /// </summary>
    public sealed class DashState
    {
        private readonly DashTuning _tuning;

        private bool _chargesInitialized;
        private int _charges;
        private float _rechargeLeft;
        private float _timeLeft;
        private bool _airDashUsed;
        private Vector3 _direction;
        private float _exitSpeed;

        public DashState(DashTuning tuning)
        {
            _tuning = tuning ?? throw new ArgumentNullException(nameof(tuning));
        }

        public bool Active => _timeLeft > 0f;

        public int Charges => _charges;

        public bool StartedThisTick { get; private set; }

        /// <summary>Horizontal velocity while the dash lasts.</summary>
        public Vector3 Velocity => _direction * _tuning.Speed;

        /// <summary>Horizontal velocity the dash leaves behind when it ends or is cancelled.</summary>
        public Vector3 ExitVelocity => _direction * _exitSpeed;

        /// <summary>Recharges and clamps charges to the current maximum; call once per tick first.</summary>
        public void Recharge(int extraCharges, float deltaTime)
        {
            StartedThisTick = false;
            int max = Mathf.Max(0, _tuning.BaseCharges + extraCharges);
            if (!_chargesInitialized)
            {
                _charges = max;
                _chargesInitialized = true;
            }

            if (_charges >= max)
            {
                _charges = max;
                _rechargeLeft = 0f;
                return;
            }

            if (_rechargeLeft <= 0f)
            {
                _rechargeLeft = _tuning.Cooldown;
            }

            _rechargeLeft -= deltaTime;
            if (_rechargeLeft <= 0f)
            {
                _charges++;
                _rechargeLeft = _charges < max ? _tuning.Cooldown + _rechargeLeft : 0f;
            }
        }

        /// <param name="direction">Horizontal unit direction of the dash.</param>
        /// <param name="entrySpeed">Horizontal speed before the dash.</param>
        /// <param name="runSpeed">Minimum speed kept after the dash.</param>
        public bool TryStart(Vector3 direction, bool grounded, float entrySpeed, float runSpeed)
        {
            if (Active || _charges <= 0 || (!grounded && _airDashUsed) || direction.sqrMagnitude < 0.5f)
            {
                return false;
            }

            _charges--;
            _airDashUsed |= !grounded;
            _timeLeft = _tuning.Duration;
            _direction = direction;
            _exitSpeed = Mathf.Max(entrySpeed, runSpeed);
            StartedThisTick = true;
            return true;
        }

        /// <summary>Advances the dash in progress; returns true on the tick it ends.</summary>
        public bool Advance(float deltaTime)
        {
            if (!Active)
            {
                return false;
            }

            _timeLeft -= deltaTime;
            return !Active;
        }

        public void Cancel()
        {
            _timeLeft = 0f;
        }

        /// <summary>Landing or jumping gives the air dash back.</summary>
        public void RestoreAirDash()
        {
            _airDashUsed = false;
        }

        public void Reset()
        {
            _chargesInitialized = false;
            _rechargeLeft = 0f;
            _timeLeft = 0f;
            _airDashUsed = false;
            StartedThisTick = false;
        }
    }
}
