using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

public class AddHashToBundleNameTaskTests : AddressableBuildTaskTestBase
{
    [Test]
    public void AddHashToBundleNameTask_DoesNotChangeHash_WhenAssetsChangeOrder()
    {
        //Setup
        string path1 = $"{TempPath}/test1.prefab";
        string path2 = $"{TempPath}/test2.prefab";
        string path3 = $"{TempPath}/test3.prefab";

        GUID guid1 = new GUID(CreateAsset(path1, "1"));
        GUID guid2 = new GUID(CreateAsset(path2, "2"));
        GUID guid3 = new GUID(CreateAsset(path3, "3"));

        List<GUID> list1 = new List<GUID>()
        {
            guid1,
            guid2,
            guid3
        };

        List<GUID> list2 = new List<GUID>()
        {
            guid2,
            guid1,
            guid3
        };

        AddressableAssetGroup group = m_Settings.CreateGroup("AddHashTestGroup", false, false, false,
            new List<AddressableAssetGroupSchema>());
        m_Settings.CreateOrMoveEntry(guid1.ToString(), group);
        m_Settings.CreateOrMoveEntry(guid2.ToString(), group);
        m_Settings.CreateOrMoveEntry(guid3.ToString(), group);

        IDependencyData dependencyData = new BuildDependencyData()
        {
            AssetInfo =
            {
                {guid1, new AssetLoadInfo(){referencedObjects = ContentBuildInterface.GetPlayerObjectIdentifiersInAsset(guid1, EditorUserBuildSettings.activeBuildTarget).ToList()}},
                {guid2, new AssetLoadInfo(){referencedObjects = ContentBuildInterface.GetPlayerObjectIdentifiersInAsset(guid2, EditorUserBuildSettings.activeBuildTarget).ToList()}},
                {guid3, new AssetLoadInfo(){referencedObjects = ContentBuildInterface.GetPlayerObjectIdentifiersInAsset(guid3, EditorUserBuildSettings.activeBuildTarget).ToList()}
}
            }
        };

        AddressableAssetsBuildContext context = new AddressableAssetsBuildContext()
        {
            Settings = m_Settings
        };

        AddHashToBundleNameTask addHashTask = new AddHashToBundleNameTask();
        var field = typeof(AddHashToBundleNameTask).GetField("m_DependencyData", BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(addHashTask, dependencyData);

        //Test
        RawHash hash1 = addHashTask.GetAssetsHash(list1, context);
        RawHash hash2 = addHashTask.GetAssetsHash(list2, context);

        //Assert
        Assert.AreEqual(hash1, hash2);

        //Cleanup
        m_Settings.RemoveGroup(group);
    }
}
