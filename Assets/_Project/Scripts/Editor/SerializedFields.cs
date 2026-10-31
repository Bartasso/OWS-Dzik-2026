using System;
using Game.Core;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    /// <summary>Sets private [SerializeField]s from editor tools; a renamed field fails loudly.</summary>
    internal static class SerializedFields
    {
        public static void Edit(Object target, Action<SerializedObject> edit)
        {
            var serialized = new SerializedObject(target);
            edit(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        public static SerializedProperty Find(SerializedObject serialized, string field)
        {
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{serialized.targetObject.GetType().Name} has no serialized field '{field}'");
            }

            return property;
        }

        public static void SetString(SerializedObject s, string field, string value) => Find(s, field).stringValue = value;

        public static void SetFloat(SerializedObject s, string field, float value) => Find(s, field).floatValue = value;

        public static void SetInt(SerializedObject s, string field, int value) => Find(s, field).intValue = value;

        public static void SetBool(SerializedObject s, string field, bool value) => Find(s, field).boolValue = value;

        public static void SetEnum<T>(SerializedObject s, string field, T value)
            where T : Enum
        {
            Find(s, field).intValue = Convert.ToInt32(value);
        }

        public static void SetObject(SerializedObject s, string field, Object value) => Find(s, field).objectReferenceValue = value;

        public static void SetObjects(SerializedObject s, string field, params Object[] values)
        {
            SerializedProperty array = Find(s, field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        public static void SetEnums<T>(SerializedObject s, string field, params T[] values)
            where T : Enum
        {
            SerializedProperty array = Find(s, field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).intValue = Convert.ToInt32(values[i]);
            }
        }

        public static void SetManagedReferences(SerializedObject s, string field, params object[] values)
        {
            SerializedProperty array = Find(s, field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                array.GetArrayElementAtIndex(i).managedReferenceValue = values[i];
            }
        }

        /// <summary>Fills a List of StatValue from (stat, value) pairs.</summary>
        public static void SetStatValues(SerializedObject s, string field, params (StatId Stat, float Value)[] values)
        {
            SerializedProperty array = Find(s, field);
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                SerializedProperty element = array.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("_stat").intValue = (int)values[i].Stat;
                element.FindPropertyRelative("_value").floatValue = values[i].Value;
            }
        }
    }
}
