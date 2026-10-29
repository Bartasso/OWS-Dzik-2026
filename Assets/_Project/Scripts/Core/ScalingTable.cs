using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Which ability a weapon category scales with for one character. Talents override the weapon's
    /// own ability per category (e.g. Arcane → Str), so weapon code never checks the character class.
    /// </summary>
    public sealed class ScalingTable
    {
        private readonly List<Entry> _overrides = new List<Entry>();

        /// <summary>The most recent override for a category wins.</summary>
        public void SetOverride(WeaponCategory category, Ability ability, ModifierSourceId source)
        {
            _overrides.Add(new Entry(category, ability, source));
        }

        /// <returns>Number of overrides removed.</returns>
        public int RemoveSource(ModifierSourceId source)
        {
            int removed = 0;
            for (int i = _overrides.Count - 1; i >= 0; i--)
            {
                if (_overrides[i].Source == source)
                {
                    _overrides.RemoveAt(i);
                    removed++;
                }
            }

            return removed;
        }

        public Ability Resolve(WeaponCategory category, Ability weaponAbility)
        {
            for (int i = _overrides.Count - 1; i >= 0; i--)
            {
                if (_overrides[i].Category == category)
                {
                    return _overrides[i].Ability;
                }
            }

            return weaponAbility;
        }

        private readonly struct Entry
        {
            public Entry(WeaponCategory category, Ability ability, ModifierSourceId source)
            {
                Category = category;
                Ability = ability;
                Source = source;
            }

            public WeaponCategory Category { get; }

            public Ability Ability { get; }

            public ModifierSourceId Source { get; }
        }
    }
}
