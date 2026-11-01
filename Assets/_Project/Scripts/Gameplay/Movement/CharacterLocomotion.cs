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
    /// </summary>
    public sealed class CharacterLocomotion
    {
        private readonly KCCMovementEngine _engine;

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

            _engine = engine;
            _engine.stepHeight = tuning.StepHeight;
            _engine.maxWalkAngle = tuning.MaxWalkAngle;
            _engine.skinWidth = tuning.SkinWidth;
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
        public void Tick(Vector3 wishDirection, bool jumpPressed, float runSpeed, int extraJumps, float deltaTime)
        {
            var input = new LocomotionInput(wishDirection, jumpPressed, Grounded, runSpeed, extraJumps);
            Simulator.Tick(input, deltaTime);

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
    }
}
