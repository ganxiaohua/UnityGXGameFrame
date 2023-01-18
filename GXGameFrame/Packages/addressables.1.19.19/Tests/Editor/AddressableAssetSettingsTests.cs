using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetSettingsTests : AddressableAssetTestBase
    {
        internal class InitializeScriptable : ScriptableObject, IObjectInitializationDataProvider
        {
            public string Name { get; }
            public ObjectInitializationData CreateObjectInitializationData()
            {
                return new ObjectInitializationData();
            }
        }

        internal class GroupTemplateTestObj : IGroupTemplate
        {
            public string Name { get; }
            public string Description { get; }
        }

        internal class InitializationObejctTest : IObjectInitializationDataProvider
        {
            public string Name { get; }
            public ObjectInitializationData CreateObjectInitializationData()
            {
                return new ObjectInitializationData();
            }
        }

        internal class DataBuilderTest : IDataBuilder
        {
            public string Name { get; set; }
            public bool CacheCleared = false;
            public bool CanBuildData<T>() where T : IDataBuilderResult
            {
                return true;
            }

            public TResult BuildData<TResult>(AddressablesDataBuilderInput builderInput) where TResult : IDataBuilderResult
            {
                return default(TResult);
            }

            public void ClearCachedData()
            {
                CacheCleared = true;
            }

            public string Description { get; }
        }

        public void SetupEntries(ref List<AddressableAssetEntry> entries, int numEntries)
        {
            var testObject = new GameObject("TestObjectSetLabel");
            PrefabUtility.SaveAsPrefabAsset(testObject, ConfigFolder + "/testasset.prefab");
            var testAssetGUID = AssetDatabase.AssetPathToGUID(ConfigFolder + "/testasset.prefab");
            entries.Add(Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName)));
            for (int i = 0; i <= numEntries; i++)
                entries.Add(Settings.CreateOrMoveEntry(testAssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName)));
        }

        [Test]
        public void GetDefaultGroupDoesNotThrowNullExceptionWhenGroupsNull()
        {
            Settings.groups.Insert(0,null);
            Assert.IsNotNull(Settings.DefaultGroup);
            Settings.groups.RemoveAt(0);
        }
        
        [Test]
        public void HasDefaultInitialGroups()
        {
            Assert.IsNotNull(Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            Assert.IsNotNull(Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName));
        }

        [Test]
        public void AddRemovelabel()
        {
            const string labelName = "Newlabel";
            Settings.AddLabel(labelName);
            Assert.Contains(labelName, Settings.labelTable.labelNames);
            Settings.RemoveLabel(labelName);
            Assert.False(Settings.labelTable.labelNames.Contains(labelName));
        }

        [Test]
        public void RenameLabel_KeepsIndexTheSame_ForNewTableEntry()
        {
            string dummyLabel1, dummyLabel2, dummyLabel3;
            dummyLabel3 = dummyLabel2 = dummyLabel1 = "dummylabel";
            string replaceMe = "replaceme";
            string useMeToReplace = "usemetoreplace";
            Settings.AddLabel(dummyLabel1);
            Settings.AddLabel(dummyLabel2);
            Settings.AddLabel(replaceMe);
            Settings.AddLabel(dummyLabel3);

            int startIndex = Settings.labelTable.GetIndexOfLabel(replaceMe);

            Settings.RenameLabel(replaceMe, useMeToReplace);

            int endIndex = Settings.labelTable.GetIndexOfLabel(useMeToReplace);

            Assert.AreEqual(startIndex, endIndex);

            Settings.RemoveLabel(dummyLabel1);
            Settings.RemoveLabel(dummyLabel2);
            Settings.RemoveLabel(dummyLabel3);
            Settings.RemoveLabel(useMeToReplace);
        }

        [Test]
        public void RenameLabel_UpdatesLabelList_WithCorrectLabels()
        {
            string replaceMe = "replaceme";
            string useMeToReplace = "usemetoreplace";
            Settings.AddLabel(replaceMe);

            Settings.RenameLabel(replaceMe, useMeToReplace);

            Assert.IsFalse(Settings.GetLabels().Contains(replaceMe));
            Assert.IsTrue(Settings.GetLabels().Contains(useMeToReplace));
            
            Settings.RemoveLabel(useMeToReplace);
        }

        [Test]
        public void RenameLabel_UpdatesAssetEntries_ThatContainUsesOfTheOldLabels()
        {
            string replaceMe = "replaceme";
            string useMeToReplace = "usemetoreplace";
            Settings.AddLabel(replaceMe);
            var assetEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.DefaultGroup);
            assetEntry.SetLabel(replaceMe, true);

            Settings.RenameLabel(replaceMe, useMeToReplace);

            Assert.IsTrue(assetEntry.labels.Contains(useMeToReplace));
            Assert.IsFalse(assetEntry.labels.Contains(replaceMe));

            Settings.RemoveAssetEntry(assetEntry);
            Settings.RemoveLabel(useMeToReplace);
        }

        [Test]
        public void GetLabels_ShouldReturnCopy()
        {
            const string labelName = "Newlabel";
            Settings.AddLabel("label_1");
            Settings.AddLabel("label_2");

            var labels = Settings.GetLabels();
            labels.Add(labelName);

            Assert.AreEqual(3, labels.Count);
            Assert.AreEqual(2, Settings.labelTable.labelNames.Count);
            Assert.IsFalse(Settings.labelTable.labelNames.Contains(labelName));
        }

        [Test]
        public void WhenLabelNameHasSquareBrackets_AddingNewLabel_ThrowsError()
        {
            string name = "[label]";
            Settings.AddLabel(name);
            LogAssert.Expect(LogType.Error, $"Label name '{name}' cannot contain '[ ]'.");
        }

        [Test]
        public void AddRemoveGroup()
        {
            const string groupName = "NewGroup";
            var group = Settings.CreateGroup(groupName, false, false, false, null);
            Assert.IsNotNull(group);
            Settings.RemoveGroup(group);
            Assert.IsNull(Settings.FindGroup(groupName));
        }

        [Test]
        public void RemoveMissingGroupsReferences_CheckGroupCount()
        {
            var size = Settings.groups.Count;
            var x = Settings.groups[size - 1];
            Settings.groups[size - 1] = null;
            bool b = Settings.RemoveMissingGroupReferences();
            Assert.AreEqual(Settings.groups.Count + 1, size);
            Settings.groups.Add(x);
            LogAssert.Expect(LogType.Log, "Addressable settings contains 1 group reference(s) that are no longer there. Removing reference(s).");
        }

        [Test]
        public void CanCreateAssetReference()
        {
            AssetReference testReference = Settings.CreateAssetReference(m_AssetGUID);
            Assert.NotNull(testReference);
            var entry = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName).GetAssetEntry(m_AssetGUID);
            Assert.AreSame(testReference.AssetGUID, entry.guid);
        }

        [Test]
        public void CreateUpdateNewEntry()
        {
            var group = Settings.CreateGroup("NewGroupForCreateOrMoveEntryTest", false, false, false, null);
            Assert.IsNotNull(group);
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, group);
            Assert.IsNotNull(entry);
            Assert.AreSame(group, entry.parentGroup);
            var localDataGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            Assert.IsNotNull(localDataGroup);
            entry = Settings.CreateOrMoveEntry(m_AssetGUID, localDataGroup);
            Assert.IsNotNull(entry);
            Assert.AreNotSame(group, entry.parentGroup);
            Assert.AreSame(localDataGroup, entry.parentGroup);
            Settings.RemoveGroup(group);
            localDataGroup.RemoveAssetEntry(entry);
            var tmp = Settings.FindAssetEntry(entry.guid);
            Assert.IsNull(Settings.FindAssetEntry(entry.guid));
        }

        [Test]
        public void CreateOrMoveEntries_CreatesNewEntries()
        {
            string guid1 = "guid1";
            string guid2 = "guid2";
            string guid3 = "guid3";

            Settings.CreateOrMoveEntries(new List<string>() { guid1, guid2, guid3 }, Settings.DefaultGroup, 
                new List<AddressableAssetEntry>(), 
                new List<AddressableAssetEntry>());

            Assert.IsNotNull(Settings.FindAssetEntry(guid1));
            Assert.IsNotNull(Settings.FindAssetEntry(guid2));
            Assert.IsNotNull(Settings.FindAssetEntry(guid3));

            Settings.RemoveAssetEntry(guid1);
            Settings.RemoveAssetEntry(guid2);
            Settings.RemoveAssetEntry(guid3);
        }

        [Test]
        public void CreateOrMoveEntries_MovesEntriesThatAlreadyExist()
        {
            string guid1 = "guid1";
            string guid2 = "guid2";
            string guid3 = "guid3";
            var group = Settings.CreateGroup("SeparateGroup", false, false, true, new List<AddressableAssetGroupSchema>());
            group.AddAssetEntry(Settings.CreateEntry(guid1, "addr1", group,false));
            group.AddAssetEntry(Settings.CreateEntry(guid2, "addr2", group,false));
            group.AddAssetEntry(Settings.CreateEntry(guid3, "addr3", group,false));

            Settings.CreateOrMoveEntries(new List<string>() { guid1, guid2, guid3 }, Settings.DefaultGroup, 
                new List<AddressableAssetEntry>(), 
                new List<AddressableAssetEntry>());

            Assert.IsNull(group.GetAssetEntry(guid1));
            Assert.IsNull(group.GetAssetEntry(guid2));
            Assert.IsNull(group.GetAssetEntry(guid3));

            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid1));
            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid2));
            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid3));

            Settings.RemoveGroup(group);
        }

        [Test]
        public void CreateOrMoveEntries_Creates_AndMovesExistingEntries_InMixedLists()
        {
            string guid1 = "guid1";
            string guid2 = "guid2";
            string guid3 = "guid3";
            var group = Settings.CreateGroup("SeparateGroup", false, false, true, new List<AddressableAssetGroupSchema>());
            group.AddAssetEntry(Settings.CreateEntry(guid1, "addr1", group,false));
            group.AddAssetEntry(Settings.CreateEntry(guid3, "addr3", group,false));

            Settings.CreateOrMoveEntries(new List<string>() { guid1, guid2, guid3 }, Settings.DefaultGroup, 
                new List<AddressableAssetEntry>(), 
                new List<AddressableAssetEntry>());

            Assert.IsNull(group.GetAssetEntry(guid1));
            Assert.IsNull(group.GetAssetEntry(guid3));

            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid1));
            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid2));
            Assert.IsNotNull(Settings.DefaultGroup.GetAssetEntry(guid3));

            Settings.RemoveGroup(group);
        }

        [Test]
        public void CannotCreateOrMoveWithoutGuid()
        {
            Assert.IsNull(Settings.CreateOrMoveEntry(null, Settings.DefaultGroup));
            Assert.IsNull(Settings.CreateSubEntryIfUnique(null, "", null));
        }

        [Test]
        public void FindAssetEntry()
        {
            var localDataGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            Assert.IsNotNull(localDataGroup);
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, localDataGroup);
            var foundEntry = Settings.FindAssetEntry(m_AssetGUID);
            Assert.AreSame(entry, foundEntry);
        }

        [Test]
        public void FindAssetEntry_IncludeImplicitIsTrue_ReturnsImplicitEntries()
        {
            var folderPath = GetAssetPath("aaFolder");
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
            var folderGuid = AssetDatabase.AssetPathToGUID(folderPath);
            Assert.IsFalse(string.IsNullOrEmpty(folderGuid));

            var asset1GUID = CreateAsset(Path.Combine(folderPath, "asset1.prefab").Replace('\\', '/'));
            var folderEntry = Settings.CreateOrMoveEntry(folderGuid, Settings.DefaultGroup);
            Assert.IsNotNull(folderEntry);

            var foundEntry = Settings.FindAssetEntry(asset1GUID, false);
            Assert.IsNull(foundEntry);

            foundEntry = Settings.FindAssetEntry(asset1GUID, true);
            Assert.AreEqual(AssetDatabase.GUIDToAssetPath(asset1GUID), foundEntry.AssetPath);

            Directory.Delete(folderPath, true);
            Settings.RemoveAssetEntry(folderEntry, false);
        }

        [Test]
        public void AddressablesClearCachedData_DoesNotThrowError()
        {
            //individual clean paths
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Assert.DoesNotThrow(() => Settings.CleanPlayerContentImpl(db));
            }

            //Clean all path
            Assert.DoesNotThrow(() => Settings.CleanPlayerContentImpl());

            //Cleanup
            Settings.BuildPlayerContentImpl();
        }

        [Test]
        public void AddressablesCleanCachedData_ClearsData()
        {
            //Setup
            Settings.BuildPlayerContentImpl();

            //Check after each clean that the data is not built
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Settings.CleanPlayerContentImpl(db);
                Assert.IsFalse(db.IsDataBuilt());
            }
        }

        [Test]
        public void AddressablesCleanAllCachedData_ClearsAllData()
        {
            //Setup
            Settings.BuildPlayerContentImpl();

            //Clean ALL data builders
            Settings.CleanPlayerContentImpl();

            //Check none have data built
            foreach (ScriptableObject so in Settings.DataBuilders)
            {
                BuildScriptBase db = so as BuildScriptBase;
                Assert.IsFalse(db.IsDataBuilt());
            }
        }

        [Test]
        public void DeletingAsset_DoesNotDeleteGroupWithSimilarName()
        {
            //Setup
            const string groupName = "NewAsset.mat";
            string assetPath = GetAssetPath(groupName);


            var mat = new Material(Shader.Find("Unlit/Color"));
            AssetDatabase.CreateAsset(mat, assetPath);

            var group = Settings.CreateGroup(groupName, false, false, false, null);
            Assert.IsNotNull(group);

            //Test
            AssetDatabase.DeleteAsset(assetPath);

            //Assert
            Settings.CheckForGroupDataDeletion(groupName);
            Assert.IsNotNull(Settings.FindGroup(groupName));

            //Clean up
            Settings.RemoveGroup(group);
            Assert.IsNull(Settings.FindGroup(groupName));
        }

        [Test]
        public void Settings_WhenActivePlayerDataBuilderIndexSetWithSameValue_DoesNotDirtyAsset()
        {
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.ActivePlayerDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex;
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreEqual(prevDC, dc);
        }

        [Test]
        public void Settings_WhenActivePlayerDataBuilderIndexSetWithDifferentValue_DoesDirtyAsset()
        {
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.ActivePlayerDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex + 1;
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreEqual(prevDC + 1, dc);
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_AddDeleteGroupTriggersSettingsSave()
        {
            // Setup
            var importedAssets = new string[1];
            var deletedAssets = new string[0];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            var newGroup = ScriptableObject.CreateInstance<AddressableAssetGroup>();
            newGroup.Name = "testGroup";
            var groupPath = ConfigFolder + "/AssetGroups/" + newGroup.Name + ".asset";
            AssetDatabase.CreateAsset(newGroup, groupPath);
            newGroup.Initialize(ScriptableObject.CreateInstance<AddressableAssetSettings>(), "testGroup", AssetDatabase.AssetPathToGUID(groupPath), false);
            importedAssets[0] = groupPath;
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC + 1, EditorUtility.GetDirtyCount(Settings));
            Assert.IsTrue(EditorUtility.IsDirty(Settings));

            deletedAssets = new string[1];
            importedAssets = new string[0];
            deletedAssets[0] = groupPath;
            EditorUtility.ClearDirty(Settings);
            prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC + 2, EditorUtility.GetDirtyCount(Settings));
            Assert.IsTrue(EditorUtility.IsDirty(Settings));
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_DeleteAssetToNullNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[1];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            Settings.groups.Add(null);
            Settings.DataBuilders.Add(null);
            Settings.GroupTemplateObjects.Add(null);
            Settings.InitializationObjects.Add(null);
            deletedAssets[0] = "";
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_ChangeImportedAssetsDoesNotTriggerSettingsSave()
        {
            var importedAssets = new string[1];
            var deletedAssets = new string[0];
            var movedAssets = new string[0];
            var movedFromAssetPaths = new string[0];
            var entry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.groups[0]);
            var prevTestObjName = entry.MainAsset.name;
            entry.MainAsset.name = "test";
            importedAssets[0] = TestFolder + "/test.prefab";
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));
            entry.MainAsset.name = prevTestObjName;
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_MovedGroupNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[0];
            var movedAssets = new string[1];
            var movedFromAssetPaths = new string[1];
            var newGroup = ScriptableObject.CreateInstance<AddressableAssetGroup>();
            newGroup.Name = "testGroup";
            var groupPath = ConfigFolder + "/AssetGroups/" + newGroup.Name + ".asset";
            AssetDatabase.CreateAsset(newGroup, groupPath);
            newGroup.Initialize(ScriptableObject.CreateInstance<AddressableAssetSettings>(), "testGroup", AssetDatabase.AssetPathToGUID(groupPath), false);
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.groups.Add(newGroup);
            string newGroupPath = ConfigFolder + "/AssetGroups/changeGroup.asset";
            AssetDatabase.MoveAsset(groupPath, newGroupPath);
            movedAssets[0] = newGroupPath;
            movedFromAssetPaths[0] = groupPath;

            // Test
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));

            //Cleanup
            Settings.RemoveGroup(newGroup);
        }

        [Test]
        public void AddressableAssetSettings_OnPostprocessAllAssets_MovedAssetToResourcesNotTriggerSettingsSave()
        {
            // Setup
            var importedAssets = new string[0];
            var deletedAssets = new string[0];
            var movedAssets = new string[1];
            var movedFromAssetPaths = new string[1];
            var assetPath = TestFolder + "/test.prefab";
            var newAssetPath = TestFolder + "/resources/test.prefab";
            if (!Directory.Exists(TestFolder + "/resources"))
            {
                Directory.CreateDirectory(TestFolder + "/resources");
                AssetDatabase.Refresh();
            }
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newAssetPath), Settings.groups[0]);
            movedAssets[0] = newAssetPath;
            movedFromAssetPaths[0] = assetPath;
            EditorUtility.ClearDirty(Settings);
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            Settings.OnPostprocessAllAssets(importedAssets, deletedAssets, movedAssets, movedFromAssetPaths);
            Assert.AreEqual(prevDC, EditorUtility.GetDirtyCount(Settings));
            Assert.IsFalse(EditorUtility.IsDirty(Settings));

            // Cleanup
            AssetDatabase.MoveAsset(newAssetPath, assetPath);
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), Settings.groups[0]);
            Directory.Delete(TestFolder + "/resources");
        }

        [Test]
        public void AddressableAssetSettings_ActivePlayerDataBuilderIndex_CanGetActivePlayModeDataBuilderIndex()
        {
            Assert.NotNull(Settings.ActivePlayerDataBuilderIndex);
        }

        [Test]
        public void AddressableAssetSettings_ActivePlayerDataBuilderIndex_CanSetActivePlayModeDataBuilderIndex()
        {
            var prevActivePlayModeDataBuilderIndex = Settings.ActivePlayerDataBuilderIndex;
            Settings.ActivePlayerDataBuilderIndex = 1;
            Assert.AreNotEqual(prevActivePlayModeDataBuilderIndex, Settings.ActivePlayerDataBuilderIndex);
            Settings.ActivePlayerDataBuilderIndex = prevActivePlayModeDataBuilderIndex;
        }

