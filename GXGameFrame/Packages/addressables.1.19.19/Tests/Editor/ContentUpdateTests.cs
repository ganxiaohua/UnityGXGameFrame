using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class ContentUpdateTests : AddressableAssetTestBase
    {
        protected override bool PersistSettings { get { return true; } }

        [Test]
        public void CanCreateContentStateData()
        {
            var group = Settings.CreateGroup("LocalStuff", false, false, false, null);
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            Settings.CreateOrMoveEntry(m_AssetGUID, group);
            var context = new AddressablesDataBuilderInput(Settings);

            var op = Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            Assert.IsTrue(string.IsNullOrEmpty(op.Error), op.Error);
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var cacheData = ContentUpdateScript.LoadContentState(tempPath);
            Assert.NotNull(cacheData);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void ContentState_WithDisabledGroups_DoesNotInclude_EntriesFromGroup()
        {
            var group = Settings.CreateGroup("RemoteStuff", false, false, false, null);
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            Settings.CreateOrMoveEntry(m_AssetGUID, group);

            var group2 = Settings.CreateGroup("LocalStuff", false, false, false, null);
            var schema2 = group2.AddSchema<BundledAssetGroupSchema>();
            schema2.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteBuildPath);
            schema2.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteLoadPath);
            schema2.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            group2.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;
            Settings.CreateOrMoveEntry(m_SceneGuids[0], group2);


            var context = new AddressablesDataBuilderInput(Settings);

            var op = Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            Assert.IsTrue(string.IsNullOrEmpty(op.Error), op.Error);
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var cacheData = ContentUpdateScript.LoadContentState(tempPath);
            Assert.NotNull(cacheData);
            Assert.NotNull(cacheData.cachedInfos.FirstOrDefault(s => s.asset.guid.ToString() == m_AssetGUID));

            schema.IncludeInBuild = false;
            context = new AddressablesDataBuilderInput(Settings);
            op = Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);
            Assert.IsTrue(string.IsNullOrEmpty(op.Error), op.Error);
            tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            cacheData = ContentUpdateScript.LoadContentState(tempPath);
            Assert.NotNull(cacheData);
            Assert.IsNull(cacheData.cachedInfos.FirstOrDefault(s => s.asset.guid.ToString() == m_AssetGUID));

            Settings.RemoveGroup(group);
            Settings.RemoveGroup(group2);
        }

        [Test]
        public void CreateCustomLocator_RetrunsLocatorWithUniqueId()
        {
            ContentCatalogData ccd = new ContentCatalogData();
            ccd.SetData(new List<ContentCatalogDataEntry>());
            ResourceLocationMap map = ccd.CreateCustomLocator("test");
            Assert.AreEqual("test", map.LocatorId);
        }

        [Test]
        public void PrepareContentUpdate()
        {
            var group = Settings.CreateGroup("LocalStuff2", false, false, false, null);
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, group);
            entry.address = "test";

            var context = new AddressablesDataBuilderInput(Settings);

            Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            var path = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            obj.GetComponent<Transform>().SetPositionAndRotation(new Vector3(10, 10, 10), Quaternion.identity);
            PrefabUtility.SavePrefabAsset(obj);
            AssetDatabase.SaveAssets();
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(Settings, tempPath);
            Assert.IsNotNull(modifiedEntries);
            Assert.GreaterOrEqual(modifiedEntries.Count, 1);
            ContentUpdateScript.CreateContentUpdateGroup(Settings, modifiedEntries, "Content Update");
            var contentGroup = Settings.FindGroup("Content Update");
            Assert.IsNotNull(contentGroup);
            var movedEntry = contentGroup.GetAssetEntry(m_AssetGUID);
            Assert.AreSame(movedEntry, entry);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void GatherModifiedEntries_WhenDependencyBundleNameIsSame_DependencyIsNotFlaggedAsModified()
        {
            // Create assets
            GameObject mainObject = new GameObject("mainObject");
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mainObject.AddComponent<MeshRenderer>().material = mat;

            string mainAssetPath = GetAssetPath("mainObject.prefab");
            string staticAssetPath = GetAssetPath("staticObject.mat");

            AssetDatabase.CreateAsset(mat, staticAssetPath);
            PrefabUtility.SaveAsPrefabAsset(mainObject, mainAssetPath);
            AssetDatabase.SaveAssets();

            // Create addressables
            AddressableAssetGroup mainAssetGroup = Settings.CreateGroup("PrefabGroup", false, false, false, null);
            AddressableAssetGroup staticContentGroup = Settings.CreateGroup("MatGroup", false, false, false, null);

            var schema = mainAssetGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            mainAssetGroup.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            schema = staticContentGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            staticContentGroup.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            AddressableAssetEntry mainEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(mainAssetPath), mainAssetGroup);
            AddressableAssetEntry staticEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(staticAssetPath), staticContentGroup);

            // Build
            var context = new AddressablesDataBuilderInput(Settings);
            Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            // Modify assets
            var mainAsset = AssetDatabase.LoadAssetAtPath<GameObject>(mainAssetPath);
            mainAsset.GetComponent<Transform>().SetPositionAndRotation(new Vector3(10, 10, 10), Quaternion.identity);
            PrefabUtility.SavePrefabAsset(mainAsset);
            AssetDatabase.SaveAssets();

            // Test
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/Library/com.unity.addressables/" + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(Settings, tempPath);

            Assert.AreEqual(1, modifiedEntries.Count);
            Assert.AreSame(modifiedEntries[0], mainEntry);

            // Cleanup
            GameObject.DestroyImmediate(mainObject);

            Settings.RemoveGroup(mainAssetGroup);
            Settings.RemoveGroup(staticContentGroup);

            AssetDatabase.DeleteAsset(mainAssetPath);
            AssetDatabase.DeleteAsset(staticAssetPath);
        }

        [Test]
        public void GatherModifiedEntries_WhenDependencyBundleNameIsChanged_DependencyIsFlaggedAsModified()
        {
            // Create assets
            GameObject mainObject = new GameObject("mainObject");
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mainObject.AddComponent<MeshRenderer>().material = mat;

            string mainAssetPath = GetAssetPath("mainObject.prefab");
            string staticAssetPath = GetAssetPath("staticObject.mat");

            AssetDatabase.CreateAsset(mat, staticAssetPath);
            PrefabUtility.SaveAsPrefabAsset(mainObject, mainAssetPath);
            AssetDatabase.SaveAssets();

            // Create addressables
            AddressableAssetGroup mainAssetGroup = Settings.CreateGroup("PrefabGroup2", false, false, false, null);
            AddressableAssetGroup staticContentGroup = Settings.CreateGroup("MatGroup2", false, false, false, null);

            var schema = mainAssetGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            mainAssetGroup.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            schema = staticContentGroup.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            staticContentGroup.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

            AddressableAssetEntry mainEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(mainAssetPath), mainAssetGroup);
            AddressableAssetEntry staticEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(staticAssetPath), staticContentGroup);

            // Build
            var context = new AddressablesDataBuilderInput(Settings);
            Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            // Modify assets
            var mainAsset = AssetDatabase.LoadAssetAtPath<GameObject>(mainAssetPath);
            mainAsset.GetComponent<Transform>().SetPositionAndRotation(new Vector3(10, 10, 10), Quaternion.identity);
            PrefabUtility.SavePrefabAsset(mainAsset);
            staticContentGroup.GetSchema<BundledAssetGroupSchema>().InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash;
            AssetDatabase.SaveAssets();

            // Test
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/Library/com.unity.addressables/" + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var modifiedEntries = ContentUpdateScript.GatherModifiedEntries(Settings, tempPath);

            Assert.AreEqual(2, modifiedEntries.Count);
            Assert.IsTrue(modifiedEntries.Contains(staticEntry));

            // Cleanup
            GameObject.DestroyImmediate(mainObject);

            Settings.RemoveGroup(mainAssetGroup);
            Settings.RemoveGroup(staticContentGroup);

            AssetDatabase.DeleteAsset(mainAssetPath);
            AssetDatabase.DeleteAsset(staticAssetPath);
        }

        [Test]
        public void GetStaticContentDependenciesOfModifiedEntries_FlagsEntryDependencies_WithStaticContentEnabled()
        {
            var mainAssetGroup = Settings.CreateGroup("MainAssetGroup", false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            var staticContentGroup = Settings.CreateGroup("StaticContentGroup", false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            mainAssetGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            staticContentGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;

            GameObject mainObject = new GameObject("mainObject");
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mainObject.AddComponent<MeshRenderer>().material = mat;

            string mainAssetPath = GetAssetPath("mainObject.prefab");
            string staticAssetPath = GetAssetPath("staticObject.mat");

            AssetDatabase.CreateAsset(mat, staticAssetPath);
            PrefabUtility.SaveAsPrefabAsset(mainObject, mainAssetPath);
            AssetDatabase.SaveAssets();

            var mainEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(mainAssetPath), mainAssetGroup);
            var staticEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(staticAssetPath), staticContentGroup);

            List<AddressableAssetEntry> modifiedEntries = new List<AddressableAssetEntry>()
            {
                mainEntry
            };

            Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>> staticDependencies = new Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>>()
            {
                {mainEntry, new List<AddressableAssetEntry>() }
            };
            ContentUpdateScript.GetStaticContentDependenciesForEntries(Settings, ref staticDependencies, null);

            Assert.AreEqual(1, staticDependencies.Count);
            Assert.AreEqual(1, staticDependencies[mainEntry].Count);
            Assert.IsTrue(staticDependencies[mainEntry].Contains(staticEntry));

            //Cleanup
            GameObject.DestroyImmediate(mainObject);

            Settings.RemoveGroup(mainAssetGroup);
            Settings.RemoveGroup(staticContentGroup);

            AssetDatabase.DeleteAsset(mainAssetPath);
            AssetDatabase.DeleteAsset(staticAssetPath);
        }

        [Test]
        public void GetStaticContentDependenciesOfModifiedEntries_DoesNotFlagEntryDependencies_WithStaticContentDisabled()
        {
            var mainAssetGroup = Settings.CreateGroup("MainAssetGroup", false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            var dynamicContentGroup = Settings.CreateGroup("DynamicContentGroup", false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            mainAssetGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            dynamicContentGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;

            GameObject mainObject = new GameObject("mainObject");
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mainObject.AddComponent<MeshRenderer>().material = mat;

            string mainAssetPath = GetAssetPath("mainObject.prefab");
            string dynamicAssetPath = GetAssetPath("dynamicObject.mat");

            AssetDatabase.CreateAsset(mat, dynamicAssetPath);
            PrefabUtility.SaveAsPrefabAsset(mainObject, mainAssetPath);
            AssetDatabase.SaveAssets();

            var mainEntry = Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(mainAssetPath), mainAssetGroup);
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(dynamicAssetPath), dynamicContentGroup);

            List<AddressableAssetEntry> modifiedEntries = new List<AddressableAssetEntry>()
            {
                mainEntry
            };

            Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>> staticDependencies = new Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>>()
            {
                {mainEntry, new List<AddressableAssetEntry>() }
            };
            ContentUpdateScript.GetStaticContentDependenciesForEntries(Settings, ref staticDependencies, null);

            Assert.AreEqual(1, staticDependencies.Count);
            Assert.AreEqual(0, staticDependencies[mainEntry].Count);

            //Cleanup
            GameObject.DestroyImmediate(mainObject);

            Settings.RemoveGroup(mainAssetGroup);
            Settings.RemoveGroup(dynamicContentGroup);

            AssetDatabase.DeleteAsset(mainAssetPath);
            AssetDatabase.DeleteAsset(dynamicAssetPath);
        }

        [Test]
        public void GetStaticContentDependenciesOfModifiedEntries_WhenFolderHasModifiedImplicitAsset_DependenciesFlaggedAsModified()
        {
            var mainAssetGroup = Settings.CreateGroup("MainAssetGroup", false, false, false, null,
                typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

            mainAssetGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            mainAssetGroup.GetSchema<BundledAssetGroupSchema>().InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash;

            GameObject mainObject = new GameObject("mainObject");
            Material mat = new Material(Shader.Find("Transparent/Diffuse"));
            mainObject.AddComponent<MeshRenderer>().material = mat;

            string folderName = "ContentUpdateFolder";
            string folderGuid = AssetDatabase.CreateFolder(ConfigFolder, folderName);
            string folderPath = Path.Combine(ConfigFolder, folderName).Replace("\\", "/");

            string mainAssetPath = Path.Combine(folderPath, "mainAsset.prefab").Replace("\\", "/");
            string subAssetPath = Path.Combine(folderPath, "subAsset.mat").Replace("\\", "/");

            AssetDatabase.CreateAsset(mat, subAssetPath);
            PrefabUtility.SaveAsPrefabAsset(mainObject, mainAssetPath);
            AssetDatabase.SaveAssets();

            string mainAssetGuid = AssetDatabase.AssetPathToGUID(mainAssetPath);
            string subAssetGuid = AssetDatabase.AssetPathToGUID(subAssetPath);

            var folderEntry = Settings.CreateOrMoveEntry(folderGuid, mainAssetGroup);
            var entries = new List<AddressableAssetEntry>();
            folderEntry.GatherAllAssets(entries, false, false, true);
            AddressableAssetEntry mainAssetEntry = entries.Find(x => x.guid == mainAssetGuid);

            var staticDependencies = new Dictionary<AddressableAssetEntry, List<AddressableAssetEntry>>()
            {
                { mainAssetEntry, new List<AddressableAssetEntry>() }
            };

            string cachedBundleName = "cachedBundleName";
            var groupGuidToCacheBundleName = new Dictionary<string, string>()
            {
                { folderGuid, cachedBundleName },
                { mainAssetGuid, cachedBundleName },
                { subAssetGuid, cachedBundleName },
            };
            ContentUpdateScript.GetStaticContentDependenciesForEntries(Settings, ref staticDependencies, groupGuidToCacheBundleName);

            Assert.AreEqual(1, staticDependencies.Count);
            Assert.AreEqual(2, staticDependencies[mainAssetEntry].Count);

            //Cleanup
            GameObject.DestroyImmediate(mainObject);
            Settings.RemoveGroup(mainAssetGroup);
            AssetDatabase.DeleteAsset(folderPath);
        }

        static IResourceLocator GetLocatorFromCatalog(IEnumerable<string> paths)
        {
            foreach (var p in paths)
                if (p.EndsWith("catalog.json"))
                    return JsonUtility.FromJson<ContentCatalogData>(File.ReadAllText(p)).CreateLocator();
            return null;
        }

        [Test]
        public void WhenContentUpdated_NewCatalogRetains_OldCatalogBundleLoadData()
        {
            var group = Settings.CreateGroup("LocalStuff3", false, false, false, null);
            Settings.BuildRemoteCatalog = true;
            Settings.RemoteCatalogBuildPath = new ProfileValueReference();
            Settings.RemoteCatalogBuildPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteBuildPath);
            Settings.RemoteCatalogLoadPath = new ProfileValueReference();
            Settings.RemoteCatalogLoadPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteLoadPath);
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.UseAssetBundleCrc = true;
            schema.UseAssetBundleCache = true;
            group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;
            Settings.CreateOrMoveEntry(m_AssetGUID, group);
            var context = new AddressablesDataBuilderInput(Settings);

            var op = Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);
            Assert.IsTrue(string.IsNullOrEmpty(op.Error), op.Error);
            var origLocator = GetLocatorFromCatalog(op.FileRegistry.GetFilePaths());
            Assert.NotNull(origLocator);

            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            var contentState = ContentUpdateScript.LoadContentState(tempPath);
            Assert.NotNull(contentState);
            Assert.NotNull(contentState.cachedBundles);
            Assert.AreEqual(1, contentState.cachedBundles.Length);
            var buildOp = ContentUpdateScript.BuildContentUpdate(Settings, tempPath);
            Assert.IsNotNull(buildOp);
            Assert.IsTrue(string.IsNullOrEmpty(buildOp.Error));

            var updatedLocator = GetLocatorFromCatalog(buildOp.FileRegistry.GetFilePaths());
            Assert.IsNotNull(updatedLocator);

            foreach (var k in updatedLocator.Keys)
            {
                if (origLocator.Locate(k, typeof(IAssetBundleResource), out var origLocs))
                {
                    Assert.IsTrue(updatedLocator.Locate(k, typeof(IAssetBundleResource), out var updatedLocs));
                    Assert.AreEqual(1, origLocs.Count);
                    Assert.AreEqual(1, updatedLocs.Count);
                    var oLoc = origLocs[0];
                    var uLoc = updatedLocs[0];
                    Assert.NotNull(oLoc.Data);
                    Assert.NotNull(uLoc.Data);
                    var oData = oLoc.Data as AssetBundleRequestOptions;
                    var uData = uLoc.Data as AssetBundleRequestOptions;
                    Assert.NotNull(oData);
                    Assert.NotNull(uData);
                    Assert.AreEqual(oData.Hash, uData.Hash);
                    Assert.AreEqual(oData.Crc, uData.Crc);
                }
            }

            Settings.RemoveGroup(group);
        }

        [Test]
        public void IsCacheDataValid_WhenNoPreviousRemoteCatalogPath_ReturnsFalseWithError()
        {
            AddressablesContentState cacheData = new AddressablesContentState();
            cacheData.editorVersion = Application.unityVersion;
            Assert.IsFalse(ContentUpdateScript.IsCacheDataValid(Settings, cacheData));
            LogAssert.Expect(LogType.Error, new Regex("Previous build had 'Build Remote Catalog' disabled.*"));
        }

        [Test]
        public void IsCacheDataValid_WhenRemoteCatalogDisabled_ReturnsFalseWithError()
        {
            AddressablesContentState cacheData = new AddressablesContentState();
            cacheData.editorVersion = Application.unityVersion;
            cacheData.remoteCatalogLoadPath = "somePath";
            var oldSetting = Settings.BuildRemoteCatalog;
            Settings.BuildRemoteCatalog = false;
            Assert.IsFalse(ContentUpdateScript.IsCacheDataValid(Settings, cacheData));
            LogAssert.Expect(LogType.Error, new Regex("Current settings have 'Build Remote Catalog' disabled.*"));
            Settings.BuildRemoteCatalog = oldSetting;
        }

        [Test]
        public void IsCacheDataValid_WhenMismatchedCatalogPaths_ReturnsFalseWithError()
        {
            AddressablesContentState cacheData = new AddressablesContentState();
            cacheData.editorVersion = Application.unityVersion;
            cacheData.remoteCatalogLoadPath = "somePath";
            var oldSetting = Settings.BuildRemoteCatalog;
            Settings.BuildRemoteCatalog = true;
            Assert.IsFalse(ContentUpdateScript.IsCacheDataValid(Settings, cacheData));
            LogAssert.Expect(LogType.Error, new Regex("Current 'Remote Catalog Load Path' does not match load path of original player.*"));
            Settings.BuildRemoteCatalog = oldSetting;
        }

        [Test]
        public void IsCacheDataValid_WhenMismatchedEditorVersions_LogsWarning()
        {
            AddressablesContentState cacheData = new AddressablesContentState();
            cacheData.editorVersion = "invalid";
            Assert.IsFalse(ContentUpdateScript.IsCacheDataValid(Settings, cacheData));
            LogAssert.Expect(LogType.Warning, new Regex(".*with version `" + cacheData.editorVersion + "`.*"));
            LogAssert.Expect(LogType.Error, new Regex("Previous.*"));
        }

        [Test]
        public void BuildContentUpdate_DoesNotDeleteBuiltData()
        {
            var oldSetting = Settings.BuildRemoteCatalog;
            Settings.BuildRemoteCatalog = true;
            var group = Settings.CreateGroup("LocalStuff3", false, false, false, null);
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(Settings, AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;
            Settings.CreateOrMoveEntry(m_AssetGUID, group);
            var context = new AddressablesDataBuilderInput(Settings);

            var op = Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

            Assert.IsTrue(string.IsNullOrEmpty(op.Error), op.Error);
            var tempPath = Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath + PlatformMappingService.GetPlatformPathSubFolder() + "/addressables_content_state.bin";
            ContentUpdateScript.BuildContentUpdate(Settings, tempPath);
            Assert.IsTrue(Directory.Exists(Addressables.BuildPath));
            Settings.BuildRemoteCatalog = oldSetting;
        }

        [Test]
        public void ContentUpdateScenes_PackedTogether_MarksAllScenesModified()
        {
            AddressableAssetGroup group = Settings.CreateGroup("SceneGroup", false, false, false, null, typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;

            AddressableAssetEntry scene1 = new AddressableAssetEntry(m_SceneGuids[0], "scene1", group, false);
            AddressableAssetEntry scene2 = new AddressableAssetEntry(m_SceneGuids[1], "scene2", group, false);
            AddressableAssetEntry scene3 = new AddressableAssetEntry(m_SceneGuids[2], "scene3", group, false);

            group.AddAssetEntry(scene1, false);
            group.AddAssetEntry(scene2, false);
            group.AddAssetEntry(scene3, false);

            List<AddressableAssetEntry> modifedEnteries = new List<AddressableAssetEntry>()
            {
                scene1
            };

            ContentUpdateScript.AddAllDependentScenesFromModifiedEntries(modifedEnteries);

            Assert.AreEqual(3, modifedEnteries.Count);
            Assert.AreEqual(scene1, modifedEnteries[0]);
            Assert.AreEqual(scene2, modifedEnteries[1]);
            Assert.AreEqual(scene3, modifedEnteries[2]);

            Settings.RemoveGroup(group);
        }

        [Test]
        public void ContentUpdateScenes_PackedTogetherByLabel_MarksAllScenesModifiedWithSharedLabel()
        {
            AddressableAssetGroup group = Settings.CreateGroup("SceneGroup", false, false, false, null, typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;

            AddressableAssetEntry scene1 = new AddressableAssetEntry(m_SceneGuids[0], "scene1", group, false);
            AddressableAssetEntry scene2 = new AddressableAssetEntry(m_SceneGuids[1], "scene2", group, false);
            AddressableAssetEntry scene3 = new AddressableAssetEntry(m_SceneGuids[2], "scene3", group, false);

            group.AddAssetEntry(scene1, false);
            group.AddAssetEntry(scene2, false);
            group.AddAssetEntry(scene3, false);

            scene1.SetLabel("label", true, true);
            scene3.SetLabel("label", true, true);

            List<AddressableAssetEntry> modifedEnteries = new List<AddressableAssetEntry>()
            {
                scene1
            };

            ContentUpdateScript.AddAllDependentScenesFromModifiedEntries(modifedEnteries);

            Assert.AreEqual(2, modifedEnteries.Count);
            Assert.AreEqual(scene1, modifedEnteries[0]);
            Assert.AreEqual(scene3, modifedEnteries[1]);

            Settings.RemoveGroup(group);
        }

        [Test]
        public void ContentUpdateScenes_PackedSeperately_MarksNoAdditionalScenes()
        {
            AddressableAssetGroup group = Settings.CreateGroup("SceneGroup", false, false, false, null, typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;

            AddressableAssetEntry scene1 = new AddressableAssetEntry(m_SceneGuids[0], "scene1", group, false);
            AddressableAssetEntry scene2 = new AddressableAssetEntry(m_SceneGuids[1], "scene2", group, false);
            AddressableAssetEntry scene3 = new AddressableAssetEntry(m_SceneGuids[2], "scene3", group, false);

            group.AddAssetEntry(scene1, false);
            group.AddAssetEntry(scene2, false);
            group.AddAssetEntry(scene3, false);

            List<AddressableAssetEntry> modifedEnteries = new List<AddressableAssetEntry>()
            {
                scene1
            };

            ContentUpdateScript.AddAllDependentScenesFromModifiedEntries(modifedEnteries);

            Assert.AreEqual(1, modifedEnteries.Count);
            Assert.AreEqual(scene1, modifedEnteries[0]);

            Settings.RemoveGroup(group);
        }

        private ContentUpdateScript.ContentUpdateContext GetContentUpdateContext(string contentUpdateTestAssetGUID, string contentUpdateTestCachedAssetHash,
            string contentUpdateTestNewInternalBundleName, string contentUpdateTestNewBundleName, string contentUpdateTestCachedBundlePath, string contentUpdateTestGroupGuid, string contentUpdateTestFileName)
        {
            var context = new ContentUpdateScript.ContentUpdateContext()
            {
                WriteData = new BundleWriteData(),
                BundleToInternalBundleIdMap = new Dictionary<string, string>(),
                GuidToPreviousAssetStateMap = new Dictionary<string, CachedAssetState>(),
                IdToCatalogDataEntryMap = new Dictionary<string, ContentCatalogDataEntry>(),
                ContentState = new AddressablesContentState(),
                PreviousAssetStateCarryOver = new List<CachedAssetState>(),
                Registry = new FileRegistry()
            };
            AddToContentUpdateContext(context, contentUpdateTestAssetGUID, contentUpdateTestCachedAssetHash,
                contentUpdateTestNewInternalBundleName, contentUpdateTestNewBundleName, contentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, contentUpdateTestFileName);
            return context;
        }

        private void AddToContentUpdateContext(ContentUpdateScript.ContentUpdateContext context, string contentUpdateTestAssetGUID, string contentUpdateTestCachedAssetHash,
            string contentUpdateTestNewInternalBundleName, string contentUpdateTestNewBundleName, string contentUpdateTestCachedBundlePath, string contentUpdateTestGroupGuid, string contentUpdateTestFileName)
        {
            context.BundleToInternalBundleIdMap.Add(contentUpdateTestNewInternalBundleName, contentUpdateTestNewBundleName);
            context.GuidToPreviousAssetStateMap.Add(contentUpdateTestAssetGUID,
                new CachedAssetState()
                {
                    bundleFileId = contentUpdateTestCachedBundlePath,
                    asset = new AssetState()
                    {
                        guid = new GUID(contentUpdateTestAssetGUID),
                        hash = Hash128.Parse(contentUpdateTestCachedAssetHash)
                    },
                    dependencies = new AssetState[] {},
                    data = null,
                    groupGuid = contentUpdateTestGroupGuid
                }
            );

            context.IdToCatalogDataEntryMap.Add(contentUpdateTestNewBundleName,
                new ContentCatalogDataEntry(typeof(IAssetBundleResource), contentUpdateTestNewBundleName, typeof(AssetBundleProvider).FullName, new[] { contentUpdateTestNewBundleName }));
            context.IdToCatalogDataEntryMap.Add(contentUpdateTestAssetGUID,
                new ContentCatalogDataEntry(typeof(IAssetBundleResource), contentUpdateTestAssetGUID, typeof(AssetBundleProvider).FullName, new[] { contentUpdateTestAssetGUID }));

            context.WriteData.AssetToFiles.Add(new GUID(contentUpdateTestAssetGUID), new List<string>() { contentUpdateTestFileName });
            context.WriteData.FileToBundle.Add(contentUpdateTestFileName, contentUpdateTestNewInternalBundleName);
        }

        private AddressableAssetEntry CreateAssetEntry(string guid, AddressableAssetGroup group)
        {
            return new AddressableAssetEntry(guid, guid, group, false);
        }

        readonly string m_ContentUpdateTestAssetGUID = GUID.Generate().ToString();
        const string k_ContentUpdateTestCachedAssetHash = "8888888888888888888";
        const string k_ContentUpdateTestNewInternalBundleName = "bundle";
        const string k_ContentUpdateTestNewBundleName = "fullbundlepath";
        const string k_ContentUpdateTestCachedBundlePath = "cachedBundle";
        const string k_ContentUpdateTestFileName = "testfile";

        [Test]
        public void DetermineRequiredAssetEntryUpdates_AssetWithoutDependenciesAndChangedHashInStaticGroup_ReturnsOnlyRevertToCachedState()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");
            File.WriteAllText(k_ContentUpdateTestNewBundleName, "TestNewBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = group.Guid;

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            assetEntry.m_cachedAssetPath = "path";
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.AreEqual(assetEntry, ops[0].AssetEntry);
            Assert.AreEqual(k_ContentUpdateTestNewBundleName, ops[0].CurrentBuildPath);
            Assert.AreEqual(k_ContentUpdateTestCachedBundlePath, ops[0].PreviousBuildPath);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            File.Delete(k_ContentUpdateTestNewBundleName);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_AssetWithDependenciesAndChangedHashInStaticGroup_ReturnsRevertToCachedStateAndDependencies()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");
            File.WriteAllText(k_ContentUpdateTestNewBundleName, "TestNewBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = group.Guid;

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            assetEntry.m_cachedAssetPath = "path";
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);
            context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID].dependencies = new AssetState[]
            {
                new AssetState()
                {
                    guid = GUID.Generate(),
                    hash = Hash128.Parse("00000000000000")
                }
            };

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.AreEqual(assetEntry, ops[0].AssetEntry);
            Assert.AreEqual(k_ContentUpdateTestNewBundleName, ops[0].CurrentBuildPath);
            Assert.AreEqual(k_ContentUpdateTestCachedBundlePath, ops[0].PreviousBuildPath);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            File.Delete(k_ContentUpdateTestNewBundleName);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_AssetWithChangedGroup_TakesNoAction()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.IsTrue(ops.Count == 0);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_AssetWithChangedHashInNonStaticGroup_TakesNoAction()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.IsTrue(ops.Count == 0);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_EntriesThatTakeNoAction_StillSetBundleFileId()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            assetEntry.m_cachedAssetPath = "path";
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.IsTrue(ops.Count == 0);
            Assert.AreEqual(k_ContentUpdateTestNewBundleName, assetEntry.BundleFileId);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_WithMissingBundleFileId_LogsErrorAndTakesNoAction()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = group.Guid;

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            assetEntry.m_cachedAssetPath = "path";
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                "", contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            LogAssert.Expect(LogType.Error, $"CachedAssetState found for {assetEntry.AssetPath} but the bundleFileId was never set on the previous build.");
            Assert.IsTrue(ops.Count == 0);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_WithMissingPreviousBundle_LogsWarningAndReturnsRevertOperation()
        {
            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = group.Guid;

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            assetEntry.m_cachedAssetPath = "path";
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);
            LogAssert.Expect(LogType.Warning, $"CachedAssetState found for {assetEntry.AssetPath} but the previous bundle at {k_ContentUpdateTestCachedBundlePath} cannot be found. " +
                $"This will not affect loading the bundle in previously built players, but loading the missing bundle in Play Mode using the play mode script " +
                $"\"Use Existing Build (requires built groups)\" will fail.");
            Assert.IsTrue(ops.Count == 1);

            Settings.RemoveGroup(group);
        }

        [Test]
        public void DetermineRequiredAssetEntryUpdates_AssetWithMatchingCachedInternalBundleId_TakesNoAction()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = group.Guid;

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = true;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestNewBundleName, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = RevertUnchangedAssetsToPreviousAssetState.DetermineRequiredAssetEntryUpdates(group, context);

            Assert.IsTrue(ops.Count == 0);

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void ApplyAssetEntryUpdates_InvalidKeyForGuidToPreviousAssetStateMap_DoesNotThrow()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);
            context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID].dependencies = new AssetState[]
            {
                new AssetState()
                {
                    guid = GUID.Generate(),
                    hash = Hash128.Parse("9823749283742")
                }
            };

            var ops = new List<RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation>()
            {
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                }
            };

            Assert.DoesNotThrow(() =>
                RevertUnchangedAssetsToPreviousAssetState.ApplyAssetEntryUpdates(ops, "BundleProvider",
                    new List<ContentCatalogDataEntry>(), context));

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void ApplyAssetEntryUpdates_DeletesCorrectBundle()
        {
            File.WriteAllText(k_ContentUpdateTestCachedBundlePath, "TestCachedAssetBundle");
            File.WriteAllText(k_ContentUpdateTestNewBundleName, "TestAssetBundle");

            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = new List<RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation>()
            {
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                },
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                }
            };

            RevertUnchangedAssetsToPreviousAssetState.ApplyAssetEntryUpdates(ops, "BundleProvider", new List<ContentCatalogDataEntry>(), context);

            Assert.IsTrue(File.Exists(k_ContentUpdateTestCachedBundlePath));
            Assert.IsFalse(File.Exists(k_ContentUpdateTestNewBundleName));

            File.Delete(k_ContentUpdateTestCachedBundlePath);
            Settings.RemoveGroup(group);
        }

        [Test]
        public void ApplyAssetEntryUpdates_RegistryOnlyContainsCachedBundleEntry()
        {
            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var ops = new List<RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation>()
            {
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                },
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                }
            };

            RevertUnchangedAssetsToPreviousAssetState.ApplyAssetEntryUpdates(ops, "BundleProvider", new List<ContentCatalogDataEntry>(), context);

            var registryPaths = context.Registry.GetFilePaths();

            Assert.AreEqual(1, registryPaths.Count());
            Assert.AreEqual(k_ContentUpdateTestCachedBundlePath, registryPaths.ElementAt(0));

            Settings.RemoveGroup(group);
        }

        [Test]
        public void ApplyAssetEntryUpdates_PreviousStateDependencies_SetInCatalogEntry()
        {
            var group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            var assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            var context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            var previousDep = new AssetState()
            {
                guid = GUID.Generate(),
                hash = Hash128.Parse("9823749283742")
            };

            var currentDep = new AssetState()
            {
                guid = GUID.Generate(),
                hash = Hash128.Parse("128747239872")
            };

            context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID].dependencies = new AssetState[]
            {
                previousDep
            };

            context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID].Dependencies.Add(currentDep);

            var ops = new List<RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation>()
            {
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                }
            };

            string depBundleFileId = "depBundleFileId";
            context.GuidToPreviousAssetStateMap.Add(previousDep.guid.ToString(), new CachedAssetState()
            {
                asset = previousDep,
                bundleFileId = depBundleFileId,
                data = previousDep,
                dependencies = new AssetState[0],
                groupGuid = contentUpdateTestGroupGuid
            });

            var locations = new List<ContentCatalogDataEntry>();
            RevertUnchangedAssetsToPreviousAssetState.ApplyAssetEntryUpdates(ops, "BundleProvider", locations, context);

            Assert.AreEqual(1, locations.Count);
            Assert.AreEqual(1, ops[0].BundleCatalogEntry.Dependencies.Count);
            Assert.AreEqual(depBundleFileId + ops[0].AssetEntry.GetHashCode(), ops[0].BundleCatalogEntry.Dependencies[0]);

            Settings.RemoveGroup(group);
        }

        [Test]
        public void ApplyAssetEntryUpdates_WhenAssetAndDependencyAreModifiedAndInSeparateGroups_SetCatalogEntryToCachedBundles()
        {
            GUID depAssetGuid = GUID.Generate();
            string oldDepGroupCachedAssetHash = "1888888888888888888";
            string newDepGroupCachedAssetHash = "1188888888888888888";
            string depGroupNewInternalBundleName = "bundle2";
            string depGroupNewBundleName = "fullbundlepath2";
            string depGroupCachedBundlePath = "cachedBundle2";
            string depGroupFileName = "testfile2";

            AddressableAssetGroup group = Settings.CreateGroup("ContentUpdateTests", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            string contentUpdateTestGroupGuid = GUID.Generate().ToString();

            AddressableAssetGroup depGroup = Settings.CreateGroup("ContentUpdateTests2", false, false, false, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            GUID depGroupGuid = GUID.Generate();

            group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            AddressableAssetEntry assetEntry = CreateAssetEntry(m_ContentUpdateTestAssetGUID, group);
            group.AddAssetEntry(assetEntry);

            depGroup.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
            AddressableAssetEntry depAssetEntry = CreateAssetEntry(depAssetGuid.ToString(), depGroup);
            depGroup.AddAssetEntry(assetEntry);

            ContentUpdateScript.ContentUpdateContext context = GetContentUpdateContext(m_ContentUpdateTestAssetGUID, k_ContentUpdateTestCachedAssetHash,
                k_ContentUpdateTestNewInternalBundleName, k_ContentUpdateTestNewBundleName,
                k_ContentUpdateTestCachedBundlePath, contentUpdateTestGroupGuid, k_ContentUpdateTestFileName);

            AddToContentUpdateContext(context, depAssetGuid.ToString(), oldDepGroupCachedAssetHash,
                depGroupNewInternalBundleName, depGroupNewBundleName,
                depGroupCachedBundlePath, depGroupGuid.ToString(), depGroupFileName);

            var previousDep = new AssetState()
            {
                guid = depAssetGuid,
                hash = Hash128.Parse(oldDepGroupCachedAssetHash)
            };

            var currentDep = new AssetState()
            {
                guid = GUID.Generate(),
                hash = Hash128.Parse(newDepGroupCachedAssetHash)
            };

            context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID].dependencies = new AssetState[]
            {
                previousDep
            };

            context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID].Dependencies.Add(currentDep);

            var ops = new List<RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation>()
            {
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = k_ContentUpdateTestCachedBundlePath,
                    AssetEntry = assetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID],
                    CurrentBuildPath = k_ContentUpdateTestNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[m_ContentUpdateTestAssetGUID]
                },
                new RevertUnchangedAssetsToPreviousAssetState.AssetEntryRevertOperation()
                {
                    PreviousBuildPath = depGroupCachedBundlePath,
                    AssetEntry = depAssetEntry,
                    BundleCatalogEntry = context.IdToCatalogDataEntryMap[depAssetGuid.ToString()],
                    CurrentBuildPath = depGroupNewBundleName,
                    PreviousAssetState = context.GuidToPreviousAssetStateMap[depAssetGuid.ToString()]
                }
            };

            var locations = new List<ContentCatalogDataEntry>();
            RevertUnchangedAssetsToPreviousAssetState.ApplyAssetEntryUpdates(ops, "BundleProvider", locations, context);

            Assert.AreEqual(k_ContentUpdateTestCachedBundlePath, context.IdToCatalogDataEntryMap[m_ContentUpdateTestAssetGUID].InternalId);
            Assert.AreEqual(depGroupCachedBundlePath, context.IdToCatalogDataEntryMap[depAssetGuid.ToString()].InternalId);

            Settings.RemoveGroup(group);
            Settings.RemoveGroup(depGroup);
        }

        [Test]
        [TestCase("{Addressables.RuntimePath}/TargetPlatform/prefabA.bundle", "{Addressables.RuntimePath}/TargetPlatform", "Library/aa/TargetPlatform")]
        [TestCase("{Addressables.RuntimePath}\\TargetPlatform\\prefabA.bundle", "{Addressables.RuntimePath}/TargetPlatform", "Library/aa/TargetPlatform")]
        [TestCase("http://localhost/TargetPlatform/prefabA.bundle", "http://localhost/TargetPlatform", "ServerData/TargetPlatform")]
        public void BundleIdToBuildPath_ReturnsBundleBuildPath(string bundleId, string rootLoadPath, string rootBuildPath)
        {
            string buildPath = RevertUnchangedAssetsToPreviousAssetState.BundleIdToBuildPath(bundleId, rootLoadPath, rootBuildPath);
            Assert.IsTrue(buildPath.StartsWith(rootBuildPath));
        }
    }
}
