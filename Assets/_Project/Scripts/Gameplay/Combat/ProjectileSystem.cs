using System;
using System.Collections.Generic;
using Game.Data;
using Game.Gameplay.Enemies;
using UnityEngine;

namespace Game.Gameplay.Combat
{
    /// <summary>
    /// Moves all player projectiles in one loop. Projectile objects are pooled visuals; the flight state lives here.
    /// Weapon values are read every frame, so tweaking the WeaponDef in Play Mode affects shots in flight.
    /// </summary>
    public sealed class ProjectileSystem : MonoBehaviour
    {
        private readonly List<Flight> _flights = new List<Flight>();
        private RunContext _context;
        private PrefabPool<Transform> _pool;
        private EnemyRegistry _enemies;
        private HitResolver _hits;

        public int ActiveCount => _flights.Count;

        public void Init(RunContext context, PrefabPool<Transform> pool, EnemyRegistry enemies, HitResolver hits)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _hits = hits ?? throw new ArgumentNullException(nameof(hits));
            _flights.Capacity = pool.Capacity;
            enabled = true;
        }

        public void Launch(WeaponDef weapon, Vector3 origin, Vector3 direction)
        {
            if (!_pool.TryGet(out Transform view))
            {
                return;
            }

            Vector3 heading = direction.normalized;
            view.position = origin;
            // Views with a trail (sprite fireballs) need to point along the flight.
            view.rotation = Quaternion.LookRotation(heading);
            view.localScale = Vector3.one * (weapon.ProjectileRadius * 2f);
            _flights.Add(new Flight(view, weapon, heading));
        }

        private void Update()
        {
            if (_context.Pause.IsPaused)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            for (int i = _flights.Count - 1; i >= 0; i--)
            {
                Flight flight = _flights[i];
                WeaponDef weapon = flight.Weapon;
                float step = weapon.ProjectileSpeed * deltaTime;
                Vector3 from = flight.View.position;
                Vector3 to = from + flight.Direction * step;

                if (_enemies.TryFindHit(from, to, weapon.ProjectileRadius, out Enemy enemy))
                {
                    _hits.Resolve(weapon, enemy, to);
                    Remove(i);
                    continue;
                }

                flight.Travelled += step;
                if (flight.Travelled >= weapon.Range)
                {
                    Remove(i);
                    continue;
                }

                flight.View.position = to;
                flight.View.localScale = Vector3.one * (weapon.ProjectileRadius * 2f);
                _flights[i] = flight;
            }
        }

        private void Remove(int index)
        {
            _pool.Release(_flights[index].View);
            int last = _flights.Count - 1;
            _flights[index] = _flights[last];
            _flights.RemoveAt(last);
        }

        private struct Flight
        {
            public Flight(Transform view, WeaponDef weapon, Vector3 direction)
            {
                View = view;
                Weapon = weapon;
                Direction = direction;
                Travelled = 0f;
            }

            public Transform View { get; }

            public WeaponDef Weapon { get; }

            public Vector3 Direction { get; }

            public float Travelled { get; set; }
        }
    }
}
