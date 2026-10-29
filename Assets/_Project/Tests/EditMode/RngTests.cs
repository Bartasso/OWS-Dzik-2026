using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class RngTests
    {
        [Test]
        public void RngStream_MatchesPcg32ReferenceOutput()
        {
            // Reference values from the official pcg32 demo (seed 42, sequence 54).
            var stream = new RngStream(42UL, 54UL);

            Assert.That(stream.NextUInt(), Is.EqualTo(0xa15c02b7u));
            Assert.That(stream.NextUInt(), Is.EqualTo(0x7b47f409u));
            Assert.That(stream.NextUInt(), Is.EqualTo(0xba1d3330u));
        }

        [Test]
        public void SameSeed_GivesSameSequence()
        {
            var a = new Rng(1234UL);
            var b = new Rng(1234UL);

            for (int i = 0; i < 100; i++)
            {
                Assert.That(a.Combat.NextUInt(), Is.EqualTo(b.Combat.NextUInt()));
            }
        }

        [Test]
        public void DifferentSeeds_GiveDifferentSequences()
        {
            var a = new Rng(1UL);
            var b = new Rng(2UL);

            Assert.That(Draw(a.Loot, 10), Is.Not.EqualTo(Draw(b.Loot, 10)));
        }

        [Test]
        public void DrawingFromOneStream_DoesNotAffectOthers()
        {
            var used = new Rng(99UL);
            var fresh = new Rng(99UL);

            Draw(used.Combat, 1000);

            Assert.That(Draw(used.Loot, 20), Is.EqualTo(Draw(fresh.Loot, 20)));
            Assert.That(Draw(used.Cards, 20), Is.EqualTo(Draw(fresh.Cards, 20)));
            Assert.That(Draw(used.Shop, 20), Is.EqualTo(Draw(fresh.Shop, 20)));
            Assert.That(Draw(used.SkillChecks, 20), Is.EqualTo(Draw(fresh.SkillChecks, 20)));
            Assert.That(Draw(used.Spawns, 20), Is.EqualTo(Draw(fresh.Spawns, 20)));
        }

        [Test]
        public void Streams_ProduceDistinctSequences()
        {
            var rng = new Rng(7UL);
            RngStream[] streams = { rng.Combat, rng.Loot, rng.Cards, rng.Shop, rng.SkillChecks, rng.Spawns };
            var sequences = new uint[streams.Length][];
            for (int i = 0; i < streams.Length; i++)
            {
                sequences[i] = Draw(streams[i], 10);
            }

            for (int i = 0; i < sequences.Length; i++)
            {
                for (int j = i + 1; j < sequences.Length; j++)
                {
                    Assert.That(sequences[i], Is.Not.EqualTo(sequences[j]), $"streams {i} and {j} are identical");
                }
            }
        }

        [Test]
        public void NextInt_StaysInRange_AndHitsEveryD20Face()
        {
            var stream = new Rng(5UL).SkillChecks;
            var seen = new bool[21];

            for (int i = 0; i < 10000; i++)
            {
                int value = stream.NextInt(1, 21);
                Assert.That(value, Is.InRange(1, 20));
                seen[value] = true;
            }

            for (int face = 1; face <= 20; face++)
            {
                Assert.That(seen[face], Is.True, $"face {face} never rolled");
            }
        }

        [Test]
        public void NextInt_SupportsNegativeAndFullRanges()
        {
            var stream = new Rng(11UL).Combat;

            for (int i = 0; i < 1000; i++)
            {
                Assert.That(stream.NextInt(-5, 5), Is.InRange(-5, 4));
                Assert.That(stream.NextInt(int.MinValue, int.MaxValue), Is.LessThan(int.MaxValue));
            }
        }

        [Test]
        public void NextInt_WithEmptyRange_Throws()
        {
            var stream = new Rng(3UL).Combat;

            Assert.Throws<ArgumentOutOfRangeException>(() => stream.NextInt(5, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.NextInt(5, 4));
        }

        [Test]
        public void NextFloat_IsInUnitInterval()
        {
            var stream = new Rng(8UL).Combat;

            for (int i = 0; i < 10000; i++)
            {
                float value = stream.NextFloat();
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
            }
        }

        private static uint[] Draw(RngStream stream, int count)
        {
            var values = new uint[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = stream.NextUInt();
            }

            return values;
        }
    }
}
