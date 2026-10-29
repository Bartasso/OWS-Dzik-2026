namespace Game.Core
{
    public interface IRandomSource
    {
        /// <summary>Uniform int in [minInclusive, maxExclusive).</summary>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Uniform float in [0, 1).</summary>
        float NextFloat();
    }
}
