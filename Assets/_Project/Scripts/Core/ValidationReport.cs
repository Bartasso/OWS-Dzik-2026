using System.Collections.Generic;

namespace Game.Core
{
    public sealed class ValidationReport
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        public IReadOnlyList<ValidationError> Errors => _errors;

        public bool HasErrors => _errors.Count > 0;

        public void AddError(string source, string message, object context)
        {
            _errors.Add(new ValidationError(source, message, context));
        }
    }
}
