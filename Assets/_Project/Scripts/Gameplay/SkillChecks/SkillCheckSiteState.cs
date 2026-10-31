using System;
using Game.Core;
using Game.Data;

namespace Game.Gameplay.SkillChecks
{
    /// <summary>One attempt per site; after the roll the site is locked whatever the result.</summary>
    public sealed class SkillCheckSiteState
    {
        private readonly SkillCheckSiteDef _definition;

        public SkillCheckSiteState(SkillCheckSiteDef definition)
        {
            _definition = definition != null ? definition : throw new ArgumentNullException(nameof(definition));
        }

        public bool IsLocked { get; private set; }

        public SkillCheckOutcome Attempt(
            StatSheet stats, CharacterDef character, SkillCheck skillCheck, EffectLoadout loadout, int nat20RewardCopies)
        {
            if (IsLocked)
            {
                throw new InvalidOperationException($"{_definition.name} was already attempted");
            }

            IsLocked = true;
            SkillDef skill = _definition.Skill;
            SkillCheckRequest request = SkillCheckRequest.FromStats(stats, skill.Ability, character.IsProficient(skill), _definition.Dc);
            SkillCheckResult result = skillCheck.Resolve(request);

            int copies = 0;
            if (result.Success)
            {
                copies = result.Nat20 ? nat20RewardCopies : 1;
                for (int i = 0; i < copies; i++)
                {
                    loadout.Add(_definition.Reward);
                }
            }

            loadout.Raise(new TriggerArgs(GameTrigger.SkillCheckResolved, result.Success ? 1f : 0f));
            return new SkillCheckOutcome(request, result, copies);
        }
    }
}
