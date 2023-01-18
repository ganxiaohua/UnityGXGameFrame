using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

// ReSharper disable DelegateSubtraction

namespace UnityEditor.AddressableAssets.GUI
{
    //[CustomEditor(typeof(AddressableAssetGroupTemplate)), CanEditMultipleObjects]
    [CustomEditor(typeof(AddressableAssetGroupTemplate))]
    class AddressableAssetGroupTemplateInspector : Editor
    {
        List<Type> m_SchemaTypes;
        bool[] m_FoldoutState;

        AddressableAssetGroupTemplate m_AddressableAssetGroupTarget;

        //        // Used for Multi-group editing
        //        AddressableAssetGroupTemplate[] m_AddressableAssetGroupTargets;
        //        bool[] m_SchemaState;
        //        int m_NumSchemasVisible = -1;
        //        // Indicates whether not some schemas are hidden
        //        bool m_HiddenSchemas = false;

        // Stores a 2D list of schemas found on the other selected asset groups.
        // Each schema list contains only schemas of the same type (e.g. BundledAssetGroupSchema).
        List<List<AddressableAssetGroupSchema>> m_GroupSchemas;

        void OnEnable()
        {
            // Single group editing
            if (targets.Length == 1)
            {
                m_AddressableAssetGroupTarget = target as AddressableAssetGroupTemplate;
            }
            //            // Multi-group editing
            //            if (targets.Length > 1)
            //            {
            //                m_AddressableAssetGroupTargets = new AddressableAssetGroupTemplate[targets.Length];
            //                for (int i = 0; i < targets.Length; i++)
            //                {
            //                    m_AddressableAssetGroupTargets[i] = targets[i] as AddressableAssetGroupTemplate;
            //                }
            //                // use item with largest index as base
            //                m_AddressableAssetGroupTarget = m_AddressableAssetGroupTargets[m_AddressableAssetGroupTargets.Length - 1];
            //                InitializeMultiSelectGroupSchemas();
            //            }

            if (m_AddressableAssetGroupTarget != null)
            {
                m_SchemaTypes = AddressableAssetUtility.GetTypes<AddressableAssetGroupSchema>();
                m_FoldoutState = new bool[m_AddressableAssetGroupTarget.SchemaObjects.Count];
            }

            for (int i = 0; i < m_FoldoutState.Length; i++)
                m_FoldoutState[i] = true;
        }

        //        void InitializeMultiSelectGroupSchemas()
        //        {
        //            var schemas = m_AddressableAssetGroupTarget.SchemaObjects;
        //            if (schemas.Count == 0)
        //            {
        //                m_HiddenSchemas = false;
        //                return;
        //            }
        //
        //            m_SchemaState = new bool[schemas.Count];
        //            m_GroupSchemas = new List<List<AddressableAssetGroupSchema>>(schemas.Count);
        //
        //            // For each m_GroupTarget schema, check if the other selected groups also have the same schema.
        //            bool allGroupsHaveSchema;
        //            for (int i = 0; i < schemas.Count; i++)
        //            {
        //                m_GroupSchemas.Add(new List<AddressableAssetGroupSchema>());
        //                Type schema = schemas[i].GetType();
        //
        //                allGroupsHaveSchema = true;
        //                // Skip last group because it's the same group as m_GroupTarget
        //                for (int j = 0; j < m_AddressableAssetGroupTargets.Length - 1; j++)
        //                {
        //                    // Group has other schemas, which will not be shown because the m_GroupTarget doesn't have this schema
        //                    if (m_AddressableAssetGroupTargets[j].SchemaObjects.Count != schemas.Count)
        //                        m_HiddenSchemas = true;
        //
        //                    // Check if other group also has this schema
        //                    if (m_AddressableAssetGroupTargets[j].HasSchema(schema))
        //                        m_GroupSchemas[i].Add(m_AddressableAssetGroupTargets[j].GetSchemaByType(schema));
        //                    else
        //                        allGroupsHaveSchema = false;
        //                }
        //
        //                // All selected groups have this schema
        //                if (allGroupsHaveSchema)
        //                {
        //                    m_NumSchemasVisible++;
        //                    m_SchemaState[i] = true;
        //                }
        //            }
        //        }

        void DrawDivider()
        {
            GUILayout.Space(1.5f);
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(2.5f));
            r.height = 1;

            if (EditorGUIUtility.isProSkin)
                EditorGUI.DrawRect(r, Color.black);
            else
                EditorGUI.DrawRect(r, Color.gray);
        }

