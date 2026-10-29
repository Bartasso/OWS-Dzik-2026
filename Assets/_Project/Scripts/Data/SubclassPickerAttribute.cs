using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Put next to [SerializeReference] to get a type dropdown in the Inspector (drawer in Game.Editor).
    /// </summary>
    public sealed class SubclassPickerAttribute : PropertyAttribute
    {
    }
}
