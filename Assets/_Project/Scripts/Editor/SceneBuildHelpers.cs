using System;
using System.Collections.Generic;
using Game.Presentation;
using UnityEditor;
using UnityEngine;
using static Game.Editor.SerializedFields;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>Shared pieces of the sandbox scene builders: primitives, materials, labels, prefabs and folders.</summary>
    internal static class SceneBuildHelpers
    {
        public static GameObject Primitive(
            string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material, bool keepCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider)
            {
                Object.DestroyImmediate(go.GetComponent<Collider>());
            }

            return go;
        }

        public static Material GetMaterial(string folder, string name, Color color)
        {
            string path = $"{folder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    throw new InvalidOperationException("URP Lit shader not found");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void Label(Transform parent, string text, float height, Camera camera, Font font)
        {
            var label = new GameObject("Label");
            label.transform.SetParent(parent, false);
            label.transform.localPosition = Vector3.up * height;
            TextMesh mesh = label.AddComponent<TextMesh>();
            ConfigureText(mesh, font, 0.05f);
            mesh.text = text;
            var billboard = label.AddComponent<Billboard>();
            Edit(billboard, s => SetObject(s, "_camera", camera));
        }

        public static TextMesh BuildNumberPrefab(string folder, Font font)
        {
            var go = new GameObject("DamageNumber");
            TextMesh mesh = go.AddComponent<TextMesh>();
            ConfigureText(mesh, font, 0.06f);
            mesh.text = "0";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{folder}/DamageNumber.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<TextMesh>();
        }

        public static T AddComponentToPrefab<T>(this GameObject prefab)
            where T : Component
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(path);
            contents.AddComponent<T>();
            PrefabUtility.SaveAsPrefabAsset(contents, path);
            PrefabUtility.UnloadPrefabContents(contents);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<T>();
        }

        public static T Disabled<T>(T behaviour)
            where T : Behaviour
        {
            behaviour.enabled = false;
            return behaviour;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
        }

        public static void AddToBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene existing in scenes)
            {
                if (existing.path == path)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ConfigureText(TextMesh mesh, Font font, float characterSize)
        {
            mesh.font = font;
            mesh.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            mesh.fontSize = 64;
            mesh.characterSize = characterSize;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
        }
    }
}
