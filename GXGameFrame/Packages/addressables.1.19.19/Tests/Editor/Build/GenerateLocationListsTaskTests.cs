using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;

public class GenerateLocationListsTaskTests : AddressableBuildTaskTestBase
{
    GenerateLocationListsTask.Input GenerateDefaultInput()
    {
        var input = new GenerateLocationListsTask.Input();
        input.AssetToFiles = new Dictionary<GUID, List<string>>();
        input.FileToBundle = new Dictionary<string, string>();
        input.Settings = m_Settings;
        input.BundleToAssetGroup = new Dictionary<string, string>();
        input.Target = EditorUserBuildSettings.activeBuildTarget;
        return input;
    }

    private bool FindLocationEntry(List<ContentCatalogDataEntry> locations, string key, out ContentCatalogDataEntry entry)
    {
        foreach (ContentCatalogDataEntry e in locations)
            if (e.Keys.Contains(key))
            {
                entry = e;
                return true;
            }
        entry = null;
        return false;
    }

    static K GetKeyForValue<K, V>(Dictionary<K, V> dict, V value) where V : class
    {
        foreach (var kvp in dict)
        {
            if (kvp.Value == value)
                return kvp.Key;
        }
        throw new System.Exception("Couldn't find value");
    }

    string CreateAddressablePrefab(GenerateLocationListsTask.Input input, string name, AddressableAssetGroup group, params string[] depFiles)
    {
        string guid = CreateAsset($"{TempPath}/{name}.prefab", name);
        var entry = m_Settings.CreateOrMoveEntry(guid, group, false, false);
        entry.address = Path.GetFileNameWithoutExtension(entry.AssetPath);
        input.AssetToFiles[new GUID(guid)] = new List<string>(depFiles);
        return guid;
    }

    AddressableAssetGroup CreateGroupMappedToBundle(GenerateLocationListsTask.Input input, string postfix)
    {
        AddressableAssetGroup group = m_Settings.CreateGroup($"testGroup{postfix}", false, false, false, null, typeof(BundledAssetGroupSchema));
        input.BundleToAssetGroup[$"bundle{postfix}"] = group.Guid;
        input.FileToBundle[$"file{postfix}"] = $"bundle{postfix}";
        return group;
    }

    void AssertLocationDependencies(GenerateLocationListsTask.Output output, string location, params string[] deps)
    {
        FindLocationEntry(output.Locations, location, out ContentCatalogDataEntry e1);
        CollectionAssert.AreEquivalent(e1.Dependencies, deps);
    }

    static List<AddressableAssetEntry> BuildAddressableAssetEntryList(AddressableAssetSettings settings)
    {
        List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
        foreach (AddressableAssetGroup group in settings.groups)
        {
            group.GatherAllAssets(entries, true, true, false);
        }
        return entries;
    }

