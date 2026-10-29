using System;

namespace Game.Core
{
    /// <summary>Groups everything one source added (a trinket effect, a boost) so it can be removed at once.</summary>
    public readonly struct ModifierSourceId : IEquatable<ModifierSourceId>
    {
        public ModifierSourceId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value != 0;

        public static bool operator ==(ModifierSourceId left, ModifierSourceId right) => left.Value == right.Value;

        public static bool operator !=(ModifierSourceId left, ModifierSourceId right) => left.Value != right.Value;

        public bool Equals(ModifierSourceId other) => Value == other.Value;

        public override bool Equals(object obj) => obj is ModifierSourceId other && Equals(other);

        public override int GetHashCode() => Value;

        public override string ToString() => $"Source#{Value}";
    }
}
