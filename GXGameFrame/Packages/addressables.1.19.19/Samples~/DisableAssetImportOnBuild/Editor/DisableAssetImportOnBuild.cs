using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
#endif
using UnityEngine.AddressableAssets;
using UnityEngine;

/// <summary>
/// Disabled AssetImporter for Player Build.
/// </summary>
public class DisableAssetImportOnBuild
{
#if UNITY_EDITOR
    /// <summary>
    /// Disables the AssetImporter for a player build.
    /// </summary>
    [MenuItem("Build/Disabled Importer Build")]
    public static void DisabledImporterBuild()
    {
        try
        {
            string buildPath = $"DisabledImporterBuildPath/{EditorUserBuildSettings.activeBuildTarget}/";
            Directory.CreateDirectory(buildPath);

            AssetDatabase.StopAssetEditing();
            BuildPlayerOptions options = new BuildPlayerOptions()
            {
                target = EditorUserBuildSettings.activeBuildTarget,
                scenes = EditorBuildSettings.scenes.Select(s => s.path).ToArray(),
                options = BuildOptions.None,
                locationPathName = $"{buildPath}/build{GetExtension()}"
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            Addressables.Log(report.summary.ToString());
        }
        finally
        {
            AssetDatabase.StartAssetEditing();
        }
    }

    static string GetExtension()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows
            || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
            return ".exe";
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX)
            return ".app";
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            return ".apk";
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            return ".ipa";
        return "";
    }

#endif
}
