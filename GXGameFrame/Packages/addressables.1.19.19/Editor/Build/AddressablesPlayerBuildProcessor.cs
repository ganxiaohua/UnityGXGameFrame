using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_2021_2_OR_NEWER
/// <summary>
/// Maintains Addresssables build data when processing a player build.
/// </summary>
public class AddressablesPlayerBuildProcessor : BuildPlayerProcessor
{
    /// <summary>
    /// Functor to override Addressables build when building Player.
    /// </summary>
    /// <remarks>
    /// Functor is invoked where Addressables settings state to build Addressables content when performing a Player build.
    ///
    /// Available in Unity 2021.2 or later.
    /// </remarks>
    public static Func<AddressableAssetSettings, AddressablesPlayerBuildResult> BuildAddressablesOverride { get; set; }
    
    /// <summary>
    /// Returns the player build processor callback order.
    /// </summary>
    public override int callbackOrder
    {
        get { return 1; }
    }
    
    [InitializeOnLoadMethod]
    private static void CleanTemporaryPlayerBuildData()
    {
        RemovePlayerBuildLinkXML(AddressableAssetSettingsDefaultObject.Settings);
    }
    
    internal static void RemovePlayerBuildLinkXML(AddressableAssetSettings settings)
    {
        string linkProjectPath = GetLinkPath(settings, false);
        string guid = AssetDatabase.AssetPathToGUID(linkProjectPath);
        if (!string.IsNullOrEmpty(guid))
            AssetDatabase.DeleteAsset(linkProjectPath);
        else if (File.Exists(linkProjectPath))
            File.Delete(linkProjectPath);
        
        DirectoryUtility.DeleteDirectory(Path.GetDirectoryName(linkProjectPath));
    }

    private static string GetLinkPath(AddressableAssetSettings settings, bool createFolder)
    {
        string folderPath;
        if (settings == null)
            folderPath = "Assets/Addressables_Temp";
        else
            folderPath = settings.ConfigFolder;

        if (createFolder && !Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.ImportAsset(folderPath);
        }
        
        return Path.Combine(folderPath, "link.xml");;
    }
    
    /// <summary>
    /// Invoked before performing a Player build. Maintains building Addressables step and processing Addressables build data.
    /// </summary>
    /// <param name="buildPlayerContext"></param>
    public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        PrepareForPlayerbuild(settings, buildPlayerContext, ShouldBuildAddressablesForPlayerBuild(settings));
    }

    internal static void PrepareForPlayerbuild(AddressableAssetSettings settings, BuildPlayerContext buildPlayerContext, bool buildAddressables)
    {
        if (settings != null && buildAddressables)
        {
            AddressablesPlayerBuildResult result;
            if (BuildAddressablesOverride != null)
            {
                try
                {
                    result = BuildAddressablesOverride.Invoke(settings);
                }
                catch(Exception e)
                {
                    result = new AddressablesPlayerBuildResult();
                    result.Error = "Exception in BuildAddressablesOverride: " + e;
                }
            }
            else
                AddressableAssetSettings.BuildPlayerContent(out result);
            
            if (result != null && !string.IsNullOrEmpty(result.Error))
                Debug.LogError($"Failed to build Addressables content, content not included in Player Build. \"{result.Error}\"");
        }
        
        if (buildPlayerContext != null)
        {
            var streamingAssetValues = GetStreamingAssetPaths();
            foreach (KeyValuePair<string, string> streamingAssetValue in streamingAssetValues)
            {
                buildPlayerContext.AddAdditionalPathToStreamingAssets(streamingAssetValue.Key,
                    streamingAssetValue.Value);
            }
        }
        
        string buildPath = Addressables.BuildPath + "/AddressablesLink/link.xml";
        if (File.Exists(buildPath))
        {
            string projectPath = GetLinkPath(settings, true);
            File.Copy(buildPath, projectPath, true);
            AssetDatabase.ImportAsset(projectPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
        }
    }

    internal static bool ShouldBuildAddressablesForPlayerBuild(AddressableAssetSettings settings)
    {
        if (settings == null)
            return false;
        
        switch (settings.BuildAddressablesWithPlayerBuild)
        {
            case AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer:
                return false;
            case AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer:
                break;
            case AddressableAssetSettings.PlayerBuildOption.PreferencesValue:
                if (!EditorPrefs.GetBool(AddressablesPreferences.kBuildAddressablesWithPlayerBuildKey, true))
                    return false;
                break;
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets a list of StreamingAsset files managed through Addressables, and relative path in StreamingAssets.
    /// </summary>
    internal static List<KeyValuePair<string, string>> GetStreamingAssetPaths()
    {
        List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>(1);
        if (Directory.Exists(Addressables.BuildPath))
            pairs.Add(new KeyValuePair<string, string>(Addressables.BuildPath, "aa"));
        return pairs;
    }
}
#else
/// <summary>
/// Maintains Addresssables build data when processing a player build.
/// </summary>
public class AddressablesPlayerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    /// <summary>
    /// Returns the player build processor callback order.
    /// </summary>
    public int callbackOrder
    {
        get { return 1; }
    }

    /// <summary>
    /// Restores temporary data created as part of a build.
    /// </summary>
    /// <param name="report">Stores temporary player build data.</param>
    public void OnPostprocessBuild(BuildReport report)
    {
        CleanTemporaryPlayerBuildData();
    }

    [InitializeOnLoadMethod]
    internal static void CleanTemporaryPlayerBuildData()
    {
        if (Directory.Exists(Addressables.PlayerBuildDataPath))
        {
            DirectoryUtility.DirectoryMove(Addressables.PlayerBuildDataPath, Addressables.BuildPath);
            DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, onlyIfEmpty: true);
        }
    }

    ///<summary>
    /// Initializes temporary build data.
    /// </summary>
    /// <param name="report">Contains build data information.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        CopyTemporaryPlayerBuildData();
    }

    internal static void CopyTemporaryPlayerBuildData()
    {
        if (Directory.Exists(Addressables.BuildPath))
        {
            if (Directory.Exists(Addressables.PlayerBuildDataPath))
            {
                Debug.LogWarning($"Found and deleting directory \"{Addressables.PlayerBuildDataPath}\", directory is managed through Addressables.");
                DirectoryUtility.DeleteDirectory(Addressables.PlayerBuildDataPath, false);
            }

            string parentDir = Path.GetDirectoryName(Addressables.PlayerBuildDataPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);
            Directory.Move(Addressables.BuildPath, Addressables.PlayerBuildDataPath );
        }
    }
}
#endif

