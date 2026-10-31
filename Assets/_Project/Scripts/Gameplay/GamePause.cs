using System;

namespace Game.Gameplay
{
    /// <summary>
    /// Simulation pause (skill check roll, death). Systems check IsPaused instead of relying on Time.timeScale,
    /// so the pause can later be synced as game state in co-op. Holds stack: each Push needs a Pop.
    /// </summary>
    public sealed class GamePause
    {
        private int _holds;

        public bool IsPaused => _holds > 0;

        public void Push()
        {
            _holds++;
        }

        public void Pop()
        {
            if (_holds == 0)
            {
                throw new InvalidOperationException("GamePause.Pop without a matching Push");
            }

            _holds--;
        }
    }
}
