using UnityEngine;

namespace Game.Data
{
    /// <summary>Unlimited, stacking item. Each copy raises the stack count of its effects.</summary>
    [CreateAssetMenu(menuName = "Game/Trinket", fileName = "Trinket")]
    public sealed class TrinketDef : EffectDefinition
    {
    }
}
