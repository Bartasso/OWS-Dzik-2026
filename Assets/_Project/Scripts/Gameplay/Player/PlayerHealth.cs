using System;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private CharacterController _controller;

        private RunContext _context;

        public float Current { get; private set; }

        public float Max => _context.Stats.Get(StatId.MaxHp);

        public bool IsDead { get; private set; }

        /// <summary>Debug god mode.</summary>
        public bool Invulnerable { get; set; }

        public float Radius => _controller.radius;

        public void Init(RunContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Current = Max;
            enabled = true;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || Invulnerable || amount <= 0f)
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

            // Max HP can drop (trade-off trinkets), so clamp every frame.
            Current = Mathf.Min(Current + _context.Stats.Get(StatId.HpRegen) * Time.deltaTime, Max);
        }
    }
}
