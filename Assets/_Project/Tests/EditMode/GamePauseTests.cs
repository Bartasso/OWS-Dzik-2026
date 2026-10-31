using System;
using Game.Gameplay;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class GamePauseTests
    {
        [Test]
        public void Holds_Stack()
        {
            var pause = new GamePause();
            Assert.That(pause.IsPaused, Is.False);

            pause.Push();
            pause.Push();
            pause.Pop();
            Assert.That(pause.IsPaused, Is.True);

            pause.Pop();
            Assert.That(pause.IsPaused, Is.False);
        }

        [Test]
        public void PopWithoutPush_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => new GamePause().Pop());
        }
    }
}
