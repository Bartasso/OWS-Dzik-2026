using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>Everything the locomotion simulation needs from outside for one tick.</summary>
    public readonly struct LocomotionInput
    {
        public LocomotionInput(
            Vector3 wishDirection,
            bool jumpPressed,
            bool grounded,
            float runSpeed,
            int extraJumps,
            bool dashPressed = false,
            bool slideHeld = false,
            Vector3 facing = default,
            Vector3 groundNormal = default,
            bool canStand = true,
            int extraDashCharges = 0)
        {
            WishDirection = wishDirection;
            JumpPressed = jumpPressed;
            Grounded = grounded;
            RunSpeed = runSpeed;
            ExtraJumps = extraJumps;
            DashPressed = dashPressed;
            SlideHeld = slideHeld;
            Facing = facing;
            GroundNormal = groundNormal.sqrMagnitude > 0f ? groundNormal : Vector3.up;
            CanStand = canStand;
            ExtraDashCharges = extraDashCharges;
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

        /// <summary>Dash was pressed since the previous tick.</summary>
        public bool DashPressed { get; }

        public bool SlideHeld { get; }

        /// <summary>Horizontal facing; the dash goes this way when there is no steering input.</summary>
        public Vector3 Facing { get; }

        /// <summary>Unit normal of the ground; up when unknown.</summary>
        public Vector3 GroundNormal { get; }

        /// <summary>There is room for the standing capsule, so a slide may end.</summary>
        public bool CanStand { get; }

        /// <summary>Dash charges on top of the base (ExtraDashCharges stat).</summary>
        public int ExtraDashCharges { get; }
    }
}
