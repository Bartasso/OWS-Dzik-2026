using Game.Core;
using Game.Data;

namespace Game.Gameplay.Combat
{
    /// <summary>
    /// Provisional M0 damage: base × ability factor (+5% per point of the scaling ability), × crit multiplier on a crit.
    /// Hits always hit (no d20 in combat). The full pipeline (weapon levels, armor) is still an open question.
    /// </summary>
    public static class DamageCalculator
    {
        public static float BaseDamage(WeaponDef weapon, StatSheet stats, ScalingTable scaling, GameBalance balance)
        {
            Ability ability = scaling.Resolve(weapon.Category, weapon.ScalingAbility);
            return weapon.BaseDamage * StatFormulas.AbilityDamageMultiplier(stats.GetAbility(ability), balance.DamagePerAbilityPoint);
        }

        public static float CritChance(StatSheet stats, GameBalance balance)
        {
            return StatFormulas.CritChance(
                stats.GetAbility(Ability.Lck), balance.BaseCritChance, balance.CritChancePerLuck, stats.Get(StatId.CritChance));
        }

        public static float CritMultiplier(StatSheet stats, GameBalance balance)
        {
            return StatFormulas.CritMultiplier(balance.BaseCritMultiplier, stats.Get(StatId.CritMultiplier));
        }

        /// <param name="combat">Rng.Combat of the current run.</param>
        public static HitResult Roll(WeaponDef weapon, StatSheet stats, ScalingTable scaling, GameBalance balance, IRandomSource combat)
        {
            float damage = BaseDamage(weapon, stats, scaling, balance);
            float critChance = CritChance(stats, balance);
            // Certain outcomes skip the draw, so they don't consume the stream.
            bool isCrit = critChance >= 1f || (critChance > 0f && combat.NextFloat() < critChance);
            if (isCrit)
            {
                damage *= CritMultiplier(stats, balance);
            }

            return new HitResult(damage, isCrit);
        }
    }
}
