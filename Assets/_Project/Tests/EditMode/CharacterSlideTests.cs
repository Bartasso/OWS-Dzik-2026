using Game.Gameplay.Movement;
using nickmaltbie.OpenKCC.Character;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>Sliding with the real capsule: shrinking under low geometry and standing up only with room.</summary>
    public class CharacterSlideTests
    {
        private const float Dt = 0.02f;
        private const float RunSpeed = 6f;
        private const float Clearance = 1.2f;

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
        public void LowBeam_BlocksWalking()
        {
            _scene.Floor();
            _scene.Box(new Vector3(6f, Clearance, -5f), new Vector3(0.5f, 0.4f, 10f));
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();

            Tick(locomotion, slide: false, ticks: 80);

            Assert.That(Local(kcc).x, Is.LessThan(6f));
        }

        [Test]
        public void LowBeam_SlideGoesUnderAndStandsUpAfter()
        {
            _scene.Floor();
            _scene.Box(new Vector3(6f, Clearance, -5f), new Vector3(0.5f, 0.4f, 10f));
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();
            Tick(locomotion, slide: false, ticks: 30);

            Tick(locomotion, slide: true, ticks: 50);
            Tick(locomotion, slide: false, ticks: 10);

            Assert.That(Local(kcc).x, Is.GreaterThan(7f));
            Assert.That(locomotion.Simulator.Sliding, Is.False);
            Assert.That(kcc.GetComponent<CapsuleCollider>().height, Is.EqualTo(KccTestScene.CapsuleHeight));
        }

        [Test]
        public void LongLowCeiling_ReleasingSlide_StaysCrouchedAndCrawlsOut()
        {
            _scene.Floor();
            _scene.Box(new Vector3(5f, Clearance, -5f), new Vector3(4f, 0.4f, 10f));
            (KCCMovementEngine kcc, CharacterLocomotion locomotion) = Character();
            Tick(locomotion, slide: false, ticks: 30);
            Tick(locomotion, slide: true, ticks: 25);
            Assume.That(Local(kcc).x, Is.InRange(5f, 9f), "setup: should be under the ceiling now");

            Tick(locomotion, slide: false, ticks: 5);
            Assert.That(locomotion.Simulator.Sliding, Is.True, "no room to stand under the ceiling");

            Tick(locomotion, slide: false, ticks: 150);
            Assert.That(Local(kcc).x, Is.GreaterThan(9f));
            Assert.That(locomotion.Simulator.Sliding, Is.False);
        }

        private (KCCMovementEngine, CharacterLocomotion) Character()
        {
            KCCMovementEngine kcc = _scene.Character(Vector3.up * KccTestScene.Hover);
            return (kcc, new CharacterLocomotion(kcc, TestAssets.Movement()));
        }

        private static void Tick(CharacterLocomotion locomotion, bool slide, int ticks)
        {
            for (int i = 0; i < ticks; i++)
            {
                locomotion.Tick(Forward, false, RunSpeed, 0, Dt, slideHeld: slide);
            }
        }

        private static Vector3 Local(KCCMovementEngine kcc)
        {
            return kcc.transform.position - KccTestScene.Origin;
        }
    }
}
