using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Data
{
    public static class DefinitionIdChecks
    {
        public static void CheckUnique(IReadOnlyList<Definition> definitions, ValidationReport report)
        {
            var seen = new Dictionary<string, Definition>(StringComparer.Ordinal);
            foreach (Definition definition in definitions)
            {
                // Empty IDs are reported by Definition.Validate.
                if (string.IsNullOrWhiteSpace(definition.Id))
                {
                    continue;
                }

                if (seen.TryGetValue(definition.Id, out Definition other))
                {
                    report.AddError(definition.name, $"duplicate Id '{definition.Id}' (also used by '{other.name}')", definition);
                }
                else
                {
                    seen.Add(definition.Id, definition);
                }
            }
        }
    }
}
