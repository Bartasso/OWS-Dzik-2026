using System;
using System.Collections.Generic;
using Game.Gameplay.Enemies;
using Game.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static Game.Editor.SerializedFields;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>
    /// Prefabs, materials and animation set-up of the PSX sandbox, made from the purchased packs in Assets/ThirdParty.
    /// Those packs are not in git (license), so everything is loaded by path and fails with a pointer to the docs.
    /// </summary>
    internal static class PsxSandboxAssets
    {
        public const string HeyHeyThere = "Assets/ThirdParty/HeyHeyThere";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Psx";
        public const string ArtFolder = "Assets/_Project/Art/Psx";

        private const string MannequinPath = HeyHeyThere + "/PSXAnimationLibraryFree/Models/mannequin_man.fbx";
        private const string MannequinAtlasPath = HeyHeyThere + "/PSXAnimationLibraryFree/Textures/atlas.png";
        private const string EnemyModelPath = HeyHeyThere + "/PSXRetroFPSMonsters/Prefabs/PF_trooper.prefab";
        private const string FireballPath = HeyHeyThere + "/PSXFX/Prefabs/64/PF_fireball.prefab";
        private static readonly string[] LoopingClips = { "idle", "walk", "run" };

        public static T Load<T>(string path)
            where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"missing {path}: import the third-party packs first (docs/THIRD_PARTY.md)");
            }

            return asset;
        }

        public static GameObject LoadPrefab(string pathUnderHeyHeyThere) => Load<GameObject>($"{HeyHeyThere}/{pathUnderHeyHeyThere}.prefab");

        /// <summary>
        /// Instantiates a pack prefab as plain geometry: unpacked, without the pack's scripts, colliders or
        /// Animation components. Keeps renderers and Animators, so no game object depends on pack code.
        /// </summary>
        public static GameObject Visual(GameObject prefab, Transform parent, string name)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = name;
            go.transform.SetParent(parent, false);

            var doomed = new List<Component>();
            foreach (Component component in go.GetComponentsInChildren<Component>(true))
            {
                if (component is MonoBehaviour || component is Collider || component is Animation)
                {
                    doomed.Add(component);
                }
            }

            foreach (Component component in doomed)
            {
                Object.DestroyImmediate(component);
            }

            return go;
        }

        public static Enemy BuildEnemyPrefab()
        {
            GameObject model = Load<GameObject>(EnemyModelPath);
            var walkSpeed = new SerializedObject(model.GetComponent<MonoBehaviour>()).FindProperty("walkSpeed").floatValue;

            // Enemy expects a unit capsule centred on the root (radius 0.5, height 2) and scales it to the definition.
            var root = new GameObject("Enemy");
            root.AddComponent<Enemy>();
            GameObject visual = Visual(model, root.transform, "Model");
            visual.transform.localPosition = new Vector3(0f, -1f, 0f);
            Animator animator = visual.GetComponentInChildren<Animator>();
            animator.applyRootMotion = false;

            var locomotion = root.AddComponent<LocomotionAnimator>();
            Edit(locomotion, s =>
            {
                SetObject(s, "_animator", animator);
                SetString(s, "_speedParameter", "Walk");
                SetFloat(s, "_valuePerMetrePerSecond", 1f / walkSpeed);
            });

            return SavePrefab(root, "Enemy").GetComponent<Enemy>();
        }

        public static Transform BuildFireballPrefab()
        {
            // The pack's projectile loops with its head on the pivot and the trail along -X; the root faces the flight.
            // Its SpriteFX stays: it builds the crossed second quad. A looping effect never destroys itself.
            var root = new GameObject("Fireball");
            var fx = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(FireballPath), root.transform);
            fx.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            return SavePrefab(root, "Fireball").transform;
        }

        /// <summary>The mannequin model with a PSX material and an idle/walk/run controller driven by "Speed".</summary>
        public static GameObject Mannequin(Transform parent)
        {
            ConfigureMannequinImport();
            var model = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(MannequinPath));
            model.name = "Model";
            model.transform.SetParent(parent, false);

            Material material = PsxMaterial("M_Mannequin", Load<Texture2D>(MannequinAtlasPath));
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
            }

            Animator animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidOperationException($"{MannequinPath}: model has no Animator, set its rig to Generic");
            }

            animator.runtimeAnimatorController = BuildLocomotionController();
            animator.applyRootMotion = false;
            return model;
        }

        public static Transform FindBone(GameObject model, string bone)
        {
            foreach (Transform child in model.GetComponentsInChildren<Transform>())
            {
                if (child.name == bone)
                {
                    return child;
                }
            }

            throw new InvalidOperationException($"{model.name}: no bone named {bone}");
        }

        private static void ConfigureMannequinImport()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(MannequinPath);
            // Generic without an avatar imports no Animator; the model must make its own.
            if (importer.animationType != ModelImporterAnimationType.Generic || importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.SaveAndReimport();
            }

            ModelImporterClipAnimation[] clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
            bool changed = importer.clipAnimations.Length == 0;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                bool loop = Array.IndexOf(LoopingClips, clip.name) >= 0;
                changed |= clip.loopTime != loop;
                clip.loopTime = loop;
            }

            if (changed)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        private static AnimatorController BuildLocomotionController()
        {
            string path = $"{ArtFolder}/AC_Albert.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.CreateBlendTreeInController("Locomotion", out BlendTree tree);
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(Clip("idle"), 0f);
            tree.AddChild(Clip("walk"), 1.5f);
            tree.AddChild(Clip("run"), 5f);
            return controller;
        }

        private static AnimationClip Clip(string name)
        {
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(MannequinPath))
            {
                if (asset is AnimationClip clip && clip.name == name)
                {
                    return clip;
                }
            }

            throw new InvalidOperationException($"{MannequinPath}: no clip named {name}");
        }

        private static Material PsxMaterial(string name, Texture2D texture)
        {
            string path = $"{ArtFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("PSX Look/Lit");
                if (shader == null)
                {
                    throw new InvalidOperationException("PSX Look/Lit shader not found: import the PSX Look pack (docs/THIRD_PARTY.md)");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetTexture("_BaseMap", texture);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject SavePrefab(GameObject root, string name)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabFolder}/{name}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
