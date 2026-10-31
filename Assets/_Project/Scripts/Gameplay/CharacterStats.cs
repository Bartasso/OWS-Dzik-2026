using Game.Core;
using Game.Data;

namespace Game.Gameplay
{
    public static class CharacterStats
    {
        public static StatSheet Build(CharacterDef character, GameBalance balance)
        {
            var stats = new StatSheet(balance.BuildStatLimits());
            foreach (StatValue value in character.BaseStats)
            {
                stats.SetBase(value.Stat, value.Value);
            }

            foreach (Ability ability in character.PrimaryAbilities)
            {
                StatId stat = StatIds.FromAbility(ability);
                stats.SetBase(stat, stats.GetBase(stat) + balance.PrimaryAbilityBonus);
            }

            return stats;
        }
    }
}
