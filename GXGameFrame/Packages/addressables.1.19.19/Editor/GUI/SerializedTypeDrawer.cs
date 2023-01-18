using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace UnityEditor.AddressableAssets.GUI
{
    [CustomPropertyDrawer(typeof(SerializedType), true)]
    internal class SerializedTypeDrawer : PropertyDrawer
    {
        List<Type> m_Types;
        FieldInfo m_SerializedFieldInfo;
        SerializedProperty m_Property;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUIMultiple(position, property, label, EditorGUI.showMixedValue);
        }

        public void OnGUIMultiple(Rect position, SerializedProperty property, GUIContent label, bool showMixed)
        {
            m_Property = property;
            if (m_SerializedFieldInfo == null)
                m_SerializedFieldInfo = GetFieldInfo(property);
            if (m_Types == null)
                m_Types = GetTypes(m_SerializedFieldInfo);

            List<GUIContent> typeContent = new List<GUIContent>();
            typeContent.Add(new GUIContent("<none>", "Clear the type."));
            foreach (var type in m_Types)
                typeContent.Add(new GUIContent(AddressableAssetUtility.GetCachedTypeDisplayName(type), ""));

            bool resetShowMixed = EditorGUI.showMixedValue;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.showMixedValue = showMixed;

            var st = (SerializedType)m_SerializedFieldInfo.GetValue(property.serializedObject.targetObject);

            int index = GetIndexForType(st.Value);
            int selectedValue = EditorGUI.Popup(position, label, index, typeContent.ToArray());

            if (selectedValue != index)
            {
                Undo.RecordObject(m_Property.serializedObject.targetObject, "Set Serialized Type");
                m_SerializedFieldInfo.SetValue(m_Property.serializedObject.targetObject,
                    new SerializedType
                    {
                        Value = selectedValue == 0 ? null : m_Types[selectedValue - 1],
                        ValueChanged = true
                    });
                EditorUtility.SetDirty(m_Property.serializedObject.targetObject);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(m_Property.serializedObject.targetObject);
            }

            EditorGUI.EndProperty();
            EditorGUI.showMixedValue = resetShowMixed;
        }

        int GetIndexForType(Type type)
        {
            if (type == null)
                return 0;
            int index = 1;
            foreach (var checkedType in m_Types)
            {
                if (checkedType == type)
                    break;
                index++;
            }

            return index;
        }

        static FieldInfo GetFieldInfo(SerializedProperty property)
        {
            var o = property.serializedObject.targetObject;
            var t = o.GetType();
            string propertyName = property.name;
            int i = property.propertyPath.IndexOf('.');
            if (i > 0)
                propertyName = property.propertyPath.Substring(0, i);
            return t.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        static List<Type> GetTypes(FieldInfo fieldInfo)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(SerializedTypeRestrictionAttribute), false);
            if (attrs.Length == 0 || !(attrs[0] is SerializedTypeRestrictionAttribute))
                return null;
            return AddressableAssetUtility.GetTypes((attrs[0] as SerializedTypeRestrictionAttribute).type);
        }
    }
}
