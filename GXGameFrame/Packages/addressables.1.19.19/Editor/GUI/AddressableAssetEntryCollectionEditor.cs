using System;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.AddressableAssets.GUI
{
#pragma warning disable 0618
    [CustomEditor(typeof(AddressableAssetEntryCollection))]
    class AddressableAssetEntryCollectionEditor : Editor
    {
        AddressableAssetEntryCollection m_Collection;
        ReorderableList m_EntriesList;

        void OnEnable()
        {
            m_Collection = target as AddressableAssetEntryCollection;
            if (m_Collection != null)
            {
                m_EntriesList = new ReorderableList(m_Collection.Entries, typeof(AddressableAssetEntry), false, true, false, false);
                m_EntriesList.drawElementCallback = DrawEntry;
                m_EntriesList.drawHeaderCallback = DrawHeader;
            }
        }

        void DrawHeader(Rect rect)
        {
            UnityEngine.GUI.Label(rect, "Asset Entries");
        }

        void DrawEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            UnityEngine.GUI.Label(rect, m_Collection.Entries[index].address);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                GUILayout.Label("No Addressables settings found.  Cannot convert Asset Entry Collection to Addressables Groups Entries.");
            }
            else
            {
                string buttonLabel = "Add Entries to Default Group and Delete Collection";
                if (GUILayout.Button(buttonLabel))
                {
                    if (AddressableAssetEntryCollection.ConvertEntryCollectionToEntries(m_Collection, settings))
                    {
                        string path = AssetDatabase.GetAssetPath(m_Collection);
                        if (string.IsNullOrEmpty(path) || !AssetDatabase.DeleteAsset(path))
                            Debug.LogError("Failed to Delete AssetEntryCollection: " + m_Collection.name);
                        Selection.objects = new UnityEngine.Object[0];
                        GUIUtility.ExitGUI();
                    }
                    else
                        Debug.LogError("Failed to convert AssetEntryCollection to AddressableAssetEntries at " + AssetDatabase.GetAssetPath(m_Collection));
                }
            }
            
            
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
           
            m_EntriesList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
#pragma warning restore 0618
}
