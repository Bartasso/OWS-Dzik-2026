using Game.Data;
using Game.Gameplay.Player;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    /// <summary>
    /// Pooled enemy: walks straight at the player and hits on contact. Placeholder AI until the flow field.
    /// The prefab is a unit capsule (radius 0.5, height 2); it is scaled to the definition's radius.
    /// </summary>
    public sealed class Enemy : MonoBehaviour
    {
        private GamePause _pause;
        private PlayerHealth _player;
        private float _contactCooldown;

        public EnemyDef Definition { get; private set; }

        public float Health { get; private set; }

        public float Radius => Definition.Radius;

        /// <summary>Slot in EnemyRegistry, -1 when not registered.</summary>
        public int RegistryIndex { get; set; } = -1;

        public void Spawn(EnemyDef definition, Vector3 groundPosition, GamePause pause, PlayerHealth player)
        {
            Definition = definition;
            Health = definition.MaxHp;
            _pause = pause;
            _player = player;
            _contactCooldown = 0f;

            float scale = definition.Radius * 2f;
            transform.localScale = Vector3.one * scale;
            // Capsule center sits at its scaled half-height (unit height 2 → scale).
            transform.position = groundPosition + Vector3.up * scale;
        }

        /// <returns>True if this hit killed the enemy.</returns>
        public bool TakeDamage(float amount)
        {
            Health -= amount;
            return Health <= 0f;
        }

        private void Update()
        {
            if (_pause.IsPaused)
            {
                return;
            }

            Vector3 toPlayer = _player.transform.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;
            float reach = Definition.Radius + _player.Radius;

            if (distance > 0.001f)
            {
                Vector3 direction = toPlayer / distance;
                transform.rotation = Quaternion.LookRotation(direction);
                if (distance > reach)
                {
                    float step = Mathf.Min(Definition.MoveSpeed * Time.deltaTime, distance - reach);
                    transform.position += direction * step;
                    return;
                }
            }

            _contactCooldown -= Time.deltaTime;
            if (_contactCooldown <= 0f)
            {
                _player.TakeDamage(Definition.ContactDamage);
                _contactCooldown = Definition.ContactInterval;
            }
        }
    }
}
