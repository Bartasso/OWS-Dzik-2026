using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>Everything the locomotion simulation needs from outside for one tick.</summary>
    public readonly struct LocomotionInput
    {
        public LocomotionInput(Vector3 wishDirection, bool jumpPressed, bool grounded, float runSpeed, int extraJumps)
        {
            WishDirection = wishDirection;
            JumpPressed = jumpPressed;
            Grounded = grounded;
            RunSpeed = runSpeed;
            ExtraJumps = extraJumps;
        }

        /// <summary>World-space direction the player steers to; only its horizontal part counts, magnitude up to 1.</summary>
        public Vector3 WishDirection { get; }

        /// <summary>Jump was pressed since the previous tick.</summary>
        public bool JumpPressed { get; }

        /// <summary>Standing on walkable ground at the end of the previous tick.</summary>
        public bool Grounded { get; }

        /// <summary>Top run speed in m/s (MoveSpeed stat).</summary>
        public float RunSpeed { get; }

        /// <summary>Air jumps on top of the ground jump (ExtraJumps stat).</summary>
        public int ExtraJumps { get; }
    }
}
