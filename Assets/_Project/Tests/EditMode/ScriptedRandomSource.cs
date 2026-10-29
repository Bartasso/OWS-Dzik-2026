using System.Collections.Generic;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>Returns predefined values; throws if asked for more than were scripted.</summary>
    internal sealed class ScriptedRandomSource : IRandomSource
    {
        private readonly Queue<int> _ints;
        private readonly Queue<float> _floats;

        public ScriptedRandomSource(int[] ints, float[] floats = null)
        {
            _ints = new Queue<int>(ints);
            _floats = new Queue<float>(floats ?? new float[0]);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            int value = _ints.Dequeue();
            Assert.That(value, Is.InRange(minInclusive, maxExclusive - 1), "scripted int outside requested range");
            return value;
        }

        public float NextFloat()
        {
            return _floats.Dequeue();
        }

        public void AssertAllConsumed()
        {
            Assert.That(_ints, Is.Empty, "not all scripted ints were used");
            Assert.That(_floats, Is.Empty, "not all scripted floats were used");
        }
    }
}
