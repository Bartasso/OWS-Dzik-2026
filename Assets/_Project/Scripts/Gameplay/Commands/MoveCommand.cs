using UnityEngine;

namespace Game.Gameplay.Commands
{
    public readonly struct MoveCommand
    {
        public MoveCommand(Vector2 direction)
        {
            Direction = direction;
        }

        /// <summary>x = strafe, y = forward, relative to the aim yaw. Magnitude up to 1.</summary>
        public Vector2 Direction { get; }
    }
}
