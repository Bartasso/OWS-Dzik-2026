using Game.Gameplay.Movement;
using nickmaltbie.OpenKCC.Character;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Game.Tests.EditMode
{
    /// <summary>The locomotion simulation driving the OpenKCC capsule against real PhysX geometry.</summary>
    public class CharacterLocomotionTests
    {
        private const float Dt = 0.02f;
        private const float RunSpeed = 6f;
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
        public void Jump_RisesToJumpHeightAndLandsGrounded()
        {
            _scene.Floor();
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();
            Tick(locomotion, Vector3.zero, ticks: 3);

            Tick(locomotion, Vector3.zero, jump: true);
            Assert.That(locomotion.Grounded, Is.False);

            float apex = 0f;
            for (int i = 0; i < 100 && !locomotion.Grounded; i++)
            {
                Tick(locomotion, Vector3.zero);
                apex = Mathf.Max(apex, Local(kcc).y);
            }

            Assert.That(apex, Is.EqualTo(1.6f).Within(0.2f));
            Assert.That(locomotion.Grounded, Is.True);
            Assert.That(Local(kcc).y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(locomotion.Simulator.VerticalVelocity, Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void Run_ClimbsLowStepWithoutJumping()
        {
            _scene.Floor();
            _scene.Box(new Vector3(1.5f, 0f, -5f), new Vector3(10f, 0.25f, 10f));
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();

            Tick(locomotion, Forward, ticks: 60);

            Assert.That(Local(kcc).x, Is.GreaterThan(3f));
            Assert.That(Local(kcc).y, Is.EqualTo(0.25f).Within(Tolerance));
            Assert.That(locomotion.Grounded, Is.True);
        }

        [Test]
        public void Run_IntoWall_LosesMomentumInsteadOfStoringIt()
        {
            _scene.Floor();
            _scene.Box(new Vector3(2f, 0f, -5f), new Vector3(1f, 3f, 10f));
            (_, CharacterLocomotion locomotion) = Character();

            Tick(locomotion, Forward, ticks: 40);

            Assert.That(locomotion.Simulator.HorizontalVelocity.magnitude, Is.LessThan(1f));
        }

        [Test]
        public void Jump_UnderCeiling_StopsRising()
        {
            _scene.Floor();
            _scene.Box(new Vector3(-5f, 2.5f, -5f), new Vector3(10f, 1f, 10f));
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();
            Tick(locomotion, Vector3.zero, ticks: 3);

            Tick(locomotion, Vector3.zero, jump: true);
            Tick(locomotion, Vector3.zero, ticks: 10);

            Assert.That(Local(kcc).y, Is.LessThan(0.5f + Tolerance), "head stays below the ceiling");
            Assert.That(locomotion.Simulator.VerticalVelocity, Is.LessThanOrEqualTo(0f));
        }

        [Test]
        public void Tick_DoesNotAllocate()
        {
            _scene.Floor();
            _scene.Box(new Vector3(2f, 0f, -5f), new Vector3(1f, 3f, 10f));
            (_, CharacterLocomotion locomotion) = Character();
            Tick(locomotion, Forward, ticks: 40);

            Assert.That(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    locomotion.Tick(Forward, i % 10 == 0, RunSpeed, 1, Dt);
                }
            }, Is.Not.AllocatingGCMemory());
        }

        private (KCCMovementEngine, CharacterLocomotion) Character()
        {
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);
            return (kcc, new CharacterLocomotion(kcc, TestAssets.Movement()));
        }

        private static void Tick(CharacterLocomotion locomotion, Vector3 wish, int ticks = 1, bool jump = false)
        {
            for (int i = 0; i < ticks; i++)
            {
                locomotion.Tick(wish, jump && i == 0, RunSpeed, 0, Dt);
            }
        }

        private static Vector3 Local(KCCMovementEngine kcc)
        {
            return kcc.transform.position - KccTestScene.Origin;
        }
    }
}
