using System;
using Game.Data;
using Game.Gameplay.SkillChecks;
using UnityEngine;

namespace Game.Gameplay
{
    public readonly struct DamageDealtEvent
    {
        public DamageDealtEvent(Vector3 position, float amount, bool isCrit, bool isKill)
        {
            Position = position;
            Amount = amount;
            IsCrit = isCrit;
            IsKill = isKill;
        }

        public Vector3 Position { get; }

        public float Amount { get; }

        public bool IsCrit { get; }

        public bool IsKill { get; }
    }

    public readonly struct SkillCheckResolvedEvent
    {
        public SkillCheckResolvedEvent(SkillCheckSiteDef site, in SkillCheckOutcome outcome)
        {
            Site = site;
            Outcome = outcome;
        }

        public SkillCheckSiteDef Site { get; }

        public SkillCheckOutcome Outcome { get; }
    }

    /// <summary>What happened in gameplay, for Presentation and UI to draw. Gameplay never listens to these.</summary>
    public sealed class CombatEvents
    {
        public event Action<DamageDealtEvent> DamageDealt;

        public event Action<SkillCheckResolvedEvent> SkillCheckResolved;

        public void RaiseDamageDealt(in DamageDealtEvent e)
        {
            DamageDealt?.Invoke(e);
        }

        public void RaiseSkillCheckResolved(in SkillCheckResolvedEvent e)
        {
            SkillCheckResolved?.Invoke(e);
        }
    }
}
