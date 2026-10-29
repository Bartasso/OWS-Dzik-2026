using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class StatFormulasTests
    {
        private const float Tolerance = 1e-6f;
        private const float BaseCrit = 0.05f;
        private const float CritPerLuck = 0.01f;

        [TestCase(0, 0f, 0.05f)]
        [TestCase(5, 0f, 0.10f)]
        [TestCase(-2, 0f, 0.03f)]
        [TestCase(0, 0.15f, 0.20f)]
        [TestCase(3, 0.10f, 0.18f)]
        public void CritChance_IsBasePlusLuckPlusBonus(int luck, float bonus, float expected)
        {
            Assert.That(StatFormulas.CritChance(luck, BaseCrit, CritPerLuck, bonus), Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void CritChance_IsClampedAtZero()
        {
            Assert.That(StatFormulas.CritChance(-50, BaseCrit, CritPerLuck, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void CritChance_IsClampedAtOne()
        {
            Assert.That(StatFormulas.CritChance(200, BaseCrit, CritPerLuck, 0.5f), Is.EqualTo(1f));
        }

        [TestCase(0, 0f)]
        [TestCase(1, 0.05f)]
        [TestCase(4, 0.20f)]
        [TestCase(20, 1f)]
        [TestCase(35, 1f)]
        [TestCase(-3, 0f)]
        public void Nat1RerollChance_IsFivePercentPerLuck_Clamped(int luck, float expected)
        {
            Assert.That(StatFormulas.Nat1RerollChance(luck, 0.05f), Is.EqualTo(expected).Within(Tolerance));
        }

        [TestCase(0.10f, 0, 0.10f)]
        [TestCase(0.10f, 4, 0.12f)]
        [TestCase(0.10f, -10, 0.05f)]
        [TestCase(0.10f, -20, 0f)]
        [TestCase(0.10f, -30, 0f)]
        [TestCase(0.80f, 10, 1f)]
        public void ProcChance_ScalesWithLuck_Clamped(float baseChance, int luck, float expected)
        {
            Assert.That(StatFormulas.ProcChance(baseChance, luck, 0.05f), Is.EqualTo(expected).Within(Tolerance));
        }

        [TestCase(0, 0f)]
        [TestCase(1, 0.25f)]
        [TestCase(4, 1f)]
        public void StackValue_Linear(int stacks, float expected)
        {
            Assert.That(StatFormulas.StackValue(StackCurve.Linear, 0.25f, stacks), Is.EqualTo(expected).Within(Tolerance));
        }

        [TestCase(0, 0f)]
        [TestCase(1, 0.5f)]
        [TestCase(3, 0.75f)]
        [TestCase(99, 0.99f)]
        public void StackValue_Hyperbolic(int stacks, float expected)
        {
            // k = 1: 1 − 1/(1 + n)
            Assert.That(StatFormulas.StackValue(StackCurve.Hyperbolic, 1f, stacks), Is.EqualTo(expected).Within(Tolerance));
        }
    }
}
