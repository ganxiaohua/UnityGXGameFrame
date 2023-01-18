using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Settings.GroupSchemas
{
    /// <summary>
    /// Schema for the player data asset group
    /// </summary>
    //[CreateAssetMenu(fileName = "PlayerDataGroupSchema.asset", menuName = "Addressables/Group Schemas/Player Data")]
    [DisplayName("Resources and Built In Scenes")]
    public class PlayerDataGroupSchema : AddressableAssetGroupSchema
    {
        [FormerlySerializedAs("m_includeResourcesFolders")]
        [SerializeField]
        bool m_IncludeResourcesFolders = true;
        /// <summary>
        /// If enabled, all assets in resources folders will have addresses generated during the build.
        /// </summary>
        public bool IncludeResourcesFolders
        {
            get
            {
                return m_IncludeResourcesFolders;
            }
            set
            {
                m_IncludeResourcesFolders = value;
                SetDirty(true);
            }
        }
        [FormerlySerializedAs("m_includeBuildSettingsScenes")]
        [SerializeField]
        bool m_IncludeBuildSettingsScenes = true;
        /// <summary>
        /// If enabled, all scenes in the editor build settings will have addresses generated during the build.
        /// </summary>
        public bool IncludeBuildSettingsScenes
        {
            get
            {
                return m_IncludeBuildSettingsScenes;
            }
            set
            {
                m_IncludeBuildSettingsScenes = value;
                SetDirty(true);
            }
        }

        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            var so = new SerializedObject(this);
            SerializedProperty prop;

            // IncludeResourcesFolders
            prop = so.FindProperty("m_IncludeResourcesFolders");
            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_IncludeResourcesFolders");
            EditorGUI.BeginChangeCheck();
            bool newIncludeResourcesFolders = (bool)EditorGUILayout.Toggle(prop.displayName, IncludeResourcesFolders);
            if (EditorGUI.EndChangeCheck())
            {
                IncludeResourcesFolders = newIncludeResourcesFolders;
                foreach (var s in otherSchemas)
                    (s as PlayerDataGroupSchema).IncludeResourcesFolders = IncludeResourcesFolders;
            }
            EditorGUI.showMixedValue = false;

            // IncludeBuildSettingsScenes
            prop = so.FindProperty("m_IncludeBuildSettingsScenes");
            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_IncludeBuildSettingsScenes");
            EditorGUI.BeginChangeCheck();
            bool newIncludeBuildSettingsScenes = (bool)EditorGUILayout.Toggle(prop.displayName, IncludeBuildSettingsScenes);
            if (EditorGUI.EndChangeCheck())
            {
                IncludeBuildSettingsScenes = newIncludeBuildSettingsScenes;
                foreach (var s in otherSchemas)
                    (s as PlayerDataGroupSchema).IncludeBuildSettingsScenes = IncludeBuildSettingsScenes;
            }
            EditorGUI.showMixedValue = false;

            so.ApplyModifiedProperties();
        }
    }
}
