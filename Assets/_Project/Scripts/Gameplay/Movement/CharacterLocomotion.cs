using System;
using Game.Data;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using UnityEngine;

namespace Game.Gameplay.Movement
{
    /// <summary>
    /// One character's movement tick: the simulation decides the velocity, the kinematic capsule
    /// resolves it against the world, and what the world blocked is fed back into the simulation.
    /// Shrinks the capsule while sliding and checks for headroom before standing up.
    /// </summary>
    public sealed class CharacterLocomotion
    {
        private readonly KCCMovementEngine _engine;
        private readonly CapsuleCollider _capsule;
        private readonly float _standingHeight;
        private readonly float _slidingHeight;

        public CharacterLocomotion(KCCMovementEngine engine, MovementTuning tuning)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (tuning == null)
            {
                throw new ArgumentNullException(nameof(tuning));
            }

            if (!engine.TryGetComponent(out _capsule))
            {
                throw new InvalidOperationException($"{engine.name}: missing CapsuleCollider");
            }

            // Resizing keeps the feet in place, which needs the capsule to stand on the object's origin.
            if (_capsule.direction != 1 || !Mathf.Approximately(_capsule.center.y, _capsule.height * 0.5f))
            {
                throw new InvalidOperationException($"{engine.name}: capsule must be vertical with its bottom at the origin");
            }

            _engine = engine;
            _engine.stepHeight = tuning.StepHeight;
            _engine.maxWalkAngle = tuning.MaxWalkAngle;
            _engine.skinWidth = tuning.SkinWidth;
            _standingHeight = _capsule.height;
            _slidingHeight = Mathf.Max(tuning.Slide.Height, _capsule.radius * 2f);
            Simulator = new LocomotionSimulator(tuning);
        }

        public LocomotionSimulator Simulator { get; }

        /// <summary>Standing on walkable ground after the last tick.</summary>
        public bool Grounded
        {
            get
            {
                KCCGroundedState ground = _engine.GroundedState;
                return ground.StandingOnGround && !ground.Sliding;
            }
        }

        /// <param name="wishDirection">World-space steering direction, magnitude up to 1.</param>
        /// <param name="jumpPressed">Jump pressed since the previous tick.</param>
        /// <param name="runSpeed">Top run speed in m/s.</param>
        /// <param name="extraJumps">Air jumps on top of the ground jump.</param>
        /// <param name="deltaTime">Fixed tick duration.</param>
        /// <param name="dashPressed">Dash pressed since the previous tick.</param>
        /// <param name="slideHeld">Slide key held.</param>
        /// <param name="facing">Horizontal facing, the dash direction without steering input.</param>
        /// <param name="extraDashCharges">Dash charges on top of the base.</param>
        public void Tick(
            Vector3 wishDirection,
            bool jumpPressed,
            float runSpeed,
            int extraJumps,
            float deltaTime,
            bool dashPressed = false,
            bool slideHeld = false,
            Vector3 facing = default,
            int extraDashCharges = 0)
        {
            KCCGroundedState ground = _engine.GroundedState;
            var input = new LocomotionInput(
                wishDirection,
                jumpPressed,
                Grounded,
                runSpeed,
                extraJumps,
                dashPressed,
                slideHeld,
                facing,
                ground.SurfaceNormal,
                HasRoomToStand(),
                extraDashCharges);
            Simulator.Tick(input, deltaTime);
            SetCapsuleHeight(Simulator.Sliding ? _slidingHeight : _standingHeight);

            Vector3 walk = Simulator.HorizontalVelocity * deltaTime;
            if (!Simulator.JumpedThisTick)
            {
                walk = _engine.GetProjectedMovement(walk);
            }

            float rise = Simulator.VerticalVelocity * deltaTime;
            _engine.MovePlayer(deltaTime, walk, Vector3.up * rise);

            Simulator.ResolveHorizontal(walk, _engine.GetMoveDisplacement(0), deltaTime);
            Simulator.ResolveVertical(rise, _engine.GetMoveDisplacement(1).y);
        }

        private bool HasRoomToStand()
        {
            float missing = _standingHeight - _capsule.height;
            if (missing <= 0f)
            {
                return true;
            }

            Transform body = _engine.transform;
            return !_engine.ColliderCast.CastSelf(
                body.position,
                body.rotation,
                Vector3.up,
                missing,
                out _,
                _engine.LayerMask,
                QueryTriggerInteraction.Ignore,
                _engine.SkinWidth);
        }

        private void SetCapsuleHeight(float height)
        {
            if (Mathf.Approximately(_capsule.height, height))
            {
                return;
            }

            _capsule.height = height;
            _capsule.center = new Vector3(0f, height * 0.5f, 0f);
        }
    }
}
