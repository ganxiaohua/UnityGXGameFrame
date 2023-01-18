using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

public class BuildScriptPackedIntegrationTests
{
    string CreateTexture(string path)
    {
        var data = ImageConversion.EncodeToPNG(new Texture2D(32, 32));
        File.WriteAllBytes(path, data);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return AssetDatabase.AssetPathToGUID(path);
    }

    static IDataBuilder GetBuilderOfType(AddressableAssetSettings settings, Type modeType)
    {
        foreach (var db in settings.DataBuilders)
        {
            var b = db;
            if (b.GetType() == modeType)
                return b as IDataBuilder;
        }
        throw new Exception("DataBuilder not found");
    }

    protected string m_SingleTestBuildFolder;
    protected string m_SingleTestAssetFolder;

    void DeleteSingleTestDirectories()
    {
        if (Directory.Exists(m_SingleTestBuildFolder))
            Directory.Delete(m_SingleTestBuildFolder, true);

        if (Directory.Exists(m_SingleTestAssetFolder))
            Directory.Delete(m_SingleTestAssetFolder, true);
    }

    string m_SettingsPath;
    AddressableAssetSettings m_Settings;
    AddressableAssetSettings Settings
    {
        get
        {
            if (m_Settings == null)
                m_Settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(m_SettingsPath);
            return m_Settings;
        }
    }

    [SetUp]
    public void Setup()
    {
        m_SingleTestBuildFolder = "Temp/TestBuild";
        m_SingleTestAssetFolder = "Assets/SingleTestFolder";
        DeleteSingleTestDirectories();
        Directory.CreateDirectory(m_SingleTestBuildFolder);
        Directory.CreateDirectory(m_SingleTestAssetFolder);
        AddressableAssetSettings settings = AddressableAssetSettings.Create(Path.Combine(m_SingleTestAssetFolder, "Settings"), "AddressableAssetSettings.Tests", false, true);
        m_SettingsPath = settings.AssetPath;
    }

    [TearDown]
    public void TearDown()
    {
        DeleteSingleTestDirectories();
    }

    [Test]
    public void IncrementalBuild_WhenBundleTimestampUnchanged_DoesNotCopy()
    {
        AddressableAssetBuildResult result;
        var group = Settings.CreateGroup("MyTestGroup", true, false, false, null, typeof(BundledAssetGroupSchema));

        var spriteEntry = Settings.CreateOrMoveEntry(CreateTexture($"{m_SingleTestAssetFolder}/testTexture.png"), group, false, false);
        Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kLocalBuildPath, m_SingleTestBuildFolder);
        Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kLocalLoadPath, "Library/LocalLoadPath");

        IDataBuilder b = GetBuilderOfType(Settings, typeof(BuildScriptPackedMode));
        b.BuildData<AddressableAssetBuildResult>(new AddressablesDataBuilderInput(Settings));

        string[] buildFiles = Directory.GetFiles(m_SingleTestBuildFolder);

        // Build again with a lock on the output bundle. This is how we ensure that the bundle is not written again
        using (File.Open(buildFiles[0], FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            result = b.BuildData<AddressableAssetBuildResult>(new AddressablesDataBuilderInput(Settings));

        Assert.AreEqual(1, buildFiles.Length, "There should only be one bundle file in the build output folder");
        Assert.IsTrue(string.IsNullOrEmpty(result.Error));
    }

    [Test]
    public void IncrementalBuild_WhenBundleTimestampChanges_CopiesNewFile()
    {
        var group = Settings.CreateGroup("MyTestGroup", true, false, false, null, typeof(BundledAssetGroupSchema));

        var spriteEntry = Settings.CreateOrMoveEntry(CreateTexture($"{m_SingleTestAssetFolder}/testTexture.png"), group, false, false);
        Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kLocalBuildPath, m_SingleTestBuildFolder);
        Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kLocalLoadPath, "Library/LocalLoadPath");

        IDataBuilder b = GetBuilderOfType(Settings, typeof(BuildScriptPackedMode));
        b.BuildData<AddressableAssetBuildResult>(new AddressablesDataBuilderInput(Settings));

        string[] buildFiles = Directory.GetFiles(m_SingleTestBuildFolder);

        byte[] initialBundleBytes = File.ReadAllBytes(buildFiles[0]);
        File.Delete(buildFiles[0]);
        File.WriteAllText(buildFiles[0], "content");
        File.SetLastWriteTime(buildFiles[0], new DateTime(2019, 1, 1));

        b.BuildData<AddressableAssetBuildResult>(new AddressablesDataBuilderInput(Settings));

        Assert.AreEqual(1, buildFiles.Length, "There should only be one bundle file in the build output folder");
        CollectionAssert.AreEqual(initialBundleBytes, File.ReadAllBytes(buildFiles[0]));
    }
}
