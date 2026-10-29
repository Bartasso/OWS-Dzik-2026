using System;

namespace Game.Core
{
    /// <summary>
    /// Deterministic PCG32 (XSH-RR) generator. Same seed and sequence always give the same values,
    /// on every platform, which replays and co-op sync depend on. Changing the algorithm breaks old seeds.
    /// </summary>
    public sealed class RngStream : IRandomSource
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private readonly ulong _increment;
        private ulong _state;

        public RngStream(ulong seed, ulong sequence)
        {
            _increment = (sequence << 1) | 1UL;
            _state = 0UL;
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        public uint NextUInt()
        {
            ulong old = _state;
            _state = unchecked(old * Multiplier + _increment);
            uint xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << (-rotation & 31));
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive),
                    $"maxExclusive ({maxExclusive}) must be greater than minInclusive ({minInclusive})");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);
            // Rejection sampling removes modulo bias.
            uint threshold = unchecked(0u - range) % range;
            uint value;
            do
            {
                value = NextUInt();
            }
            while (value < threshold);

            return (int)(minInclusive + (long)(value % range));
        }

        public float NextFloat()
        {
            // 24 random bits fill the float mantissa exactly, so the result is always < 1.
            return (NextUInt() >> 8) * (1f / 16777216f);
        }
    }
}
