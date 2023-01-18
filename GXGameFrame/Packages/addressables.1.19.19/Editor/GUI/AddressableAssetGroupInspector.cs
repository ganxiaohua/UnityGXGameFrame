using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

// ReSharper disable DelegateSubtraction

namespace UnityEditor.AddressableAssets.GUI
{
    [CustomEditor(typeof(AddressableAssetGroup)), CanEditMultipleObjects]
    class AddressableAssetGroupInspector : Editor
    {
        AddressableAssetGroup m_GroupTarget;
        List<Type> m_SchemaTypes;

        // Used for Multi-group editing
        AddressableAssetGroup[] m_GroupTargets;

        // Stores a 2D list of schemas found on the other selected asset groups.
        // Each schema list contains only schemas of the same type (e.g. BundledAssetGroupSchema).
        List<List<AddressableAssetGroupSchema>> m_GroupSchemas;

        void OnEnable()
        {
            m_GroupTargets = new AddressableAssetGroup[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                m_GroupTargets[i] = targets[i] as AddressableAssetGroup;
            }

            // use item with largest index as base
            m_GroupTarget = m_GroupTargets[m_GroupTargets.Length - 1];

            if (m_GroupTarget != null)
            {
                m_GroupTarget.Settings.OnModification += OnSettingsModification;
                m_SchemaTypes = AddressableAssetUtility.GetTypes<AddressableAssetGroupSchema>();
            }
        }

        void OnDisable()
        {
            if (m_GroupTarget != null)
                m_GroupTarget.Settings.OnModification -= OnSettingsModification;
        }

        void OnSettingsModification(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent evnt, object o)
        {
            switch (evnt)
            {
                case AddressableAssetSettings.ModificationEvent.GroupAdded:
                case AddressableAssetSettings.ModificationEvent.GroupRemoved:
                case AddressableAssetSettings.ModificationEvent.GroupRenamed:
                case AddressableAssetSettings.ModificationEvent.BatchModification:
                case AddressableAssetSettings.ModificationEvent.ActiveProfileSet:
                case AddressableAssetSettings.ModificationEvent.GroupSchemaAdded:
                case AddressableAssetSettings.ModificationEvent.GroupSchemaModified:
                case AddressableAssetSettings.ModificationEvent.GroupSchemaRemoved:
                    Repaint();
                    break;
            }
        }

