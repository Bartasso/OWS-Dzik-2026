using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    /// <summary>Alive enemies, with O(1) removal. Brute-force queries until a spatial grid is needed.</summary>
    public sealed class EnemyRegistry
    {
        private readonly List<Enemy> _alive = new List<Enemy>();

        public int Count => _alive.Count;

        public Enemy this[int index] => _alive[index];

        public void Add(Enemy enemy)
        {
            if (enemy.RegistryIndex >= 0)
            {
                throw new InvalidOperationException($"{enemy.name} is already registered");
            }

            enemy.RegistryIndex = _alive.Count;
            _alive.Add(enemy);
        }

        public void Remove(Enemy enemy)
        {
            int index = enemy.RegistryIndex;
            if (index < 0 || index >= _alive.Count || _alive[index] != enemy)
            {
                throw new InvalidOperationException($"{enemy.name} is not registered");
            }

            int last = _alive.Count - 1;
            Enemy moved = _alive[last];
            _alive[index] = moved;
            moved.RegistryIndex = index;
            _alive.RemoveAt(last);
            enemy.RegistryIndex = -1;
        }

        /// <summary>
        /// First enemy (closest to <paramref name="from"/>) whose body touches a sphere swept from → to,
        /// tested on the ground plane so fast projectiles can't tunnel through.
        /// </summary>
        public bool TryFindHit(Vector3 from, Vector3 to, float radius, out Enemy hit)
        {
            hit = null;
            var start = new Vector2(from.x, from.z);
            Vector2 segment = new Vector2(to.x, to.z) - start;
            float segmentLengthSq = segment.sqrMagnitude;
            float bestT = float.MaxValue;

            for (int i = 0; i < _alive.Count; i++)
            {
                Enemy enemy = _alive[i];
                Vector3 center = enemy.transform.position;
                Vector2 toCenter = new Vector2(center.x, center.z) - start;
                float t = segmentLengthSq > 0f ? Mathf.Clamp01(Vector2.Dot(toCenter, segment) / segmentLengthSq) : 0f;
                float reach = radius + enemy.Radius;
                if ((toCenter - segment * t).sqrMagnitude <= reach * reach && t < bestT)
                {
                    bestT = t;
                    hit = enemy;
                }
            }

            return hit != null;
        }
    }
}
