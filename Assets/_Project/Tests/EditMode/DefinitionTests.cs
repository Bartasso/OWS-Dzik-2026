using System.Collections.Generic;
using Game.Core;
using Game.Data;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class DefinitionTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        [TestCase("weapon.sword")]
        [TestCase("trinket.grandpas_die")]
        [TestCase("gear.ring.grandmas_signet")]
        [TestCase("talent.wizard2")]
        public void ValidId_HasNoErrors(string id)
        {
            Assert.That(Validate(Create(id)).HasErrors, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void MissingId_IsError(string id)
        {
            Assert.That(Validate(Create(id)).HasErrors, Is.True);
        }

        [TestCase("sword")]
        [TestCase("Weapon.Sword")]
        [TestCase("weapon sword")]
        [TestCase("weapon.")]
        [TestCase(".sword")]
        [TestCase("weapon..sword")]
        [TestCase("weapon.sword-2")]
        [TestCase("1weapon.sword")]
        public void MalformedId_IsError(string id)
        {
            Assert.That(Validate(Create(id)).HasErrors, Is.True);
        }

        [Test]
        public void DuplicateIds_AreReported()
        {
            var definitions = new List<Definition> { Create("weapon.sword"), Create("weapon.bow"), Create("weapon.sword") };
            var report = new ValidationReport();

            DefinitionIdChecks.CheckUnique(definitions, report);

            Assert.That(report.Errors.Count, Is.EqualTo(1));
            Assert.That(report.Errors[0].Context, Is.SameAs(definitions[2]));
        }

        [Test]
        public void UniqueIds_AreNotReported()
        {
            var definitions = new List<Definition> { Create("weapon.sword"), Create("weapon.bow") };
            var report = new ValidationReport();

            DefinitionIdChecks.CheckUnique(definitions, report);

            Assert.That(report.HasErrors, Is.False);
        }

        private TestDefinition Create(string id)
        {
            var definition = ScriptableObject.CreateInstance<TestDefinition>();
            definition.name = "TestDefinition";
            _created.Add(definition);
            PrivateField.Set(definition, "_id", id);
            return definition;
        }

        private static ValidationReport Validate(IValidatable validatable)
        {
            var report = new ValidationReport();
            validatable.Validate(report);
            return report;
        }
    }
}
