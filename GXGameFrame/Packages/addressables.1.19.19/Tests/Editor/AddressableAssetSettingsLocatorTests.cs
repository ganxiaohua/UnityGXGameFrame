#if UNITY_2020_1_OR_NEWER
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetSettingsLocatorTests
    {
        AddressableAssetSettings m_Settings;
        const string TempPath = "TempGen";
        string GetPath(string a) => $"Assets/{TempPath}/{a}";
        EditorBuildSettingsScene[] m_PreviousScenes;

        [SetUp]
        public void Setup()
        {
            if (AssetDatabase.IsValidFolder($"Assets/{TempPath}"))
            {
                AssetDatabase.DeleteAsset($"Assets/{TempPath}");
                AssetDatabase.Refresh();
            }
            AssetDatabase.CreateFolder("Assets", "TempGen");
            m_PreviousScenes = EditorBuildSettings.scenes;
            m_Settings = AddressableAssetSettings.Create(GetPath("Settings"), "AddressableAssetSettings.Tests", true, true);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void Teardown()
        {
            AssetDatabase.Refresh();
            // Many of the tests keep recreating assets in the same path, so we need to unload them completely so they don't get reused by the next test
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(m_Settings));
            Resources.UnloadAsset(m_Settings);
            if (AssetDatabase.IsValidFolder($"Assets/{TempPath}"))
                AssetDatabase.DeleteAsset($"Assets/{TempPath}");
            EditorBuildSettings.scenes = m_PreviousScenes;
            AssetDatabase.Refresh();
        }

        string CreateAsset(string assetName, string path)
        {
            AssetDatabase.CreateAsset(UnityEngine.AddressableAssets.Tests.TestObject.Create(assetName), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return AssetDatabase.AssetPathToGUID(path);
        }

        string CreateScene(string path)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
            return AssetDatabase.AssetPathToGUID(path);
        }

        string CreateFolder(string folderName, string[] assetNames, HashSet<object> guids = null)
        {
            var path = GetPath(folderName);
            Directory.CreateDirectory(path);
            foreach (var a in assetNames)
            {
                var guid = a.EndsWith(".unity") ? CreateScene(Path.Combine(path, a)) : CreateAsset(a, Path.Combine(path, a));
                guids?.Add(guid);
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            return AssetDatabase.AssetPathToGUID(path);
        }

        void AssertLocateResult<T>(IResourceLocator locator, object key, params string[] expectedInternalIds)
        {
            var type = typeof(T);
            Assert.IsTrue(locator.Locate(key, type, out var locations));
            Assert.IsNotNull(locations);
            if (type != typeof(object))
            {
                foreach (var l in locations)
                    Assert.IsTrue(type.IsAssignableFrom(l.ResourceType));
            }
            Assert.AreEqual(expectedInternalIds.Length, locations.Count);
            foreach (var e in expectedInternalIds)
                Assert.NotNull(locations.FirstOrDefault(s => s.InternalId == e), $"Locations do not contain entry with internal id of {e}");
        }

        [UnityTest]
        public IEnumerator CanLoadAssetAsync_InEditMode()
        {
            var entry = m_Settings.CreateOrMoveEntry(CreateAsset("x", GetPath("x.asset")), m_Settings.DefaultGroup).address = "x";
            Addressables.Instance.hasStartedInitialization = false;
            Addressables.Instance.InitializeAsync($"GUID:{AssetDatabase.AssetPathToGUID(m_Settings.AssetPath)}");
            var op = Addressables.LoadAssetAsync<UnityEngine.AddressableAssets.Tests.TestObject>("x");
            while (!op.IsDone)
                yield return null;
            Assert.IsNotNull(op.Result);
            op.Release();
            m_Settings.RemoveAssetEntry(entry);
        }

        [Test]
        public void CanLoadAssetSync_InEditMode()
        {
            var entry = m_Settings.CreateOrMoveEntry(CreateAsset("y", GetPath("y.asset")), m_Settings.DefaultGroup).address = "y";
            Addressables.Instance.hasStartedInitialization = false;
            Addressables.Instance.InitializeAsync($"GUID:{AssetDatabase.AssetPathToGUID(m_Settings.AssetPath)}");
            var op = Addressables.LoadAssetAsync<UnityEngine.AddressableAssets.Tests.TestObject>("y");
            op.WaitForCompletion();
            Assert.IsNotNull(op.Result);
            op.Release();
            m_Settings.RemoveAssetEntry(entry);
        }

        [Test]
        public void WhenLocatorWithSingleAsset_LocateWithAddress_ReturnsSingleLocation()
        {
            var path = GetPath("asset1.asset");
            m_Settings.CreateOrMoveEntry(CreateAsset("asset1", path), m_Settings.DefaultGroup).address = "address1";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "address1", path);
        }

        [Test]
        public void WhenLocatorWithSingleMonoscript_LocateReturnsSingleLocation()
        {
            string id = "mono";
            var path = GetPath("mono.asset");
            AssetDatabase.CreateAsset(UnityEngine.AddressableAssets.Tests.TestObjectWithSerializableField.Create(id), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            m_Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), m_Settings.DefaultGroup).address = id;
            var locator = new AddressableAssetSettingsLocator(m_Settings);

            Assert.IsTrue(locator.Locate(id, null, out var locs));
            Assert.IsNotNull(locs);
            Assert.AreEqual(1, locs.Count);
            Assert.AreEqual(path, locs[0].InternalId, $"Locations do not contain entry with internal id of {path}");
        }

        [Test]
        public void WhenLocatorWithSingleAsset_LocateWithSameAddressAsLabel_ReturnsSingleLocation()
        {
            var path = GetPath("asset1.asset");
            var e = m_Settings.CreateOrMoveEntry(CreateAsset("asset1", path), m_Settings.DefaultGroup);
            e.address = "address1";
            e.SetLabel("address1", true, true);
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "address1", path);
        }

        [Test]
        public void WhenLocatorWithSingleAsset_LocateWithInvalidKeyReturnsFalse()
        {
            m_Settings.CreateOrMoveEntry(CreateAsset("asset1", GetPath("asset1.asset")), m_Settings.DefaultGroup).address = "address1";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            Assert.IsFalse(locator.Locate("invalid", null, out var locs));
            Assert.IsNull(locs);
        }

        [Test]
        public void WhenLocatorWithSingleAsset_LocateWithGuidReturnsSingleLocation()
        {
            var guid = CreateAsset("asset1", GetPath("asset1.asset"));
            m_Settings.CreateOrMoveEntry(guid, m_Settings.DefaultGroup).address = "address1";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), guid, GetPath("asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithMultipeAssets_LocateWithAddressReturnsSingleLocation()
        {
            m_Settings.CreateOrMoveEntry(CreateAsset("asset1", GetPath("asset1.asset")), m_Settings.DefaultGroup).address = "address1";
            m_Settings.CreateOrMoveEntry(CreateAsset("asset2", GetPath("asset2.asset")), m_Settings.DefaultGroup).address = "address2";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "address1", GetPath("asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithMultipeAssets_LocateWithSharedAddressReturnsMultipleLocations()
        {
            m_Settings.CreateOrMoveEntry(CreateAsset("asset1", GetPath("asset1.asset")), m_Settings.DefaultGroup).address = "address1";
            m_Settings.CreateOrMoveEntry(CreateAsset("asset2", GetPath("asset2.asset")), m_Settings.DefaultGroup).address = "address1";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "address1", GetPath("asset1.asset"), GetPath("asset2.asset"));
        }

        [Test]
        public void WhenLocatorWithMultipeAssets_LocateWithSharedLabelReturnsMultipleLocations()
        {
            m_Settings.CreateOrMoveEntry(CreateAsset("asset1", GetPath("asset1.asset")), m_Settings.DefaultGroup).SetLabel("label", true, true);
            m_Settings.CreateOrMoveEntry(CreateAsset("asset2", GetPath("asset2.asset")), m_Settings.DefaultGroup).SetLabel("label", true, true);
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "label", GetPath("asset1.asset"), GetPath("asset2.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsThatMatchAssetsInFolderAndResources_LocateAllMatches()
        {
            CreateFolder("Resources/TestFolder", new string[] { "asset1.asset"});
            var folderGUID = CreateFolder("TestFolder", new string[] { "asset1.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TestFolder";
            var assetGUID = CreateAsset("asset1", GetPath("asset1.asset"));
            m_Settings.CreateOrMoveEntry(assetGUID, m_Settings.DefaultGroup).address = "TestFolder/asset1.asset";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TestFolder/asset1.asset",
                "TestFolder/asset1",
                GetPath("TestFolder/asset1.asset"),
                GetPath("asset1.asset")
            );
        }

        [Test]
        public void WhenLocatorWithAssetsInMarkedFolder_LocateWithAssetReferenceSucceeds()
        {
            CreateFolder("TestFolder1/TestFolder2", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID = AssetDatabase.AssetPathToGUID(GetPath("TestFolder1"));
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF1";
            var assetRef = m_Settings.CreateAssetReference(AssetDatabase.AssetPathToGUID(GetPath("TestFolder1/TestFolder2/asset1.asset")));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), assetRef.RuntimeKey, GetPath("TestFolder1/TestFolder2/asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInFolder_LocateWithAssetKeySucceeds()
        {
            var folderGUID = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/asset1.asset", GetPath("TestFolder/asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInMatchingFolders_LocateWithAssetKeySucceeds()
        {
            var folderGUID1 = CreateFolder("TestFolder1", new string[] { "asset1_1.asset", "asset2_1.asset", "asset3_1.asset" });
            var folderGUID2 = CreateFolder("TestFolder2", new string[] { "asset1_2.asset", "asset2_2.asset", "asset3_2.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID1, m_Settings.DefaultGroup).address = "TF";
            m_Settings.CreateOrMoveEntry(folderGUID2, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/asset1_1.asset", GetPath("TestFolder1/asset1_1.asset"));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/asset1_2.asset", GetPath("TestFolder2/asset1_2.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInMatchingFolderAndAssets_LocateWithAssetKeySucceeds()
        {
            var folderGUID1 = CreateFolder("TestFolder1", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID2 = CreateFolder("TestFolder2", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID1, m_Settings.DefaultGroup).address = "TF";
            m_Settings.CreateOrMoveEntry(folderGUID2, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/asset1.asset", GetPath("TestFolder1/asset1.asset"), GetPath("TestFolder2/asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetAndFolderNameMatch_LocateWithAssetKeySucceeds()
        {
            var folderGUID = CreateFolder("TestName", new string[] { "TestName.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/TestName.asset", GetPath("TestName/TestName.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetAndFolderAddrMatch_LocateWithAssetKeySucceeds()
        {
            var folderGUID = CreateFolder("TestName", new string[] { "TF.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/TF.asset", GetPath("TestName/TF.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInFolder_LocateWithFolderKeyFails()
        {
            var folderGUID = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            Assert.IsFalse(locator.Locate("TF", null, out var locations));
        }

        [Test]
        public void WhenLocatorWithAssetsInFolder_LocateWithFolderLabelSucceeds()
        {
            var folderGUID = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderEntry = m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup);
            folderEntry.address = "TF";
            folderEntry.SetLabel("FolderLabel", true, true, true);
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "FolderLabel", GetPath("TestFolder/asset1.asset"), GetPath("TestFolder/asset2.asset"), GetPath("TestFolder/asset3.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInFolderWithSimilarNames_LocateWithAssetKeySucceeds()
        {
            var folderGUID = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset.asset", "asset1_more.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID, m_Settings.DefaultGroup).address = "TF";
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(new AddressableAssetSettingsLocator(m_Settings), "TF/asset1.asset", GetPath("TestFolder/asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInNestedFolders_LocateWithAssetKeySucceeds()
        {
            var folderGUID1 = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID2 = CreateFolder("TestFolder/TestSubFolder1", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID3 = CreateFolder("TestFolder/TestSubFolder1/TestSubFolder2", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID1, m_Settings.DefaultGroup).address = "TF";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF/asset1.asset", GetPath("TestFolder/asset1.asset"));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF/TestSubFolder1/asset1.asset", GetPath("TestFolder/TestSubFolder1/asset1.asset"));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF/TestSubFolder1/TestSubFolder2/asset1.asset", GetPath("TestFolder/TestSubFolder1/TestSubFolder2/asset1.asset"));
        }

        [Test]
        public void WhenLocatorWithAssetsInNestedFoldersThatAreBothAddressable_LocateWithAssetKeySucceeds()
        {
            var folderGUID1 = CreateFolder("TestFolder", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID2 = CreateFolder("TestFolder/TestSubFolder1", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            var folderGUID3 = CreateFolder("TestFolder/TestSubFolder1/TestSubFolder2", new string[] { "asset1.asset", "asset2.asset", "asset3.asset" });
            m_Settings.CreateOrMoveEntry(folderGUID1, m_Settings.DefaultGroup).address = "TF";
            m_Settings.CreateOrMoveEntry(folderGUID2, m_Settings.DefaultGroup).address = "TF2";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF/asset1.asset", GetPath("TestFolder/asset1.asset"));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF2/asset1.asset", GetPath("TestFolder/TestSubFolder1/asset1.asset"));
            AssertLocateResult<UnityEngine.AddressableAssets.Tests.TestObject>(locator, "TF2/TestSubFolder2/asset1.asset", GetPath("TestFolder/TestSubFolder1/TestSubFolder2/asset1.asset"));
        }

        void CreateAndAddScenesToEditorBuildSettings(string scenePrefix, int count, HashSet<object> guids = null)
        {
            var sceneList = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < count; i++)
            {
                var path = GetPath($"{scenePrefix}{i}.unity");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, i == 0 ? NewSceneMode.Single : NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, path);
                var guid = AssetDatabase.AssetPathToGUID(path);
                guids?.Add(guid);
                sceneList.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = sceneList.ToArray();
        }

        [Test]
        public void WhenLocatorWithScenesInSceneList_LocateWithSceneIndexKeyReturnsLocationForScene()
        {
            CreateAndAddScenesToEditorBuildSettings("testScene", 3);
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            for (int i = 0; i < 3; i++)
                AssertLocateResult<SceneInstance>(locator, i, GetPath($"testScene{i}.unity"));
        }

        [Test]
        public void WhenLocatorWithScenesInSceneList_LocateWithSceneGUIDKeyReturnsLocationForScene()
        {
            CreateAndAddScenesToEditorBuildSettings("testScene", 3);
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                AssertLocateResult<SceneInstance>(locator, EditorBuildSettings.scenes[i].guid.ToString(), GetPath($"testScene{i}.unity"));
        }

        [Test]
        public void WhenLocatorWithScenesInSceneList_LocateWithSceneNameKeyReturnsLocationForScene()
        {
            CreateAndAddScenesToEditorBuildSettings("testScene", 3);
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                AssertLocateResult<SceneInstance>(locator, $"testScene{i}", GetPath($"testScene{i}.unity"));
        }

        [Test]
        public void LocateWithSceneInstanceType_DoesNotReturnNonSceneLocations()
        {
            CreateAndAddScenesToEditorBuildSettings("test", 1);
            var guid = CreateAsset("test0", GetPath("asset1.asset"));
            m_Settings.CreateOrMoveEntry(guid, m_Settings.DefaultGroup).address = "test0";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            Assert.IsTrue(locator.Locate("test0", typeof(SceneInstance), out var locations));
            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(typeof(SceneInstance), locations[0].ResourceType);
        }

        [Test]
        public void LocateWithSceneInstanceType_ReturnsLocationsWhenTypeNull()
        {
            CreateAndAddScenesToEditorBuildSettings("test", 1);
            var guid = CreateAsset("test0", GetPath("asset1.asset"));
            m_Settings.CreateOrMoveEntry(guid, m_Settings.DefaultGroup).address = "test0";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            Assert.IsTrue(locator.Locate("test0", null, out var locations));
            Assert.AreEqual(2, locations.Count);
            Assert.AreEqual(typeof(SceneInstance), locations[0].ResourceType);
        }

        [Test]
        public void LocateWithNonSceneInstanceType_DoesNotReturnSceneLocations()
        {
            CreateAndAddScenesToEditorBuildSettings("test", 1);
            var guid = CreateAsset("test0", GetPath("asset1.asset"));
            m_Settings.CreateOrMoveEntry(guid, m_Settings.DefaultGroup).address = "test0";
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            Assert.IsTrue(locator.Locate("test0", typeof(UnityEngine.AddressableAssets.Tests.TestObject), out var locations));
            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(typeof(UnityEngine.AddressableAssets.Tests.TestObject), locations[0].ResourceType);
        }

        [Test]
        public void WhenLocatorWithScenesInSceneList_LocateWithISceneInstanceType_ReturnsLocationForScene()
        {
            CreateAndAddScenesToEditorBuildSettings("testScene", 3);
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                AssertLocateResult<SceneInstance>(locator, $"testScene{i}", GetPath($"testScene{i}.unity"));
        }

        public void RunResourcesTestWithAsset(bool IncludeResourcesFolders)
        {
            int builtInResourceCount = ResourcesTestUtility.GetResourcesEntryCount(m_Settings, true);
            var path = GetPath("Resources/test.asset");
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            AssetDatabase.CreateAsset(UnityEngine.AddressableAssets.Tests.TestObject.Create("test"), path);
            AssetDatabase.SaveAssets();
            m_Settings.FindGroup(g => g.HasSchema<PlayerDataGroupSchema>()).GetSchema<PlayerDataGroupSchema>().IncludeResourcesFolders = IncludeResourcesFolders;
            var locator = new AddressableAssetSettingsLocator(m_Settings);
            var res = locator.Locate("test", null, out var locations);
            Assert.AreEqual(res, IncludeResourcesFolders);
            if (IncludeResourcesFolders)
                Assert.AreEqual(1 + builtInResourceCount, locations.Count + builtInResourceCount);
            else
                Assert.IsNull(locations);
            AssetDatabase.DeleteAsset(path);
            Directory.Delete(dir);
        }

        [Test]
        public void WhenAGroupHasIncludeResourcesFoldersEnabled_LocateFindsAssetInResourcesFolder()
        {
            RunResourcesTestWithAsset(true);
        }

        [Test]
        public void WhenAGroupHasIncludeResourcesFoldersDisabled_LocateFailesForAssetInResourcesFolder()
        {
            RunResourcesTestWithAsset(false);
        }

        static HashSet<object> ExpectedKeys = new HashSet<object>(new object[]
        {
            "TestScene0",   //scene name in build settings
            0,              //index if TestScene0
            "TestScene1",
            1,
            "TestScene2",
            2,
            //assets in a folder with an address
            "AssetAddress",
            "AssetLabel",
            "TF/asset2.asset",
            "TF/asset3.asset",
            //scenes in a folder
            "TF/TestSubFolder2/scene1.unity",
            "TF/TestSubFolder2/scene2.unity",
            "TF/TestSubFolder2/scene3.unity",
            //label applied to folder
            "FolderLabel1",
            //assets in subfolder without address
            "TestFolder/TestSubFolder1/asset1.asset",
            "TestFolder/TestSubFolder1/asset2.asset",
            "TestFolder/TestSubFolder1/asset3.asset",
            //assets in resource in subfolder with address
            "TestFolder/TestSubFolder1/Resources/asset1.asset",
            "TestFolder/TestSubFolder1/Resources/asset2.asset",
            "TestFolder/TestSubFolder1/Resources/asset3.asset",
            //resources keys
            "asset1",
            "asset2",
            "asset3",
        });

        void SetupLocatorAssets()
        {
            var folderGUID1 = CreateFolder("TestFolder",
                new string[] { "asset1.asset", "asset2.asset", "asset3.asset" }, ExpectedKeys);
            var folderGUID2 = CreateFolder("TestFolder/TestSubFolder1",
                new string[] { "asset1.asset", "asset2.asset", "asset3.asset" }, ExpectedKeys);
            var folderGUID3 = CreateFolder("TestFolder/TestSubFolder1/Resources",
                new string[] { "asset1.asset", "asset2.asset", "asset3.asset" }, ExpectedKeys);
            var folderGUID4 = CreateFolder("TestFolder/TestSubFolder2",
                new string[] { "scene1.unity", "scene2.unity", "scene3.unity" }, ExpectedKeys);
            CreateAndAddScenesToEditorBuildSettings("TestScene", 3, ExpectedKeys);
            var assetInFolder = m_Settings.CreateOrMoveEntry(
                AssetDatabase.AssetPathToGUID(GetPath("TestFolder/asset1.asset")), m_Settings.DefaultGroup);
            assetInFolder.address = "AssetAddress";
            assetInFolder.SetLabel("AssetLabel", true, true, true);
            var tf = m_Settings.CreateOrMoveEntry(folderGUID1, m_Settings.DefaultGroup);
            tf.address = "TF";
            tf.SetLabel("FolderLabel1", true, true, true);
            var tf2 = m_Settings.CreateOrMoveEntry(folderGUID2, m_Settings.DefaultGroup);
            tf2.address = "TestFolder/TestSubFolder1";
        }

        [UnityTest]
        public IEnumerator Locator_KeysProperty_Contains_Expected_Keys_ForAllBuildScripts()
        {
            using (new HideResourceFoldersScope())
            {
                SetupLocatorAssets();

                AddressablesDataBuilderInput input = new AddressablesDataBuilderInput(m_Settings);
                input.Logger = new BuildLog();

                var fastMode = ScriptableObject.CreateInstance<BuildScriptFastMode>();
                var virtualMode = ScriptableObject.CreateInstance<BuildScriptVirtualMode>();
                var packedMode = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
                var packedPlayMode = ScriptableObject.CreateInstance<BuildScriptPackedPlayMode>();

                AddressablesImpl fastModeImpl = new AddressablesImpl(new DefaultAllocationStrategy());
                fastModeImpl.AddResourceLocator(new AddressableAssetSettingsLocator(m_Settings));

                var fastModeSettingsPath = fastMode.BuildData<AddressableAssetBuildResult>(input).OutputPath;
                var virtualModeSettingsPath = virtualMode.BuildData<AddressableAssetBuildResult>(input).OutputPath;
                var packedModeSettingsPath = packedMode.BuildData<AddressableAssetBuildResult>(input).OutputPath;
                var packedPlayModeSettingsPath = packedPlayMode.BuildData<AddressableAssetBuildResult>(input).OutputPath;

                AddressablesImpl fmImpl = new AddressablesImpl(new DefaultAllocationStrategy());
                AddressablesImpl virtualImpl = new AddressablesImpl(new DefaultAllocationStrategy());
                AddressablesImpl packedImpl = new AddressablesImpl(new DefaultAllocationStrategy());
                AddressablesImpl packedPlayImpl = new AddressablesImpl(new DefaultAllocationStrategy());

                fmImpl.AddResourceLocator(new AddressableAssetSettingsLocator(m_Settings));
                virtualImpl.AddResourceLocator(new AddressableAssetSettingsLocator(m_Settings));
                packedImpl.AddResourceLocator(new AddressableAssetSettingsLocator(m_Settings));
                packedPlayImpl.AddResourceLocator(new AddressableAssetSettingsLocator(m_Settings));

                var fastModeHandle = fmImpl.ResourceManager.StartOperation(new FastModeInitializationOperation(fmImpl, m_Settings), default(AsyncOperationHandle));
                var virtualHandle = virtualImpl.InitializeAsync(virtualModeSettingsPath);
                var packedHandle = packedImpl.InitializeAsync(packedModeSettingsPath);
                var packedPlayHandle = packedPlayImpl.InitializeAsync(packedPlayModeSettingsPath);
                while (!fastModeHandle.IsDone && !virtualHandle.IsDone && !packedHandle.IsDone &&
                       !packedPlayHandle.IsDone)
                    yield return null;

                var fastModeKeys = fmImpl.ResourceLocators.First(l => l.GetType() == typeof(AddressableAssetSettingsLocator)).Keys;
                var virtualModeKeys = virtualImpl.ResourceLocators.First(l => l.GetType() == typeof(AddressableAssetSettingsLocator)).Keys;
                var packedModeKeys = packedImpl.ResourceLocators.First(l => l.GetType() == typeof(AddressableAssetSettingsLocator)).Keys;
                var packedPlayKeys = packedPlayImpl.ResourceLocators.First(l => l.GetType() == typeof(AddressableAssetSettingsLocator)).Keys;


                var builtInPackageResourcesEntries = ResourcesTestUtility.GetResourcesEntries(m_Settings, true);
                foreach (var key in builtInPackageResourcesEntries)
                {
                    ExpectedKeys.Add(key.guid);
                    ExpectedKeys.Add(key.address);
                }

                //Get our baseline
                Assert.AreEqual(ExpectedKeys.Count, fastModeKeys.Count());
                foreach (var key in fastModeKeys)
                    Assert.IsTrue(ExpectedKeys.Contains(key));

                //Transitive property to check other build scripts
                CollectionAssert.AreEqual(fastModeKeys, virtualModeKeys);
                CollectionAssert.AreEqual(fastModeKeys, packedPlayKeys);
                CollectionAssert.AreEqual(fastModeKeys, packedModeKeys);
            }
        }

        [Test]
        public void Locator_KeysProperty_Contains_Expected_Keys()
        {
            using (new HideResourceFoldersScope())
            {
                SetupLocatorAssets();
                var locator = new AddressableAssetSettingsLocator(m_Settings);
                if (ExpectedKeys.Count != locator.Keys.Count())
                {
                    Debug.Log("GENERATED");
                    int i = 0;
                    foreach (var k in locator.Keys)
                    {
                        Debug.Log($"[{i}] {k}");
                        i++;
                    }

                    Debug.Log("EXPECTED");
                    i = 0;
                    foreach (var k in ExpectedKeys)
                    {
                        Debug.Log($"[{i}] {k}");
                        i++;
                    }
                }

                var builtInPackageResourcesEntries = ResourcesTestUtility.GetResourcesEntries(m_Settings, true);
                foreach (var key in builtInPackageResourcesEntries)
                {
                    ExpectedKeys.Add(key.guid);
                    ExpectedKeys.Add(key.address);
                }

                Assert.AreEqual(ExpectedKeys.Count, locator.Keys.Count());
                int index = 0;
                foreach (var k in locator.Keys)
                {
                    Assert.IsTrue(ExpectedKeys.Contains(k), $"Cannot find key {k}, index={index} in expected keys");
                    index++;
                }
            }
        }
    }
}

#endif
