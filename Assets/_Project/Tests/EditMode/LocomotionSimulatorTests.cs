using System;
using Game.Core;
using Game.Data;
using Game.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    public class LocomotionSimulatorTests
    {
        private const float Dt = 0.02f;
        private const float RunSpeed = 6f;

        private static readonly Vector3 Forward = Vector3.forward;

        [Test]
        public void Ground_AcceleratesToRunSpeed_NotInstantly()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(groundAcceleration: 60f));

            Step(sim, Forward, grounded: true);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(60f * Dt).Within(1e-4f));

            Step(sim, Forward, grounded: true, ticks: 20);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(RunSpeed).Within(1e-4f));
        }

        [Test]
        public void Ground_WithoutInput_StopsWithDeceleration()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);

            Step(sim, Vector3.zero, grounded: true);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(RunSpeed - 50f * Dt).Within(1e-4f));

            Step(sim, Vector3.zero, grounded: true, ticks: 20);
            Assert.That(sim.HorizontalVelocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Air_WithoutInput_KeepsMomentum()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);

            Step(sim, Vector3.zero, grounded: false, ticks: 20);

            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(RunSpeed).Within(1e-4f));
        }

        [Test]
        public void Air_Steering_TurnsButNeverExceedsRunSpeed()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);

            Step(sim, Vector3.right, grounded: false, ticks: 20);

            Vector3 velocity = sim.HorizontalVelocity;
            Assert.That(velocity.x, Is.GreaterThan(1f), "air control should steer");
            Assert.That(velocity.magnitude, Is.LessThanOrEqualTo(RunSpeed + 1e-4f));
        }

        [Test]
        public void Air_KeepsMomentumAboveRunSpeed()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20, runSpeed: 12f);

            Step(sim, Forward, grounded: false, ticks: 10);

            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(12f).Within(1e-4f));
        }

        [Test]
        public void Jump_ReachesConfiguredHeight()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(jumpHeight: 1.6f));
            Step(sim, Vector3.zero, grounded: true);

            Step(sim, Vector3.zero, grounded: true, jump: true);
            Assert.That(sim.JumpedThisTick, Is.True);

            float height = sim.VerticalVelocity * Dt;
            while (sim.VerticalVelocity > 0f)
            {
                Step(sim, Vector3.zero, grounded: false);
                height += Mathf.Max(0f, sim.VerticalVelocity) * Dt;
            }

            // Discrete integration overshoots the analytic apex by at most one tick of rise.
            Assert.That(height, Is.EqualTo(1.6f).Within(0.15f));
        }

        [Test]
        public void Jump_GroundProbeStillTouching_DoesNotCancelRise()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Vector3.zero, grounded: true, jump: true);
            float launch = sim.VerticalVelocity;

            Step(sim, Vector3.zero, grounded: true);

            Assert.That(sim.VerticalVelocity, Is.GreaterThan(0f));
            Assert.That(sim.VerticalVelocity, Is.LessThan(launch));
        }

        [Test]
        public void Coyote_JumpShortlyAfterLeavingGround_Works()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(coyoteTime: 0.12f));
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Vector3.zero, grounded: false, ticks: 5);

            Step(sim, Vector3.zero, grounded: false, jump: true);

            Assert.That(sim.JumpedThisTick, Is.True);
        }

        [Test]
        public void Coyote_Expired_NoJumpWithoutExtraJumps()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(coyoteTime: 0.12f));
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Vector3.zero, grounded: false, ticks: 10);

            Step(sim, Vector3.zero, grounded: false, jump: true);

            Assert.That(sim.JumpedThisTick, Is.False);
            Assert.That(sim.VerticalVelocity, Is.LessThan(0f));
        }

        [Test]
        public void Buffer_PressShortlyBeforeLanding_JumpsOnLanding()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(coyoteTime: 0f, jumpBufferTime: 0.15f));
            Step(sim, Vector3.zero, grounded: false, ticks: 10);
            Step(sim, Vector3.zero, grounded: false, jump: true);
            Assert.That(sim.JumpedThisTick, Is.False);
            Step(sim, Vector3.zero, grounded: false, ticks: 4);

            Step(sim, Vector3.zero, grounded: true);

            Assert.That(sim.JumpedThisTick, Is.True);
        }

        [Test]
        public void Buffer_PressTooEarly_IsForgotten()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement(coyoteTime: 0f, jumpBufferTime: 0.15f));
            Step(sim, Vector3.zero, grounded: false, jump: true);
            Step(sim, Vector3.zero, grounded: false, ticks: 10);

            Step(sim, Vector3.zero, grounded: true);

            Assert.That(sim.JumpedThisTick, Is.False);
        }

        [Test]
        public void ExtraJumps_AllowThatManyAirJumps_ResetOnLanding()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Vector3.zero, grounded: true, jump: true, extraJumps: 2);

            for (int i = 0; i < 2; i++)
            {
                Step(sim, Vector3.zero, grounded: false, ticks: 5, extraJumps: 2);
                Step(sim, Vector3.zero, grounded: false, jump: true, extraJumps: 2);
                Assert.That(sim.JumpedThisTick, Is.True, $"air jump {i + 1}");
            }

            Step(sim, Vector3.zero, grounded: false, ticks: 5, extraJumps: 2);
            Step(sim, Vector3.zero, grounded: false, jump: true, extraJumps: 2);
            Assert.That(sim.JumpedThisTick, Is.False, "third air jump");

            while (sim.VerticalVelocity > 0f)
            {
                Step(sim, Vector3.zero, grounded: false, extraJumps: 2);
            }

            Step(sim, Vector3.zero, grounded: true, extraJumps: 2);
            Assert.That(sim.AirJumpsUsed, Is.Zero);
        }

        [Test]
        public void NegativeExtraJumps_StillAllowGroundJump()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true, extraJumps: -1);

            Step(sim, Vector3.zero, grounded: true, jump: true, extraJumps: -1);

            Assert.That(sim.JumpedThisTick, Is.True);
        }

        [Test]
        public void Falling_IsCappedAtMaxFallSpeed()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());

            Step(sim, Vector3.zero, grounded: false, ticks: 500);

            Assert.That(sim.VerticalVelocity, Is.EqualTo(-40f));
        }

        [Test]
        public void ResolveHorizontal_Blocked_DropsLostSpeed()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Vector3 intended = sim.HorizontalVelocity * Dt;

            sim.ResolveHorizontal(intended, new Vector3(0.03f, 0f, 0f), Dt);

            Assert.That(sim.HorizontalVelocity.x, Is.EqualTo(0.03f / Dt).Within(1e-3f));
            Assert.That(sim.HorizontalVelocity.z, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void ResolveHorizontal_Unblocked_KeepsVelocity()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Vector3 before = sim.HorizontalVelocity;

            sim.ResolveHorizontal(before * Dt, before * Dt, Dt);

            Assert.That(sim.HorizontalVelocity, Is.EqualTo(before));
        }

        [Test]
        public void ResolveVertical_Ceiling_EndsRise()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Vector3.zero, grounded: true, jump: true);

            sim.ResolveVertical(sim.VerticalVelocity * Dt, 0f);

            Assert.That(sim.VerticalVelocity, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(-0.02f)]
        public void Tick_NonPositiveDeltaTime_Throws(float deltaTime)
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => sim.Tick(new LocomotionInput(Forward, false, true, RunSpeed, 0), deltaTime));
        }

        [Test]
        public void MovementTuning_Valid_HasNoErrors_AndBadValuesAreReported()
        {
            var assets = new TestAssets();
            try
            {
                GameBalance balance = assets.Balance();
                Assert.That(Validate(balance).HasErrors, Is.False);

                PrivateField.Set(balance, "_movement", new MovementTuning(60f, 50f, 15f, 0f, 40f, 1.6f, 0.1f, 0.1f, 95f, 0.3f, 0.01f));
                ValidationReport report = Validate(balance);
                Assert.That(report.Errors, Has.Some.Matches<ValidationError>(e => e.Message.Contains("_gravity")));
                Assert.That(report.Errors, Has.Some.Matches<ValidationError>(e => e.Message.Contains("_maxWalkAngle")));
            }
            finally
            {
                assets.DestroyAll();
            }
        }

        private static ValidationReport Validate(GameBalance balance)
        {
            var report = new ValidationReport();
            balance.Validate(report);
            return report;
        }

        private static void Step(
            LocomotionSimulator sim,
            Vector3 wish,
            bool grounded,
            int ticks = 1,
            bool jump = false,
            int extraJumps = 0,
            float runSpeed = RunSpeed)
        {
            for (int i = 0; i < ticks; i++)
            {
                sim.Tick(new LocomotionInput(wish, jump && i == 0, grounded, runSpeed, extraJumps), Dt);
            }
        }
    }
}