    [Test]
    public void WhenAssetLoadsFromBundle_ProviderTypesIncludesBundledAssetProvider()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();
        AddressableAssetGroup groupX = CreateGroupMappedToBundle(input, "X");
        CreateAddressablePrefab(input, "p1", groupX, "fileX");
        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);
        GenerateLocationListsTask.Output output = GenerateLocationListsTask.ProcessInput(input);
        CollectionAssert.Contains(output.ProviderTypes, typeof(BundledAssetProvider));
    }

    [Test]
    public void WhenIncludeGUIDInCatalog_SetFalse_GUIDSNotIncluded()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();
        AddressableAssetGroup groupX = CreateGroupMappedToBundle(input, "X");
        var schema = groupX.GetSchema<BundledAssetGroupSchema>();
        var guid = CreateAddressablePrefab(input, "p1", groupX, "fileX");
        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);
        schema.IncludeGUIDInCatalog = true;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if (l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.Contains(l.Keys, guid);

        schema.IncludeGUIDInCatalog = false;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if (l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.DoesNotContain(l.Keys, guid);
    }

    [Test]
    public void WhenIncludeAddressOptionChanged_LocationsKeysAreSetCorrectly()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();
        AddressableAssetGroup groupX = CreateGroupMappedToBundle(input, "X");
        var schema = groupX.GetSchema<BundledAssetGroupSchema>();
        var guid = CreateAddressablePrefab(input, "p1", groupX, "fileX");
        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);

        schema.IncludeAddressInCatalog = true;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if (l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.Contains(l.Keys, "p1");

        schema.IncludeAddressInCatalog = false;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if (l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.DoesNotContain(l.Keys, "p1");
    }

    [Test]
    public void WhenIncludeLabelsOptionChanged_LocationsKeysAreSetCorrectly()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();
        AddressableAssetGroup groupX = CreateGroupMappedToBundle(input, "X");
        var schema = groupX.GetSchema<BundledAssetGroupSchema>();
        var guid = CreateAddressablePrefab(input, "p1", groupX, "fileX");
        groupX.GetAssetEntry(guid).SetLabel("LABEL1", true, true, true);
        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);

        schema.IncludeLabelsInCatalog = true;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if (l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.Contains(l.Keys, "LABEL1");

        schema.IncludeLabelsInCatalog = false;
        foreach (var l in GenerateLocationListsTask.ProcessInput(input).Locations)
            if(l.Provider == typeof(BundledAssetProvider).FullName)
                CollectionAssert.DoesNotContain(l.Keys, "LABEL1");
    }

    [Test]
    public void WhenGroupCreatesMultipleBundles_AllBundlesInAssetGroupToBundlesMap()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();
        AddressableAssetGroup group = m_Settings.CreateGroup($"groupX", false, false, false, null, typeof(BundledAssetGroupSchema));
        input.BundleToAssetGroup["bundleX"] = group.Guid;
        input.BundleToAssetGroup["bundleY"] = group.Guid;
        input.FileToBundle["fileX"] = "bundle1";
        input.FileToBundle["fileY"] = "bundle2";
        CreateAddressablePrefab(input, "p1", group, "fileX");
        CreateAddressablePrefab(input, "p2", group, "fileY");
        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);
        GenerateLocationListsTask.Output output = GenerateLocationListsTask.ProcessInput(input);
        CollectionAssert.AreEquivalent(new string[] { "bundle1", "bundle2" }, output.AssetGroupToBundles[group]);
    }

    [Test]
    public void WhenAssetHasDependencyOnBundle_AssetLocationIncludesRecursiveBundleDependencies()
    {
        GenerateLocationListsTask.Input input = GenerateDefaultInput();

        AddressableAssetGroup groupX = CreateGroupMappedToBundle(input, "X");
        AddressableAssetGroup groupY = CreateGroupMappedToBundle(input, "Y");
        AddressableAssetGroup groupZ = CreateGroupMappedToBundle(input, "Z");
        AddressableAssetGroup groupW = CreateGroupMappedToBundle(input, "W");

        CreateAddressablePrefab(input, "p1", groupX, "fileX", "fileY");
        CreateAddressablePrefab(input, "p2", groupY, "fileY");
        CreateAddressablePrefab(input, "p3", groupY, "fileY", "fileZ");
        CreateAddressablePrefab(input, "p4", groupZ, "fileZ");
        CreateAddressablePrefab(input, "p5", groupZ, "fileZ", "fileW");
        CreateAddressablePrefab(input, "p6", groupW, "fileW");

        input.AddressableAssetEntries = BuildAddressableAssetEntryList(input.Settings);
        GenerateLocationListsTask.Output output = GenerateLocationListsTask.ProcessInput(input);

        AssertLocationDependencies(output, "p1", "bundleX", "bundleY", "bundleZ", "bundleW");
        AssertLocationDependencies(output, "p2", "bundleY", "bundleZ", "bundleW");
        AssertLocationDependencies(output, "p3", "bundleY", "bundleZ", "bundleW");
        AssertLocationDependencies(output, "p4", "bundleZ", "bundleW");
        AssertLocationDependencies(output, "p5", "bundleZ", "bundleW");
        AssertLocationDependencies(output, "p6", "bundleW");
    }

    //static 
    [Test]
    [TestCase("abc", BuildTarget.XboxOne, @"\abc")]
    [TestCase("abc", BuildTarget.StandaloneWindows64, @"\abc")]
    [TestCase("abc", BuildTarget.iOS, @"/abc")]
    [TestCase("abc", BuildTarget.Android, @"/abc")]
    [TestCase("abc", BuildTarget.StandaloneLinux64, @"/abc")]
    [TestCase("abc", BuildTarget.Switch, @"/abc")]
    [TestCase("abc", BuildTarget.StandaloneOSX, @"/abc")]
    public void WhenBuildTargetIsWindowsOrXBox_BackSlashUsedInLoadPath(string id, BuildTarget target, string expected)
    {
        AddressableAssetGroup group = m_Settings.CreateGroup($"xyz", false, false, false, null, typeof(BundledAssetGroupSchema));
        var bag = group.GetSchema<BundledAssetGroupSchema>();
        var expectedPath = $"{bag.LoadPath.GetValue(m_Settings)}{expected}".Replace('/', GenerateLocationListsTask.PathSeparatorForPlatform(target));
        var path = GenerateLocationListsTask.GetLoadPath(group, id, target);
        Assert.AreEqual(expectedPath, path);
        m_Settings.RemoveGroup(group);
    }

    [Test]
    [TestCase("abc", BuildTarget.XboxOne)]
    [TestCase("abc", BuildTarget.StandaloneWindows64)]
    [TestCase("abc", BuildTarget.iOS)]
    [TestCase("abc", BuildTarget.Android)]
    [TestCase("abc", BuildTarget.StandaloneLinux64)]
    [TestCase("abc", BuildTarget.Switch)]
    [TestCase("abc", BuildTarget.StandaloneOSX)]
    public void WhenPathIsRemote_WithTrailingSlash_PathIsNotMalformed(string id, BuildTarget target)
    {
        //Setup
        string baseId = m_Settings.profileSettings.Reset();
        string profileId = m_Settings.profileSettings.AddProfile("remote", baseId);
        m_Settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, "http://127.0.0.1:80/");
        m_Settings.activeProfileId = profileId;
        AddressableAssetGroup group = m_Settings.CreateGroup($"xyz", false, false, false, null, typeof(BundledAssetGroupSchema));
        var bag = group.GetSchema<BundledAssetGroupSchema>();
        bag.LoadPath.Id = m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kRemoteLoadPath);

        //Test
        var path = GenerateLocationListsTask.GetLoadPath(group, id, target);
        string pahWithoutHttp = path.Replace("http://", "");

        //Assert
        Assert.IsFalse(path.Contains("/\\"));
        Assert.IsFalse(pahWithoutHttp.Contains("//"));

        //Cleanup
        m_Settings.RemoveGroup(group);
        m_Settings.activeProfileId = baseId;
    }

    [Test]
    [TestCase("abc", BuildTarget.XboxOne)]
    [TestCase("abc", BuildTarget.StandaloneWindows64)]
    [TestCase("abc", BuildTarget.iOS)]
    [TestCase("abc", BuildTarget.Android)]
    [TestCase("abc", BuildTarget.StandaloneLinux64)]
    [TestCase("abc", BuildTarget.Switch)]
    [TestCase("abc", BuildTarget.StandaloneOSX)]
    public void WhenPathIsRemote_WithoutTrailingSlash_PathIsNotMalformed(string id, BuildTarget target)
    {
        //Setup
        string baseId = m_Settings.profileSettings.Reset();
        string profileId = m_Settings.profileSettings.AddProfile("remote", baseId);
        m_Settings.profileSettings.SetValue(profileId, AddressableAssetSettings.kRemoteLoadPath, "http://127.0.0.1:80");
        m_Settings.activeProfileId = profileId;
        AddressableAssetGroup group = m_Settings.CreateGroup($"xyz", false, false, false, null, typeof(BundledAssetGroupSchema));
        var bag = group.GetSchema<BundledAssetGroupSchema>();
        bag.LoadPath.Id = m_Settings.profileSettings.GetVariableId(AddressableAssetSettings.kRemoteLoadPath);

        //Test
        var path = GenerateLocationListsTask.GetLoadPath(group, id, target);
        string pahWithoutHttp = path.Replace("http://", "");

        //Assert
        Assert.IsFalse(path.Contains("/\\"));
        Assert.IsFalse(pahWithoutHttp.Contains("//"));

        //Cleanup
        m_Settings.RemoveGroup(group);
        m_Settings.activeProfileId = baseId;
    }

    //[Test]
    //public void WhenEntryAddressContainsBrackets_ExceptionIsThrown()
    //{
    // TODO:
    //}
    //}
}
