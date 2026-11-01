using System.Collections.Generic;
using Game.Core;
using Game.Data;
using UnityEngine;

namespace Game.Tests.EditMode
{
    /// <summary>Creates in-memory definitions for tests and destroys them afterwards.</summary>
    internal sealed class TestAssets
    {
        private readonly List<Object> _created = new List<Object>();

        public TrinketDef Trinket(string id, params Effect[] effects)
        {
            return CreateEffectDefinition<TrinketDef>(id, effects);
        }

        public TalentDef Talent(string id, params Effect[] effects)
        {
            return CreateEffectDefinition<TalentDef>(id, effects);
        }

        /// <summary>A valid balance with the GDD values.</summary>
        public GameBalance Balance()
        {
            GameBalance balance = Create<GameBalance>("GameBalance");
            PrivateField.Set(balance, "_baseCritChance", 0.05f);
            PrivateField.Set(balance, "_critChancePerLuck", 0.01f);
            PrivateField.Set(balance, "_baseCritMultiplier", 2f);
            PrivateField.Set(balance, "_damagePerAbilityPoint", 0.05f);
            PrivateField.Set(balance, "_primaryAbilityBonus", 2);
            PrivateField.Set(balance, "_interactRadius", 2.5f);
            PrivateField.Set(balance, "_procChancePerLuck", 0.05f);
            PrivateField.Set(balance, "_proficiencyBonus", 2);
            PrivateField.Set(balance, "_nat1RerollChancePerLuck", 0.05f);
            PrivateField.Set(balance, "_nat20RewardCopies", 2);
            PrivateField.Set(balance, "_skillCheckPauseSeconds", 2f);
            PrivateField.Set(balance, "_movement", Movement());
            return balance;
        }

        /// <summary>The sandbox movement values; individual tests override what they measure.</summary>
        public static MovementTuning Movement(
            float groundAcceleration = 60f,
            float airAcceleration = 15f,
            float jumpHeight = 1.6f,
            float coyoteTime = 0.12f,
            float jumpBufferTime = 0.15f)
        {
            return new MovementTuning(
                groundAcceleration: groundAcceleration,
                groundDeceleration: 50f,
                overspeedDeceleration: 8f,
                airAcceleration: airAcceleration,
                gravity: 30f,
                maxFallSpeed: 40f,
                jumpHeight: jumpHeight,
                coyoteTime: coyoteTime,
                jumpBufferTime: jumpBufferTime,
                maxWalkAngle: 50f,
                stepHeight: 0.35f,
                skinWidth: 0.01f,
                dash: new DashTuning(speed: 22f, duration: 0.15f, cooldown: 2f, invulnerability: 0.2f, baseCharges: 1),
                slide: new SlideTuning(
                    startSpeed: 4f,
                    stopSpeed: 2.5f,
                    friction: 4f,
                    slopeFactor: 1f,
                    steering: 4f,
                    height: 1f,
                    jumpBoost: 1.5f,
                    jumpBoostWindow: 0.2f,
                    softSpeedCap: 14f));
        }

        public SkillDef Skill(string id, Ability ability)
        {
            SkillDef skill = CreateDefinition<SkillDef>(id);
            PrivateField.Set(skill, "_displayName", id);
            PrivateField.Set(skill, "_ability", ability);
            return skill;
        }

        public WeaponDef Weapon(string id, WeaponCategory category, Ability scaling, float baseDamage)
        {
            WeaponDef weapon = CreateDefinition<WeaponDef>(id);
            PrivateField.Set(weapon, "_displayName", id);
            PrivateField.Set(weapon, "_category", category);
            PrivateField.Set(weapon, "_scalingAbility", scaling);
            PrivateField.Set(weapon, "_baseDamage", baseDamage);
            PrivateField.Set(weapon, "_fireInterval", 0.5f);
            PrivateField.Set(weapon, "_projectileSpeed", 20f);
            PrivateField.Set(weapon, "_projectileRadius", 0.25f);
            PrivateField.Set(weapon, "_range", 30f);
            return weapon;
        }

        public CharacterDef Character(string id, Ability[] primary, StatValue[] baseStats, params SkillDef[] proficiencies)
        {
            CharacterDef character = CreateDefinition<CharacterDef>(id);
            PrivateField.Set(character, "_displayName", id);
            PrivateField.Set(character, "_primaryAbilities", primary);
            PrivateField.Set(character, "_baseStats", new List<StatValue>(baseStats));
            PrivateField.Set(character, "_proficiencies", new List<SkillDef>(proficiencies));
            return character;
        }

        public SkillCheckSiteDef Site(string id, SkillDef skill, int dc, EffectDefinition reward)
        {
            SkillCheckSiteDef site = CreateDefinition<SkillCheckSiteDef>(id);
            PrivateField.Set(site, "_displayName", id);
            PrivateField.Set(site, "_skill", skill);
            PrivateField.Set(site, "_dc", dc);
            PrivateField.Set(site, "_reward", reward);
            return site;
        }

        public void DestroyAll()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        private T CreateEffectDefinition<T>(string id, Effect[] effects)
            where T : EffectDefinition
        {
            T definition = CreateDefinition<T>(id);
            PrivateField.Set(definition, "_displayName", id);
            PrivateField.Set(definition, "_effects", new List<Effect>(effects));
            return definition;
        }

        private T CreateDefinition<T>(string id)
            where T : Definition
        {
            T definition = Create<T>(id);
            PrivateField.Set(definition, "_id", id);
            return definition;
        }

        private T Create<T>(string name)
            where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            _created.Add(asset);
            return asset;
        }
    }
}
