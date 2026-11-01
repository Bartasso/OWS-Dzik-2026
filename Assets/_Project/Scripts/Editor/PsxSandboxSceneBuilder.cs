using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Combat;
using Game.Gameplay.Enemies;
using Game.Gameplay.Input;
using Game.Gameplay.Player;
using Game.Gameplay.SkillChecks;
using Game.Presentation;
using Game.Sandbox;
using nickmaltbie.OpenKCC.Character;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Game.Editor.PsxSandboxAssets;
using static Game.Editor.SceneBuildHelpers;
using static Game.Editor.SerializedFields;

namespace Game.Editor
{
    /// <summary>
    /// Builds the sandbox again on the purchased PSX packs: a walled castle courtyard with a tech corner,
    /// the mannequin as Albert, pack monsters and a sprite fireball. Same data and gameplay as the plain sandbox.
    /// </summary>
    public static class PsxSandboxSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/SandboxPsx.unity";
        private const string InputPath = "Assets/_Project/Input/GameInput.inputactions";
        private const string SkyboxPath = HeyHeyThere + "/PSXSkyboxes/Materials/Panoramic/M_day_clouds.mat";
        private const string PsxScreenType = "HeyHeyThere.PSXLook.PSXScreen, HeyHeyThere.PSXLook";
        private const float TileSize = 4f;
        private const int TilesPerSide = 15;
        private const float HalfArena = TileSize * TilesPerSide / 2f;

        private static readonly (string Prefab, Vector3 Position, float Yaw)[] Props =
        {
            ("PSXCastleKit/Prefabs/PF_pillar_castle_tall", new Vector3(-10f, 0f, -10f), 0f),
            ("PSXCastleKit/Prefabs/PF_pillar_castle_tall", new Vector3(-10f, 0f, 10f), 0f),
            ("PSXCastleKit/Prefabs/PF_pillar_castle_tall", new Vector3(10f, 0f, -10f), 0f),
            ("PSXCastleKit/Prefabs/PF_well", new Vector3(-6f, 0f, -8f), 0f),
            ("PSXCastleKit/Prefabs/PF_standing_stone", new Vector3(-18f, 0f, -15f), 20f),
            ("PSXCastleKit/Prefabs/PF_standing_stone", new Vector3(-21f, 0f, -12f), -35f),
            ("PSXCastleKit/Prefabs/PF_cask", new Vector3(-15f, 0f, 5f), 0f),
            ("PSXCastleKit/Prefabs/PF_cask", new Vector3(-15.8f, 0f, 5.7f), 40f),
            ("PSXCastleKit/Prefabs/PF_crate_castle", new Vector3(-20f, 0f, 18f), 15f),
            ("PSXCastleKit/Prefabs/PF_statue_knight", new Vector3(0f, 0f, -24f), 0f),
            ("PSXCastleKit/Prefabs/PF_torch_stand", new Vector3(-4f, 0f, 4f), 0f),
            ("PSXCastleKit/Prefabs/PF_torch_stand", new Vector3(4f, 0f, -4f), 0f),
            ("PSXTechBaseKit/Prefabs/PF_pillar_tech_tall", new Vector3(12f, 0f, 12f), 0f),
            ("PSXTechBaseKit/Prefabs/PF_computer_tower", new Vector3(14f, 0f, 24f), 180f),
            ("PSXTechBaseKit/Prefabs/PF_console_tech", new Vector3(22f, 0f, 14f), -90f),
            ("PSXTechBaseKit/Prefabs/PF_crate_tech", new Vector3(22f, 0f, 22f), 0f),
            ("PSXTechBaseKit/Prefabs/PF_crate_tech", new Vector3(23.2f, 0f, 22f), 10f),
            ("PSXTechBaseKit/Prefabs/PF_crate_tech_small", new Vector3(22.5f, 1f, 22f), 30f),
            ("PSXTechBaseKit/Prefabs/PF_barrel_explosive", new Vector3(18f, 0f, 20f), 0f),
        };