#pragma warning disable 618
        [Test]
        public void AddressableAssetSettings_RemoveSchemaTemplate_CannotRemoveGroupSchemaTemplate()
        {
            LogAssert.Expect(LogType.Error, "GroupSchemaTemplates are deprecated, use GroupTemplateObjects");
            Settings.RemoveSchemaTemplate(0);
        }

#pragma warning restore 618

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CanGetGroupTemplateObject()
        {
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
        }

        [Test]
        public void AddressableAssetSettings_AddGroupTemplateObject_CanAddGroupTemplateObject()
        {
            var template = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            template.name = "testGroup";
            Settings.AddGroupTemplateObject(template);
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
            Assert.Greater(Settings.GroupTemplateObjects.Count, 1);
            Assert.AreEqual(groupTemplate.Name, template.name);
            Assert.AreSame(groupTemplate, template);

            Assert.IsTrue(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", typeof(BundledAssetGroupSchema)));
        }

        [Test]
        public void AddressableAssetSettings_SetGroupTemplateObjectAtIndex_CanSetGroupTemplateObject()
        {
            // Setup
            var testTemplate1 = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            var testTemplate2 = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            var template = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            testTemplate1.name = "test1";
            testTemplate2.name = "test2";
            template.name = "testGroupIndex";
            var restoredObjects = new List<ScriptableObject>(Settings.GroupTemplateObjects);
            Settings.AddGroupTemplateObject(testTemplate1);
            Settings.AddGroupTemplateObject(testTemplate2);
            var saveTemplate = Settings.GetGroupTemplateObject(0);
            var checkUnchangedTemplate = Settings.GetGroupTemplateObject(1);

            // Test
            Settings.SetGroupTemplateObjectAtIndex((Settings.GroupTemplateObjects.Count - 1), template, true);
            Settings.SetGroupTemplateObjectAtIndex(0, template, true);
            var groupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(template, groupTemplate);
            groupTemplate = Settings.GetGroupTemplateObject(0);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(template, groupTemplate);
            groupTemplate = Settings.GetGroupTemplateObject(1);
            Assert.NotNull(groupTemplate);
            Assert.AreSame(checkUnchangedTemplate, groupTemplate);

            /* Cleanup
             * Restore GroupTemplateObjects
             */
            Settings.RemoveGroupTemplateObject(2, true);
            Settings.RemoveGroupTemplateObject(1, true);
            Settings.SetGroupTemplateObjectAtIndex(0, saveTemplate, true);
            Assert.AreEqual(restoredObjects.Count, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(restoredObjects[0], Settings.GroupTemplateObjects[0]);
        }

        [Test]
        public void AddressableAssetSettings_AddGroupTemplateObject_CannotAddInvalidGroupTemplateObject()
        {
            int currentGroupTemplateCount = Settings.GroupTemplateObjects.Count;
            var unchangedGroupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            ScriptableObject groupTemplate = null;
            Assert.IsFalse(Settings.AddGroupTemplateObject(groupTemplate as IGroupTemplate));
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));

            var groupTemplateTest = new GroupTemplateTestObj();
            Assert.IsFalse(Settings.AddGroupTemplateObject(groupTemplateTest as IGroupTemplate));
            Assert.AreEqual(currentGroupTemplateCount, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_CreateAndAddGroupTemplate_CannotAddInvalidGroupTemplateObject()
        {
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate(null, "test template function", null));
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", new Type[0]));
            var testParams = new Type[1];
            testParams[0] = null;
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", testParams));
            testParams[0] = typeof(ScriptableObject);
            Assert.IsFalse(Settings.CreateAndAddGroupTemplate("testCreatAndAdd", "test template function", testParams));
        }

        [Test]
        public void AddressableAssetSettings_SetGroupTemplateObjectAtIndex_CannotSetInvalidGroupTemplateObject()
        {
            int currentGroupTemplateCount = Settings.GroupTemplateObjects.Count;
            var unchangedGroupTemplate = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            ScriptableObject groupTemplate = null;
            Assert.IsFalse(Settings.SetGroupTemplateObjectAtIndex(Settings.GroupTemplateObjects.Count - 1, groupTemplate as IGroupTemplate));
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));

            var groupTemplateTest = new GroupTemplateTestObj();
            Assert.IsFalse(Settings.SetGroupTemplateObjectAtIndex(Settings.GroupTemplateObjects.Count - 1, groupTemplateTest as IGroupTemplate));
            Assert.AreEqual(currentGroupTemplateCount, Settings.GroupTemplateObjects.Count);
            Assert.AreSame(unchangedGroupTemplate, Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_RemoveGroupTemplateObject_CannotRemoveNonExistentGroupTemplateObject()
        {
            Assert.IsFalse(Settings.RemoveGroupTemplateObject(Settings.GroupTemplateObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CannotGetNonExistentGroupTemplateObject()
        {
            Assert.IsNull(Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count));
        }

        internal static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        [Test]
        public void AddressableAssetSettings_GetGroupTemplateObject_CannotGetFromEmptyGroupTemplateObjectList()
        {
            var testSettings = new AddressableAssetSettings();
            while (!IsNullOrEmpty(testSettings.GroupTemplateObjects))
            {
                testSettings.RemoveGroupTemplateObject(0);
            }
            Assert.AreEqual(0, testSettings.GroupTemplateObjects.Count);
            Assert.IsNull(testSettings.GetGroupTemplateObject(1));
        }

        [Test]
        public void AddressableAssetSettings_AddDataBuilder_CanAddDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;

            // Test
            Assert.IsTrue(Settings.AddDataBuilder(testBuilder as IDataBuilder));
            Assert.AreEqual(buildersCount + 1, Settings.DataBuilders.Count);
            Assert.AreEqual(Settings.DataBuilders[Settings.DataBuilders.Count - 1], testBuilder);
        }

        [Test]
        public void AddressableAssetSettings_RemoveDataBuilder_CanRemoveDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;
            Settings.AddDataBuilder(testBuilder as IDataBuilder);

            // Test
            Assert.IsTrue(Settings.RemoveDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.AreEqual(buildersCount, Settings.DataBuilders.Count);
            Assert.AreEqual(lastBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetDataBuilder_CanSetDataBuilder()
        {
            var testBuilder = Settings.GetDataBuilder(0);
            var testBuilderTwo = Settings.GetDataBuilder(1);
            var lastBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            int buildersCount = Settings.DataBuilders.Count;
            Settings.AddDataBuilder(testBuilder as IDataBuilder);

            // Test
            Assert.IsTrue(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testBuilderTwo));
            Assert.AreEqual(Settings.GetDataBuilder(Settings.DataBuilders.Count - 1), testBuilderTwo);

            //Cleanup
            Assert.IsTrue(Settings.RemoveDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.AreEqual(buildersCount, Settings.DataBuilders.Count);
            Assert.AreEqual(lastBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_AddDataBuilder_CannotAddInvalidDataBuilders()
        {
            int currentDataBuildersCount = Settings.DataBuilders.Count;
            var unchangedDataBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            ScriptableObject testBuilder = null;
            Assert.IsFalse(Settings.AddDataBuilder(testBuilder as IDataBuilder));
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));

            var testDataBuilder = new DataBuilderTest();
            Assert.IsFalse(Settings.AddDataBuilder(testDataBuilder as IDataBuilder));
            Assert.AreEqual(currentDataBuildersCount, Settings.DataBuilders.Count);
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetDataBuilderAtIndex_CannotSetInvalidDataBuilders()
        {
            int currentDataBuildersCount = Settings.DataBuilders.Count;
            var unchangedDataBuilder = Settings.GetDataBuilder(Settings.DataBuilders.Count - 1);
            ScriptableObject testBuilder = null;
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testBuilder as IDataBuilder));
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));

            var testDataBuilder = new DataBuilderTest();
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(Settings.DataBuilders.Count - 1, testDataBuilder as IDataBuilder));
            Assert.AreEqual(currentDataBuildersCount, Settings.DataBuilders.Count);
            Assert.AreSame(unchangedDataBuilder, Settings.GetDataBuilder(Settings.DataBuilders.Count - 1));
            Assert.IsFalse(Settings.SetDataBuilderAtIndex(5, testDataBuilder));
        }

        [Test]
        public void AddressableAssetSettings_GetDataBuilder_CannotGetNonExistentDataBuilder()
        {
            Assert.IsNull(Settings.GetDataBuilder(Settings.DataBuilders.Count));
        }

        [Test]
        public void AddressableAssetSettings_RemoveDataBuilder_CannotRemoveNonExistentDataBuilder()
        {
            Assert.IsFalse(Settings.RemoveDataBuilder(Settings.DataBuilders.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetDataBuilder_CannotGetDataBuilderFromEmpty()
        {
            var testSettings = new AddressableAssetSettings();
            while (!IsNullOrEmpty(testSettings.DataBuilders))
            {
                testSettings.RemoveDataBuilder(0);
            }
            Assert.AreEqual(0, testSettings.DataBuilders.Count);
            Assert.IsNull(testSettings.GetDataBuilder(1));
        }

        [Test]
        public void AddressableAssetSettings_AddInitializationObject_CanAddInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;

            // Test
            Assert.IsTrue(Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider));
            Assert.AreEqual(initObjectsCount + 1, Settings.InitializationObjects.Count);
            Assert.AreEqual(Settings.InitializationObjects[Settings.InitializationObjects.Count - 1], testInitObject);

            // Cleanup
            Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1);
        }

        [Test]
        public void AddressableAssetSettings_SetInitializationObject_CanSetInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);

            // Test
            Assert.IsTrue(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, testInitObjectTwo as IObjectInitializationDataProvider));
            Assert.AreEqual(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1), testInitObjectTwo);

            // Cleanup
            Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1);
        }

        [Test]
        public void AddressableAssetSettings_RemoveInitializationObject_CanRemoveInitializationObject()
        {
            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            var testInitObjectTwo = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObjectTwo.name = "testObjTwo";
            var lastInitObject = Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1);
            int initObjectsCount = Settings.InitializationObjects.Count;
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);

            /* Cleanup */
            Assert.IsTrue(Settings.RemoveInitializationObject(Settings.InitializationObjects.Count - 1));
            Assert.AreEqual(initObjectsCount, Settings.InitializationObjects.Count);
            Assert.AreEqual(lastInitObject, Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_AddInitializationObject_CannotAddInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            ScriptableObject initObject = null;
            Assert.IsFalse(Settings.AddInitializationObject(initObject as IObjectInitializationDataProvider));
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));

            var initTestObject = new InitializationObejctTest();
            Assert.IsFalse(Settings.AddInitializationObject(initTestObject as IObjectInitializationDataProvider));
            Assert.AreEqual(currentInitObjectsCount , Settings.InitializationObjects.Count);
            Assert.IsNull(Settings.GetDataBuilder(Settings.InitializationObjects.Count - 1));
        }

        [Test]
        public void AddressableAssetSettings_SetInitializationObjectAtIndex_CannotSetInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            ScriptableObject initObject = null;
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, initObject as IObjectInitializationDataProvider));
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));

            var initTestObject = new InitializationObejctTest();
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(Settings.InitializationObjects.Count - 1, initTestObject as IObjectInitializationDataProvider));

            var testInitObject = ScriptableObject.CreateInstance<InitializeScriptable>();
            testInitObject.name = "testObj";
            Settings.AddInitializationObject(testInitObject as IObjectInitializationDataProvider);
            Assert.IsFalse(Settings.SetInitializationObjectAtIndex(2, testInitObject));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetInvalidInitializationObject()
        {
            int currentInitObjectsCount = Settings.InitializationObjects.Count;
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count - 1));
            Assert.IsNull(Settings.GetInitializationObject(-1));
        }

        [Test]
        public void AddressableAssetSettings_RemoveInitializationObject_CannotRemoveNonExistentInitializationObject()
        {
            Assert.IsFalse(Settings.RemoveInitializationObject(Settings.InitializationObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetNonExistentInitializationObject()
        {
            Assert.IsNull(Settings.GetInitializationObject(Settings.InitializationObjects.Count));
        }

        [Test]
        public void AddressableAssetSettings_GetInitializationObject_CannotGetInitializationObjectFromEmptyList()
        {
            var testSettings = new AddressableAssetSettings();
            while (!IsNullOrEmpty(testSettings.InitializationObjects))
            {
                testSettings.RemoveInitializationObject(0);
            }
            Assert.AreEqual(0, testSettings.InitializationObjects.Count);
            Assert.IsNull(testSettings.GetInitializationObject(1));
        }

        [Test]
        public void AddressableAssetSettings_MoveAssetsFromResources_CanMoveAssetsFromResources()
        {
            // Setup
            var testGuidsToPaths = new Dictionary<string, string>();
            var testObject = new GameObject("TestObjectMoveAssets");
            PrefabUtility.SaveAsPrefabAsset(testObject, ConfigFolder + "/testasset.prefab");
            var testAssetGUID = AssetDatabase.AssetPathToGUID(ConfigFolder + "/testasset.prefab");
            Settings.CreateOrMoveEntry(testAssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var originalAssetEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var prevAddress = originalAssetEntry.address;
            var prevDC = EditorUtility.GetDirtyCount(Settings);
            var prevGroup = originalAssetEntry.parentGroup;
            var prevPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var prevPathTwo = AssetDatabase.GUIDToAssetPath(testAssetGUID);
            var testGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            var testAssetPath = TestFolder + "/testMoveAssets";
            testGuidsToPaths[m_AssetGUID] = testAssetPath + "/resources/test.prefab";
            testGuidsToPaths[testAssetGUID] = testAssetPath + "/resources/testasset.prefab";

            // Test
            Settings.MoveAssetsFromResources(testGuidsToPaths, testGroup);
            var dc = EditorUtility.GetDirtyCount(Settings);
            Assert.AreNotEqual(prevPath, AssetDatabase.GUIDToAssetPath(m_AssetGUID));
            Assert.AreNotEqual(prevPathTwo, AssetDatabase.GUIDToAssetPath(testAssetGUID));
            Assert.AreNotEqual(prevGroup, Settings.FindAssetEntry(m_AssetGUID).parentGroup);
            Assert.AreNotEqual(prevGroup, Settings.FindAssetEntry(testAssetGUID).parentGroup);
            Assert.AreEqual(prevDC + 1, dc);

            testGuidsToPaths[m_AssetGUID] = TestFolder + "/test.prefab";
            testGuidsToPaths[testAssetGUID] = TestFolder + "/testasset.prefab";
            Settings.MoveAssetsFromResources(testGuidsToPaths, prevGroup);
            originalAssetEntry = Settings.FindAssetEntry(m_AssetGUID);
            Assert.AreEqual(originalAssetEntry.address, "test");

            //Cleanup
            originalAssetEntry.address = prevAddress;
            if (Directory.Exists(testAssetPath))
                AssetDatabase.DeleteAsset(testAssetPath);
            EditorBuildSettings.RemoveConfigObject(testAssetPath);
        }

        [Test]
        public void AddressableAssetSettings_MoveAssetsFromResources_AddressRespectFolderHierarchy()
        {
            // Setup
            var guidsToPaths = new Dictionary<string, string>();
            var obj = new GameObject("TestObjectMoveAssets");

            var objFolder = TestFolder + "/Resources/subfolder/subsubfolder";
            if (!Directory.Exists(objFolder))
            {
                Directory.CreateDirectory(objFolder);
            }

            var objPath = objFolder + "/testasset.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, objPath);

            var guid = AssetDatabase.AssetPathToGUID(objPath);
            var playerDataGroup = Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
            Settings.CreateOrMoveEntry(guid, playerDataGroup);

            var destinationFolder = TestFolder + "/testMoveAssets";
            guidsToPaths[guid] = destinationFolder + "/testasset.prefab";

            // Test
            var defaultLocalGroup = Settings.FindGroup(AddressableAssetSettings.DefaultLocalGroupName);
            Settings.MoveAssetsFromResources(guidsToPaths, defaultLocalGroup);

            var idx = objPath.ToLower().LastIndexOf("resources/");
            var expectedAddress = objPath.Substring(idx + 10).Replace(Path.GetExtension(objPath), "");
            var entry = Settings.FindAssetEntry(guid);
            Assert.AreEqual(expectedAddress, entry.address);
            Assert.AreEqual(guidsToPaths[guid], AssetDatabase.GUIDToAssetPath(guid));
            Assert.AreEqual(defaultLocalGroup, entry.parentGroup);

            //Cleanup
            if (Directory.Exists(TestFolder + "/Resources"))
                AssetDatabase.DeleteAsset(TestFolder + "/Resources");
            EditorBuildSettings.RemoveConfigObject(TestFolder + "/Resources");

            Settings.RemoveAssetEntry(guid);
            AssetDatabase.DeleteAsset(guidsToPaths[guid]);
            if (Directory.Exists(destinationFolder))
                AssetDatabase.DeleteAsset(destinationFolder);
            EditorBuildSettings.RemoveConfigObject(destinationFolder);
        }

        [Test]
        public void AddressableAssetSettings_MoveAssetsFromResources_CannotMoveNullOrInvalidAsset()
        {
            Settings.MoveAssetsFromResources(null, null);
            var testAssetEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName));
            var currentGroup = testAssetEntry.parentGroup;
            var testGuidsToPaths = new Dictionary<string, string>();
            var currentPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            var newAssetPath = TestFolder + "/testMoveAssets";
            testGuidsToPaths[m_AssetGUID] = newAssetPath + "/test.prefab";
            Settings.MoveAssetsFromResources(testGuidsToPaths, null);
            Settings.MoveEntry(testAssetEntry, null);
            Assert.AreEqual(currentPath, AssetDatabase.GUIDToAssetPath(m_AssetGUID));
            Assert.AreEqual(currentGroup, Settings.FindAssetEntry(m_AssetGUID).parentGroup);
        }

        [Test]
        public void AddressableAssetSettings_SetMaxConcurrentWebRequests_CanSet()
        {
            Settings.MaxConcurrentWebRequests = 100;
            Assert.AreEqual(100, Settings.MaxConcurrentWebRequests);
        }

        [Test]
        public void AddressableAssetSettings_SetMaxConcurrentWebRequestsWithValueOutsideOfBounds_IsClamped()
        {
            Settings.MaxConcurrentWebRequests = 0;
            Assert.AreEqual(1, Settings.MaxConcurrentWebRequests);
            Settings.MaxConcurrentWebRequests = 2000;
            Assert.AreEqual(1024, Settings.MaxConcurrentWebRequests);
        }

        [TestCase(1)]
        [TestCase(5)]
        public void AddressableAssetSettings_SetLabelValueForEntries_CanSet(int numEntries)
        {
            // Setup
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            var newLabel = "testSetLabelValueForEntries";
            SetupEntries(ref entries, numEntries);
            var prevDC = EditorUtility.GetDirtyCount(Settings);

            // Test
            Settings.SetLabelValueForEntries(entries, newLabel, true, true);
            foreach (var e in entries)
                Assert.IsTrue(e.labels.Contains(newLabel));
            Assert.AreEqual(prevDC + 1, EditorUtility.GetDirtyCount(Settings));

            // Cleanup
            Settings.RemoveLabel(newLabel);
        }

        [TestCase(1, 2)]
        [TestCase(5, 8)]
        public void AddressableAssetSettings_RemoveLabel_RemoveLabelShouldRemoveDeletedLabelFromEntries(int numEntries, int numLabels)
        {
            // Setup
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>();
            SetupEntries(ref entries, numEntries);
            List<string> testLabels = new List<string>();
            for (int i = 1; i <= numLabels; i++)
            {
                var newLabel = "testSetLabelValueForEntries" + i;
                testLabels.Add(newLabel);
                Settings.SetLabelValueForEntries(entries, newLabel, true, true);
            }

            // Test
            // Remove half the labels
            for (int i = 0; i < numLabels / 2; i++)
                Settings.RemoveLabel(testLabels[i]);

            foreach (var e in entries)
                foreach (var l in testLabels)
                    Assert.IsTrue(e.labels.Contains(l));

            // Check that each of the first half of labels were removed
            foreach (var e in entries)
            {
                e.CreateKeyList();
                for (int i = 0; i < numLabels; i++)
                {
                    if (i < numLabels / 2)
                        Assert.IsFalse(e.labels.Contains(testLabels[i]));
                    else
                        Assert.IsTrue(e.labels.Contains(testLabels[i]));
                }
            }

            // Cleanup
            for (int i = numLabels / 2; i < numLabels; i++)
            {
                Settings.RemoveLabel(testLabels[i]);
            }
        }

        [Test]
        public void AddressableAssetSettings_HashChanges_WhenGroupIsAdded()
        {
            var prevHash = Settings.currentHash;
            var testGroupObj = Settings.GetGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            var testGroup = Settings.groups[0];
            Assert.AreEqual(Settings.currentHash, prevHash);
            Settings.AddGroupTemplateObject(testGroupObj);
            Settings.groups.Add(testGroup);
            var newHash = Settings.currentHash;
            Assert.AreNotEqual(newHash, prevHash);
            Settings.RemoveGroupTemplateObject(Settings.GroupTemplateObjects.Count - 1);
            Settings.groups.RemoveAt(Settings.groups.Count - 1);
        }

        [Test]
        public void CustomEntryCommand_WhenRegistered_InvokeIsCalled()
        {
            string notSet = null;
            AddressableAssetSettings.RegisterCustomAssetEntryCommand("cmd1", s => notSet = "set");
            Assert.IsTrue(AddressableAssetSettings.InvokeAssetEntryCommand("cmd1", new AddressableAssetEntry[] {}));
            Assert.AreEqual("set", notSet);
            AddressableAssetSettings.UnregisterCustomAssetEntryCommand("cmd1");
        }

        [Test]
        public void CustomEntryCommand_WhenCommandThrows_InvokeDoesNotThrow()
        {
            AddressableAssetSettings.RegisterCustomAssetEntryCommand("cmd1", s => throw new Exception());
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Encountered exception when running Asset Entry Command 'cmd1': Exception of type 'System.Exception' was thrown.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetEntryCommand("cmd1", new AddressableAssetEntry[] {}));
            });
            AddressableAssetSettings.UnregisterCustomAssetEntryCommand("cmd1");
        }

        [Test]
        public void CustomEntryCommand_WhenCommandHasNullEntries_ReturnsFalseAndLogsError()
        {
            AddressableAssetSettings.RegisterCustomAssetEntryCommand("cmd1", s => {});
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Asset Entry Command 'cmd1' called with null entry collection.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetEntryCommand("cmd1", null));
            });
            AddressableAssetSettings.UnregisterCustomAssetEntryCommand("cmd1");
        }

        [Test]
        public void CustomEntryCommand_WhenCommandDoesNotExist_ReturnsFalseAndLogsError()
        {
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Asset Entry Command 'cmd' not found.  Ensure that it is registered by calling RegisterCustomAssetEntryCommand.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetEntryCommand("cmd", new AddressableAssetEntry[] {}));
            });
        }

        [Test]
        public void CustomEntryCommand_RegisterWithValidIdAndFunc_Succeeds()
        {
            AddressableAssetSettings.RegisterCustomAssetEntryCommand("cmd1", s => {});
            CollectionAssert.Contains(AddressableAssetSettings.CustomAssetEntryCommands, "cmd1");
            AddressableAssetSettings.UnregisterCustomAssetEntryCommand("cmd1");
        }

        [Test]
        public void CustomEntryCommand_UnregisterWithInvalidIdParameters_Fails()
        {
            LogAssert.Expect(LogType.Error, "UnregisterCustomAssetEntryCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetEntryCommand(""));
            LogAssert.Expect(LogType.Error, "UnregisterCustomAssetEntryCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetEntryCommand(null));
            LogAssert.Expect(LogType.Error, $"UnregisterCustomAssetEntryCommand - command id 'doesntexist' is not registered.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetEntryCommand("doesntexist"));
            CollectionAssert.IsEmpty(AddressableAssetSettings.CustomAssetEntryCommands);
        }

        [Test]
        public void CustomEntryCommand_RegisterWithInvalidParameters_Fails()
        {
            LogAssert.Expect(LogType.Error, "RegisterCustomAssetEntryCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetEntryCommand("", s => {}));
            LogAssert.Expect(LogType.Error, "RegisterCustomAssetEntryCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetEntryCommand(null, s => {}));
            LogAssert.Expect(LogType.Error, $"RegisterCustomAssetEntryCommand - command functor for id 'valid'.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetEntryCommand("valid", null));
            CollectionAssert.IsEmpty(AddressableAssetSettings.CustomAssetEntryCommands);
        }

        [Test]
        public void CustomGroupCommand_WhenRegistered_InvokeIsCalled()
        {
            string notSet = null;
            AddressableAssetSettings.RegisterCustomAssetGroupCommand("cmd1", s => notSet = "set");
            AddressableAssetSettings.InvokeAssetGroupCommand("cmd1", new AddressableAssetGroup[] {});
            Assert.AreEqual("set", notSet);
            AddressableAssetSettings.UnregisterCustomAssetGroupCommand("cmd1");
        }

        [Test]
        public void CustomGroupCommand_RegisterWithValidIdAndFunc_Succeeds()
        {
            AddressableAssetSettings.RegisterCustomAssetGroupCommand("cmd1", s => {});
            CollectionAssert.Contains(AddressableAssetSettings.CustomAssetGroupCommands, "cmd1");
            AddressableAssetSettings.UnregisterCustomAssetGroupCommand("cmd1");
        }

        [Test]
        public void CustomGroupCommand_UnregisterWithInvalidIdParameters_Fails()
        {
            LogAssert.Expect(LogType.Error, "UnregisterCustomAssetGroupCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetGroupCommand(""));
            LogAssert.Expect(LogType.Error, "UnregisterCustomAssetGroupCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetGroupCommand(null));
            LogAssert.Expect(LogType.Error, $"UnregisterCustomAssetGroupCommand - command id 'doesntexist' is not registered.");
            Assert.IsFalse(AddressableAssetSettings.UnregisterCustomAssetGroupCommand("doesntexist"));
            CollectionAssert.IsEmpty(AddressableAssetSettings.CustomAssetGroupCommands);
        }

        [Test]
        public void CustomGroupCommand_RegisterWithInvalidParameters_Fails()
        {
            LogAssert.Expect(LogType.Error, "RegisterCustomAssetGroupCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetGroupCommand("", s => {}));
            LogAssert.Expect(LogType.Error, "RegisterCustomAssetGroupCommand - invalid command id.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetGroupCommand(null, s => {}));
            LogAssert.Expect(LogType.Error, $"RegisterCustomAssetGroupCommand - command functor for id 'valid'.");
            Assert.IsFalse(AddressableAssetSettings.RegisterCustomAssetGroupCommand("valid", null));
            CollectionAssert.IsEmpty(AddressableAssetSettings.CustomAssetGroupCommands);
        }

        [Test]
        public void CustomGroupCommand_WhenCommandThrows_InvokeDoesNotThrow()
        {
            AddressableAssetSettings.RegisterCustomAssetGroupCommand("cmd1", s => throw new Exception());
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Encountered exception when running Asset Group Command 'cmd1': Exception of type 'System.Exception' was thrown.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetGroupCommand("cmd1", new AddressableAssetGroup[] {}));
            });
            AddressableAssetSettings.UnregisterCustomAssetGroupCommand("cmd1");
        }

        [Test]
        public void CustomGroupCommand_WhenCommandHasNullGroups_ReturnsFalseAndLogsError()
        {
            AddressableAssetSettings.RegisterCustomAssetGroupCommand("cmd1", s => {});
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Asset Group Command 'cmd1' called with null group collection.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetGroupCommand("cmd1", null));
            });
            AddressableAssetSettings.UnregisterCustomAssetGroupCommand("cmd1");
        }

        [Test]
        public void CustomGroupCommand_WhenCommandDoesNotExist_ReturnsFalseAndLogsError()
        {
            Assert.DoesNotThrow(() =>
            {
                LogAssert.Expect(LogType.Error, $"Asset Group Command 'cmd' not found.  Ensure that it is registered by calling RegisterCustomAssetGroupCommand.");
                Assert.IsFalse(AddressableAssetSettings.InvokeAssetGroupCommand("cmd", new AddressableAssetGroup[] {}));
            });
        }

        [Test]
        public void AssetGroupSchemaTemplate_CreateTemplate()
        {
            string description = "This is a Test Schema";
            string groupName = "testGroup";
            var testSchemaTemplate = AddressableAssetGroupSchemaTemplate.Create(
                groupName, description, typeof(CustomTestSchema));
            Assert.AreEqual(testSchemaTemplate.Description, description);
            Assert.AreEqual(testSchemaTemplate.DisplayName, groupName);
            Assert.IsNotNull(testSchemaTemplate.GetTypes());
        }

            [Test]
        public void NullifyBundleFileIds_SetsBundleFileIdsToNull()
        {
            AddressableAssetSettings.NullifyBundleFileIds(Settings);
            foreach (var group in Settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    Assert.IsNull(entry.BundleFileId);
                }
            }
        }
    }
}
