using System.Text.RegularExpressions;
using Game.Core;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Base for every content definition (weapon, gear, trinket, talent...). The Id is stable forever:
    /// meta-progression saves reference it.
    /// </summary>
    public abstract class Definition : ScriptableObject, IValidatable
    {
        // "category.name", e.g. "weapon.sword", "trinket.grandpas_die".
        private static readonly Regex IdPattern = new Regex(@"^[a-z][a-z0-9_]*(\.[a-z0-9_]+)+$");

        [SerializeField] private string _id;

        public string Id => _id;

        public void Validate(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                report.AddError(name, "missing Id", this);
            }
            else if (!IdPattern.IsMatch(_id))
            {
                report.AddError(name, $"Id '{_id}' must look like 'category.name' (lowercase letters, digits, underscores)", this);
            }

            ValidateDefinition(report);
        }

        /// <summary>Checks specific to the concrete definition. The Id is already checked.</summary>
        protected virtual void ValidateDefinition(ValidationReport report)
        {
        }

        protected virtual void OnValidate()
        {
            ValidationLog.ValidateAndLog(this);
        }
    }
}
