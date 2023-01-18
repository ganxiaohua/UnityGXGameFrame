using System;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class AddressableReadOnly : PropertyAttribute { }

    [CustomPropertyDrawer(typeof(AddressableReadOnly))]
    internal class AddressableReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property);
            EditorGUI.EndDisabledGroup();
        }
    }
}