        // Skill check sites in the same order as SandboxData.Sites: cat on tree, overturned cart, vending machine.
        private static readonly (string Prefab, Vector3 Position, Vector3 Euler)[] SiteVisuals =
        {
            ("PSXCastleKit/Prefabs/PF_gargoyle", new Vector3(10f, 0f, 8f), new Vector3(0f, 200f, 0f)),
            ("PSXCastleKit/Prefabs/PF_table_trestle", new Vector3(-12f, 0f, 6f), new Vector3(0f, 30f, 90f)),
            ("PSXTechBaseKit/Prefabs/PF_computer_tower", new Vector3(4f, 0f, -14f), new Vector3(0f, 150f, 0f)),
        };

        private static readonly string[] PickupVisuals =
        {
            "PSXCastleKit/Prefabs/PF_powerup_damage", "PSXCastleKit/Prefabs/PF_powerup_shield", "PSXCastleKit/Prefabs/PF_health_orb",
            "PSXCastleKit/Prefabs/PF_rune", "PSXCastleKit/Prefabs/PF_key_gold",
        };

        [MenuItem("Tools/Sandbox/Build PSX Sandbox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder(PrefabFolder);
            EnsureFolder(ArtFolder);

            // Opening a new scene unloads unused assets, so everything below is created or loaded after it.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SandboxData data = SandboxDataBuilder.Build();
            InputActionAsset input = Load<InputActionAsset>(InputPath);
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            Transform fireballPrefab = BuildFireballPrefab();
            Enemy enemyPrefab = BuildEnemyPrefab();
            TextMesh numberPrefab = BuildNumberPrefab(PrefabFolder, font);

            BuildEnvironment();
            Camera camera = BuildCamera();
            (GameObject player, Transform visual, PlayerAim aim, PlayerMotor motor, PlayerShooter shooter, PlayerHealth health, Interactor interactor) = BuildPlayer(input);
            Edit(camera.gameObject.AddComponent<ThirdPersonCamera>(), s =>
            {
                SetObject(s, "_aim", aim);
                SetObject(s, "_target", visual);
            });

            var game = new GameObject("Game");
            var bootstrap = game.AddComponent<GameBootstrap>();
            var spawner = Disabled(game.AddComponent<EnemySpawner>());
            var projectiles = Disabled(game.AddComponent<ProjectileSystem>());

            var sites = new List<SkillCheckSite>();
            for (int i = 0; i < data.Sites.Length; i++)
            {
                sites.Add(BuildSite(data.Sites[i], SiteVisuals[i], camera, font));
            }

            for (int i = 0; i < data.PickupTrinkets.Length; i++)
            {
                BuildPickup(data, i, bootstrap, player.transform, camera, font);
            }

            Edit(bootstrap, s =>
            {
                SetObject(s, "_balance", data.Balance);
                SetObject(s, "_character", data.Albert);
                SetObject(s, "_spawnSettings", data.Spawns);
                SetObject(s, "_playerAim", aim);
                SetObject(s, "_playerMotor", motor);
                SetObject(s, "_playerShooter", shooter);
                SetObject(s, "_playerHealth", health);
                SetObject(s, "_interactor", interactor);
                SetObject(s, "_inputReader", player.GetComponent<PlayerInputReader>());
                SetObject(s, "_enemySpawner", spawner);
                SetObject(s, "_projectileSystem", projectiles);
                SetObject(s, "_projectilePrefab", fireballPrefab);
                SetInt(s, "_projectilePoolSize", 128);
                SetObject(s, "_enemyPrefab", enemyPrefab);
                SetInt(s, "_enemyPoolSize", data.Spawns.MaxAlive + 10);
                SetObjects(s, "_skillCheckSites", sites.ToArray());
            });

            Edit(new GameObject("Damage Numbers").AddComponent<DamageNumbers>(), s =>
            {
                SetObject(s, "_bootstrap", bootstrap);
                SetObject(s, "_camera", camera);
                SetObject(s, "_prefab", numberPrefab);
            });

            Edit(new GameObject("Sandbox Debug Panel").AddComponent<SandboxDebugPanel>(), s =>
            {
                SetObject(s, "_bootstrap", bootstrap);
                SetObject(s, "_settings", data.Settings);
                SetObject(s, "_playerHealth", health);
                SetObject(s, "_playerShooter", shooter);
                SetObject(s, "_interactor", interactor);
                SetObject(s, "_enemySpawner", spawner);
                SetObject(s, "_projectiles", projectiles);
            });

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"PSX sandbox scene built: {ScenePath}. Press Play.");
        }

