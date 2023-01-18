using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableBuildTaskTestBase
{
    protected AddressableAssetSettings m_Settings;
    protected const string TempPath = "Assets/TempGen";

    [SetUp]
    public void Setup()
    {
        using (new IgnoreFailingLogMessage())
        {
            if (AssetDatabase.IsValidFolder(TempPath))
                AssetDatabase.DeleteAsset(TempPath);
            Directory.CreateDirectory(TempPath);

            m_Settings = AddressableAssetSettings.Create(Path.Combine(TempPath, "Settings"),
                "AddressableAssetSettings.Tests", false, true);
        }
    }

    [TearDown]
    public void Teardown()
    {
        // Many of the tests keep recreating assets in the same path, so we need to unload them completely so they don't get reused by the next test
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_Settings));
        Resources.UnloadAsset(m_Settings);
        if (Directory.Exists(TempPath))
            Directory.Delete(TempPath, true);
        AssetDatabase.Refresh();
    }

    protected static string CreateAsset(string assetPath, string objectName)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objectName;
        //this is to ensure that bundles are different for every run.
        go.transform.localPosition = UnityEngine.Random.onUnitSphere;
        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        UnityEngine.Object.DestroyImmediate(go, false);
        return AssetDatabase.AssetPathToGUID(assetPath);
    }
}
