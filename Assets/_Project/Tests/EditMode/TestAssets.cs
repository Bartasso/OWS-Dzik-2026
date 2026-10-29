using System.Collections.Generic;
using Game.Data;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>Creates in-memory definitions for tests and destroys them afterwards.</summary>
    internal sealed class TestAssets
    {
        private readonly List<Object> _created = new List<Object>();

        public TrinketDef Trinket(string id, params Effect[] effects)
        {
            return Create<TrinketDef>(id, effects);
        }

        public TalentDef Talent(string id, params Effect[] effects)
        {
            return Create<TalentDef>(id, effects);
        }

        public void DestroyAll()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        private T Create<T>(string id, Effect[] effects)
            where T : EffectDefinition
        {
            var definition = ScriptableObject.CreateInstance<T>();
            definition.name = id;
            _created.Add(definition);
            PrivateField.Set(definition, "_id", id);
            PrivateField.Set(definition, "_displayName", id);
            PrivateField.Set(definition, "_effects", new List<Effect>(effects));
            return definition;
        }
    }
}