        private static void BuildEnvironment()
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.93f, 0.8f);
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            RenderSettings.skybox = Load<Material>(SkyboxPath);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.68f, 0.75f);
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 75f;

            Transform level = new GameObject("Level").transform;
            var groundCollider = new GameObject("Ground Collider").AddComponent<BoxCollider>();
            groundCollider.transform.SetParent(level, false);
            groundCollider.size = new Vector3(HalfArena * 2f, 1f, HalfArena * 2f);
            groundCollider.center = new Vector3(0f, -0.5f, 0f);

            Vector3 tileScale = new Vector3(TileSize / 2f, 1f, TileSize / 2f);
            int half = TilesPerSide / 2;
            for (int x = -half; x <= half; x++)
            {
                for (int z = -half; z <= half; z++)
                {
                    string floor = x >= 2 && z >= 2 ? "PSXTechBaseKit/Prefabs/PF_floor_tech"
                        : x == 0 || z == 0 ? "PSXCastleKit/Prefabs/PF_floor_cobble"
                        : "PSXCastleKit/Prefabs/PF_floor_flags";
                    Place(level, floor, new Vector3(x * TileSize, 0f, z * TileSize), Quaternion.identity, tileScale);
                }
            }

            // Perimeter: castle walls doubled in size (4 m wide, 6 m tall), tall pillars on the corners.
            Vector3 wallScale = new Vector3(2f, 2f, 2f);
            for (int i = 0; i < TilesPerSide; i++)
            {
                float along = -HalfArena + TileSize * (i + 0.5f);
                Place(level, "PSXCastleKit/Prefabs/PF_wall_castle", new Vector3(along, 0f, HalfArena), Quaternion.identity, wallScale);
                Place(level, "PSXCastleKit/Prefabs/PF_wall_castle", new Vector3(along, 0f, -HalfArena), Quaternion.Euler(0f, 180f, 0f), wallScale);
                Place(level, "PSXCastleKit/Prefabs/PF_wall_castle", new Vector3(HalfArena, 0f, along), Quaternion.Euler(0f, 90f, 0f), wallScale);
                Place(level, "PSXCastleKit/Prefabs/PF_wall_castle", new Vector3(-HalfArena, 0f, along), Quaternion.Euler(0f, -90f, 0f), wallScale);
            }

            foreach (float x in new[] { -HalfArena, HalfArena })
            {
                foreach (float z in new[] { -HalfArena, HalfArena })
                {
                    Place(level, "PSXCastleKit/Prefabs/PF_pillar_castle_tall", new Vector3(x, 0f, z), Quaternion.identity, Vector3.one * 1.5f);
                }
            }

            foreach ((string prefab, Vector3 position, float yaw) in Props)
            {
                Place(level, prefab, position, Quaternion.Euler(0f, yaw, 0f), Vector3.one);
            }
        }

        private static void Place(Transform parent, string prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(LoadPrefab(prefab), parent);
            go.transform.SetLocalPositionAndRotation(position, rotation);
            go.transform.localScale = scale;
            go.isStatic = true;
        }

        private static Camera BuildCamera()
        {
            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();

            // Added by name so game code never compiles against the pack, which is not in git.
            Type psxScreen = Type.GetType(PsxScreenType);
            if (psxScreen == null)
            {
                throw new InvalidOperationException($"missing {PsxScreenType}: import the PSX Look pack (docs/THIRD_PARTY.md)");
            }

            // 360 lines instead of the PS1's 240 keeps the debug labels readable.
            Edit(cameraObject.AddComponent(psxScreen), s => SetInt(s, "lines", 360));
            return camera;
        }

        private static (GameObject, Transform, PlayerAim, PlayerMotor, PlayerShooter, PlayerHealth, Interactor) BuildPlayer(InputActionAsset input)
        {
            var player = new GameObject("Player (Albert)");
            (CapsuleCollider capsule, KCCMovementEngine engine) = KinematicBody(player, 0.4f, 2f);
            Transform visual = InterpolatedVisual(player.transform);
            Transform pose = SlidingPose(visual);

            GameObject model = Mannequin(pose);
            GameObject staff = Visual(LoadPrefab("PSXMeleeWeapons/Prefabs/PF_wizard_staff"), FindBone(model, "RightHand"), "Staff");
            staff.transform.localPosition = Vector3.zero;

            var locomotion = visual.gameObject.AddComponent<LocomotionAnimator>();
            Edit(locomotion, s => SetObject(s, "_animator", model.GetComponent<Animator>()));

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(player.transform, false);
            muzzle.localPosition = new Vector3(0.5f, 1.6f, 0.6f);

            var aim = player.AddComponent<PlayerAim>();
            var motor = Disabled(player.AddComponent<PlayerMotor>());
            Edit(motor, s => SetObject(s, "_engine", engine));
            AddSlidePose(pose, motor);
            Edit(visual.gameObject.AddComponent<SlideAnimator>(), s =>
            {
                SetObject(s, "_motor", motor);
                SetObject(s, "_animator", model.GetComponent<Animator>());
            });
            var shooter = Disabled(player.AddComponent<PlayerShooter>());
            Edit(shooter, s => SetObject(s, "_muzzle", muzzle));
            var health = Disabled(player.AddComponent<PlayerHealth>());
            Edit(health, s => SetObject(s, "_capsule", capsule));
            var interactor = Disabled(player.AddComponent<Interactor>());
            var inputReader = Disabled(player.AddComponent<PlayerInputReader>());
            Edit(inputReader, s => SetObject(s, "_actions", input));
            return (player, visual, aim, motor, shooter, health, interactor);
        }

        private static SkillCheckSite BuildSite(Game.Data.SkillCheckSiteDef definition, (string Prefab, Vector3 Position, Vector3 Euler) visualSpec, Camera camera, Font font)
        {
            var root = new GameObject($"Site: {definition.DisplayName}");
            root.transform.position = visualSpec.Position;
            GameObject visual = Visual(LoadPrefab(visualSpec.Prefab), root.transform, "Visual");
            visual.transform.localRotation = Quaternion.Euler(visualSpec.Euler);
            float top = RestOnGround(visual);
            visual.AddComponent<BoxCollider>();

            var site = Disabled(root.AddComponent<SkillCheckSite>());
            Edit(site, s => SetObject(s, "_definition", definition));
            Edit(root.AddComponent<SkillCheckSitePresenter>(), s =>
            {
                SetObject(s, "_site", site);
                SetObject(s, "_renderer", visual.GetComponentInChildren<Renderer>());
                SetString(s, "_colorProperty", "_Color");
            });
            Label(root.transform, $"{definition.DisplayName}\n({definition.Skill.DisplayName}, DC {definition.Dc})", top + 1f, camera, font);
            return site;
        }

        private static void BuildPickup(SandboxData data, int index, GameBootstrap bootstrap, Transform player, Camera camera, Font font)
        {
            Game.Data.TrinketDef trinket = data.PickupTrinkets[index];
            var root = new GameObject($"Pickup: {trinket.DisplayName}");
            root.transform.position = new Vector3(-6f + index * 3f, 0f, 5f);
            GameObject visual = Visual(LoadPrefab(PickupVisuals[index % PickupVisuals.Length]), root.transform, "Visual");
            visual.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            Edit(root.AddComponent<TestTrinketPickup>(), s =>
            {
                SetObject(s, "_bootstrap", bootstrap);
                SetObject(s, "_settings", data.Settings);
                SetObject(s, "_trinket", trinket);
                SetObject(s, "_player", player);
                SetObject(s, "_visual", visual);
            });
            Label(root.transform, trinket.DisplayName, 1.6f, camera, font);
        }

        /// <summary>Lifts a rotated prop so its lowest point touches the ground; returns its height.</summary>
        private static float RestOnGround(GameObject visual)
        {
            Bounds bounds = WorldBounds(visual);
            visual.transform.position += Vector3.up * (visual.transform.parent.position.y - bounds.min.y);
            return bounds.size.y;
        }

        private static Bounds WorldBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException($"{go.name}: no renderers");
            }

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }
    }
}
