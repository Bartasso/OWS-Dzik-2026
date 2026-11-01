using System;
using Game.Core;
using Game.Data;
using Game.Sandbox;
using UnityEditor;
using UnityEngine;
using static Game.Editor.SerializedFields;

namespace Game.Editor
{
    internal sealed class SandboxData
    {
        public GameBalance Balance;
        public CharacterDef Albert;
        public EnemySpawnSettings Spawns;
        public SkillCheckSiteDef[] Sites;
        public TrinketDef[] PickupTrinkets;
        public SandboxSettings Settings;
    }

    /// <summary>
    /// Creates the test.* data assets for the sandbox. Existing assets are left untouched,
    /// so values tuned in the Inspector survive rebuilding the scene.
    /// </summary>
    internal static class SandboxDataBuilder
    {
        public const string Folder = "Assets/_Project/Data/Test";
        private const string BalancePath = "Assets/_Project/Data/GameBalance.asset";

        public static SandboxData Build()
        {
            var balance = AssetDatabase.LoadAssetAtPath<GameBalance>(BalancePath);
            if (balance == null)
            {
                throw new InvalidOperationException($"missing {BalancePath}");
            }

            EnsureMovementTuning(balance);

            SkillDef animals = Skill("Skill_Animals", "test.skill_animals", "Animal Handling", Ability.Wis);
            SkillDef athletics = Skill("Skill_Athletics", "test.skill_athletics", "Athletics", Ability.Str);
            SkillDef mechanisms = Skill("Skill_MagicMechanisms", "test.skill_magic_mechanisms", "Magic Mechanisms", Ability.Int);
            SkillDef riddles = Skill("Skill_Riddles", "test.skill_riddles", "Riddles", Ability.Int);

            WeaponDef staff = Asset<WeaponDef>("Weapon_FireballStaff", s =>
            {
                SetString(s, "_id", "test.fireball_staff");
                SetString(s, "_displayName", "Fireball Staff");
                SetEnum(s, "_category", WeaponCategory.Arcane);
                SetEnum(s, "_scalingAbility", Ability.Int);
                SetFloat(s, "_baseDamage", 10f);
                SetFloat(s, "_fireInterval", 0.4f);
                SetFloat(s, "_projectileSpeed", 22f);
                SetFloat(s, "_projectileRadius", 0.25f);
                SetFloat(s, "_range", 35f);
            });

            CharacterDef albert = Asset<CharacterDef>("Character_Albert", s =>
            {
                SetString(s, "_id", "test.albert");
                SetString(s, "_displayName", "Albert");
                SetEnums(s, "_primaryAbilities", Ability.Int, Ability.Wis);
                SetStatValues(s, "_baseStats",
                    (StatId.MaxHp, 100f), (StatId.HpRegen, 1f), (StatId.AttackSpeed, 1f), (StatId.MoveSpeed, 6f));
                SetObjects(s, "_proficiencies", riddles, mechanisms);
                SetObject(s, "_startingWeapon", staff);
            });

            EnemyDef goblin = Enemy("Enemy_Goblin", "test.goblin", "Tracksuit Goblin", EnemyKind.Normal, 30f, 3f, 0.5f, 8f, 1f);
            EnemyDef owlbear = Enemy("Enemy_Owlbear", "test.owlbear", "Owlbear", EnemyKind.Boss, 500f, 2.2f, 1.2f, 25f, 1.5f);

            EnemySpawnSettings spawns = Asset<EnemySpawnSettings>("EnemySpawnSettings_Test", s =>
            {
                SetObject(s, "_regularEnemy", goblin);
                SetObject(s, "_boss", owlbear);
                SetFloat(s, "_spawnInterval", 1.2f);
                SetInt(s, "_maxAlive", 30);
                SetFloat(s, "_spawnRadiusMin", 14f);
                SetFloat(s, "_spawnRadiusMax", 22f);
                SetInt(s, "_bossEveryKills", 25);
            });

            TrinketDef energyDrink = Trinket("Trinket_EnergyDrink", "test.energy_drink", "Night Shop Energy Drink", Rarity.Rare,
                new StatModifierEffect(StatId.AttackSpeed, ModifierOp.PercentAdd, 0.25f, StackCurve.Linear),
                new StatModifierEffect(StatId.MaxHp, ModifierOp.PercentAdd, -0.10f, StackCurve.Linear));
            TrinketDef slippers = Trinket("Trinket_PoolSlippers", "test.pool_slippers", "Pool Slippers", Rarity.Uncommon,
                new StatModifierEffect(StatId.Dex, ModifierOp.Flat, 1f, StackCurve.Linear),
                new StatModifierEffect(StatId.MoveSpeed, ModifierOp.PercentAdd, 0.15f, StackCurve.Linear));
            TrinketDef grandpasDie = Trinket("Trinket_GrandpasDie", "test.grandpas_die", "Grandpa's Pocket Die", Rarity.Rare,
                new StatModifierEffect(StatId.SkillCheckBonus, ModifierOp.Flat, 1f, StackCurve.Linear));
            TrinketDef clover = Trinket("Trinket_FourLeafClover", "test.four_leaf_clover", "Four-Leaf Clover", Rarity.Uncommon,
                new StatModifierEffect(StatId.CritChance, ModifierOp.Flat, 0.1f, StackCurve.Hyperbolic));
            TrinketDef amulet = Trinket("Trinket_FairAmulet", "test.fair_amulet", "Church Fair Amulet", Rarity.Epic,
                new StatModifierEffect(StatId.Lck, ModifierOp.Flat, 1f, StackCurve.Linear),
                new StatModifierEffect(StatId.SkillCheckAdvantage, ModifierOp.Flat, 1f, StackCurve.Linear));

            TalentDef lucky = Talent("Talent_Lucky", "test.lucky", "Lucky", Rarity.Rare,
                new StatOnTriggerEffect(GameTrigger.BossKilled, StatId.Lck, ModifierOp.Flat, 1f));
            TalentDef sageFist = Talent("Talent_SageFist", "test.sage_fist", "Sage's Fist", Rarity.Epic,
                new ScalingOverrideEffect(WeaponCategory.Arcane, Ability.Str));
            TalentDef oxStrength = Talent("Talent_OxStrength", "test.ox_strength", "Ox Strength", Rarity.Common,
                new StatModifierEffect(StatId.Str, ModifierOp.Flat, 3f, StackCurve.Linear));

            SkillCheckSiteDef cat = Site("Site_CatOnTree", "test.site_cat", "Get the Cat Down from the Tree", animals, 12, clover);
            SkillCheckSiteDef cart = Site("Site_OverturnedCart", "test.site_cart", "Lift the Overturned Cart", athletics, 15, energyDrink);
            SkillCheckSiteDef vending = Site("Site_VendingMachine", "test.site_vending", "Outsmart the Enchanted Vending Machine", mechanisms, 14, grandpasDie);

            SandboxSettings settings = Asset<SandboxSettings>("SandboxSettings", s =>
            {
                SetFloat(s, "_pickupRadius", 1.5f);
                SetFloat(s, "_pickupRespawnSeconds", 2f);
                SetObjects(s, "_testTalents", lucky, sageFist, oxStrength);
                SetFloat(s, "_testProcChance", 0.1f);
            });

            return new SandboxData
            {
                Balance = balance,
                Albert = albert,
                Spawns = spawns,
                Sites = new[] { cat, cart, vending },
                PickupTrinkets = new[] { energyDrink, slippers, grandpasDie, clover, amulet },
                Settings = settings,
            };
        }

