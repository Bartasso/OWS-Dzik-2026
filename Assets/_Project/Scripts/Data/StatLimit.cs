using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>Clamp applied to a stat's final value. Use Infinity / -Infinity for an open end.</summary>
    [Serializable]
    public struct StatLimit
    {
        [SerializeField] private StatId _stat;
        [SerializeField] private float _min;
        [SerializeField] private float _max;

        public StatLimit(StatId stat, float min, float max)
        {
            _stat = stat;
            _min = min;
            _max = max;
        }

        public StatId Stat => _stat;

        public float Min => _min;

        public float Max => _max;
    }
}
