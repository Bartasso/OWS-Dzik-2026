using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class ProjectDataValidator
    {
        private const string DataRoot = "Assets/_Project";

        public static ValidationReport Run()
        {
            var report = new ValidationReport();
            var definitions = new List<Definition>();
            int balanceCount = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { DataRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null)
                {
                    report.AddError(path, "failed to load (missing or broken script?)", null);
                    continue;
                }

                if (asset is IValidatable validatable)
                {
                    validatable.Validate(report);
                }

                if (asset is Definition definition)
                {
                    definitions.Add(definition);
                }

                if (asset is GameBalance)
                {
                    balanceCount++;
                }
            }

            DefinitionIdChecks.CheckUnique(definitions, report);

            if (balanceCount != 1)
            {
                report.AddError(nameof(GameBalance), $"expected exactly one GameBalance asset under {DataRoot}, found {balanceCount}", null);
            }

            return report;
        }
    }
}
