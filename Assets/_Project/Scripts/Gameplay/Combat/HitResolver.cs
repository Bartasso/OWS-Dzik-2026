using System;
using Game.Core;
using Game.Data;
using Game.Gameplay.Enemies;
using UnityEngine;

namespace Game.Gameplay.Combat
{
    /// <summary>Applies a weapon hit: damage roll, triggers for effects, events for presentation, kills.</summary>
    public sealed class HitResolver
    {
        private readonly RunContext _context;
        private readonly EnemySpawner _spawner;

        public HitResolver(RunContext context, EnemySpawner spawner)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _spawner = spawner != null ? spawner : throw new ArgumentNullException(nameof(spawner));
        }

        public void Resolve(WeaponDef weapon, Enemy enemy, Vector3 point)
        {
            HitResult hit = DamageCalculator.Roll(weapon, _context.Stats, _context.Scaling, _context.Balance, _context.Rng.Combat);
            EnemyKind kind = enemy.Definition.Kind;
            bool killed = enemy.TakeDamage(hit.Damage);

            EffectLoadout loadout = _context.Loadout;
            loadout.Raise(new TriggerArgs(GameTrigger.Hit, hit.Damage));
            if (hit.IsCrit)
            {
                loadout.Raise(new TriggerArgs(GameTrigger.Crit, hit.Damage));
            }

            _context.Events.RaiseDamageDealt(new DamageDealtEvent(point, hit.Damage, hit.IsCrit, killed));

            if (!killed)
            {
                return;
            }

            loadout.Raise(new TriggerArgs(GameTrigger.Kill, 0f));
            if (kind == EnemyKind.Elite)
            {
                loadout.Raise(new TriggerArgs(GameTrigger.EliteKilled, 0f));
            }
            else if (kind == EnemyKind.Boss)
            {
                loadout.Raise(new TriggerArgs(GameTrigger.BossKilled, 0f));
            }

            _spawner.OnKilled(enemy);
        }
    }
}
