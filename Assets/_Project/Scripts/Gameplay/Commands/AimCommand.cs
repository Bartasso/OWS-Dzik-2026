using UnityEngine;

namespace Game.Gameplay.Commands
{
    public readonly struct AimCommand
    {
        public AimCommand(Vector2 deltaDegrees)
        {
            DeltaDegrees = deltaDegrees;
        }

        /// <summary>x = yaw change, y = pitch change (positive looks up).</summary>
        public Vector2 DeltaDegrees { get; }
    }
}
