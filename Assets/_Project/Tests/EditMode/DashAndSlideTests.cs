using Game.Gameplay.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>Dash, slide and slide-jump in the locomotion simulation (values from TestAssets.Movement).</summary>
    public class DashAndSlideTests
    {
        private const float Dt = 0.02f;
        private const float RunSpeed = 6f;
        private const float DashSpeed = 22f;

        private static readonly Vector3 Forward = Vector3.forward;

        [Test]
        public void Dash_MovesAtDashSpeedWithoutGravity_ThenLeavesRunSpeed()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: false, ticks: 5);

            Step(sim, Forward, grounded: false, dash: true);
            Assert.That(sim.Dash.StartedThisTick, Is.True);
            Assert.That(sim.HorizontalVelocity, Is.EqualTo(Forward * DashSpeed));
            Assert.That(sim.VerticalVelocity, Is.Zero);

            Step(sim, Forward, grounded: false, ticks: 10);
            Assert.That(sim.Dash.Active, Is.False);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(RunSpeed).Within(0.5f));
            Assert.That(sim.VerticalVelocity, Is.LessThan(0f), "gravity is back after the dash");
        }

        [Test]
        public void Dash_WithoutInput_GoesAlongFacing()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);

            Step(sim, Vector3.zero, grounded: true, dash: true, facing: Vector3.right);

            Assert.That(sim.HorizontalVelocity, Is.EqualTo(Vector3.right * DashSpeed));
        }

        [Test]
        public void Dash_SpendsChargeAndRechargesAfterCooldown()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);
            Step(sim, Forward, grounded: true, dash: true);
            Step(sim, Forward, grounded: true, ticks: 10);

            Step(sim, Forward, grounded: true, dash: true);
            Assert.That(sim.Dash.StartedThisTick, Is.False, "no charge left");

            Step(sim, Forward, grounded: true, ticks: 90);
            Step(sim, Forward, grounded: true, dash: true);
            Assert.That(sim.Dash.StartedThisTick, Is.True, "recharged after 2 s");
        }

        [Test]
        public void ExtraDashCharges_AllowBackToBackDashes()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true, extraDashCharges: 1);
            Step(sim, Forward, grounded: true, dash: true, extraDashCharges: 1);
            Step(sim, Forward, grounded: true, ticks: 10, extraDashCharges: 1);

            Step(sim, Forward, grounded: true, dash: true, extraDashCharges: 1);

            Assert.That(sim.Dash.StartedThisTick, Is.True);
        }

        [Test]
        public void AirDash_OncePerAirtime_GivenBackByAJump()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: false, ticks: 10, extraDashCharges: 2, extraJumps: 1);
            Step(sim, Forward, grounded: false, dash: true, extraDashCharges: 2, extraJumps: 1);
            Step(sim, Forward, grounded: false, ticks: 10, extraDashCharges: 2, extraJumps: 1);

            Step(sim, Forward, grounded: false, dash: true, extraDashCharges: 2, extraJumps: 1);
            Assert.That(sim.Dash.StartedThisTick, Is.False, "second air dash without landing or jumping");

            Step(sim, Forward, grounded: false, jump: true, extraDashCharges: 2, extraJumps: 1);
            Assert.That(sim.JumpedThisTick, Is.True);
            Step(sim, Forward, grounded: false, dash: true, extraDashCharges: 2, extraJumps: 1);
            Assert.That(sim.Dash.StartedThisTick, Is.True, "the air jump gives the air dash back");
        }

        [Test]
        public void Dash_UnderLowCeiling_IsIgnored()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);

            Step(sim, Forward, grounded: true, dash: true, canStand: false);

            Assert.That(sim.Dash.StartedThisTick, Is.False);
        }

        [Test]
        public void Slide_StartsOnlyAboveStartSpeed()
        {
            var slow = new LocomotionSimulator(TestAssets.Movement());
            Step(slow, Forward, grounded: true, ticks: 20, runSpeed: 3f);
            Step(slow, Forward, grounded: true, slide: true, runSpeed: 3f);
            Assert.That(slow.Sliding, Is.False);

            var fast = new LocomotionSimulator(TestAssets.Movement());
            Step(fast, Forward, grounded: true, ticks: 20);
            Step(fast, Forward, grounded: true, slide: true);
            Assert.That(fast.Sliding, Is.True);
        }

        [Test]
        public void Slide_OnFlatGround_LosesSpeedToFrictionOnly()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);

            Step(sim, Vector3.zero, grounded: true, ticks: 10, slide: true);

            Assert.That(sim.Sliding, Is.True);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(RunSpeed - 4f * Dt * 10).Within(1e-3f));
        }

        [Test]
        public void Slide_GainsSpeedDownhill_LosesItUphill()
        {
            // 20° slope falling towards +Z.
            Vector3 normal = Quaternion.Euler(20f, 0f, 0f) * Vector3.up;

            var downhill = new LocomotionSimulator(TestAssets.Movement());
            Step(downhill, Forward, grounded: true, ticks: 20, normal: normal);
            Step(downhill, Vector3.zero, grounded: true, ticks: 10, slide: true, normal: normal);
            Assert.That(downhill.HorizontalVelocity.magnitude, Is.GreaterThan(RunSpeed));

            var uphill = new LocomotionSimulator(TestAssets.Movement());
            Step(uphill, -Forward, grounded: true, ticks: 20, normal: normal);
            Step(uphill, Vector3.zero, grounded: true, ticks: 10, slide: true, normal: normal);
            Assert.That(uphill.HorizontalVelocity.magnitude, Is.LessThan(RunSpeed - 4f * Dt * 10));
        }

        [Test]
        public void Slide_EndsWhenReleased_UnlessThereIsNoRoomToStand()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Step(sim, Forward, grounded: true, slide: true);

            Step(sim, Forward, grounded: true, canStand: false);
            Assert.That(sim.Sliding, Is.True, "stays crouched under a low ceiling");

            Step(sim, Forward, grounded: true);
            Assert.That(sim.Sliding, Is.False);
        }

        [Test]
        public void Slide_TooSlowUnderLowCeiling_CrawlsOut()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Step(sim, Forward, grounded: true, slide: true);

            Step(sim, Forward, grounded: true, ticks: 200, canStand: false);

            Assert.That(sim.Sliding, Is.True);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(2.5f).Within(0.2f));
        }

        [Test]
        public void Slide_LeavingTheGround_EndsIt()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Step(sim, Forward, grounded: true, slide: true);

            Step(sim, Forward, grounded: false, slide: true);

            Assert.That(sim.Sliding, Is.False);
        }

        [Test]
        public void SlideJump_RightAfterSlideStarts_AddsBoost()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Step(sim, Forward, grounded: true, slide: true);
            float before = sim.HorizontalVelocity.magnitude;

            Step(sim, Forward, grounded: true, slide: true, jump: true);

            Assert.That(sim.JumpedThisTick, Is.True);
            Assert.That(sim.Sliding, Is.False);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.GreaterThan(before + 0.5f));
        }

        [Test]
        public void SlideJump_LateInTheSlide_GetsNoBoost()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 20);
            Step(sim, Vector3.zero, grounded: true, ticks: 20, slide: true);
            float before = sim.HorizontalVelocity.magnitude;

            Step(sim, Vector3.zero, grounded: true, slide: true, jump: true);

            Assert.That(sim.JumpedThisTick, Is.True);
            Assert.That(sim.HorizontalVelocity.magnitude, Is.LessThanOrEqualTo(before));
        }

        [Test]
        public void SlideJump_AtSoftSpeedCap_GetsNoBoost()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 30, runSpeed: 14f);
            Step(sim, Vector3.zero, grounded: true, slide: true, runSpeed: 14f);
            float before = sim.HorizontalVelocity.magnitude;

            Step(sim, Vector3.zero, grounded: true, slide: true, jump: true, runSpeed: 14f);

            Assert.That(sim.HorizontalVelocity.magnitude, Is.LessThanOrEqualTo(before + 0.05f));
        }

        [Test]
        public void Ground_SpeedAboveRunSpeed_BleedsOffSlowly()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Forward, grounded: true, ticks: 30, runSpeed: 12f);

            Step(sim, Forward, grounded: true);

            Assert.That(sim.HorizontalVelocity.magnitude, Is.EqualTo(12f - 8f * Dt).Within(1e-3f));
        }

        [Test]
        public void Jump_UnderLowCeiling_WaitsInBufferUntilThereIsRoom()
        {
            var sim = new LocomotionSimulator(TestAssets.Movement());
            Step(sim, Vector3.zero, grounded: true);

            Step(sim, Vector3.zero, grounded: true, jump: true, canStand: false);
            Assert.That(sim.JumpedThisTick, Is.False);

            Step(sim, Vector3.zero, grounded: true);
            Assert.That(sim.JumpedThisTick, Is.True);
        }

        private static void Step(
            LocomotionSimulator sim,
            Vector3 wish,
            bool grounded,
            int ticks = 1,
            bool jump = false,
            bool dash = false,
            bool slide = false,
            bool canStand = true,
            Vector3 facing = default,
            Vector3 normal = default,
            float runSpeed = RunSpeed,
            int extraJumps = 0,
            int extraDashCharges = 0)
        {
            for (int i = 0; i < ticks; i++)
            {
                bool first = i == 0;
                var input = new LocomotionInput(
                    wish,
                    jump && first,
                    grounded,
                    runSpeed,
                    extraJumps,
                    dash && first,
                    slide,
                    facing,
                    normal,
                    canStand,
                    extraDashCharges);
                sim.Tick(input, Dt);
            }
        }
    }
}
