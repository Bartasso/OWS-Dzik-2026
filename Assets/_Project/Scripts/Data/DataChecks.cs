using Game.Core;
using UnityEngine;

namespace Game.Data
{
    internal static class DataChecks
    {
        public static void Positive(ValidationReport report, Object owner, string field, float value)
        {
            if (!(value > 0f) || float.IsInfinity(value))
            {
                report.AddError(owner.name, $"{field} must be > 0, is {value}", owner);
            }
        }

        public static void NotNegative(ValidationReport report, Object owner, string field, float value)
        {
            if (!(value >= 0f) || float.IsInfinity(value))
            {
                report.AddError(owner.name, $"{field} must be >= 0, is {value}", owner);
            }
        }

        public static void Required(ValidationReport report, Object owner, string field, Object value)
        {
            if (value == null)
            {
                report.AddError(owner.name, $"missing {field} reference", owner);
            }
        }

        public static void DisplayName(ValidationReport report, Object owner, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(owner.name, "missing display name", owner);
            }
        }
    }
}
