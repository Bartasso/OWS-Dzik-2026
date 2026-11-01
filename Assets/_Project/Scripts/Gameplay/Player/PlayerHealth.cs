using System;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider _capsule;

        private readonly InvulnerabilityTimer _iFrames = new InvulnerabilityTimer();
        private RunContext _context;

        public float Current { get; private set; }

        public float Max => _context.Stats.Get(StatId.MaxHp);

        public bool IsDead { get; private set; }

        /// <summary>Debug god mode.</summary>
        public bool Invulnerable { get; set; }

        /// <summary>Temporarily immune to damage, e.g. dash i-frames.</summary>
        public bool HasIFrames => _iFrames.Active;

        public float Radius => _capsule.radius;

        public void Init(RunContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Current = Max;
            enabled = true;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || Invulnerable || _iFrames.Active || amount <= 0f)
            {
                return;
            }

            Current -= amount;
            _context.Loadout.Raise(new TriggerArgs(GameTrigger.DamageTaken, amount));
            if (Current <= 0f)
            {
                Current = 0f;
                IsDead = true;
                _context.Pause.Push();
            }
        }

        public void GrantIFrames(float seconds)
        {
            _iFrames.Grant(seconds);
        }

        private void Awake()
        {
            if (_capsule == null)
            {
                throw new InvalidOperationException($"{name}: missing CapsuleCollider reference");
            }
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            // Max HP can drop (trade-off trinkets), so clamp every frame.
            Current = Mathf.Min(Current + _context.Stats.Get(StatId.HpRegen) * Time.deltaTime, Max);
        }

        private void FixedUpdate()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            _iFrames.Tick(Time.deltaTime);
        }
    }
}
