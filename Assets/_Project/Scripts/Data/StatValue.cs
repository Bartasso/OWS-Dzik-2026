using System;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public struct StatValue
    {
        [SerializeField] private StatId _stat;
        [SerializeField] private float _value;

        public StatValue(StatId stat, float value)
        {
            _stat = stat;
            _value = value;
        }

        public StatId Stat => _stat;

        public float Value => _value;
    }
}
