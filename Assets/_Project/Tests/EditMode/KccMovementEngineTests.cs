using System;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Regression scenarios for the vendored OpenKCC collision core, run against real PhysX queries.
    /// They pin current behaviour so later changes to the copy cannot silently break it.
    /// </summary>
    public class KccMovementEngineTests
    {
        private const float Dt = 0.02f;
        private const float Speed = 5f;
        private const float Tolerance = 0.05f;

        private static readonly Vector3 Forward = Vector3.right;

        private KccTestScene _scene;

        [SetUp]
        public void SetUp()
        {
            _scene = new KccTestScene();
        }

        [TearDown]
        public void TearDown()
        {
            _scene.Destroy();
        }

        [Test]
        public void FlatGround_Walk_MovesFullDistanceAndStaysGrounded()
        {
            _scene.Floor();
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, Forward, 20);

            Vector3 feet = Local(kcc);
            Assert.That(feet.x, Is.EqualTo(Speed * Dt * 20).Within(Tolerance));
            Assert.That(feet.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(kcc.GroundedState.StandingOnGround, Is.True);
            Assert.That(kcc.GroundedState.Sliding, Is.False);
        }

        [Test]
        public void NoGround_IsFalling()
        {
            _scene.Floor();
            KCCMovementEngine kcc = _scene.Character(Vector3.up * 5f);

            Settle(kcc);

            Assert.That(kcc.GroundedState.OnGround, Is.False);
            Assert.That(kcc.GroundedState.Falling, Is.True);
        }

        [Test]
        public void Wall_HeadOn_StopsAtWall()
        {
            const float wallX = 2f;
            _scene.Floor();
            _scene.Box(new Vector3(wallX, 0f, -5f), new Vector3(1f, 3f, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, Forward, 40);

            float stopX = wallX - KccTestScene.CapsuleRadius;
            Assert.That(Local(kcc).x, Is.EqualTo(stopX).Within(Tolerance));
            Assert.That(kcc.GroundedState.StandingOnGround, Is.True);
        }

        [Test]
        public void Wall_Diagonal_SlidesAlongWall()
        {
            const float wallX = 1f;
            _scene.Floor();
            _scene.Box(new Vector3(wallX, 0f, -10f), new Vector3(1f, 3f, 20f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, new Vector3(1f, 0f, 1f).normalized, 40);

            Vector3 feet = Local(kcc);
            Assert.That(feet.x, Is.LessThanOrEqualTo(wallX - KccTestScene.CapsuleRadius + Tolerance));
            Assert.That(feet.z, Is.GreaterThan(1f), "character should slide along the wall, not stick to it");
        }

        [Test]
        public void Step_BelowStepHeight_ClimbsOnto()
        {
            const float stepHeight = 0.2f;
            _scene.Floor();
            _scene.Box(new Vector3(1f, 0f, -5f), new Vector3(10f, stepHeight, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, Forward, 30);

            Vector3 feet = Local(kcc);
            Assert.That(feet.x, Is.GreaterThan(1.5f));
            Assert.That(feet.y, Is.EqualTo(stepHeight).Within(Tolerance));
            Assert.That(kcc.GroundedState.StandingOnGround, Is.True);
        }

        [Test]
        public void Step_AboveStepHeight_Blocks()
        {
            const float stepX = 1f;
            _scene.Floor();
            _scene.Box(new Vector3(stepX, 0f, -5f), new Vector3(10f, 0.6f, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, Forward, 30);

            Vector3 feet = Local(kcc);
            Assert.That(feet.x, Is.LessThanOrEqualTo(stepX - KccTestScene.CapsuleRadius + Tolerance));
            Assert.That(feet.y, Is.EqualTo(0f).Within(Tolerance));
        }

        [Test]
        public void Step_ClimbReportsSnapUpBounce()
        {
            _scene.Floor();
            _scene.Box(new Vector3(0.6f, 0f, -5f), new Vector3(10f, 0.2f, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);
            Settle(kcc);

            bool snappedUp = false;
            for (int tick = 0; tick < 30 && !snappedUp; tick++)
            {
                kcc.MovePlayer(Dt, Forward * Speed * Dt);
                for (int i = 0; i < kcc.LastBounces.Count; i++)
                {
                    snappedUp |= kcc.LastBounces[i].action == KCCUtils.MovementAction.SnapUp;
                }
            }

            Assert.That(snappedUp, Is.True);
        }

        [Test]
        public void Ledge_SmallDrop_SnapsDownAndStaysGrounded()
        {
            const float drop = 0.2f;
            _scene.Box(new Vector3(-10f, -1f, -5f), new Vector3(11f, 1f, 10f));
            _scene.Box(new Vector3(1f, -1f - drop, -5f), new Vector3(10f, 1f, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Walk(kcc, Forward, 30);

            Vector3 feet = Local(kcc);
            Assert.That(feet.x, Is.GreaterThan(2f));
            Assert.That(feet.y, Is.EqualTo(-drop).Within(Tolerance));
            Assert.That(kcc.GroundedState.StandingOnGround, Is.True);
        }

        [TestCase(30f, false)]
        [TestCase(70f, true)]
        public void Slope_SlidesOnlyAboveMaxWalkAngle(float angle, bool sliding)
        {
            _scene.Slope(angle);
            float radius = KccTestScene.CapsuleRadius;
            float feetY = radius / Mathf.Cos(angle * Mathf.Deg2Rad) - radius + KccTestScene.Hover;
            KCCMovementEngine kcc = _scene.Character(Vector3.up * feetY);

            kcc.CheckGrounded(false, false);

            Assert.That(kcc.GroundedState.StandingOnGround, Is.True);
            Assert.That(kcc.GroundedState.Angle, Is.EqualTo(angle).Within(1f));
            Assert.That(kcc.GroundedState.Sliding, Is.EqualTo(sliding));
        }

        [Test]
        public void Overlap_PushesOut()
        {
            _scene.Floor();
            _scene.Box(new Vector3(0.2f, 0.5f, -0.5f), Vector3.one);
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Settle(kcc);

            Assert.That(Local(kcc).x, Is.LessThanOrEqualTo(0.2f - KccTestScene.CapsuleRadius + Tolerance));
        }

        [Test]
        public void Teleport_ResetsVelocityTracking()
        {
            _scene.Floor();
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);
            Walk(kcc, Forward, 10);

            kcc.TeleportPlayer(KccTestScene.Origin + new Vector3(-5f, KccTestScene.Hover, 0f));
            Settle(kcc);

            Assert.That(kcc.worldVelocity.Average().magnitude, Is.LessThan(Tolerance));
        }

        [TestCase(0f)]
        [TestCase(-0.02f)]
        public void MovePlayer_NonPositiveDeltaTime_Throws(float deltaTime)
        {
            _scene.Floor();
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            Assert.Throws<ArgumentOutOfRangeException>(() => kcc.MovePlayer(deltaTime, Forward));
        }

        [Test]
        public void MovePlayer_DoesNotAllocate()
        {
            _scene.Floor();
            _scene.Box(new Vector3(1f, 0f, -5f), new Vector3(10f, 0.2f, 10f));
            _scene.Box(new Vector3(3f, 0f, -5f), new Vector3(1f, 3f, 10f));
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);

            // Warm up: first-use lookups, then reach the wall so every tick bounces.
            Walk(kcc, Forward, 40);

            Vector3 move = Forward * Speed * Dt;
            Vector3 vertical = Vector3.down * 0.01f;
            Assert.That(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    kcc.MovePlayer(Dt, move, vertical);
                }
            }, Is.Not.AllocatingGCMemory());
        }

        private static void Walk(KCCMovementEngine kcc, Vector3 direction, int ticks)
        {
            Settle(kcc);
            Vector3 move = direction * Speed * Dt;
            for (int i = 0; i < ticks; i++)
            {
                kcc.MovePlayer(Dt, move);
            }

            // A few still ticks let the clamped snap-down finish.
            for (int i = 0; i < 5; i++)
            {
                kcc.MovePlayer(Dt, Vector3.zero);
            }
        }

        /// <summary>One still tick so the grounded state is known before moving.</summary>
        private static void Settle(KCCMovementEngine kcc)
        {
            kcc.MovePlayer(Dt, Vector3.zero);
        }

        private static Vector3 Local(KCCMovementEngine kcc)
        {
            return kcc.transform.position - KccTestScene.Origin;
        }
    }
}