        /// <summary>Fills each movement block only while it is unset, so Inspector tuning survives rebuilds.</summary>
        private static void EnsureMovementTuning(GameBalance balance)
        {
            MovementTuning movement = balance.Movement;
            if (movement == null || movement.Gravity <= 0f)
            {
                Edit(balance, s =>
                {
                    SetFloat(s, "_movement._groundAcceleration", 60f);
                    SetFloat(s, "_movement._groundDeceleration", 50f);
                    SetFloat(s, "_movement._airAcceleration", 15f);
                    SetFloat(s, "_movement._gravity", 30f);
                    SetFloat(s, "_movement._maxFallSpeed", 40f);
                    SetFloat(s, "_movement._jumpHeight", 1.6f);
                    SetFloat(s, "_movement._coyoteTime", 0.12f);
                    SetFloat(s, "_movement._jumpBufferTime", 0.15f);
                    SetFloat(s, "_movement._maxWalkAngle", 50f);
                    SetFloat(s, "_movement._stepHeight", 0.35f);
                    SetFloat(s, "_movement._skinWidth", 0.01f);
                });
            }

            if (balance.Movement.OverspeedDeceleration <= 0f)
            {
                Edit(balance, s => SetFloat(s, "_movement._overspeedDeceleration", 8f));
            }

            if (balance.Movement.Dash == null || balance.Movement.Dash.Speed <= 0f)
            {
                Edit(balance, s =>
                {
                    SetFloat(s, "_movement._dash._speed", 22f);
                    SetFloat(s, "_movement._dash._duration", 0.15f);
                    SetFloat(s, "_movement._dash._cooldown", 2f);
                    SetFloat(s, "_movement._dash._invulnerability", 0.2f);
                    SetInt(s, "_movement._dash._baseCharges", 1);
                });
            }

            if (balance.Movement.Slide == null || balance.Movement.Slide.StartSpeed <= 0f)
            {
                Edit(balance, s =>
                {
                    SetFloat(s, "_movement._slide._startSpeed", 4f);
                    SetFloat(s, "_movement._slide._stopSpeed", 2.5f);
                    SetFloat(s, "_movement._slide._friction", 4f);
                    SetFloat(s, "_movement._slide._slopeFactor", 1f);
                    SetFloat(s, "_movement._slide._steering", 4f);
                    SetFloat(s, "_movement._slide._height", 1f);
                    SetFloat(s, "_movement._slide._jumpBoost", 1.5f);
                    SetFloat(s, "_movement._slide._jumpBoostWindow", 0.2f);
                    SetFloat(s, "_movement._slide._softSpeedCap", 14f);
                });
            }
        }

