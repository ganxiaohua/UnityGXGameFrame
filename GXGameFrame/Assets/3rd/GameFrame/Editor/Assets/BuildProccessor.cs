using Eden.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder { get; }

    public void OnPostprocessBuild(BuildReport report)
    {
        var playerDataPath = AddressablesHelper.PlayerDataPath;
        if (!Directory.Exists(playerDataPath)) return;
        FileUtil.DeleteFileOrDirectory(playerDataPath);
        FileUtil.DeleteFileOrDirectory(playerDataPath + ".meta");
        if (Directory.GetFiles(Application.streamingAssetsPath).Length != 0) return;
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath);
        FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + ".meta");
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        BuildScript.CopyToStreamingAssets(BuildScript.isFullRes);
    }
}