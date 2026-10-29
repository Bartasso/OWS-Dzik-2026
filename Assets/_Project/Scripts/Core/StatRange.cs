using System;

namespace Game.Core
{
    public readonly struct StatRange
    {
        public static readonly StatRange Unbounded = new StatRange(float.NegativeInfinity, float.PositiveInfinity);

        public StatRange(float min, float max)
        {
            if (float.IsNaN(min) || float.IsNaN(max) || min > max)
            {
                throw new ArgumentException($"invalid stat range [{min}, {max}]");
            }

            Min = min;
            Max = max;
        }

        public float Min { get; }

        public float Max { get; }

        public float Clamp(float value)
        {
            if (value < Min)
            {
                return Min;
            }

            return value > Max ? Max : value;
        }
    }
}
