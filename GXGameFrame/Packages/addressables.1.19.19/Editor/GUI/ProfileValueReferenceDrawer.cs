using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace UnityEditor.AddressableAssets.GUI
{
    [CustomPropertyDrawer(typeof(ProfileValueReference), true)]
    class ProfileValueReferenceDrawer : PropertyDrawer
    {
        FieldInfo m_SerializedFieldInfo;
        SerializedProperty m_Property;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUIMultiple(position, property, label, EditorGUI.showMixedValue);
        }

        public void OnGUIMultiple(Rect position, SerializedProperty property, GUIContent label, bool showMixed)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return;
            EditorGUI.BeginProperty(position, label, property);

            if (m_SerializedFieldInfo == null)
                m_SerializedFieldInfo = GetFieldInfo(property);

            EditorGUI.BeginProperty(position, label, property);
            var st = (ProfileValueReference)m_SerializedFieldInfo.GetValue(property.serializedObject.targetObject);

            bool wasChanged = false;
            string currentPathDisplay = st.Id;
            if (showMixed)
                currentPathDisplay = "—";

            var newId = ProfilesEditor.ValueGUI(position, settings, label.text, currentPathDisplay, ref wasChanged);
            if (newId != "—")
                st.Id = newId; // ignore mixed value

            if (wasChanged)
            {
                st = (ProfileValueReference)m_SerializedFieldInfo.GetValue(property.serializedObject.targetObject);
                if (st != null && st.OnValueChanged != null)
                    st.OnValueChanged(st);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                return 0;
            var idProp = property.FindPropertyRelative("m_Id");
            return ProfilesEditor.CalcGUIHeight(settings, label.text, idProp.stringValue);
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
