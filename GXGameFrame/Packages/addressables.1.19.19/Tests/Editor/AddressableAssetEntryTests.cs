using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.U2D;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetEntryTests : AddressableAssetTestBase
    {
        string m_guid;
        AddressableAssetGroup m_testGroup;
        protected override void OnInit()
        {
            var path = GetAssetPath("subObjectTest.asset");
            AssetDatabase.CreateAsset(UnityEngine.AddressableAssets.Tests.TestObject.Create("test"), path);

            AssetDatabase.AddObjectToAsset(UnityEngine.AddressableAssets.Tests.TestObject2.Create("test2"), path);
            AssetDatabase.AddObjectToAsset(UnityEngine.AddressableAssets.Tests.TestObject2.Create("test3"), path);
            AssetDatabase.AddObjectToAsset(UnityEngine.AddressableAssets.Tests.TestObject2.Create("test4"), path);
            AssetDatabase.AddObjectToAsset(UnityEngine.AddressableAssets.Tests.TestObject2.Create("test5"), path);
            AssetDatabase.SaveAssets();

            m_guid = AssetDatabase.AssetPathToGUID(path);
            Settings.CreateOrMoveEntry(m_guid, Settings.DefaultGroup);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            m_testGroup = Settings.CreateGroup("testGroup", false, false, false, null, typeof(BundledAssetGroupSchema));
        }

        protected override void OnCleanup()
        {
            Settings.RemoveGroup(m_testGroup);
        }

        [Test]
        public void CreateKeyList_Returns_ExpectedKeys()
        {
            var e = Settings.DefaultGroup.GetAssetEntry(m_guid);
            e.SetAddress("address");
            e.SetLabel("label", true, true, true);
            CollectionAssert.AreEqual(new string[] { "address", m_guid, "label" }, e.CreateKeyList(true, true, true));
            CollectionAssert.AreEqual(new string[] { m_guid, "label" }, e.CreateKeyList(false, true, true));
            CollectionAssert.AreEqual(new string[] { "address", "label" }, e.CreateKeyList(true, false, true));
            CollectionAssert.AreEqual(new string[] { "address", "label" }, e.CreateKeyList(true, false, true));
            CollectionAssert.AreEqual(new string[] {"label" }, e.CreateKeyList(false, false, true));
            CollectionAssert.AreEqual(new string[] { "address" }, e.CreateKeyList(true, false, false));
            CollectionAssert.AreEqual(new string[] { m_guid }, e.CreateKeyList(false, true, false));
        }

        [Test]
        public void EditorInitialization_RemovesAssetEntriesThatDoesNotExistOnDisk()
        {
            //setup
            var group = Settings.DefaultGroup;
            AddressableAssetEntry assetEntry = new AddressableAssetEntry("fakeguid", "fakeaddress", group, false);
            assetEntry.m_cachedAssetPath = "Assets/NotARealAssetPath/test.prefab";
            group.AddAssetEntry(assetEntry, false);

            //test
            AddressableEditorInitialization.PurgeInvalidAssetEntries(Settings);

            //Assert
            Assert.IsFalse(group.entries.Contains(assetEntry));
        }

        [Test]
        public void EditorInitialization_DoesNotDeleteFoldersThatAreStillOnDisk()
        {
            //Setup
            string folderPath = "Assets/Temp/FakeAddressablesFolder/";
            Directory.CreateDirectory(folderPath);
            AssetDatabase.ImportAsset(folderPath);
            AddressableAssetEntry entry = new AddressableAssetEntry(AssetDatabase.AssetPathToGUID(folderPath),
                folderPath, m_testGroup, false);
            m_testGroup.AddAssetEntry(entry);
            entry.m_cachedAssetPath = folderPath;

            Assert.IsTrue(m_testGroup.entries.Contains(entry), "Folder entry is no longer in Addressable group before purge.");

            //Test
            AddressableEditorInitialization.PurgeInvalidAssetEntries(Settings);

            //Assert
            Assert.IsTrue(m_testGroup.entries.Contains(entry), "Folder entry is no longer in Addressable group after purge.");

            //Cleanup
            m_testGroup.RemoveAssetEntry(entry);
            AssetDatabase.DeleteAsset(folderPath);
        }

        [Test]
        public void EditorInitialization_DoesDeleteFoldersThatAreNotOnDisk()
        {
            //Setup
            string folderPath = "Assets/Temp/FakeAddressablesFolder/";
            AddressableAssetEntry entry = new AddressableAssetEntry(AssetDatabase.AssetPathToGUID(folderPath),
                folderPath, m_testGroup, false);
            entry.m_cachedAssetPath = folderPath;
            m_testGroup.AddAssetEntry(entry);

            //Test
            AddressableEditorInitialization.PurgeInvalidAssetEntries(Settings);

            //Assert
            Assert.IsFalse(m_testGroup.entries.Contains(entry), "Invalid asset entry folder is still in Asset Group after purge.");
        }

        [Test]
        public void GetAssetLoadPath_Returns_ExpectedPath()
        {
            var schema = Settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>();
            var e = Settings.DefaultGroup.GetAssetEntry(m_guid);
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.FullPath;
            Assert.AreEqual(e.AssetPath, e.GetAssetLoadPath(true, null));
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Filename;
            Assert.AreEqual(Path.GetFileName(e.AssetPath), e.GetAssetLoadPath(true, null));
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
            Assert.AreEqual(m_guid, e.GetAssetLoadPath(true, null));
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Dynamic;
            Assert.AreEqual(m_guid, e.GetAssetLoadPath(true, null));
            Assert.AreEqual(m_guid.Substring(0, 1), e.GetAssetLoadPath(true, new HashSet<string>()));
            var hs = new HashSet<string>();
            hs.Add(m_guid.Substring(0, 1));
            Assert.AreEqual(m_guid.Substring(0, 2), e.GetAssetLoadPath(true, hs));
        }

        [Test]
        public void CreateCatalogEntries_WhenObjectHasMultipleSubObjectWithSameType_OnlyOneSubEntryIsCreated()
        {
            var e = Settings.DefaultGroup.GetAssetEntry(m_guid);
            var entries = new List<ContentCatalogDataEntry>();
            var providerTypes = new HashSet<Type>();
            e.CreateCatalogEntries(entries, false, "doesntMatter", null, null, providerTypes);
            Assert.AreEqual(2, entries.Count);
        }

        [Test]
        public void DefaultTypeAssetEntry_ResetsCachedTypeData()
        {
            //Setup
            var path = GetAssetPath("entry.prefab");
            PrefabUtility.SaveAsPrefabAsset(new GameObject(), path);
            string guid = AssetDatabase.AssetPathToGUID(path);

            AddressableAssetEntry entry = new AddressableAssetEntry(guid, "testaddress", Settings.DefaultGroup, false);
            entry.m_cachedMainAssetType = typeof(DefaultAsset);

            //Test
            entry.CreateCatalogEntries(new List<ContentCatalogDataEntry>(), false, "fakeProvider", new List<object>(), null, new Dictionary<GUID, AssetLoadInfo>()
            {
                { new GUID(guid), new AssetLoadInfo() {includedObjects = new List<ObjectIdentifier>()} }
            },
                new HashSet<Type>(), true, false, false, new HashSet<string>());

            //Assert
            Assert.AreEqual(typeof(GameObject), entry.m_cachedMainAssetType);

            //Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void CreateCatalogEntries_OverridesMainTypeIfWrong()
        {
            var e = Settings.DefaultGroup.GetAssetEntry(m_guid);
            var entries = new List<ContentCatalogDataEntry>();
            var providerTypes = new HashSet<Type>();
            var savedType = e.m_cachedMainAssetType;
            e.m_cachedMainAssetType = typeof(Texture2D);//something arbitrarily wrong.
            e.CreateCatalogEntries(entries, false, "doesntMatter", null, null, providerTypes);
            e.m_cachedMainAssetType = savedType;
            Assert.AreEqual(2, entries.Count);
            bool foundOnlyTestObjects = true;
            foreach (var entry in entries)
            {
                if (entry.ResourceType != typeof(UnityEngine.AddressableAssets.Tests.TestObject) &&
                    (entry.ResourceType != typeof(UnityEngine.AddressableAssets.Tests.TestObject2)))
                {
                    foundOnlyTestObjects = false;
                }
            }
            Assert.IsTrue(foundOnlyTestObjects);
        }

        [Test]
        public void CreateCatalogEntries_EditorTypesShouldBeStripped()
        {
            var e = Settings.DefaultGroup.GetAssetEntry(m_guid);
            var entries = new List<ContentCatalogDataEntry>();
            var providerTypes = new HashSet<Type>();
            var savedType = e.m_cachedMainAssetType;
            e.m_cachedMainAssetType = typeof(UnityEditor.AssetImporter);
            e.CreateCatalogEntries(entries, false, "doesntMatter", null, null, providerTypes);
            e.m_cachedMainAssetType = savedType;
            Assert.AreEqual(0, entries.Count);
        }

        [Test]
        public void MainAsset_WhenEntryIsForSubAsset_ShouldReturnMainAsset()
        {
            var mainAssetEntry = Settings.DefaultGroup.GetAssetEntry(m_guid);
            var entries = new List<AddressableAssetEntry>();
            Settings.DefaultGroup.GatherAllAssets(entries, true, true, true);

            var subAssetEntry = entries.FirstOrDefault(e => e.IsSubAsset);
            Assert.NotNull(subAssetEntry);
            Assert.AreEqual(mainAssetEntry.MainAsset, subAssetEntry.MainAsset);
        }

        [Test]
        public void TargetAsset_WhenEntryIsForMainAsset_ShouldReturnMainAsset()
        {
            var entry = Settings.DefaultGroup.GetAssetEntry(m_guid);
            Assert.AreEqual(entry.MainAsset, entry.TargetAsset);
        }

        [Test]
        public void TargetAsset_WhenMainAssetIsSpriteAtlas_ShouldReturnSprite()
        {
            var atlasPath = CreateSpriteAtlasWithSprite();
            var guid = AssetDatabase.AssetPathToGUID(atlasPath);
            Settings.CreateOrMoveEntry(guid, Settings.DefaultGroup);
            AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            var sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);

            var mainAssetEntry = Settings.DefaultGroup.GetAssetEntry(guid);
            var entries = new List<AddressableAssetEntry>();
            mainAssetEntry.GatherAllAssets(entries, false, true, true);

            // Assert
            Assert.AreEqual(sprites.Length, entries.Count);
            foreach (var entry in entries)
            {
                Assert.IsTrue(sprites.Any(s => s.name == entry.TargetAsset.name));
                Assert.IsTrue(entry.TargetAsset is Sprite);
            }

            // Cleanup
            Settings.RemoveAssetEntry(guid);
            File.Delete(atlasPath);
        }

        [Test]
        public void TargetAsset_WhenEntryIsForSubAsset_ShouldReturnSubObject()
        {
            var mainAssetEntry = Settings.DefaultGroup.GetAssetEntry(m_guid);
            var entries = new List<AddressableAssetEntry>();
            Settings.DefaultGroup.GatherAllAssets(entries, false, true, true);
            var subObjects = AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetEntry.AssetPath);
            Assert.AreEqual(subObjects.Length, entries.Count);
            foreach (var entry in entries)
            {
                Assert.IsTrue(subObjects.Contains(entry.TargetAsset));
            }
        }
        
        [Test]
        public void WhenGettingFolderSubEntry_OnlyReturnsIfValidSubEntry()
        {
            AddressableAssetEntry mainFolderEntry = null;
            AddressableAssetEntry subFolderEntry = null;
            string testAssetFolder = GetAssetPath("TestFolder");
            
            try
            {
                //Setup
                string testAssetSubFolder = Path.Combine(testAssetFolder, "SubFolder");

                string mainPrefabPath = Path.Combine(testAssetFolder, "mainFolder.prefab").Replace('\\', '/');
                string subPrefabPath = Path.Combine(testAssetSubFolder, "subFolder.prefab").Replace('\\', '/');

                Directory.CreateDirectory(testAssetFolder);
                PrefabUtility.SaveAsPrefabAsset(new GameObject("mainFolderAsset"), mainPrefabPath);

                Directory.CreateDirectory(testAssetSubFolder);
                PrefabUtility.SaveAsPrefabAsset(new GameObject("subFolderAsset"), subPrefabPath);

                string mainFolderGuid = AssetDatabase.AssetPathToGUID(testAssetFolder);
                string subFolderGuid = AssetDatabase.AssetPathToGUID(testAssetSubFolder);
                mainFolderEntry = Settings.CreateOrMoveEntry(mainFolderGuid, m_testGroup, false);
                subFolderEntry = Settings.CreateOrMoveEntry(subFolderGuid, m_testGroup, false);

                //Test
                var entry = mainFolderEntry.GetFolderSubEntry(AssetDatabase.AssetPathToGUID(mainPrefabPath), mainPrefabPath);
                Assert.IsNotNull(entry, "Prefab in main folder is expected to be valid subAsset of main folder.");
                entry = mainFolderEntry.GetFolderSubEntry(AssetDatabase.AssetPathToGUID(subPrefabPath), subPrefabPath);
                Assert.IsNull(entry, "Prefab in addressable sub folder is not expected to be valid subAsset of main folder.");

                entry = subFolderEntry.GetFolderSubEntry(AssetDatabase.AssetPathToGUID(mainPrefabPath), mainPrefabPath);
                Assert.IsNull(entry, "Prefab in main folder is not expected to be valid subAsset of sub folder.");
                entry = subFolderEntry.GetFolderSubEntry(AssetDatabase.AssetPathToGUID(subPrefabPath), subPrefabPath);
                Assert.IsNotNull(entry, "Prefab in addressable sub folder is expected to be valid subAsset of sub folder.");
            }
            finally
            {
                //Cleanup
                Settings.RemoveAssetEntry(mainFolderEntry, false);
                Settings.RemoveAssetEntry(subFolderEntry, false);
                Directory.Delete(testAssetFolder, true);
            }
        }
        
        [TestCase(true)]
        [TestCase(false)]
        public void WhenGatherFolderEntries_ReturnsCorrectAssetObjects(bool includeSubObjects)
        {
            AddressableAssetEntry addrFolderEntry = null;
            string addressableFolderPath = GetAssetPath("TestFolder");
            
            try
            {
                //Setup
                string folderGuid = CreateFolderDeep(addressableFolderPath);
                addrFolderEntry = Settings.CreateOrMoveEntry(folderGuid, m_testGroup, false);
                
                string testAssetPath = Path.Combine(addressableFolderPath, "testObject.asset").Replace('\\', '/');
                var testAsset = TestObject.Create("testObject", testAssetPath);
                testAsset.AddTestSubObject();

                //Test
                List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
                addrFolderEntry.GatherFolderEntries(entries, true, includeSubObjects, null);
                if (includeSubObjects)
                    Assert.AreEqual(entries.Count, 2, "GatherFolder entries was expected to return the Asset added and its subObject");
                else
                    Assert.AreEqual(entries.Count, 1, "GatherFolder entries was expected to only return the Asset added and not its subObject");
            }
            finally
            {
                //Cleanup
                Settings.RemoveAssetEntry(addrFolderEntry, false);
                AssetDatabase.DeleteAsset(addressableFolderPath);
            }
        }

        [Test]
        public void WhenClassReferencedByAddressableAssetEntryIsReloaded_CachedMainAssetTypeIsReset()
        {
            // Setup
            var path = GetAssetPath("resetCachedMainAssetTypeTestGroup.asset");
            AddressableAssetGroup group = ScriptableObject.CreateInstance<AddressableAssetGroup>();
            AddressableAssetEntry entry = new AddressableAssetEntry(m_guid, "address", null, false);
            group.AddAssetEntry(entry);

            Assert.IsNull(entry.m_cachedMainAssetType);
            Assert.AreEqual(typeof(UnityEngine.AddressableAssets.Tests.TestObject), entry.MainAssetType);

            // Test
            AssetDatabase.CreateAsset(group, path);
            Resources.UnloadAsset(group);

            var reloadedGroup = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(path);
            var reloadedEntry = reloadedGroup.GetAssetEntry(m_guid);
            Assert.IsNull(reloadedEntry.m_cachedMainAssetType);

            // Cleanup
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void GatherAllAssets_WhenResourcesExist_RecurseAllIsFalse_ReturnsEntriesForValidFilesAndTopFoldersOnly()
        {
            using (new HideResourceFoldersScope())
            {
                var resourcePath = GetAssetPath("Resources");
                var subFolderPath = resourcePath + "/Subfolder";
                Directory.CreateDirectory(subFolderPath);

                var group = Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
                var resourceEntry = Settings.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, group, false);
                int builtInResourcesCount = ResourcesTestUtility.GetResourcesEntryCount(Settings, false);

                var r1GUID = CreateAsset(resourcePath + "/testResource1.prefab", "testResource1");
                var r2GUID = CreateAsset(subFolderPath + "/testResource2.prefab", "testResource2");

                var entries = new List<AddressableAssetEntry>();
                resourceEntry.GatherAllAssets(entries, false, false, true);

                // Assert
                var subFolderGUID = AssetDatabase.AssetPathToGUID(subFolderPath);
                Assert.AreEqual(2 + builtInResourcesCount, entries.Count);
                Assert.IsTrue(entries.Any(e => e.guid == r1GUID));
                Assert.IsFalse(entries.Any(e => e.guid == r2GUID));
                Assert.IsTrue(entries.Any(e => e.guid == subFolderGUID));

                // Cleanup
                Directory.Delete(resourcePath, true);
            }
        }

        [Test]
        public void GatherAllAssets_WhenResourcesExist_RecurseAllIsTrue_ReturnsEntriesRecursivelyForValidFilesOnly()
        {
            using (new HideResourceFoldersScope())
            {
                var resourcePath = GetAssetPath("Resources");
                var subFolderPath = resourcePath + "/Subfolder";
                Directory.CreateDirectory(subFolderPath);

                var group = Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
                var resourceEntry = Settings.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, group, false);
                int builtInResourcesCount = ResourcesTestUtility.GetResourcesEntryCount(Settings, true);

                var r1GUID = CreateAsset(resourcePath + "/testResource1.prefab", "testResource1");
                var r2GUID = CreateAsset(subFolderPath + "/testResource2.prefab", "testResource2");

                var entries = new List<AddressableAssetEntry>();
                resourceEntry.GatherAllAssets(entries, false, true, true);

                // Assert
                var subFolderGUID = AssetDatabase.AssetPathToGUID(subFolderPath);
                Assert.AreEqual(2 + builtInResourcesCount, entries.Count);
                Assert.IsTrue(entries.Any(e => e.guid == r1GUID));
                Assert.IsTrue(entries.Any(e => e.guid == r2GUID));
                Assert.IsFalse(entries.Any(e => e.guid == subFolderGUID));

                // Cleanup
                Directory.Delete(resourcePath, true);
            }
        }

        [Test]
        public void GatherAllAssets_WhenNonEmptyAddressableFolderExist_RecurseAllIsFalse_ReturnsEntriesForValidFilesAndTopFoldersOnly()
        {
            var folderAssetPath = GetAssetPath("folderAsset");
            var prefabPath1 = folderAssetPath + "/testAsset1_gatherAllAssets.prefab";
            var subFolderPath = folderAssetPath + "/Subfolder";
            var prefabPath2 = subFolderPath + "/testAsset2_gatherAllAssets.prefab";
            Directory.CreateDirectory(subFolderPath);

            var a1GUID = CreateAsset(prefabPath1, Path.GetFileNameWithoutExtension(prefabPath1));
            var a2GUID = CreateAsset(prefabPath2, Path.GetFileNameWithoutExtension(prefabPath2));

            AssetDatabase.ImportAsset(folderAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var folderAssetGUID = AssetDatabase.AssetPathToGUID(folderAssetPath);
            var folderAssetEntry = Settings.CreateOrMoveEntry(folderAssetGUID, m_testGroup, false);

            var entries = new List<AddressableAssetEntry>();
            folderAssetEntry.GatherAllAssets(entries, false, false, true);

            // Assert
            var subFolderGUID = AssetDatabase.AssetPathToGUID(subFolderPath);
            string csvEntries = "";
            if (entries.Count != 2)
            {
                foreach (AddressableAssetEntry assetEntry in entries)
                    csvEntries += assetEntry.AssetPath + ", ";
            }
            Assert.AreEqual(2, entries.Count, $"Expected 2 (testAsset1_gatherAllAssets and testAsset2_gatherAllAssets), but was {entries.Count}: ({csvEntries})");
            Assert.IsTrue(entries.Any(e => e.guid == a1GUID));
            Assert.IsFalse(entries.Any(e => e.guid == a2GUID));
            Assert.IsTrue(entries.Any(e => e.guid == subFolderGUID));
            Assert.IsFalse(entries.Any(e => e.guid == folderAssetGUID));

            // Cleanup
            Settings.RemoveAssetEntry(folderAssetPath);
            Directory.Delete(folderAssetPath, true);
        }

        [Test]
        public void GatherAllAssets_WhenNonEmptyAddressableFolderExist_RecurseAllIsTrue_ReturnsEntriesRecursivelyForValidFilesOnly()
        {
            var folderAssetPath = GetAssetPath("folderAsset");
            var prefabPath1 = folderAssetPath + "/testAsset1_gatherAllAssets.prefab";
            var subFolderPath = folderAssetPath + "/Subfolder";
            var prefabPath2 = subFolderPath + "/testAsset2_gatherAllAssets.prefab";
            Directory.CreateDirectory(subFolderPath);

            var a1GUID = CreateAsset(prefabPath1, Path.GetFileNameWithoutExtension(prefabPath1));
            var a2GUID = CreateAsset(prefabPath2, Path.GetFileNameWithoutExtension(prefabPath2));

            AssetDatabase.ImportAsset(folderAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var folderAssetGUID = AssetDatabase.AssetPathToGUID(folderAssetPath);
            var folderAssetEntry = Settings.CreateOrMoveEntry(folderAssetGUID, m_testGroup, false);

            var entries = new List<AddressableAssetEntry>();
            folderAssetEntry.GatherAllAssets(entries, false, true, true);

            // Assert
            var subFolderGUID = AssetDatabase.AssetPathToGUID(subFolderPath);
            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue(entries.Any(e => e.guid == a1GUID));
            Assert.IsTrue(entries.Any(e => e.guid == a2GUID));
            Assert.IsFalse(entries.Any(e => e.guid == folderAssetGUID));
            Assert.IsFalse(entries.Any(e => e.guid == subFolderGUID));

            // Cleanup
            Settings.RemoveAssetEntry(folderAssetPath);
            Directory.Delete(folderAssetPath, true);
        }

        [Test]
        public void GatherAllAssetReferenceDrawableEntries_ReturnsBuiltInScenes()
        {
            //Setup
            string scenePath = "TestScenePath";
            var savedCache = BuiltinSceneCache.scenes;
            BuiltinSceneCache.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(AddressableAssetEntry.EditorSceneListName, m_testGroup, false);

            //Test
            List<IReferenceEntryData> results = new List<IReferenceEntryData>();
            entry.GatherAllAssetReferenceDrawableEntries(results, Settings);

            //Assert
            Assert.AreEqual(0, results.Count);

            //Cleanup
            BuiltinSceneCache.scenes = savedCache;
            Settings.RemoveAssetEntry(AddressableAssetEntry.EditorSceneListName, false);
        }

        [Test]
        public void GatherAllAssetReferenceDrawableEntries_DoesNotReturnResources()
        {
            //Setup
            string scenePath = "TestScenePath";
            var savedCache = BuiltinSceneCache.scenes;
            BuiltinSceneCache.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, m_testGroup, false);

            //Test
            List<IReferenceEntryData> results = new List<IReferenceEntryData>();
            entry.GatherAllAssetReferenceDrawableEntries(results, Settings);

            //Assert
            Assert.AreEqual(0, results.Count);

            //Cleanup
            BuiltinSceneCache.scenes = savedCache;
            Settings.RemoveAssetEntry(AddressableAssetEntry.ResourcesName, false);
        }

        [Test]
        public void GatherAllAssetReferenceDrawableEntries_ReturnsFolderSubAssets()
        {
            //Setup
            string testAssetFolder = GetAssetPath("TestFolder");
            string testAssetSubFolder = Path.Combine(testAssetFolder, "SubFolder");

            string mainPrefabPath = Path.Combine(testAssetFolder, "mainFolder.prefab").Replace('\\', '/');
            string subPrefabPath = Path.Combine(testAssetSubFolder, "subFolder.prefab").Replace('\\', '/');

            Directory.CreateDirectory(testAssetFolder);
            PrefabUtility.SaveAsPrefabAsset(new GameObject("mainFolderAsset"), mainPrefabPath);

            Directory.CreateDirectory(testAssetSubFolder);
            PrefabUtility.SaveAsPrefabAsset(new GameObject("subFolderAsset"), subPrefabPath);

            string guid = AssetDatabase.AssetPathToGUID(testAssetFolder);
            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, m_testGroup, false);
            List<IReferenceEntryData> results = new List<IReferenceEntryData>();

            //Test
            entry.GatherAllAssetReferenceDrawableEntries(results, Settings);

            //Assert
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(mainPrefabPath, results[0].AssetPath);
            Assert.AreEqual(subPrefabPath, results[1].AssetPath);

            //Cleanup
            Directory.Delete(testAssetFolder, true);
            Settings.RemoveAssetEntry(guid, false);
        }

        [Test]
        public void GatherAllAssetReferenceDrawableEntries_DoesNotReturnScenesInFolder_IfSceneIsInBuiltInScenes()
        {
            //Setup
            string testAssetFolder = GetAssetPath("TestFolder");
            string testAssetSubFolder = Path.Combine(testAssetFolder, "SubFolder");
            Directory.CreateDirectory(testAssetFolder);
            Directory.CreateDirectory(testAssetSubFolder);

            AssetDatabase.ImportAsset(testAssetFolder);
            AssetDatabase.ImportAsset(testAssetSubFolder);

            string mainPrefabPath = Path.Combine(testAssetFolder, "mainFolder.prefab").Replace('\\', '/');
            string subPrefabPath = Path.Combine(testAssetSubFolder, "subFolder.prefab").Replace('\\', '/');
            string scenePath = Path.Combine(testAssetFolder, "TestScenePath.unity").Replace('\\', '/');

            var savedCache = BuiltinSceneCache.scenes;
            BuiltinSceneCache.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };

            PrefabUtility.SaveAsPrefabAsset(new GameObject("mainFolderAsset"), mainPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(new GameObject("subFolderAsset"), subPrefabPath);
            EditorSceneManager.SaveScene(EditorSceneManager.NewScene(NewSceneSetup.EmptyScene), scenePath);

            string guid = AssetDatabase.AssetPathToGUID(testAssetFolder);
            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, m_testGroup, false);
            List<IReferenceEntryData> results = new List<IReferenceEntryData>();

            //Test
            entry.GatherAllAssetReferenceDrawableEntries(results, Settings);

            //Assert
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(mainPrefabPath, results[0].AssetPath);
            Assert.AreEqual(subPrefabPath, results[1].AssetPath);

            //Cleanup
            Directory.Delete(testAssetFolder, true);
            BuiltinSceneCache.scenes = savedCache;
            Settings.RemoveAssetEntry(guid, false);
        }

        [Test]
        public void GatherAllAssetReferenceDrawableEntries_AddsSimpleAssetEntries()
        {
            //Setup
            string guid = "12345698655";
            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, m_testGroup, false);
            entry.m_cachedAssetPath = "TestPath";
            List<IReferenceEntryData> results = new List<IReferenceEntryData>();

            //Test
            entry.GatherAllAssetReferenceDrawableEntries(results, Settings);

            //Assert
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(entry.AssetPath, results[0].AssetPath);

            //Cleanup
            Settings.RemoveAssetEntry(guid, false);
        }

        [Test]
        public void GatherResourcesEntries_GathersAllResourceEntries_IncludingLowercase()
        {
            var resourcePath = GetAssetPath("Resources");
            string testAssetFolder = GetAssetPath("TestFolder");
            var subFolderPath = testAssetFolder + "/resources";
            
            string r1GUID = null;
            string r2GUID = null;
            
            try
            {
                r1GUID = CreateAsset(resourcePath + "/testResourceupper.prefab", "testResourceupper");
                r2GUID = CreateAsset(subFolderPath + "/testResourcelower.prefab", "testResourcelower");

                var group = Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
                var resourceEntry = Settings.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, group, false);

                var entries = new List<AddressableAssetEntry>();
                resourceEntry.GatherResourcesEntries(entries, true, null);

                // Assert
                Assert.IsTrue(entries.Any(e => e.guid == r1GUID));
                Assert.IsTrue(entries.Any(e => e.guid == r2GUID));
            }
            finally
            {
                // Cleanup
                if (!string.IsNullOrEmpty(r1GUID))
                    Settings.RemoveAssetEntry(r1GUID);
                if (!string.IsNullOrEmpty(r2GUID))
                    Settings.RemoveAssetEntry(r2GUID);
                if (!string.IsNullOrEmpty(AddressableAssetEntry.ResourcesName))
                    Settings.RemoveAssetEntry(AddressableAssetEntry.ResourcesName);
                AssetDatabase.DeleteAsset(resourcePath);
                AssetDatabase.DeleteAsset(testAssetFolder);
            }
        }

        [Test]
        public void GetRuntimeProviderType_HandlesEmptyProviderString()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            Type providerType = entry.GetRuntimeProviderType(null, typeof(GameObject));
            Assert.IsNull(providerType);
        }

        [Test]
        public void GetRuntimeProviderType_ReturnsAtlasProviderForSpriteAtlas()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            Type providerType = entry.GetRuntimeProviderType(typeof(AssetDatabaseProvider).FullName, typeof(SpriteAtlas));
            Assert.AreEqual(typeof(AtlasSpriteProvider), providerType);
        }

        [TestCase(typeof(AssetDatabaseProvider))]
        [TestCase(typeof(JsonAssetProvider))]
        [TestCase(typeof(BundledAssetProvider))]
        [TestCase(typeof(AssetBundleProvider))]
        [TestCase(typeof(TextDataProvider))]
        public void GetRuntimeProviderType_ReturnsProviderTypeForNonAtlas(Type testProviderType)
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            Type providerType = entry.GetRuntimeProviderType(testProviderType.FullName, typeof(GameObject));
            Assert.AreEqual(testProviderType, providerType);
        }

        [Test]
        public void GetRuntimeProviderType_HandlesInvalidProviderString()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            Type providerType = entry.GetRuntimeProviderType("NotARealProvider", typeof(GameObject));
            Assert.IsNull(providerType);
        }

        [TestCase(typeof(AssetDatabaseProvider))]
        [TestCase(typeof(JsonAssetProvider))]
        [TestCase(typeof(BundledAssetProvider))]
        [TestCase(typeof(AssetBundleProvider))]
        [TestCase(typeof(TextDataProvider))]
        public void GetRuntimeProviderType_HandlesNullAssetType(Type testProviderType)
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            Type providerType = entry.GetRuntimeProviderType(testProviderType.FullName, null);
            Assert.AreEqual(testProviderType, providerType);
        }

        [Test]
        public void WhenAddressHasSquareBracketsAndGuidIsNotEmptyString_CreatingNewEntry_ThrowsError()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "[Entry]", null, false);
            LogAssert.Expect(LogType.Error, $"Address '{entry.address}' cannot contain '[ ]'.");
        }

        [Test]
        public void WhenAddressHasSquareBracketsAndGuidIsEmptyString_CreatingNewEntry_ThrowsNothing()
        {
            Assert.DoesNotThrow(() => new AddressableAssetEntry("", "[Entry]", null, false));
        }

        [Test]
        public void WhenAddressHasSquareBracketsAndGuidIsNotEmptyString_SettingTheAddressOnExistingEntry_ThrowsError()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("12345698655", "Entry", null, false);
            entry.SetAddress("[Entry]");
            LogAssert.Expect(LogType.Error, $"Address '{entry.address}' cannot contain '[ ]'.");
        }

        [Test]
        public void WhenAddressHasSquareBracketsAndGuidIsEmptyString_SettingTheAddressOnExistingEntry_ThrowsNothing()
        {
            AddressableAssetEntry entry = new AddressableAssetEntry("", "Entry", null, false);
            Assert.DoesNotThrow(() => entry.SetAddress("[Entry]"));
        }

        [Test]
        public void WhenTargetAssetNotFoundInAssetDatabase_ReloadObject()
        {
            var obj = UnityEngine.AddressableAssets.Tests.TestObject.Create("test_targetAsset");
            var obj2 = UnityEngine.AddressableAssets.Tests.TestObject.Create("test_targetAsset2");
            string path = GetAssetPath("test_targetAsset.asset");

            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.SaveAssets();
            var entry = new AddressableAssetEntry(AssetDatabase.AssetPathToGUID(path), "Entry", null, false);
            UnityEngine.Object targetAsset = entry.TargetAsset;

            AssetDatabase.CreateAsset(obj2, path);
            AssetDatabase.SaveAssets();

            Assert.IsFalse(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(targetAsset, out string guid, out long localId));
            UnityEngine.Object targetAsset2 = entry.TargetAsset;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(targetAsset2, out string guid2, out long localId2));

            AssetDatabase.DeleteAsset(path);
        }

