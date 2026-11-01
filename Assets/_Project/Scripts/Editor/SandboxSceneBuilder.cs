using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Combat;
using Game.Gameplay.Enemies;
using Game.Gameplay.Input;
using Game.Gameplay.Player;
using Game.Gameplay.SkillChecks;
using Game.Presentation;
using nickmaltbie.OpenKCC.Character;
using Game.Sandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Game.Editor.SceneBuildHelpers;
using static Game.Editor.SerializedFields;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>
    /// Builds the sandbox test scene from scratch (no hand-edited .unity files). Rerun after code changes.
    /// Data assets are created once and kept; prefabs, materials and the scene are rebuilt.
    /// </summary>
    public static class SandboxSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Sandbox.unity";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Test";
        private const string ArtFolder = "Assets/_Project/Art/Test";
        private const string InputPath = "Assets/_Project/Input/GameInput.inputactions";

        private static readonly Vector3[] SitePositions = { new Vector3(10f, 0f, 8f), new Vector3(-12f, 0f, 6f), new Vector3(4f, 0f, -14f) };
        private static readonly Color[] SiteColors = { new Color(0.45f, 0.3f, 0.15f), new Color(0.55f, 0.4f, 0.25f), new Color(0.1f, 0.6f, 0.6f) };
        private static readonly Vector3[] SiteScales = { new Vector3(1f, 5f, 1f), new Vector3(3f, 1.2f, 1.8f), new Vector3(1.2f, 2.2f, 0.8f) };

        private static readonly Color[] PickupColors =
        {
            new Color(1f, 0.85f, 0.1f), new Color(0.2f, 0.8f, 1f), Color.white, new Color(0.2f, 0.85f, 0.2f), new Color(0.65f, 0.25f, 0.9f),
        };

        [MenuItem("Tools/Sandbox/Build Sandbox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder(SandboxDataBuilder.Folder);
            EnsureFolder(PrefabFolder);
            EnsureFolder(ArtFolder);
            EnsureFolder("Assets/_Project/Scenes");

            // Opening a new scene unloads unused assets, so everything below is created or loaded after it.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SandboxData data = SandboxDataBuilder.Build();
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            if (input == null)
            {
                throw new InvalidOperationException($"missing {InputPath}");
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Transform fireballPrefab = BuildPrimitivePrefab("Fireball", PrimitiveType.Sphere, GetMaterial("Fireball", new Color(1f, 0.45f, 0.05f))).transform;
            Enemy enemyPrefab = BuildPrimitivePrefab("Enemy", PrimitiveType.Capsule, GetMaterial("Enemy", new Color(0.8f, 0.15f, 0.15f))).AddComponentToPrefab<Enemy>();
            TextMesh numberPrefab = BuildNumberPrefab(PrefabFolder, font);

            BuildScene(scene, data, input, font, fireballPrefab, enemyPrefab, numberPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"Sandbox scene built: {ScenePath}. Press Play.");
        }

        private static void BuildScene(
            Scene scene, SandboxData data, InputActionAsset input, Font font, Transform fireballPrefab, Enemy enemyPrefab, TextMesh numberPrefab)
        {
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.5f);

            GameObject ground = Primitive("Ground", PrimitiveType.Plane, null, Vector3.zero, new Vector3(10f, 1f, 10f), GetMaterial("Ground", new Color(0.35f, 0.4f, 0.35f)));
            Material obstacle = GetMaterial("Obstacle", new Color(0.5f, 0.5f, 0.55f));
            Primitive("Obstacle", PrimitiveType.Cube, null, new Vector3(-6f, 1f, -8f), new Vector3(3f, 2f, 3f), obstacle);
            Primitive("Obstacle", PrimitiveType.Cube, null, new Vector3(15f, 1.5f, -5f), new Vector3(2f, 3f, 6f), obstacle);
            Primitive("Obstacle", PrimitiveType.Cube, null, new Vector3(-18f, 0.75f, -15f), new Vector3(5f, 1.5f, 2f), obstacle);
            ground.isStatic = true;

            var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            cameraObject.AddComponent<AudioListener>();

            // Player
            var player = new GameObject("Player (Albert)");
            (CapsuleCollider capsule, KCCMovementEngine engine) = KinematicBody(player, 0.4f, 2f);
            Transform visual = InterpolatedVisual(player.transform);
            Primitive("Body", PrimitiveType.Capsule, visual, new Vector3(0f, 1f, 0f), new Vector3(0.8f, 1f, 0.8f), GetMaterial("Player", new Color(0.2f, 0.35f, 0.85f)), keepCollider: false);
            Primitive("Hat", PrimitiveType.Cylinder, visual, new Vector3(0f, 2.15f, 0f), new Vector3(0.7f, 0.12f, 0.7f), GetMaterial("Hat", new Color(0.35f, 0.15f, 0.55f)), keepCollider: false);
            Primitive("Nose", PrimitiveType.Cube, visual, new Vector3(0f, 1.55f, 0.4f), Vector3.one * 0.15f, GetMaterial("Skin", new Color(0.95f, 0.75f, 0.6f)), keepCollider: false);
            Primitive("Staff", PrimitiveType.Cube, visual, new Vector3(0.5f, 1.1f, 0.25f), new Vector3(0.08f, 1.8f, 0.08f), GetMaterial("Staff", new Color(0.4f, 0.25f, 0.1f)), keepCollider: false);
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(player.transform, false);
            muzzle.localPosition = new Vector3(0.5f, 1.6f, 0.6f);

            var aim = player.AddComponent<PlayerAim>();
            var motor = Disabled(player.AddComponent<PlayerMotor>());
            Edit(motor, s => SetObject(s, "_engine", engine));
            var shooter = Disabled(player.AddComponent<PlayerShooter>());
            Edit(shooter, s => SetObject(s, "_muzzle", muzzle));
            var health = Disabled(player.AddComponent<PlayerHealth>());
            Edit(health, s => SetObject(s, "_capsule", capsule));
            var interactor = Disabled(player.AddComponent<Interactor>());
            var inputReader = Disabled(player.AddComponent<PlayerInputReader>());
            Edit(inputReader, s => SetObject(s, "_actions", input));

            var thirdPerson = cameraObject.AddComponent<ThirdPersonCamera>();
            Edit(thirdPerson, s =>
            {
                SetObject(s, "_aim", aim);
                SetObject(s, "_target", visual);
            });

            // Systems
            var game = new GameObject("Game");
            var bootstrap = game.AddComponent<GameBootstrap>();
            var spawner = Disabled(game.AddComponent<EnemySpawner>());
            var projectiles = Disabled(game.AddComponent<ProjectileSystem>());

            var sites = new List<SkillCheckSite>();
            for (int i = 0; i < data.Sites.Length; i++)
            {
                sites.Add(BuildSite(data.Sites[i], SitePositions[i], SiteScales[i], SiteColors[i], camera, font));
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
                SetObject(s, "_inputReader", inputReader);
                SetObject(s, "_enemySpawner", spawner);
                SetObject(s, "_projectileSystem", projectiles);
                SetObject(s, "_projectilePrefab", fireballPrefab);
                SetInt(s, "_projectilePoolSize", 128);
                SetObject(s, "_enemyPrefab", enemyPrefab);
                SetInt(s, "_enemyPoolSize", data.Spawns.MaxAlive + 10);
                SetObjects(s, "_skillCheckSites", sites.ToArray());
            });

            var numbers = new GameObject("Damage Numbers").AddComponent<DamageNumbers>();
            Edit(numbers, s =>
            {
                SetObject(s, "_bootstrap", bootstrap);
                SetObject(s, "_camera", camera);
                SetObject(s, "_prefab", numberPrefab);
            });

            var panel = new GameObject("Sandbox Debug Panel").AddComponent<SandboxDebugPanel>();
            Edit(panel, s =>
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
        }

        private static SkillCheckSite BuildSite(Game.Data.SkillCheckSiteDef definition, Vector3 position, Vector3 scale, Color color, Camera camera, Font font)
        {
            var root = new GameObject($"Site: {definition.DisplayName}");
            root.transform.position = position;
            GameObject visual = Primitive("Visual", PrimitiveType.Cube, root.transform, new Vector3(0f, scale.y / 2f, 0f), scale, GetMaterial($"Site{definition.name}", color));
            var site = Disabled(root.AddComponent<SkillCheckSite>());
            Edit(site, s => SetObject(s, "_definition", definition));
            var presenter = root.AddComponent<SkillCheckSitePresenter>();
            Edit(presenter, s =>
            {
                SetObject(s, "_site", site);
                SetObject(s, "_renderer", visual.GetComponent<Renderer>());
            });
            Label(root.transform, $"{definition.DisplayName}\n({definition.Skill.DisplayName}, DC {definition.Dc})", scale.y + 1f, camera, font);
            return site;
        }

        private static void BuildPickup(SandboxData data, int index, GameBootstrap bootstrap, Transform player, Camera camera, Font font)
        {
            Game.Data.TrinketDef trinket = data.PickupTrinkets[index];
            var root = new GameObject($"Pickup: {trinket.DisplayName}");
            root.transform.position = new Vector3(-6f + index * 3f, 0f, 5f);
            GameObject visual = Primitive("Visual", PrimitiveType.Cube, root.transform, new Vector3(0f, 0.6f, 0f), Vector3.one * 0.6f,
                GetMaterial($"Pickup{trinket.name}", PickupColors[index % PickupColors.Length]), keepCollider: false);
            var pickup = root.AddComponent<TestTrinketPickup>();
            Edit(pickup, s =>
            {
                SetObject(s, "_bootstrap", bootstrap);
                SetObject(s, "_settings", data.Settings);
                SetObject(s, "_trinket", trinket);
                SetObject(s, "_player", player);
                SetObject(s, "_visual", visual);
            });
            Label(root.transform, trinket.DisplayName, 1.6f, camera, font);
        }

        private static GameObject BuildPrimitivePrefab(string name, PrimitiveType type, Material material)
        {
            GameObject go = Primitive(name, type, null, Vector3.zero, Vector3.one, material, keepCollider: false);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabFolder}/{name}.prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static Material GetMaterial(string name, Color color) => SceneBuildHelpers.GetMaterial(ArtFolder, name, color);
    }
}
