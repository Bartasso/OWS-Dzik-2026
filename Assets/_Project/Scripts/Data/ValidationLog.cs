using Game.Core;
using UnityEngine;

namespace Game.Data
{
    public static class ValidationLog
    {
        public static ValidationReport ValidateAndLog(IValidatable validatable)
        {
            var report = new ValidationReport();
            validatable.Validate(report);
            LogErrors(report);
            return report;
        }

        public static void LogErrors(ValidationReport report)
        {
            foreach (ValidationError error in report.Errors)
            {
                Debug.LogError(error.ToString(), error.Context as Object);
            }
        }
    }
}
