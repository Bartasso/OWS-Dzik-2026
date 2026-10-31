using Game.Core;

namespace Game.Gameplay.SkillChecks
{
    public readonly struct SkillCheckOutcome
    {
        public SkillCheckOutcome(in SkillCheckRequest request, in SkillCheckResult result, int rewardCopies)
        {
            Request = request;
            Result = result;
            RewardCopies = rewardCopies;
        }

        public SkillCheckRequest Request { get; }

        public SkillCheckResult Result { get; }

        /// <summary>0 on failure.</summary>
        public int RewardCopies { get; }
    }
}
