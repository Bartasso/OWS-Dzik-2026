using System;
using System.Collections.Generic;
using Game.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    /// <summary>Type dropdown for [SerializeReference, SubclassPicker] fields and list elements.</summary>
    [CustomPropertyDrawer(typeof(SubclassPickerAttribute))]
    public sealed class SubclassPickerDrawer : PropertyDrawer
    {
        private const string NoneLabel = "(none: pick a type)";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "[SubclassPicker] needs [SerializeReference]");
                return;
            }

            var buttonRect = new Rect(
                position.x + EditorGUIUtility.labelWidth,
                position.y,
                position.width - EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);

            object current = property.managedReferenceValue;
            string typeLabel = current == null ? NoneLabel : ObjectNames.NicifyVariableName(current.GetType().Name);
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(typeLabel), FocusType.Keyboard))
            {
                ShowTypeMenu(property, current?.GetType());
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private void ShowTypeMenu(SerializedProperty property, Type currentType)
        {
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            var menu = new GenericMenu();

            foreach (Type type in CandidateTypes(BaseType()))
            {
                Type picked = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), type == currentType, () =>
                {
                    serializedObject.Update();
                    SerializedProperty target = serializedObject.FindProperty(propertyPath);
                    target.managedReferenceValue = Activator.CreateInstance(picked);
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private Type BaseType()
        {
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            return fieldType;
        }

        private static List<Type> CandidateTypes(Type baseType)
        {
            var types = new List<Type>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (!type.IsAbstract && !type.IsGenericType && type.IsSerializable && type.GetConstructor(Type.EmptyTypes) != null)
                {
                    types.Add(type);
                }
            }

            types.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return types;
        }
    }
}
