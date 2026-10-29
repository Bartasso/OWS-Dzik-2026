namespace Game.Core
{
    /// <summary>
    /// Data that checks itself: missing references, empty or malformed IDs, out-of-range values.
    /// Runs in OnValidate, from Tools/Validate All Data and at game startup.
    /// </summary>
    public interface IValidatable
    {
        void Validate(ValidationReport report);
    }
}