        private static SkillDef Skill(string file, string id, string displayName, Ability ability)
        {
            return Asset<SkillDef>(file, s =>
            {
                SetString(s, "_id", id);
                SetString(s, "_displayName", displayName);
                SetEnum(s, "_ability", ability);
            });
        }

        private static EnemyDef Enemy(
            string file, string id, string displayName, EnemyKind kind, float hp, float speed, float radius, float damage, float interval)
        {
            return Asset<EnemyDef>(file, s =>
            {
                SetString(s, "_id", id);
                SetString(s, "_displayName", displayName);
                SetEnum(s, "_kind", kind);
                SetFloat(s, "_maxHp", hp);
                SetFloat(s, "_moveSpeed", speed);
                SetFloat(s, "_radius", radius);
                SetFloat(s, "_contactDamage", damage);
                SetFloat(s, "_contactInterval", interval);
            });
        }

        private static TrinketDef Trinket(string file, string id, string displayName, Rarity rarity, params Effect[] effects)
        {
            return Asset<TrinketDef>(file, s => FillEffectDefinition(s, id, displayName, rarity, effects));
        }

        private static TalentDef Talent(string file, string id, string displayName, Rarity rarity, params Effect[] effects)
        {
            return Asset<TalentDef>(file, s => FillEffectDefinition(s, id, displayName, rarity, effects));
        }

        private static void FillEffectDefinition(SerializedObject s, string id, string displayName, Rarity rarity, Effect[] effects)
        {
            SetString(s, "_id", id);
            SetString(s, "_displayName", displayName);
            SetEnum(s, "_rarity", rarity);
            SetManagedReferences(s, "_effects", effects);
        }

        private static SkillCheckSiteDef Site(string file, string id, string displayName, SkillDef skill, int dc, EffectDefinition reward)
        {
            return Asset<SkillCheckSiteDef>(file, s =>
            {
                SetString(s, "_id", id);
                SetString(s, "_displayName", displayName);
                SetObject(s, "_skill", skill);
                SetInt(s, "_dc", dc);
                SetObject(s, "_reward", reward);
            });
        }

        private static T Asset<T>(string fileName, Action<SerializedObject> fill)
            where T : ScriptableObject
        {
            string path = $"{Folder}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                return existing;
            }

            // Filled before CreateAsset, so OnValidate never sees a half-empty asset.
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = fileName;
            Edit(asset, fill);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