        public override void OnInspectorGUI()
        {
            try
            {
                serializedObject.Update();

                if (targets.Length == 1)
                {
                    DrawSingleGroup();
                }
                //                else if (targets.Length > 1)
                //                {
                //                    DrawMultipleGroups();
                //                }

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

        void DrawSingleGroup()
        {
            EditorGUILayout.LabelField("Group Template Description");
            m_AddressableAssetGroupTarget.Description = EditorGUILayout.TextArea(m_AddressableAssetGroupTarget.Description);

            int objectCount = m_AddressableAssetGroupTarget.SchemaObjects.Count;
            if (m_FoldoutState == null || m_FoldoutState.Length != objectCount)
                m_FoldoutState = new bool[objectCount];

            for (int i = 0; i < objectCount; i++)
            {
                var schema = m_AddressableAssetGroupTarget.SchemaObjects[i];
                int currentIndex = i;

                DrawDivider();
                EditorGUILayout.BeginHorizontal();
                m_FoldoutState[i] = EditorGUILayout.Foldout(m_FoldoutState[i], AddressableAssetUtility.GetCachedTypeDisplayName(m_AddressableAssetGroupTarget.SchemaObjects[i].GetType()));

                GUILayout.FlexibleSpace();
                GUIStyle gearIconStyle = UnityEngine.GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");

                if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Popup"), FocusType.Keyboard, gearIconStyle))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(AddressableAssetGroup.RemoveSchemaContent, false, () =>
                    {
                        var schemaName = AddressableAssetUtility.GetCachedTypeDisplayName(m_AddressableAssetGroupTarget.SchemaObjects[currentIndex].GetType());
                        if (EditorUtility.DisplayDialog("Remove selected schema?", "Are you sure you want to remove " + schemaName + " schema?\n\nYou cannot undo this action.", "Yes", "No"))
                        {
                            m_AddressableAssetGroupTarget.RemoveSchema(currentIndex);
                            var newFoldoutstate = new bool[objectCount - 1];
                            for (int j = 0; j < newFoldoutstate.Length; j++)
                            {
                                if (j < i)
                                    newFoldoutstate[j] = m_FoldoutState[j];
                                else
                                    newFoldoutstate[j] = m_FoldoutState[currentIndex + 1];
                            }

                            m_FoldoutState = newFoldoutstate;
                        }
                    });
                    menu.AddItem(AddressableAssetGroup.MoveSchemaUpContent, false, () =>
                    {
                        if (currentIndex > 0)
                        {
                            m_AddressableAssetGroupTarget.SchemaObjects[currentIndex] = m_AddressableAssetGroupTarget.SchemaObjects[currentIndex - 1];
                            m_AddressableAssetGroupTarget.SchemaObjects[currentIndex - 1] = schema;
                            return;
                        }
                    });
                    menu.AddItem(AddressableAssetGroup.MoveSchemaDownContent, false, () =>
                    {
                        if (currentIndex < m_AddressableAssetGroupTarget.SchemaObjects.Count - 1)
                        {
                            m_AddressableAssetGroupTarget.SchemaObjects[currentIndex] = m_AddressableAssetGroupTarget.SchemaObjects[currentIndex + 1];
                            m_AddressableAssetGroupTarget.SchemaObjects[currentIndex + 1] = schema;
                            return;
                        }
                    });
                    menu.AddSeparator("");
                    menu.AddItem(AddressableAssetGroup.ExpandSchemaContent, false, () =>
                    {
                        m_FoldoutState[currentIndex] = true;
                        m_AddressableAssetGroupTarget.SchemaObjects[currentIndex].ShowAllProperties();
                    });
                    menu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();

                if (m_FoldoutState[i])
                {
                    try
                    {
                        EditorGUI.indentLevel++;
                        m_AddressableAssetGroupTarget.SchemaObjects[i].OnGUI();
                        EditorGUI.indentLevel--;
                    }
                    catch (Exception se)
                    {
                        Debug.LogException(se);
                    }
                }
            }

            DrawDivider();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUIStyle addSchemaButton = new GUIStyle(UnityEngine.GUI.skin.button);
            addSchemaButton.fontSize = 12;
            addSchemaButton.fixedWidth = 225;
            addSchemaButton.fixedHeight = 22;

            if (EditorGUILayout.DropdownButton(new GUIContent("Add Schema", "Add new schema to this group."), FocusType.Keyboard, addSchemaButton))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < m_SchemaTypes.Count; i++)
                {
                    var type = m_SchemaTypes[i];

                    if (Array.IndexOf(m_AddressableAssetGroupTarget.GetTypes(), type) == -1)
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

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        //        void DrawMultipleGroups()
        //        {
        //            // Group Template Description
        //            EditorGUILayout.LabelField("Group Template Description");
        //            for(int i = 0; i < m_AddressableAssetGroupTargets.Length - 1; i++)
        //            {
        //                if (m_AddressableAssetGroupTargets[i].Description != m_AddressableAssetGroupTarget.Description)
        //                {
        //                    EditorGUI.showMixedValue = true;
        //                    break;
        //                }
        //            }
        //            EditorGUI.BeginChangeCheck();
        //            m_AddressableAssetGroupTarget.Description = EditorGUILayout.TextArea(m_AddressableAssetGroupTarget.Description);
        //            EditorGUI.showMixedValue = false;
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                for (int i = 0; i < m_AddressableAssetGroupTargets.Length - 1; i++)
        //                {
        //                   m_AddressableAssetGroupTargets[i].Description = m_AddressableAssetGroupTarget.Description;
        //                }
        //            }
        //
        //            // Schemas
        //            int objectCount = m_AddressableAssetGroupTarget.SchemaObjects.Count;
        //            if (m_FoldoutState == null || m_FoldoutState.Length != objectCount)
        //                m_FoldoutState = new bool[objectCount];
        //
        //            for (int i = 0; i < objectCount; i++)
        //            {
        //                if (!m_SchemaState[i]) continue;
        //
        //                var schema = m_AddressableAssetGroupTarget.SchemaObjects[i];
        //                int currentIndex = i;
        //
        //                DrawDivider();
        //                EditorGUILayout.BeginHorizontal();
        //                m_FoldoutState[i] = EditorGUILayout.Foldout(m_FoldoutState[i], m_AddressableAssetGroupTarget.SchemaObjects[i].DisplayName());
        //
        //                GUILayout.FlexibleSpace();
        //                GUIStyle gearIconStyle = UnityEngine.GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");
        //
        //                if (EditorGUILayout.DropdownButton(EditorGUIUtility.IconContent("_Popup"), FocusType.Keyboard, gearIconStyle))
        //                {
        //                    var menu = new GenericMenu();
        //                    menu.AddItem(AddressableAssetGroup.RemoveSchemaContent, false, () =>
        //                    {
        //                        var schemaName = m_AddressableAssetGroupTarget.SchemaObjects[currentIndex].DisplayName();
        //                        if (EditorUtility.DisplayDialog("Remove selected schema?", "Are you sure you want to remove " + schemaName + " schema?\n\nYou cannot undo this action.", "Yes", "No"))
        //                        {
        //                            Type schemaType = m_AddressableAssetGroupTarget.SchemaObjects[currentIndex].GetType();
        //                            m_AddressableAssetGroupTarget.RemoveSchema(currentIndex);
        //                            for (int j = 0; j < m_AddressableAssetGroupTargets.Length - 1; j++)
        //                            {
        //                                int removeIndex = m_AddressableAssetGroupTargets[j].FindSchema(schemaType);
        //                                m_AddressableAssetGroupTargets[j].RemoveSchema(removeIndex);
        //                            }
        //
        //                            InitializeMultiSelectGroupSchemas();
        //
        //                           var newFoldoutstate = new bool[objectCount - 1];
        //                            for (int j = 0; j < newFoldoutstate.Length; j++)
        //                            {
        //                                if (j < i)
        //                                    newFoldoutstate[j] = m_FoldoutState[j];
        //                                else
        //                                    newFoldoutstate[j] = m_FoldoutState[currentIndex + 1];
        //                            }
        //
        //                            m_FoldoutState = newFoldoutstate;
        //                            return;
        //                        }
        //                    });
        //                    menu.AddItem(AddressableAssetGroup.MoveSchemaUpContent, false, () =>
        //                    {
        //                        foreach (var group in m_AddressableAssetGroupTargets)
        //                        {
        //                            int index = group.FindSchema(schema.GetType());
        //                            if (index > 0)
        //                            {
        //                                var temp = group.SchemaObjects[index];
        //                                group.SchemaObjects[index] = group.SchemaObjects[index - 1];
        //                                group.SchemaObjects[index - 1] = temp;
        //                            }
        //                        }
        //                        InitializeMultiSelectGroupSchemas();
        //                        return;
        //                    });
        //                    menu.AddItem(AddressableAssetGroup.MoveSchemaDownContent, false, () =>
        //                    {
        //                        foreach (var group in m_AddressableAssetGroupTargets)
        //                        {
        //                            int index = group.FindSchema(schema.GetType());
        //                            if (index >= 0 && index < group.SchemaObjects.Count - 1)
        //                            {
        //                                var temp = group.SchemaObjects[index];
        //                                group.SchemaObjects[index] = group.SchemaObjects[index + 1];
        //                                group.SchemaObjects[index + 1] = temp;
        //                            }
        //                        }
        //                        InitializeMultiSelectGroupSchemas();
        //                        return;
        //                    });
        //                    menu.AddSeparator("");
        //                    menu.AddItem(AddressableAssetGroup.ExpandSchemaContent, false, () =>
        //                    {
        //                        m_FoldoutState[currentIndex] = true;
        //                        foreach (var group in m_AddressableAssetGroupTargets)
        //                        {
        //                            int index = group.FindSchema(schema.GetType());
        //                            if (index != -1)
        //                            {
        //                                group.SchemaObjects[index].ShowAllProperties();
        //                            }
        //                        }
        //                    });
        //                    menu.ShowAsContext();
        //                }
        //
        //                EditorGUILayout.EndHorizontal();
        //
        //                if (m_FoldoutState[i])
        //                {
        //                    try
        //                    {
        //                        EditorGUI.indentLevel++;
        //                        m_AddressableAssetGroupTarget.SchemaObjects[i].OnGUIMultiple(m_GroupSchemas[i]);
        //                        EditorGUI.indentLevel--;
        //                    }
        //                    catch (Exception se)
        //                    {
        //                        Debug.LogException(se);
        //                    }
        //                }
        //            }
        //
        //            if (m_HiddenSchemas)
        //            {
        //                DrawDivider();
        //                EditorGUILayout.HelpBox(new GUIContent("Only schemas that are on all selected groups can be multi-edited."));
        //            }
        //
        //            DrawDivider();
        //            EditorGUILayout.BeginHorizontal();
        //            GUILayout.FlexibleSpace();
        //            GUIStyle addSchemaButton = new GUIStyle(UnityEngine.GUI.skin.button);
        //            addSchemaButton.fontSize = 12;
        //            addSchemaButton.fixedWidth = 225;
        //            addSchemaButton.fixedHeight = 22;
        //
        //            if (EditorGUILayout.DropdownButton(new GUIContent("Add Schema", "Add new schema to this group."), FocusType.Keyboard, addSchemaButton))
        //            {
        //                var menu = new GenericMenu();
        //                for (int i = 0; i < m_SchemaTypes.Count; i++)
        //                {
        //                    var type = m_SchemaTypes[i];
        //                    var schema = (AddressableAssetGroupSchema)CreateInstance(type);
        //
        //                    bool allGroupsDoNotHave = true;
        //                    foreach (var group in m_AddressableAssetGroupTargets)
        //                    {
        //                        if (group.HasSchema(type))
        //                            allGroupsDoNotHave = false;
        //                    }
        //
        //                    if (allGroupsDoNotHave)
        //                    {
        //                        menu.AddItem(new GUIContent(schema.DisplayName(), ""), false, () =>
        //                        {
        //                            OnAddSchema(type, true);
        //                            return;
        //                        });
        //                    }
        //                    else
        //                    {
        //                        menu.AddDisabledItem(new GUIContent(schema.DisplayName(), ""), true);
        //                    }
        //                }
        //
        //                menu.ShowAsContext();
        //            }
        //
        //            GUILayout.FlexibleSpace();
        //            EditorGUILayout.EndHorizontal();
        //        }

        void OnAddSchema(Type schemaType, bool multiSelect = false)
        {
            if (!m_AddressableAssetGroupTarget.AddSchema(schemaType))
                return;
            //            if (multiSelect)
            //            {
            //                for (int i = 0; i < m_AddressableAssetGroupTargets.Length - 1; i++)
            //                {
            //                   if(!m_AddressableAssetGroupTargets[i].AddSchema(schemaType))
            //                        return;
            //                }
            //                InitializeMultiSelectGroupSchemas();
            //            }

            var newFoldoutState = new bool[m_AddressableAssetGroupTarget.SchemaObjects.Count];
            for (int i = 0; i < m_FoldoutState.Length; i++)
                newFoldoutState[i] = m_FoldoutState[i];
            m_FoldoutState = newFoldoutState;
            m_FoldoutState[m_FoldoutState.Length - 1] = true;
        }
    }
}
