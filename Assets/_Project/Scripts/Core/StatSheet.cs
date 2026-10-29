using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// All stats of one character. Final values are cached and recomputed only after a change,
    /// so reads are O(1) and allocation-free.
    /// </summary>
    public sealed class StatSheet
    {
        private readonly float[] _base = new float[StatIds.Count];
        private readonly float[] _final = new float[StatIds.Count];
        private readonly bool[] _dirty = new bool[StatIds.Count];
        private readonly StatRange[] _limits;
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private int _lastSourceId;

        public StatSheet(StatRange[] limits)
        {
            if (limits == null)
            {
                throw new ArgumentNullException(nameof(limits));
            }

            if (limits.Length != StatIds.Count)
            {
                throw new ArgumentException($"expected {StatIds.Count} stat limits, got {limits.Length}", nameof(limits));
            }

            _limits = (StatRange[])limits.Clone();
            for (int i = 0; i < StatIds.Count; i++)
            {
                _dirty[i] = true;
            }
        }

        public ModifierSourceId CreateSource()
        {
            _lastSourceId++;
            return new ModifierSourceId(_lastSourceId);
        }

        public float GetBase(StatId stat)
        {
            return _base[Index(stat)];
        }

        public void SetBase(StatId stat, float value)
        {
            int index = Index(stat);
            if (StatIds.IsInteger(stat))
            {
                EnsureWhole(stat, value);
            }

            _base[index] = value;
            _dirty[index] = true;
        }

        public void AddModifier(in StatModifier modifier)
        {
            int index = Index(modifier.Stat);
            if (!modifier.Source.IsValid)
            {
                throw new ArgumentException($"modifier for {modifier.Stat} has no source", nameof(modifier));
            }

            if (StatIds.IsInteger(modifier.Stat))
            {
                if (modifier.Op != ModifierOp.Flat)
                {
                    throw new ArgumentException($"{modifier.Stat} is an integer stat and accepts only Flat modifiers, got {modifier.Op}", nameof(modifier));
                }

                EnsureWhole(modifier.Stat, modifier.Value);
            }

            _modifiers.Add(modifier);
            _dirty[index] = true;
        }

        /// <returns>Number of modifiers removed.</returns>
        public int RemoveSource(ModifierSourceId source)
        {
            int removed = 0;
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (_modifiers[i].Source == source)
                {
                    _dirty[(int)_modifiers[i].Stat] = true;
                    _modifiers.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        public float Get(StatId stat)
        {
            int index = Index(stat);
            if (_dirty[index])
            {
                _final[index] = Compute(index);
                _dirty[index] = false;
            }

            return _final[index];
        }

        public int GetInt(StatId stat)
        {
            if (!StatIds.IsInteger(stat))
            {
                throw new ArgumentException($"{stat} is not an integer stat", nameof(stat));
            }

            return (int)Math.Round(Get(stat));
        }

        public int GetAbility(Ability ability)
        {
            return GetInt(StatIds.FromAbility(ability));
        }

        private float Compute(int index)
        {
            float flat = 0f;
            float percentAdd = 0f;
            float percentMult = 1f;
            foreach (StatModifier modifier in _modifiers)
            {
                if ((int)modifier.Stat != index)
                {
                    continue;
                }

                switch (modifier.Op)
                {
                    case ModifierOp.Flat:
                        flat += modifier.Value;
                        break;
                    case ModifierOp.PercentAdd:
                        percentAdd += modifier.Value;
                        break;
                    case ModifierOp.PercentMult:
                        percentMult *= 1f + modifier.Value;
                        break;
                    default:
                        throw new InvalidOperationException($"unknown modifier op {modifier.Op}");
                }
            }

            float value = (_base[index] + flat) * (1f + percentAdd) * percentMult;
            return _limits[index].Clamp(value);
        }

        private static int Index(StatId stat)
        {
            int index = (int)stat;
            if ((uint)index >= StatIds.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }

            return index;
        }

        private static void EnsureWhole(StatId stat, float value)
        {
            if (value != (float)Math.Round(value))
            {
                throw new ArgumentException($"{stat} is an integer stat, got {value}");
            }
        }
    }
}
