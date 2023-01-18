using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using static UnityEditor.AddressableAssets.Settings.AddressablesFileEnumeration;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetFolderSubfolderTests : AddressableAssetTestBase
    {
        string m_TestFolderPath;

        string m_AddrParentFolderPath;
        string m_AddrChildSubfolderPath;

        string m_ParentObjPath;
        string m_AddrParentObjPath;
        string m_ChildObjPath;

        AddressableAssetGroup m_ParentGroup;
        AddressableAssetGroup m_ChildGroup;

        /* Creates the following folder structure
        * /AddrParentFolder/
        *       parentObj.prefab
        *       addrParentObj.prefab
        *       /AddrChildSubfolder/
        *               childObj.prefab
        */
        protected override void OnInit()
        {
            // Create directories
            m_TestFolderPath = TestFolder;
            m_AddrParentFolderPath = m_TestFolderPath + "/AddrParentFolder";
            m_AddrChildSubfolderPath = m_AddrParentFolderPath + "/AddrChildSubfolder";

            string addrParentFolderGuid = AssetDatabase.CreateFolder(m_TestFolderPath, "AddrParentFolder");
            string addrChildFolderGuid = AssetDatabase.CreateFolder(m_AddrParentFolderPath, "AddrChildSubfolder");

            // Create prefabs
            GameObject parentObj = new GameObject("ParentObject");
            GameObject addrParentObj = new GameObject("AddrParentObject");
            GameObject childObj = new GameObject("ChildObject");

            m_ParentObjPath = m_AddrParentFolderPath + "/parentObj.prefab";
            m_AddrParentObjPath = m_AddrParentFolderPath + "/addrParentObj.prefab";
            m_ChildObjPath = m_AddrChildSubfolderPath + "/childObj.prefab";

            PrefabUtility.SaveAsPrefabAsset(parentObj, m_ParentObjPath);
            PrefabUtility.SaveAsPrefabAsset(addrParentObj, m_AddrParentObjPath);
            PrefabUtility.SaveAsPrefabAsset(childObj, m_ChildObjPath);

            // Create groups
            const string parentGroupName = "ParentGroup";
            const string childGroupName = "ChildGroup";

            m_ParentGroup = Settings.CreateGroup(parentGroupName, false, false, false, null, typeof(BundledAssetGroupSchema));
            m_ChildGroup = Settings.CreateGroup(childGroupName, false, false, false, null, typeof(BundledAssetGroupSchema));

            // Create entries
            Settings.CreateOrMoveEntry(addrParentFolderGuid, m_ParentGroup);

            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(m_AddrParentObjPath), m_ChildGroup);
            Settings.CreateOrMoveEntry(addrChildFolderGuid, m_ChildGroup);
        }

        protected override void OnCleanup()
        {
            Settings.RemoveGroup(m_ParentGroup);
            Settings.RemoveGroup(m_ChildGroup);

            AssetDatabase.DeleteAsset(m_TestFolderPath + "/");
            AssetDatabase.Refresh();
        }

        List<string> GetValidAssetPaths(string path, AddressableAssetSettings settings)
        {
            List<string> pathsWithCache;
            using (var cache = new AddressablesFileEnumerationCache(settings, true, null))
            {
                pathsWithCache = EnumerateAddressableFolder(path, settings, true).ToList<string>();
            }
            List<string> pathsWithoutCache = EnumerateAddressableFolder(path, settings, true).ToList<string>();

            // Compare the results of two different code paths: with cache and without cache
            Assert.AreEqual(pathsWithCache.Count, pathsWithoutCache.Count);
            for (int i = 0; i < pathsWithCache.Count; i++)
            {
                Assert.AreEqual(pathsWithCache[i], pathsWithoutCache[i]);
            }
            return pathsWithCache;
        }

        [Test]
        public void Build_WithAddrParentFolderAndAddrSubfolders_InSeparateGroups_Succeeds()
        {
            var context = new AddressablesDataBuilderInput(Settings);
            foreach (IDataBuilder db in Settings.DataBuilders)
            {
                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                    db.BuildData<AddressablesPlayerBuildResult>(context);
            }
        }

        [Test]
        public void WhenSubfolderIsAddr_AddrParentFolder_DoesNotInclude_SubfolderContents()
        {
            List<string> assetPaths = GetValidAssetPaths(m_AddrParentFolderPath, Settings);
            Assert.IsFalse(assetPaths.Contains(m_ChildObjPath));
        }

        [Test]
        public void WhenAssetIsAddr_AssetIsNotIncludedInParentAddrFolder()
        {
            List<string> assetPaths = GetValidAssetPaths(m_AddrParentFolderPath, Settings);
            Assert.IsFalse(assetPaths.Contains(m_AddrParentObjPath));
        }

        [Test]
        public void EnumerateFiles_ReturnsFilesOnly()
        {
            List<string> assetPaths = EnumerateAddressableFolder(m_AddrParentFolderPath, Settings, true);
            foreach (string path in assetPaths)
            {
                Assert.IsFalse(Directory.Exists(path));
            }
        }

        [Test]
        public void WhenEmptyFolderIsAddr_EnumerateFiles_ReturnsNothing()
        {
            string path = m_TestFolderPath + "/AddrEmptyFolder";
            string guid = AssetDatabase.CreateFolder(m_TestFolderPath, "AddrEmptyFolder");
            Settings.CreateOrMoveEntry(guid, m_ParentGroup);

            List<string> assetPaths = EnumerateAddressableFolder(path, Settings, true);
            Assert.AreEqual(0, assetPaths.Count);

            Settings.RemoveAssetEntry(guid);
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void WhenEnumerateFilesIsNonRecursive_ReturnTopLevelAssetsOnly()
        {
            string path = m_AddrParentFolderPath + "/ChildFolder";
            string guid = AssetDatabase.CreateFolder(m_AddrParentFolderPath, "ChildFolder");

            GameObject obj = new GameObject("TestObject");
            string objPath = path + "/childObj.prefab";
     
            PrefabUtility.SaveAsPrefabAsset(obj, objPath);
            
            List<string> assetPaths = EnumerateAddressableFolder(m_AddrParentFolderPath, Settings, false);
            Assert.AreEqual(1, assetPaths.Count);
            Assert.AreEqual(m_ParentObjPath, assetPaths[0]);

            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void WhenPathDoesNotExist_EnumerateFiles_ThrowsException()
        {
            string path = "PathDoesntExist";
            Exception ex = Assert.Throws<Exception>(() =>
            {
                EnumerateAddressableFolder(path, Settings, false);
            });
            Assert.AreEqual($"Path {path} cannot be enumerated because it does not exist", ex.Message);
        }

        [Test]
        public void WhenPathIsNotInTree_EnumerateFiles_ReturnsFiles()
        {
            string path = m_AddrParentFolderPath + "/ChildFolder";
            string guid = AssetDatabase.CreateFolder(m_AddrParentFolderPath, "ChildFolder");

            GameObject obj = new GameObject("TestObject");
            string objPath = path + "/childObj.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, objPath);
            List<string> assetPaths = EnumerateAddressableFolder(path, Settings, true);
            Assert.AreEqual(1, assetPaths.Count);
            Assert.AreEqual(objPath, assetPaths[0]);

            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void WhenFileIsNotInAssetDatabase_EnumerateFiles_DoesNotReturnPath()
        {
            string folderPath = m_TestFolderPath + "/TestFolder";
            AssetDatabase.CreateFolder(m_TestFolderPath, "TestFolder");
            string filePath = Path.Combine(folderPath, ".hiddenfile");
            File.Create(filePath).Close();

            List<string> assetPaths = EnumerateAddressableFolder(folderPath, Settings, true);
            Assert.AreEqual(0, assetPaths.Count);

            File.Delete(filePath);
            AssetDatabase.DeleteAsset(folderPath);
        }

        [Test]
        public void WhenNoFolderIsAddressable_EnumerateFiles_ReturnsNothing()
        {
            string parentFolderGuid = AssetDatabase.AssetPathToGUID(m_AddrParentFolderPath);
            string childFolderGuid = AssetDatabase.AssetPathToGUID(m_AddrChildSubfolderPath);
            Settings.RemoveAssetEntry(parentFolderGuid);
            Settings.RemoveAssetEntry(childFolderGuid);

            using (var cache = new AddressablesFileEnumerationCache(Settings, true, null))
            {
                List<string> assetPaths = EnumerateAddressableFolder(m_TestFolderPath, Settings, false);
                Assert.AreEqual(0, assetPaths.Count);
            }

            Settings.CreateOrMoveEntry(parentFolderGuid, m_ParentGroup);
            Settings.CreateOrMoveEntry(childFolderGuid, m_ChildGroup);
        }

        [Test]
        public void WhenAFolderIsAddressable_FileEnumerationCache_CreatesFileTree()
        {
            using (var cache = new AddressablesFileEnumerationCache(Settings, false, null))
            {
                Assert.IsTrue(m_PrecomputedTree != null);
            }
        }

        [Test]
        public void WhenNoFolderIsAddressable_FileEnumerationCache_DoesNotCreateFileTree()
        {
            string parentFolderGuid = AssetDatabase.AssetPathToGUID(m_AddrParentFolderPath);
            string childFolderGuid = AssetDatabase.AssetPathToGUID(m_AddrChildSubfolderPath);
            Settings.RemoveAssetEntry(parentFolderGuid);
            Settings.RemoveAssetEntry(childFolderGuid);

            using (var cache = new AddressablesFileEnumerationCache(Settings, true, null))
            {
                Assert.IsTrue(m_PrecomputedTree == null);
            }

            Settings.CreateOrMoveEntry(parentFolderGuid, m_ParentGroup);
            Settings.CreateOrMoveEntry(childFolderGuid, m_ChildGroup);
        }

        [Test]
        public void WhenPrepopulateAssetsIsTrue_FileEnumerationCache_AddsAllAssetsToTree()
        {
            using (var cache = new AddressablesFileEnumerationCache(Settings, true, null))
            {
                AddressableAssetTree.TreeNode node = m_PrecomputedTree.FindNode(m_ChildObjPath, false);
                Assert.IsTrue(node != null);
            }
        }

        [Test]
        public void WhenPrepopulateAssetsIsFalse_FileEnumerationCache_OnlyAddsAddressablesToTree()
        {
            using (var cache = new AddressablesFileEnumerationCache(Settings, false, null))
            {
                AddressableAssetTree.TreeNode node = m_PrecomputedTree.FindNode(m_ChildObjPath, false);
                Assert.IsTrue(node == null);
            }
        }
    }
}
