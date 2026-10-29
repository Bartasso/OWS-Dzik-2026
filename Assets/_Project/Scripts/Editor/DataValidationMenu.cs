using Game.Core;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class DataValidationMenu
    {
        [MenuItem("Tools/Validate All Data")]
        public static void ValidateAll()
        {
            ValidationReport report = ProjectDataValidator.Run();
            if (report.HasErrors)
            {
                ValidationLog.LogErrors(report);
                Debug.LogError($"Data validation failed: {report.Errors.Count} error(s).");
            }
            else
            {
                Debug.Log("Data validation passed.");
            }
        }
    }
}
