using System;
using System.Collections.Generic;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using Game.Gameplay.Enemies;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Tests.EditMode
{
    public class EnemyRegistryTests
    {
        private readonly TestAssets _assets = new TestAssets();
        private readonly List<GameObject> _objects = new List<GameObject>();
        private EnemyDef _goblin;

        [SetUp]
        public void SetUp()
        {
            _goblin = ScriptableObject.CreateInstance<EnemyDef>();
            PrivateField.Set(_goblin, "_radius", 0.5f);
            PrivateField.Set(_goblin, "_maxHp", 30f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _objects)
            {
                Object.DestroyImmediate(go);
            }

            Object.DestroyImmediate(_goblin);
            _assets.DestroyAll();
        }

        [Test]
        public void FastProjectile_DoesNotTunnelThroughEnemy()
        {
            var registry = new EnemyRegistry();
            registry.Add(EnemyAt(new Vector3(5f, 0f, 0f)));

            // One frame step of 10 m jumps right over the enemy.
            bool hit = registry.TryFindHit(new Vector3(0f, 1f, 0f), new Vector3(10f, 1f, 0f), 0.25f, out Enemy enemy);

            Assert.That(hit, Is.True);
            Assert.That(enemy, Is.Not.Null);
        }

        [Test]
        public void Miss_WhenPathPassesBeside()
        {
            var registry = new EnemyRegistry();
            registry.Add(EnemyAt(new Vector3(5f, 0f, 2f)));

            Assert.That(registry.TryFindHit(new Vector3(0f, 1f, 0f), new Vector3(10f, 1f, 0f), 0.25f, out _), Is.False);
        }

        [Test]
        public void Hit_ReturnsFirstEnemyAlongPath()
        {
            var registry = new EnemyRegistry();
            Enemy far = EnemyAt(new Vector3(8f, 0f, 0f));
            Enemy near = EnemyAt(new Vector3(3f, 0f, 0f));
            registry.Add(far);
            registry.Add(near);

            registry.TryFindHit(Vector3.zero, new Vector3(10f, 0f, 0f), 0.25f, out Enemy hit);

            Assert.That(hit, Is.SameAs(near));
        }

        [Test]
        public void Remove_KeepsOtherEnemiesRegistered()
        {
            var registry = new EnemyRegistry();
            Enemy a = EnemyAt(Vector3.zero);
            Enemy b = EnemyAt(Vector3.one);
            Enemy c = EnemyAt(Vector3.one * 2f);
            registry.Add(a);
            registry.Add(b);
            registry.Add(c);

            registry.Remove(a);

            Assert.That(registry.Count, Is.EqualTo(2));
            Assert.That(a.RegistryIndex, Is.EqualTo(-1));
            Assert.That(registry[b.RegistryIndex], Is.SameAs(b));
            Assert.That(registry[c.RegistryIndex], Is.SameAs(c));
            Assert.Throws<InvalidOperationException>(() => registry.Remove(a));
        }

        private Enemy EnemyAt(Vector3 groundPosition)
        {
            var go = new GameObject("Enemy");
            _objects.Add(go);
            var enemy = go.AddComponent<Enemy>();
            enemy.Spawn(_goblin, groundPosition, new GamePause(), null);
            return enemy;
        }
    }
}