        void DrawDivider()
        {
            GUILayout.Space(1.5f);
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(2.5f));
            r.x = 0;
            r.width = EditorGUIUtility.currentViewWidth;
            r.height = 1;

            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }
            EditorGUI.DrawRect(r, color);
        }
        
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            try
            {
                serializedObject.Update();
                DrawSchemas(GetSchemasToDraw());
                serializedObject.ApplyModifiedProperties();
            }
            catch (UnityEngine.ExitGUIException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        List<AddressableAssetGroupSchema> GetSchemasToDraw()
        {
            List<AddressableAssetGroupSchema> values = new List<AddressableAssetGroupSchema>();

            if (m_GroupTargets == null || m_GroupTargets.Length == 0)
                return values;

            values.AddRange(m_GroupTarget.Schemas);

            foreach (var group in m_GroupTargets)
            {
                if (group != m_GroupTarget)
                    values = values.Intersect(group.Schemas, new GroupSchemasCompare()).ToList();
            }

            return values;
        }

        List<AddressableAssetGroupSchema> GetSchemasForOtherTargets(AddressableAssetGroupSchema schema)
        {
            List<AddressableAssetGroupSchema> values = m_GroupTargets
                .Where(t => t.HasSchema(schema.GetType()) && t != m_GroupTarget)
                .Select(t => t.GetSchema(schema.GetType())).ToList();

            return values;
        }

        void DrawSchemas(List<AddressableAssetGroupSchema> schemas)
        {
            GUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            var activeProfileName = m_GroupTarget.Settings.profileSettings.GetProfileName(m_GroupTarget.Settings.activeProfileId);
            if (string.IsNullOrEmpty(activeProfileName))
            {
                m_GroupTarget.Settings.activeProfileId = null; //this will reset it to default.
                activeProfileName = m_GroupTarget.Settings.profileSettings.GetProfileName(m_GroupTarget.Settings.activeProfileId);
            }
            EditorGUILayout.PrefixLabel("Active Profile: " + activeProfileName);
            if (GUILayout.Button("Inspect Top Level Settings"))
            {
                EditorGUIUtility.PingObject(AddressableAssetSettingsDefaultObject.Settings);
                Selection.activeObject = AddressableAssetSettingsDefaultObject.Settings;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6);
            bool doDrawDivider = false;

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < schemas.Count; i++)
            {
                var schema = schemas[i];
                var schemaType = schema.GetType();
                int currentIndex = i;
                
                string foldoutKey = "Addressables.GroupSchema." + schemaType.Name;
                bool foldoutActive = AddressablesGUIUtility.GetFoldoutValue(foldoutKey);
                
                string helpUrl = null;
                if(schemaType == typeof(BundledAssetGroupSchema))
                    helpUrl = AddressableAssetUtility.GenerateDocsURL("GroupSettings.html#content-packing--loading-settings");
                if(schemaType == typeof(ContentUpdateGroupSchema))
                    helpUrl = AddressableAssetUtility.GenerateDocsURL("GroupSettings.html#content-update-restriction");
                Action helpAction = () =>
                {
                    Application.OpenURL(helpUrl);
                };
                
                Action<Rect> menuAction = rect =>
                {
                    var menu = new GenericMenu();
                    menu.AddItem(AddressableAssetGroup.RemoveSchemaContent, false, () =>
                    {
                        if (EditorUtility.DisplayDialog("Remove selected schema?", "Are you sure you want to remove " + AddressableAssetUtility.GetCachedTypeDisplayName(schemaType) + " schema?\n\nYou cannot undo this action.", "Yes", "No"))
                        {
                            m_GroupTarget.RemoveSchema(schemaType);
                        }
                    });
                    menu.AddItem(AddressableAssetGroup.MoveSchemaUpContent, false, () =>
                    {
                        if (currentIndex > 0)
                        {
                            m_GroupTarget.Schemas[currentIndex] = m_GroupTarget.Schemas[currentIndex - 1];
                            m_GroupTarget.Schemas[currentIndex - 1] = schema;
                        }
                    });
                    menu.AddItem(AddressableAssetGroup.MoveSchemaDownContent, false, () =>
                    {
                        if (currentIndex < m_GroupTarget.Schemas.Count - 1)
                        {
                            m_GroupTarget.Schemas[currentIndex] = m_GroupTarget.Schemas[currentIndex + 1];
                            m_GroupTarget.Schemas[currentIndex + 1] = schema;
                        }
                    });
                    menu.AddSeparator("");
                    menu.AddItem(AddressableAssetGroup.ExpandSchemaContent, false, () =>
                    {
                        if (foldoutActive == false)
                        {
                            foldoutActive = true;
                            AddressablesGUIUtility.SetFoldoutValue(foldoutKey, foldoutActive);
                        }
                        foreach (var targetSchema in m_GroupTarget.Schemas)
                            targetSchema.ShowAllProperties();
                    });
                    menu.ShowAsContext();
                };

                EditorGUI.BeginChangeCheck();
                foldoutActive = AddressablesGUIUtility.BeginFoldoutHeaderGroupWithHelp(foldoutActive, new GUIContent(AddressableAssetUtility.GetCachedTypeDisplayName(schemaType)),
                    string.IsNullOrEmpty(helpUrl) ? null : helpAction, 0, m_GroupTarget.ReadOnly ? null : menuAction);
                if (EditorGUI.EndChangeCheck())
                    AddressablesGUIUtility.SetFoldoutValue(foldoutKey, foldoutActive);
                EditorGUI.EndFoldoutHeaderGroup();
                
                if (foldoutActive)
                {
                    try
                    {
                        EditorGUI.indentLevel++;
                        if (m_GroupTargets.Length == 1)
                            schema.OnGUI();
                        else
                            schema.OnGUIMultiple(GetSchemasForOtherTargets(schema));
                        EditorGUI.indentLevel--;
                    }
                    catch (Exception se)
                    {
                        Debug.LogException(se);
                    }
                    GUILayout.Space(10);
                }
                
                if (foldoutActive && i == schemas.Count-1)
                    doDrawDivider = true;
            }
            
            if (doDrawDivider)
                DrawDivider();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUIStyle addSchemaButton = new GUIStyle(UnityEngine.GUI.skin.button);
            addSchemaButton.fontSize = 12;
            addSchemaButton.fixedWidth = 225;
            addSchemaButton.fixedHeight = 22;

            if (!m_GroupTarget.ReadOnly)
            {
                if (EditorGUILayout.DropdownButton(new GUIContent("Add Schema", "Add new schema to this group."), FocusType.Keyboard, addSchemaButton))
                {
                    var menu = new GenericMenu();
                    for (int i = 0; i < m_SchemaTypes.Count; i++)
                    {
                        var type = m_SchemaTypes[i];

                        if (m_GroupTarget.GetSchema(type) == null)
                        {
                            menu.AddItem(new GUIContent(AddressableAssetUtility.GetCachedTypeDisplayName(type), ""), false, () => OnAddSchema(type));
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent(AddressableAssetUtility.GetCachedTypeDisplayName(type), ""), true);
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void OnAddSchema(Type schemaType, bool multiSelect = false)
        {
            if (targets.Length > 1)
            {
                foreach (var t in m_GroupTargets)
                    if (!t.HasSchema(schemaType))
                        t.AddSchema(schemaType);
            }
            else
                m_GroupTarget.AddSchema(schemaType);
        }

        class GroupSchemasCompare : IEqualityComparer<AddressableAssetGroupSchema>
        {
            public bool Equals(AddressableAssetGroupSchema x, AddressableAssetGroupSchema y)
            {
                if (x.GetType() == y.GetType())
                    return true;

                return false;
            }

            public int GetHashCode(AddressableAssetGroupSchema obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
    }
}
