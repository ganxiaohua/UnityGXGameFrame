using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Settings.GroupSchemas
{
    /// <summary>
    /// Schema for content updates.
    /// </summary>
    //  [CreateAssetMenu(fileName = "ContentUpdateGroupSchema.asset", menuName = "Addressables/Group Schemas/Content Update")]
    [DisplayName("Content Update Restriction")]
    public class ContentUpdateGroupSchema : AddressableAssetGroupSchema
    {
        enum ContentType
        {
            CanChangePostRelease,
            CannotChangePostRelease
        }

        [FormerlySerializedAs("m_staticContent")]
        [SerializeField]
        bool m_StaticContent;
        /// <summary>
        /// Is the group static.  This property is used in determining which assets need to be moved to a new remote group during the content update process.
        /// </summary>
        public bool StaticContent
        {
            get { return m_StaticContent; }
            set
            {
                m_StaticContent = value;
                SetDirty(true);
            }
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            ContentType current = m_StaticContent ? ContentType.CannotChangePostRelease : ContentType.CanChangePostRelease;
            var newType = (ContentType)EditorGUILayout.EnumPopup("Update Restriction", current);
            if (newType != current)
                StaticContent = newType == ContentType.CannotChangePostRelease;
        }

        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            var so = new SerializedObject(this);
            var prop = so.FindProperty("m_StaticContent");

            // Type/Static Content
            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_StaticContent");
            EditorGUI.BeginChangeCheck();
            ContentType current = m_StaticContent ? ContentType.CannotChangePostRelease : ContentType.CanChangePostRelease;
            var newType = (ContentType)EditorGUILayout.EnumPopup("Update Restriction", current);
            if (EditorGUI.EndChangeCheck())
            {
                StaticContent = newType == ContentType.CannotChangePostRelease;
                foreach (var s in otherSchemas)
                    (s as ContentUpdateGroupSchema).StaticContent = (newType == ContentType.CannotChangePostRelease);
            }
            EditorGUI.showMixedValue = false;

            so.ApplyModifiedProperties();
        }
    }
}
