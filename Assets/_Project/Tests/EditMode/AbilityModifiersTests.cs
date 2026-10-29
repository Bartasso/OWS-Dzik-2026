using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class AbilityModifiersTests
    {
        [Test]
        public void AllAbilities_StartAtZero()
        {
            var modifiers = new AbilityModifiers();

            foreach (Ability ability in Enum.GetValues(typeof(Ability)))
            {
                Assert.That(modifiers[ability], Is.EqualTo(0), ability.ToString());
            }
        }

        [Test]
        public void EachAbility_IsStoredIndependently()
        {
            var modifiers = new AbilityModifiers();
            var abilities = (Ability[])Enum.GetValues(typeof(Ability));
            for (int i = 0; i < abilities.Length; i++)
            {
                modifiers[abilities[i]] = i - 3;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                Assert.That(modifiers[abilities[i]], Is.EqualTo(i - 3), abilities[i].ToString());
            }
        }

        [Test]
        public void Count_MatchesEnum()
        {
            Assert.That(AbilityModifiers.Count, Is.EqualTo(Enum.GetValues(typeof(Ability)).Length));
        }

        [Test]
        public void UnknownAbility_Throws()
        {
            var modifiers = new AbilityModifiers();

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = modifiers[(Ability)99]);
        }
    }
}
