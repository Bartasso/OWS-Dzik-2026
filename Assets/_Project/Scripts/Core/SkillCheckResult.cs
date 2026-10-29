namespace Game.Core
{
    public readonly struct SkillCheckResult
    {
        public SkillCheckResult(int natural, int total, bool success, bool wasRerolled)
        {
            Natural = natural;
            Total = total;
            Success = success;
            WasRerolled = wasRerolled;
        }

        /// <summary>The final d20 face, after advantage/disadvantage and the LCK reroll.</summary>
        public int Natural { get; }

        public int Total { get; }

        public bool Success { get; }

        /// <summary>A natural 1 was rerolled thanks to LCK; Natural holds the reroll.</summary>
        public bool WasRerolled { get; }

        public bool Nat20 => Natural == 20;

        public bool Nat1 => Natural == 1;
    }
}