#pragma warning disable 0618
        [Test]
        public void CanConvertEntryCollectionToEntries()
        {
            Settings.DenyEntryCollectionPermission = true;
            var collectionPath = Path.Combine(TestFolder, "collection.asset").Replace('\\', '/');
            var collection = ScriptableObject.CreateInstance<AddressableAssetEntryCollection>();

            var assetPath = GetAssetPath("test.prefab");
            var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            var collectionEntry = new AddressableAssetEntry(assetGuid, "TestAssetEntry", null, false);
            collectionEntry.m_cachedAssetPath = "TestPath";
            collection.Entries.Add(collectionEntry);
            AssetDatabase.CreateAsset(collection, collectionPath);
            Settings.DenyEntryCollectionPermission = false;

            bool converted = this.Settings.ConvertAssetEntryCollections(new List<string>() {collectionPath});
            Assert.IsTrue(converted, "Failed to convert AssetEntryCollection to standard Group Entries");
            var addedEntry = Settings.DefaultGroup.GetAssetEntry(assetGuid);
            Assert.IsNotNull(addedEntry, "Could not find entry in default Group.");
        }
#pragma warning restore 0618

        string CreateSpriteAtlasWithSprite()
        {
            // create a Sprite atlas, + sprite
            var spriteAtlasPath = GetAssetPath("testAtlas.spriteatlas");
            SpriteAtlas spriteAtlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(spriteAtlas, spriteAtlasPath);

            Texture2D texture = Texture2D.whiteTexture;
            byte[] data = texture.EncodeToPNG();
            var texturePath = GetAssetPath("testTexture.png");
            File.WriteAllBytes(texturePath, data);
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            TextureImporter importer = TextureImporter.GetAtPath(texturePath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();

            SpriteAtlasExtensions.Add(spriteAtlas, new[] { AssetDatabase.LoadAssetAtPath<Texture>(texturePath) });
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget, false);

            return spriteAtlasPath;
        }
    }
}
