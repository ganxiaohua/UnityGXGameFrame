using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests
{
    class AddressableAssetEntryTreeViewTests : EditorAddressableAssetsTestFixture
    {
        const string k_TreeViewTestFolderPath = TempPath + "/TreeViewTests";

        [SetUp]
        public void TestSetup()
        {
            Directory.CreateDirectory(k_TreeViewTestFolderPath);
            ProjectConfigData.HierarchicalSearch = false;
            ProjectConfigData.ShowGroupsAsHierarchy = false;
        }

        [Test]
        public void BuildTree_Structure_WhenOnlyWithDefaultGroups_CreatesBuiltInDataAndDefaultGroups()
        {
            var tree = CreateExpandedTree();
            Assert.AreEqual(2, tree.Root.children.Count);

            var playerDataRow = tree.Root.children.First(c => c.displayName == AddressableAssetSettings.PlayerDataGroupName);
            Assert.AreEqual(2, playerDataRow.children.Count);
            Assert.IsTrue(playerDataRow.children.Any(c => c.displayName == AddressableAssetEntry.EditorSceneListName));
            Assert.IsTrue(playerDataRow.children.Any(c => c.displayName == AddressableAssetEntry.ResourcesName));

            var defaultGroupRow = tree.Root.children.First(c => c.displayName == AddressableAssetSettings.DefaultLocalGroupName + " (Default)");
            Assert.False(defaultGroupRow.hasChildren);
        }

        [Test]
        public void BuildTree_Structure_WhenGroupHierarchyIsEnabled_CreatesHierarchyFromDashesInGroupName()
        {
            var nameWithDashes = "group-name-with-dashes";
            var group = CreateGroup(nameWithDashes);

            ProjectConfigData.ShowGroupsAsHierarchy = true;
            var tree = CreateExpandedTree();

            var parts = nameWithDashes.Split('-');
            TreeViewItem item = tree.Root;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                item = item.children.FirstOrDefault(c => c.displayName == parts[i]);
                Assert.NotNull(item);
            }
            // Last child is the complete name of the group
            Assert.NotNull(item.children.FirstOrDefault(c => c.displayName == nameWithDashes));
        }

        [Test]
        public void BuildTree_Scenes_CreatesAnEntryOnlyForScenesInBuild()
        {
            var sceneGuid1 = CreateScene(k_TreeViewTestFolderPath + "/includedScene1.unity", addToBuild: true);
            var sceneGuid2 = CreateScene(k_TreeViewTestFolderPath + "/excludedScene.unity", addToBuild: false);

            var tree = CreateExpandedTree();

            var scenesListRow = tree.GetRows().First(c => c.displayName == AddressableAssetEntry.EditorSceneListName);
            Assert.AreEqual(1, scenesListRow.children.Count);
            Assert.True(scenesListRow.children.Any(r => r.displayName == GetName(sceneGuid1)));
        }

        [Test]
        public void BuildTree_Resources_CreatesEntriesForAllFilesInResourcesFolders()
        {
            Directory.CreateDirectory(k_TreeViewTestFolderPath + "/Resources");
            Directory.CreateDirectory(k_TreeViewTestFolderPath + "/SubFolder/Resources");
            var resourceGuid1 = CreateAsset(k_TreeViewTestFolderPath + "/Resources/testResource1.prefab");
            var resourceGuid2 = CreateAsset(k_TreeViewTestFolderPath + "/SubFolder/Resources/testResource3.prefab");

            var tree = CreateExpandedTree();

            var resourcesRow = tree.GetRows().First(c => c.displayName == AddressableAssetEntry.ResourcesName);
            var resourcesCount = ResourcesTestUtility.GetResourcesEntryCount(m_Settings, true);
            Assert.AreEqual(resourcesCount, resourcesRow.children.Count);
            Assert.True(resourcesRow.children.Any(r => r.displayName == GetName(resourceGuid1)));
            Assert.True(resourcesRow.children.Any(r => r.displayName == GetName(resourceGuid2)));
        }

        [Test]
        public void BuildTree_Groups_CreatesEntriesForEachOne()
        {
            CreateGroup("testGroup1");
            CreateGroup("testGroup2");
            var tree = CreateExpandedTree();
            Assert.AreEqual(4, tree.Root.children.Count);
        }

        [Test]
        public void BuildTree_Assets_CreatesEntriesForAddressableAssetsOnly()
        {
            var guid1 = CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab");
            var guid2 = CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab");
            MakeAddressable(m_Settings.DefaultGroup, guid1);

            var tree = CreateExpandedTree();

            var defaultGroupRow = tree.Root.children.First(c => c.displayName == AddressableAssetSettings.DefaultLocalGroupName + " (Default)");
            Assert.AreEqual(1, defaultGroupRow.children.Count);
        }

        [Test]
        public void BuildTree_Assets_SpriteAtlas_CreatesEntriesForAtlasAndAllSprites()
        {
            var guid1 = CreateSpriteTexture(k_TreeViewTestFolderPath + "/testTexture1.png");
            var guid2 = CreateSpriteTexture(k_TreeViewTestFolderPath + "/testTexture2.png");
            var atlasPath = k_TreeViewTestFolderPath + "/testAtlas.spriteatlas";
            MakeAddressable(m_Settings.DefaultGroup, CreateSpriteAtlas(atlasPath, new[] {guid1, guid2}));

            var tree = CreateExpandedTree();

            var defaultGroupRow = tree.Root.children.First(c => c.displayName == AddressableAssetSettings.DefaultLocalGroupName + " (Default)");
            Assert.AreEqual(1, defaultGroupRow.children.Count);
            var atlasRow = defaultGroupRow.children.First();
            Assert.AreEqual(2, atlasRow.children.Count);
        }

        [Test]
        public void Search_WhenAssetNameMatches_ReturnsMatchingAssets()
        {
            var searchStr = "matchingStr";
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"), "testAsset1-" + searchStr);
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab"));

            var tree = CreateExpandedTree();
            ProjectConfigData.HierarchicalSearch = false;
            var result = tree.Search(searchStr);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(entry.address, result.First().displayName);
        }

        [Test]
        public void Search_WhenAssetFilePathMatches_ReturnsMatchingAssets()
        {
            var searchStr = "matchingStr";
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + $"/testAsset1-{searchStr}.prefab"));
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab"));

            var tree = CreateExpandedTree();
            ProjectConfigData.HierarchicalSearch = false;
            var result = tree.Search(searchStr);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(entry.address, result.First().displayName);
        }

        [Test]
        public void Search_WhenGroupMatches_ReturnsMatchingGroups()
        {
            var searchStr = "matchingStr";
            var group = CreateGroup("Group-" + searchStr);
            MakeAddressable(group, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"));

            var tree = CreateExpandedTree();
            ProjectConfigData.HierarchicalSearch = false;
            var result = tree.Search(searchStr);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(group.Name, result.First().displayName);
        }

        [Test]
        public void Search_WhenLabelMatch_ReturnsMatchingAssets()
        {
            var searchStr = "matchingStr";
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"));
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testPrefab2.prefab"));
            m_Settings.SetLabelValueForEntries(new List<AddressableAssetEntry>(){entry}, "label-" + searchStr, true, true);

            var tree = CreateExpandedTree();
            ProjectConfigData.HierarchicalSearch = false;
            var result = tree.Search(searchStr);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(entry.address, result.First().displayName);
        }

        [Test]
        public void Search_Hierarchical_WhenAssetNameMatches_ReturnsMatchingAssetsAndContainingGroups()
        {
            var searchStr = "matchingStr";
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"), "testAsset1-" + searchStr);
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab"));

            ProjectConfigData.HierarchicalSearch = true;
            var tree = CreateExpandedTree();
            var result = tree.Search(searchStr);

            Assert.AreEqual(2, result.Count);
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == m_Settings.DefaultGroup.Name + " (Default)"));
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == entry.address));
        }

        [Test]
        public void Search_Hierarchical_WhenAssetFilePathMatches_ReturnsMatchingAssetsAndContainingGroups()
        {
            var searchStr = "matchingStr";
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + $"/testAsset1-{searchStr}.prefab"));
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab"));

            ProjectConfigData.HierarchicalSearch = true;
            var tree = CreateExpandedTree();
            var result = tree.Search(searchStr);

            Assert.AreEqual(2, result.Count);
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == m_Settings.DefaultGroup.Name + " (Default)"));
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == entry.address));
        }

        [Test]
        public void Search_Hierarchical_WhenGroupMatches_ReturnsGroupAndItsAssets()
        {
            var searchStr = "matchingStr";
            var groupName = "Group-" + searchStr;
            var group = CreateGroup(groupName);
            MakeAddressable(group, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"));
            MakeAddressable(group, CreateAsset(k_TreeViewTestFolderPath + "/testAsset2.prefab"));

            ProjectConfigData.HierarchicalSearch = true;
            var tree = CreateExpandedTree();
            var result = tree.Search(searchStr);

            Assert.AreEqual(3, result.Count);
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == groupName));
            foreach (var entry in group.entries)
                Assert.NotNull(result.FirstOrDefault(r => r.displayName == entry.address));
        }

        [Test]
        public void Search_Hierarchical_WhenLabelMatches_ReturnsMatchingAssetsAndContainingGroups()
        {
            var searchStr = "matchingStr";
            MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"));
            var entry = MakeAddressable(m_Settings.DefaultGroup, CreateAsset(k_TreeViewTestFolderPath + "/testPrefab2.prefab"));
            m_Settings.SetLabelValueForEntries(new List<AddressableAssetEntry>(){entry}, "label-" + searchStr, true, true);

            ProjectConfigData.HierarchicalSearch = true;
            var tree = CreateExpandedTree();
            var result = tree.Search(searchStr);

            Assert.AreEqual(2, result.Count);
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == m_Settings.DefaultGroup.Name + " (Default)"));
            Assert.NotNull(result.FirstOrDefault(r => r.displayName == entry.address));
        }

        [Test]
        public void Search_Hierarchical_WithGroupHierarchyEnabled_WhenAssetFilePathMatches_ReturnsMatchingAssetsAndHierarchy()
        {
            var nameWithDashes = "group-name-with-dashes";
            var searchStr = "matchingStr";
            var group = CreateGroup(nameWithDashes);
            var entry = MakeAddressable(group, CreateAsset(k_TreeViewTestFolderPath + "/testAsset1.prefab"), searchStr);
            MakeAddressable(group, CreateAsset(k_TreeViewTestFolderPath + "/testPrefab2.prefab"));

            ProjectConfigData.HierarchicalSearch = true;
            ProjectConfigData.ShowGroupsAsHierarchy = true;
            var tree = CreateExpandedTree();
            var result = tree.Search(searchStr);

            var parts = nameWithDashes.Split('-');
            Assert.AreEqual(parts.Length + 1, result.Count);
            Assert.NotNull(result.FirstOrDefault(c => c.displayName == entry.address));
            for (int i = 0; i < parts.Length - 1; i++)
            {
                Assert.NotNull(result.FirstOrDefault(c => c.displayName == parts[i]));
            }
            // Last child is the full name of the group
            Assert.NotNull(result.FirstOrDefault(c => c.displayName == nameWithDashes));
        }
        
        [Test]
        public void CopyAddressesToClipboard_Simple()
        {
            List<AssetEntryTreeViewItem> nodesToSelect = new List<AssetEntryTreeViewItem>();
            
            AddressableAssetEntry entry1 = new AddressableAssetEntry("0001", "address1", null, false);
            
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry1, 0));

            //Save users previous clipboard so it doesn't get eaten during test
            string previousClipboard = GUIUtility.systemCopyBuffer;
            
            AddressableAssetEntryTreeView.CopyAddressesToClipboard(nodesToSelect);
            
            string result = GUIUtility.systemCopyBuffer;
            GUIUtility.systemCopyBuffer = previousClipboard;
            
            Assert.AreEqual("address1", result, "Entry's address was incorrectly copied.");
        }
        
        [Test]
        public void CopyAddressesToClipboard_Multiple()
        {
            List<AssetEntryTreeViewItem> nodesToSelect = new List<AssetEntryTreeViewItem>();
            
            AddressableAssetEntry entry1 = new AddressableAssetEntry("0001", "address1", null, false);
            AddressableAssetEntry entry2 = new AddressableAssetEntry("0002", "address2", null, false);
            AddressableAssetEntry entry3 = new AddressableAssetEntry("0003", "address3", null, false);
            
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry1, 0));
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry2, 0));
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry3, 0));

            //Save users previous clipboard so it doesn't get eaten during test
            string previousClipboard = GUIUtility.systemCopyBuffer;
            
            AddressableAssetEntryTreeView.CopyAddressesToClipboard(nodesToSelect);
            
            string result = GUIUtility.systemCopyBuffer;
            GUIUtility.systemCopyBuffer = previousClipboard;
            
            Assert.AreEqual("address1,address2,address3", result, "Entry's address was incorrectly copied.");
        }

        [Test]
        public void CopyAddressesToClipboard_MaintainsOrder()
        {
            List<AssetEntryTreeViewItem> nodesToSelect = new List<AssetEntryTreeViewItem>();
            
            AddressableAssetEntry entry1 = new AddressableAssetEntry("0001", "address1", null, false);
            AddressableAssetEntry entry2 = new AddressableAssetEntry("0002", "address2", null, false);
            AddressableAssetEntry entry3 = new AddressableAssetEntry("0003", "address3", null, false);
            
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry2, 0));
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry3, 0));
            nodesToSelect.Add(new AssetEntryTreeViewItem(entry1, 0));

            //Save users previous clipboard so it doesn't get eaten during test
            string previousClipboard = GUIUtility.systemCopyBuffer;
            
            AddressableAssetEntryTreeView.CopyAddressesToClipboard(nodesToSelect);
            
            string result = GUIUtility.systemCopyBuffer;
            GUIUtility.systemCopyBuffer = previousClipboard;
            
            Assert.AreEqual("address2,address3,address1", result, "Entry's address was incorrectly copied.");
        }
        

        List<AddressableAssetEntry> GetAllEntries(bool includeSubObjects = false)
        {
            var entries = new List<AddressableAssetEntry>();
            m_Settings.GetAllAssets(entries, includeSubObjects);
            return entries;
        }

        AddressableAssetEntryTreeView CreateExpandedTree()
        {
            var tree = new AddressableAssetEntryTreeView(m_Settings);
            tree.Reload();
            var count = tree.GetRows().Count;
            tree.ExpandAll();
            while (count != tree.GetRows().Count)
            {
                tree.Reload();
                count = tree.GetRows().Count;
                tree.ExpandAll();
            }

            return tree;
        }

        AddressableAssetEntry MakeAddressable(AddressableAssetGroup group, string guid, string address = null)
        {
            AddressableAssetEntry entry = m_Settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address == null ? Path.GetFileNameWithoutExtension(entry.AssetPath) : address;
            return entry;
        }

        AddressableAssetGroup CreateGroup(string name)
        {
            return m_Settings.CreateGroup(name, false, false, false, null, typeof(BundledAssetGroupSchema));
        }

        string GetName(string assetGuid)
        {
            return Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(assetGuid));
        }
    }
}
