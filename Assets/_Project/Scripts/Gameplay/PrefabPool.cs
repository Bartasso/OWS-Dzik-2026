using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Gameplay
{
    /// <summary>
    /// Fixed-size pool, filled at load time: no Instantiate during combat. Running out logs an error once
    /// and refuses the request, so a too-small capacity is visible instead of silently allocating.
    /// </summary>
    public sealed class PrefabPool<T>
        where T : Component
    {
        private readonly Stack<T> _free;
        private readonly string _prefabName;
        private bool _reportedEmpty;

        public PrefabPool(T prefab, int capacity, Transform parent)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"pool of {prefab.name} needs capacity > 0");
            }

            _prefabName = prefab.name;
            Capacity = capacity;
            _free = new Stack<T>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                T item = Object.Instantiate(prefab, parent);
                item.gameObject.SetActive(false);
                _free.Push(item);
            }
        }

        public int Capacity { get; }

        public int ActiveCount => Capacity - _free.Count;

        public bool TryGet(out T item)
        {
            if (_free.Count == 0)
            {
                if (!_reportedEmpty)
                {
                    Debug.LogError($"Pool of {_prefabName} is exhausted ({Capacity}); raise its capacity");
                    _reportedEmpty = true;
                }

                item = null;
                return false;
            }

            item = _free.Pop();
            item.gameObject.SetActive(true);
            return true;
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            _free.Push(item);
        }
    }
}
