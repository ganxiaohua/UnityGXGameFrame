#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>
/// Imports an existing AddressableAssetGroup and existing AddressableAssetGroupSchemas.
/// </summary>
public class ImportExistingGroup : EditorWindow
{
    string groupPath;
    string groupName;
    string schemaFolder;

    [MenuItem("Window/Asset Management/Addressables/Import Groups", priority = 2063)]
    public static void ShowWindow()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorUtility.DisplayDialog("Error", "Attempting to open Import Groups window, but no Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
            return;
        }
        GetWindow(typeof(ImportExistingGroup), false, "Import Groups");
    }

    void OnGUI()
    {
        groupPath = EditorGUILayout.TextField(new GUIContent("Group Path", "The path of the group asset to import, for example 'Packages/com.unity.example/MyGroup.asset'."), groupPath);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import Groups"))
            {
                ImportGroup(groupPath, schemaFolder);
                groupName = Path.GetFileNameWithoutExtension(groupPath);
            }
        }

        GUILayout.Space(10f);
        groupName = EditorGUILayout.TextField(new GUIContent("Group Name", "The name of the group that the schemas will be added to. This should be the filename of the imported group, for example 'MyGroup'."), groupName);
        schemaFolder = EditorGUILayout.TextField(new GUIContent("Schema Folder", "The folder containing the schema assets of the group to import, for example 'Packages/com.unity.example/Schemas'."), schemaFolder);
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Import Schemas"))
                ImportSchemas(groupName, schemaFolder);
        }
    }

    /// <summary>
    /// Adds an existing group to the default Settings. This will copy the group to the default location, typically in the Assets/AddressableAssetsData folder.
    /// </summary>
    /// <param name="groupPath">The path of the group.</param>
    /// <param name="schemaFolder">The folder containing only the group's schema assets.</param>
    public void ImportGroup(string groupPath, string schemaFolder)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorUtility.DisplayDialog("Error", "Cannot import group. No Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
            return;
        }
        ImportGroupInternal(settings, groupPath);
    }

    /// <summary>
    /// Adds existing schemas to a group. This will copy the schemas to the default location, typically in the Assets/AddressableAssetsData folder.
    /// </summary>
    /// <param name="groupPath">The name of the group.</param>
    /// <param name="schemaFolder">The folder containing the schema assets.</param>
    public void ImportSchemas(string groupName, string schemaFolder)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            EditorUtility.DisplayDialog("Error", "Cannot import schemas. No Addressables Settings file exists.  \n\nOpen 'Window/Asset Management/Addressables/Groups' for more info.", "Ok");
            return;
        }
        ImportSchemasInternal(settings, groupName, schemaFolder);
    }

    void ImportGroupInternal(AddressableAssetSettings settings, string groupPath)
    {
        if (string.IsNullOrEmpty(groupPath) || Path.GetExtension(groupPath).ToLower() != ".asset" || !File.Exists(groupPath))
        {
            Debug.LogError($"Group at '{groupPath}' not a valid group asset. Group will not be imported.");
            return;
        }
        AddressableAssetGroup oldGroup = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(groupPath);
        if (oldGroup == null)
        {
            Debug.LogError($"Cannot load group asset at '{groupPath}'. Group will not be imported.");
            return;
        }
        if (settings.FindGroup(oldGroup.Name) != null)
        {
            Debug.LogError($"Settings already contains group '{oldGroup.Name}'. Group will not be imported.");
            return;
        }

        string groupFileName = Path.GetFileName(groupPath);
        string newGroupPath =  $"{settings.GroupFolder}/{groupFileName}";
        newGroupPath = newGroupPath.Replace("\\", "/");
        if (File.Exists(newGroupPath))
        {
            Debug.LogError($"File already exists at '{newGroupPath}'. Group will not be imported.");
            return;
        }
        if (!AssetDatabase.CopyAsset(groupPath, newGroupPath))
            Debug.LogError("Failed to copy group asset. Importing group failed.");
    }

    void ImportSchemasInternal(AddressableAssetSettings settings, string groupName, string schemaFolder)
    {
        if (string.IsNullOrEmpty(schemaFolder) || !Directory.Exists(schemaFolder))
        {
            Debug.LogError($"Schema folder path is not a valid folder '{schemaFolder}'. Schemas will not be imported.");
            return;
        }
        AddressableAssetGroup group = settings.FindGroup(groupName);
        if (group == null)
        {
            Debug.LogError($"Settings does not contain group '{groupName}'. Schemas will not be imported.");
            return;
        }

        string[] schemaPaths = Directory.GetFiles(schemaFolder);
        foreach (string unparsedPath in schemaPaths)
        {
            if (Path.GetExtension(unparsedPath).ToLower() != ".asset")
                continue;

            string path = unparsedPath.Replace("\\", "/");
            AddressableAssetGroupSchema schema = AssetDatabase.LoadAssetAtPath<AddressableAssetGroupSchema>(path);
            if (schema == null)
            {
                Debug.LogError($"Cannot load schema asset at '{path}'. Schema will not be imported.");
                continue;
            }
            if (schema is BundledAssetGroupSchema bundledSchema)
            {
                List<string> variableNames = schema.Group.Settings.profileSettings.GetVariableNames();
                SetBundledAssetGroupSchemaPaths(settings, bundledSchema.BuildPath, AddressableAssetSettings.kLocalBuildPath,"LocalBuildPath",  variableNames);
                SetBundledAssetGroupSchemaPaths(settings, bundledSchema.LoadPath, AddressableAssetSettings.kLocalLoadPath,"LocalLoadPath",  variableNames);
            }
            group.AddSchema(schema);
        }
    }

    internal void SetBundledAssetGroupSchemaPaths(AddressableAssetSettings settings, ProfileValueReference pvr, string newVariableName, string oldVariableName, List<string> variableNames)
    {
        if (variableNames.Contains(newVariableName))
            pvr.SetVariableByName(settings, newVariableName);
        else if (variableNames.Contains(oldVariableName))
            pvr.SetVariableByName(settings, oldVariableName);
        else
            Debug.LogWarning("Default path variable " + newVariableName + " not found when initializing BundledAssetGroupSchema. Please manually set the path via the groups window.");
    }
}
#endif
