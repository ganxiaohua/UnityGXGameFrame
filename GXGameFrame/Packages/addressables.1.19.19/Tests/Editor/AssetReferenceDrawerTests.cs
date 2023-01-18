using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEditor.U2D;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace UnityEditor.AddressableAssets.Tests
{
    [TestFixture]
    public class AssetReferenceDrawerTestsFixture : AddressableAssetTestBase
    {
        protected string m_fbxAssetPath;

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (File.Exists(m_fbxAssetPath))
            {
                AssetDatabase.DeleteAsset(m_fbxAssetPath);
                AssetDatabase.Refresh();
            }
        }

        internal AssetReferenceDrawer m_AssetReferenceDrawer { get; set; }
        internal List<Object> _subAssets;
        internal SerializedProperty _property;
        internal SpriteAtlas _atlas;

        internal class TestObjectWithRef : TestObject
        {
            [SerializeField]
            public AssetReference Reference = new AssetReference();
        }
        internal class TestObjectWithRestrictedRef : TestObject
        {
            [SerializeField]
            [AssetReferenceUILabelRestriction(new[] { "HD" })]
            private AssetReference Reference = new AssetReference();
        }

        internal class TestObjectWithRestrictedRefByMultipleLabels : TestObject
        {
            [SerializeField]
            [AssetReferenceUILabelRestriction(new[] { "HDR", "test", "default" })]
            private AssetReference ReferenceMultiple = new AssetReference();
        }

        internal class TestObjectWithRestrictedRefInNestedClass : TestObject
        {
            [SerializeField]
            NestedClass OneLevelNested = new NestedClass();

            [SerializeField]
            TwoLevelNestedClass TwoLevelNested = new TwoLevelNestedClass();

            [Serializable]
            class TwoLevelNestedClass
            {
                [SerializeField]
                NestedClass Nested = new NestedClass();
            }

            [Serializable]
            class NestedClass
            {
                [SerializeField]
                [AssetReferenceUILabelRestriction(new[] { "HD" })]
                AssetReference ReferenceInNestedClass = new AssetReference();
            }
        }

        internal class TestSubObjectsSpriteAtlas : TestObject
        {
            [SerializeField]
            public AssetReferenceSprite testSpriteReference;
        }

        internal class TestSubObjectsSpriteAtlasList : TestObject
        {
            [SerializeField]
            public AssetReferenceSprite[] testSpriteReference;
        }

        internal class TestAssetReferenceDrawer : AssetReferenceDrawer
        {
            TestAssetReferencePopup _popup;
            internal void SetAssetReference(AssetReference ar)
            {
                m_AssetRefObject = ar;
            }

            internal void TreeSetup(TreeViewState treeState)
            {
                _popup = new TestAssetReferencePopup(this, "testpopup", "");
                _popup.TreeSetup(treeState, this, _popup);
            }

            internal void TreeSelectionChangedHelper(IList<int> selectedIds)
            {
                _popup.TreeSelectionChangedHelper(selectedIds);
            }

            class TestAssetReferencePopup : AssetReferencePopup
            {
                TestSelectionTree _testTree;
                internal TestAssetReferencePopup(AssetReferenceDrawer drawer, string guid, string nonAddressedAsset)
                    : base(drawer, guid, nonAddressedAsset, false) {}

                internal void TreeSetup(TreeViewState treeState, AssetReferenceDrawer testARD, AssetReferencePopup popup)
                {
                    _testTree = new TestSelectionTree(treeState, testARD, popup, "testtree", "");
                    _testTree.Reload();
                }

                internal void TreeSelectionChangedHelper(IList<int> selectedIds)
                {
                    _testTree.SelectionChangedHelper(selectedIds);
                }

                class TestSelectionTree : AssetReferencePopup.AssetReferenceTreeView
                {
                    internal TestSelectionTree(TreeViewState state, AssetReferenceDrawer drawer,
                                               AssetReferencePopup popup, string guid, string nonAddressedAsset)
                        : base(state, drawer, popup, guid, nonAddressedAsset) {}

                    internal void SelectionChangedHelper(IList<int> selectedIds)
                    {
                        SelectionChanged(selectedIds);
                    }
                }
            }
        }

        internal class TestAssetReference : AssetReference
        {
            internal TestAssetReference(string guid) : base(guid) {}

            internal Object CachedAssetProperty
            {
                get
                {
                    return CachedAsset;
                }
                set
                {
                    CachedAsset = value;
                }
            }
        }

        internal void SetupDefaultSettings()
        {
            if (!Directory.Exists("Assets/AddressableAssetsData"))
                Directory.CreateDirectory("Assets/AddressableAssetsData");
            AddressableAssetSettingsDefaultObject.Settings = Settings;
        }

        internal void TearDownTestDir()
        {
            if (Directory.Exists(ConfigFolder + "/test"))
                AssetDatabase.DeleteAsset(ConfigFolder + "/test");
        }

        internal Object SetupAssetReference(out string assetPath)
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            AssetReference ar = new AssetReference();
            assetPath = AssetDatabase.GUIDToAssetPath(m_AssetGUID);
            m_AssetReferenceDrawer.m_AssetRefObject = ar;
            m_AssetReferenceDrawer.m_AssetRefObject.SubObjectName = "test";
            var testObject = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return testObject;
        }

        internal SerializedProperty SetupForSetObjectNullTests(out string newEntryGuid)
        {
            // Setup Original AssetReference to not be null
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            var assetPath = ConfigFolder + "/test" + "/test.prefab";
            CreateTestPrefabAddressable(assetPath);
            newEntryGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AssetReference ar = new AssetReference(newEntryGuid);
            SetupDefaultSettings();

            // Setup property
            TestObjectWithRef obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            Settings.CreateOrMoveEntry(newEntryGuid, Settings.groups[0]);
            obj.Reference = ar;
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            m_AssetReferenceDrawer.m_AssetRefObject = ar;
            AssetReferenceDrawerUtilities.GatherFilters(property);
            string sprGuid;
            FieldInfo propertyFieldInfo = typeof(TestObjectWithRef).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, obj, propertyFieldInfo, "", out sprGuid);

            return property;
        }

        internal SerializedProperty SetupForSetSubAssets(SpriteAtlas spriteAtlas, int numReferences, bool setReferences = true, int numToSet = -1)
        {
            // Setup AssetReference selected
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            var assetPath = AssetDatabase.GetAssetOrScenePath(spriteAtlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AssetReferenceSprite ar = new AssetReferenceSprite(atlasGuid);

            // Setup property
            if (numToSet == -1)
                numToSet = numReferences - 1;
            var targetObjects = new Object[numReferences];
            for (int i = 0; i < numReferences; i++)
            {
                var testScriptable = TestSubObjectsSpriteAtlas.CreateInstance<TestSubObjectsSpriteAtlas>();

                // Preset references for certain tests
                if (setReferences && i <= numToSet)
                    testScriptable.testSpriteReference = ar;
                targetObjects[i] = testScriptable;
            }
            var so = new SerializedObject(targetObjects);
            var property = so.FindProperty("testSpriteReference");
            m_AssetReferenceDrawer.m_AssetRefObject = ar;
            AssetReferenceDrawerUtilities.GatherFilters(property);
            SetupDefaultSettings();
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            return property;
        }

        internal SerializedProperty SetupForSetSubAssetsList(SpriteAtlas spriteAtlas,
            int numReferences,
            int selectedElement,
            int numElements = 1,
            bool setReferences = true,
            int numToSet = -1)
        {
            // Setup AssetReference selected
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            var assetPath = AssetDatabase.GetAssetOrScenePath(spriteAtlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            AssetReferenceSprite sar = new AssetReferenceSprite(atlasGuid);

            // Default to preset all references
            if (setReferences && numToSet == -1)
                numToSet = numReferences - 1;

            // Select one element per reference array as a targetObject
            var targetObjects = new Object[numElements];
            for (int refIdx = 0; refIdx < numReferences; refIdx++)
            {
                var testScriptable = TestSubObjectsSpriteAtlasList.CreateInstance<TestSubObjectsSpriteAtlasList>();
                testScriptable.testSpriteReference = new AssetReferenceSprite[numElements];

                // Preset reference array elements for certain tests
                if (setReferences && refIdx <= numToSet)
                {
                    for (int i = 0; i < numElements; i++)
                    {
                        AssetReferenceSprite ar = new AssetReferenceSprite(atlasGuid);
                        testScriptable.testSpriteReference[i] = ar;
                        testScriptable.testSpriteReference[i].SubObjectName = "test";
                    }
                }

                var serialObj = new SerializedObject(testScriptable);
                var serProp = serialObj.FindProperty("testSpriteReference.Array.data[" + selectedElement + "]");
                targetObjects[refIdx] = serProp.serializedObject.targetObject;
            }

            // Get main property to pass to AssetReferenceDrawer
            var property = SetupPropertyDrawerSetListTests(targetObjects, sar, selectedElement);

            return property;
        }

        internal SerializedProperty SetupPropertyDrawerSetListTests(Object[] targetObjects, AssetReference ar, int selectedElement)
        {
            var so = new SerializedObject(targetObjects);
            var property = so.FindProperty("testSpriteReference.Array.data[" + selectedElement + "]");
            m_AssetReferenceDrawer.m_AssetRefObject = ar;
            AssetReferenceDrawerUtilities.GatherFilters(property);
            SetupDefaultSettings();
            return property;
        }

        internal string CreateTestPrefabAddressable(string newEntryPath, bool createEntry = true)
        {
            GameObject testObject = new GameObject("TestObject");
            Directory.CreateDirectory(ConfigFolder + "/test");
            PrefabUtility.SaveAsPrefabAsset(testObject, newEntryPath);
            var folderGuid = AssetDatabase.AssetPathToGUID(ConfigFolder + "/test");
            if (createEntry)
            {
                Settings.CreateOrMoveEntry(folderGuid, Settings.groups[0]);
            }

            return folderGuid;
        }
        
        internal Object SetUpSingleSprite(out string atlasGuid)
        {
            // Setup Sprite data
            var texture = new Texture2D(32, 32);
            var data = ImageConversion.EncodeToPNG(texture);
            UnityEngine.Object.DestroyImmediate(texture);

            // Setup Sprite
            Directory.CreateDirectory(ConfigFolder + "/test");
            var spritePath = ConfigFolder + "/test" + "/testSprite.png";
            var atlasPath = ConfigFolder + "/test" + "/testAtlas.spriteatlas";
            var newAtlas = new SpriteAtlas();
            AssetDatabase.GenerateUniqueAssetPath(ConfigFolder);
            File.WriteAllBytes(spritePath, data);

            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritesheet = new SpriteMetaData[] { new SpriteMetaData() { name = "topleft", pivot = Vector2.zero, rect = new Rect(0, 0, 16, 16) },
                                                          new SpriteMetaData() { name = "testSprite", pivot = Vector2.zero, rect = new Rect(16, 16, 16, 16) }};
            importer.SaveAndReimport();

            // Add sprite to subassets
            Object spr = AssetDatabase.LoadAssetAtPath(spritePath, typeof(Sprite));
            spr.name = "testSprite";
            
            // Setup Atlas
            var spriteList = new[] {spr};
            newAtlas.Add(spriteList);
            AssetDatabase.CreateAsset(newAtlas, atlasPath);
            AssetDatabase.Refresh();
            SpriteAtlasExtensions.Add(newAtlas, spriteList);
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { newAtlas }, EditorUserBuildSettings.activeBuildTarget, false);

            var spriteGuid = AssetDatabase.AssetPathToGUID(spritePath);
            Settings.CreateOrMoveEntry(spriteGuid, Settings.groups[0]);
            atlasGuid = AssetDatabase.AssetPathToGUID(atlasPath);
            Settings.CreateOrMoveEntry(atlasGuid, Settings.groups[0]);

            return spr;
        }


        internal SpriteAtlas SetUpSpriteAtlas(int numAtlasObjects, out List<Object> subAssets)
        {
            // Setup Sprite data
            var texture = new Texture2D(32, 32);
            var data = ImageConversion.EncodeToPNG(texture);
            UnityEngine.Object.DestroyImmediate(texture);

            // Setup Sprites
            subAssets = new List<Object>();
            Directory.CreateDirectory(ConfigFolder + "/test");
            var atlasPath = ConfigFolder + "/test" + "/testAtlas.spriteatlas";
            var newAtlas = new SpriteAtlas();
            var sprites = new Object[numAtlasObjects];
            for (int i = 0; i < numAtlasObjects; i++)
            {
                // Create Sprite asset
                AssetDatabase.GenerateUniqueAssetPath(ConfigFolder);
                var newSpritePath = ConfigFolder + "/test" + "/testSprite" + i + ".png";
                File.WriteAllBytes(newSpritePath, data);

                AssetDatabase.ImportAsset(newSpritePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(newSpritePath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritesheet = new SpriteMetaData[] { new SpriteMetaData() { name = "topleft", pivot = Vector2.zero, rect = new Rect(0, 0, 16, 16) },
                                                              new SpriteMetaData() { name = "testSprite" + i, pivot = Vector2.zero, rect = new Rect(16, 16, 16, 16) }};
                importer.SaveAndReimport();

                // Add sprite to subassets
                Object spr = AssetDatabase.LoadAssetAtPath(newSpritePath, typeof(Sprite));
                spr.name = "testSprite" + i;
                sprites[i] = spr;
                subAssets.Add(spr);
            }
            // Setup Atlas
            newAtlas.Add(sprites);
            AssetDatabase.CreateAsset(newAtlas, atlasPath);
            AssetDatabase.Refresh();
            SpriteAtlasExtensions.Add(newAtlas, sprites);
            SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { newAtlas }, EditorUserBuildSettings.activeBuildTarget, false);

            var atlasGuid = AssetDatabase.AssetPathToGUID(atlasPath);
            Settings.CreateOrMoveEntry(atlasGuid, Settings.groups[0]);

            return newAtlas;
        }

        public void SetUpForSubassetPerformanceTests(int numAtlasObjects, int numReferences, int selectedId)
        {
            // Drawer Setup
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            _subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out _subAssets);
            _property = SetupForSetSubAssets(atlas, numReferences, true);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetSubAssets(_property, _subAssets[selectedId], propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            AssetReferenceDrawerUtilities.GatherFilters(_property);
        }

        public void SetupForSetAssetsPerformanceTests(int numReferences)
        {
            _subAssets = new List<Object>();
            _atlas = SetUpSpriteAtlas(numReferences, out _subAssets);
            _property = SetupForSetSubAssets(_atlas, numReferences);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
        }

        public void SetObjectForPerformanceTests()
        {
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, _property, _atlas, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text, out string guid);
        }

        public void SetMainAssetsForPerformanceTests()
        {
            _subAssets = new List<Object>();
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, _property, _atlas, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
        }

        public void GetNameForAssetForPerformanceTests()
        {
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.GetNameForAsset(ref m_AssetReferenceDrawer.m_ReferencesSame, _property, false, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
        }

        public void GetSubAssetsListForPerformanceTests()
        {
            AssetReferenceDrawerUtilities.GetSubAssetsList(m_AssetReferenceDrawer.m_AssetRefObject);
        }

        public void GetSelectedSubassetIndexForPerformanceTests()
        {
            m_AssetReferenceDrawer.GetSelectedSubassetIndex(_subAssets, out var selIndex, out var objNames);
        }

        public void CheckTargetObjectsSubassetsAreDifferentForPerformanceTests()
        {
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.CheckTargetObjectsSubassetsAreDifferent(_property, m_AssetReferenceDrawer.m_AssetRefObject.SubObjectName, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
        }
    }

    public class AssetReferenceDrawerTests : AssetReferenceDrawerTestsFixture
    {
        [Test]
        public void CanRestrictLabel()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            TestObjectWithRestrictedRef obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            List<AssetReferenceUIRestrictionSurrogate> restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);
            Assert.AreEqual(restrictions.Count, 1);
            Assert.True(restrictions.First().ToString().Contains("HD"));
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }

        [Test]
        public void CanRestrictMultipleLabels()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            TestObjectWithRestrictedRefByMultipleLabels obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRefByMultipleLabels>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("ReferenceMultiple");
            AssetReferenceDrawerUtilities.GatherFilters(property);
            List<AssetReferenceUIRestrictionSurrogate> restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);
            string restriction = restrictions.First().ToString();
            Assert.True(restriction.Contains("HDR"));
            Assert.True(restriction.Contains("test"));
            Assert.True(restriction.Contains("default"));
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }

        [Test]
        public void AssetReferenceDrawer_GatherFilters_CanRestrictInSingleNestedClass()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            TestObjectWithRestrictedRefInNestedClass obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRefInNestedClass>();
            var so = new SerializedObject(obj);
            var oneLevelProp = so.FindProperty("OneLevelNested.ReferenceInNestedClass");
            var restrictions = AssetReferenceDrawerUtilities.GatherFilters(oneLevelProp);
            Assert.True(restrictions.First().ToString().Contains("HD"));
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }
        
        [Test]
        public void AssetReferenceDrawer_GatherFilters_CanRestrictInDoubleNestedClass()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            TestObjectWithRestrictedRefInNestedClass obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRefInNestedClass>();
            var so = new SerializedObject(obj);
            var twoLevelProp = so.FindProperty("TwoLevelNested.Nested.ReferenceInNestedClass");
            var restrictions = AssetReferenceDrawerUtilities.GatherFilters(twoLevelProp);
            Assert.True(restrictions.First().ToString().Contains("HD"));
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }
        
        [Test]
        public void AssetReferenceDrawer_ValidateAsset_CanValidateAssetWithRestrictionsFromPath()
        {
            // Setup AssetReference
            string assetPath = "";
            var testObject = SetupAssetReference(out assetPath);
            
            // Setup property
            TestObjectWithRestrictedRef obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            var restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);

            // Test
            string guid;
            
            FieldInfo propertyFieldInfo = typeof(TestObjectWithRestrictedRef).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, testObject, propertyFieldInfo, "", out guid));
            Assert.False(AssetReferenceDrawerUtilities.ValidateAsset(m_AssetReferenceDrawer.m_AssetRefObject, restrictions, assetPath));
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }
        
        [Test]
        public void AssetReferenceDrawer_ValidateAsset_CanValidateAssetWithoutRestrictionsFromPath()
        {
            // Setup AssetReference
            string assetPath = "";
            var testObject = SetupAssetReference(out assetPath);

            // Setup property
            TestObjectWithRef obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            var restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);

            // Test
            string guid;
            
            FieldInfo propertyFieldInfo = typeof(TestObjectWithRestrictedRef).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, testObject, propertyFieldInfo, "", out guid));
            Assert.IsTrue(AssetReferenceDrawerUtilities.ValidateAsset(m_AssetReferenceDrawer.m_AssetRefObject, restrictions, assetPath));
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }

        [Test]
        public void AssetReferenceDrawer_IsAssetPathInAddressableDirectory_PathInAddressableFolder()
        {
            // Asset setup
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            string assetName = "";
            var newEntryPath = ConfigFolder + "/test" + "/test.prefab";
            var folderGuid = CreateTestPrefabAddressable(newEntryPath);

            // Test
            Assert.IsTrue(Settings.IsAssetPathInAddressableDirectory(newEntryPath, out assetName));
            Assert.AreEqual(assetName, folderGuid + "/test.prefab");

            // Cleanup
            Settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(newEntryPath));
            Settings.RemoveAssetEntry(folderGuid);
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }

        [Test]
        public void AssetReferenceDrawer_IsAssetPathInAddressableDirectory_PathNotInAddressableFolder()
        {
            // Asset setup
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            string assetName = "";
            var newEntryPath = ConfigFolder + "/test" + "/test.prefab";
            var folderGuid = CreateTestPrefabAddressable(newEntryPath, false);

            // Test
            Assert.IsFalse(Settings.IsAssetPathInAddressableDirectory(newEntryPath, out assetName));
            Assert.AreEqual(assetName, "");

            // Cleanup
            Settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(newEntryPath));
            Settings.RemoveAssetEntry(folderGuid);
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }

        [Test]
        public void AssetReferenceDrawer_IsAssetPathInAddressableDirectory_PathEmptyString()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            string assetName = "";

            Assert.IsFalse(Settings.IsAssetPathInAddressableDirectory("", out assetName));
            Assert.AreEqual(assetName, "");
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }

        [Test]
        public void AssetReferenceDrawer_IsAssetPathInAddressableDirectory_PathPointToNonexistentAsset()
        {
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            string assetName = "";

            Assert.IsFalse(Settings.IsAssetPathInAddressableDirectory(ConfigFolder + "/test.prefab", out assetName));
            Assert.AreEqual(assetName, "");
            TearDownTestDir();
            m_AssetReferenceDrawer = null;
        }

        [Test]
        public void AssetReferenceDrawer_SelectionChanged_CanSelectSameNameAssetsInDifferentGroups()
        {
            // Drawer Setup
            var testAssetReferenceDrawer = new TestAssetReferenceDrawer();
            testAssetReferenceDrawer.SetAssetReference(new AssetReference());
            TestObjectWithRef obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            testAssetReferenceDrawer.m_Restrictions = AssetReferenceDrawerUtilities.GatherFilters(property);

            // Entries setup
            var newEntryPath = ConfigFolder + "/test" + "/test.prefab";
            var testEntry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.groups[0]);
            GameObject testObject = new GameObject("TestObject");
            Directory.CreateDirectory(ConfigFolder + "/test");
            PrefabUtility.SaveAsPrefabAsset(testObject, newEntryPath);
            var newEntryGuid = AssetDatabase.AssetPathToGUID(newEntryPath);
            var secondTestEntry = Settings.CreateOrMoveEntry(newEntryGuid, Settings.groups[1]);

            // Tree setup
            var testId = testEntry.AssetPath.GetHashCode();
            List<int> selectedIds = new List<int>() { testId };
            var treeState = new TreeViewState();
            treeState.selectedIDs = selectedIds;
            Directory.CreateDirectory("Assets/AddressableAssetsData");
            AddressableAssetSettingsDefaultObject.Settings = Settings;
            testAssetReferenceDrawer.TreeSetup(treeState);


            // Test
            testAssetReferenceDrawer.TreeSelectionChangedHelper(selectedIds);
            Assert.AreEqual(m_AssetGUID, testAssetReferenceDrawer.newGuid);
            selectedIds[0] = secondTestEntry.AssetPath.GetHashCode();
            testAssetReferenceDrawer.TreeSelectionChangedHelper(selectedIds);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(newEntryPath), testAssetReferenceDrawer.newGuid);

            // Cleanup
            if (Directory.Exists("Assets/AddressableAssetsData"))
                AssetDatabase.DeleteAsset("Assets/AddressableAssetsData");
            EditorBuildSettings.RemoveConfigObject("Assets/AddressableAssetsData");
            Settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(newEntryPath));
            Settings.RemoveAssetEntry(m_AssetGUID);
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }

        [Test]
        [Ignore("This test checks for a specific path, these paths aren't proper test paths.")]
        public void AssetReferenceDrawer_HandleDragAndDrop_CanRecognizeNonAddressableInAddressableFolder()
        {
            // ScriptableObject property and Drawer setup
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            m_AssetReferenceDrawer.m_AssetRefObject = new AssetReference();
            TestObjectWithRestrictedRef obj = ScriptableObject.CreateInstance<TestObjectWithRestrictedRef>();
            var so = new SerializedObject(obj);
            var propertyName = "Reference";
            var property = so.FindProperty(propertyName);
            m_AssetReferenceDrawer.m_label = new GUIContent(propertyName);

            // Group setup
            string groupName = "TestGroup";
            var newGroup = Settings.CreateGroup(groupName, false, false, false, null);

            // Asset setup
            var newEntryPath = ConfigFolder + "/test" + "/test.prefab";
            var folderGuid = CreateTestPrefabAddressable(newEntryPath);
            AssetDatabase.Refresh();
            var newAssetGuid = AssetDatabase.AssetPathToGUID(newEntryPath);
            Settings.CreateOrMoveEntry(folderGuid, Settings.groups[2]);
            SetupDefaultSettings();

            // Test
            m_AssetReferenceDrawer.DragAndDropNotFromAddressableGroupWindow(newEntryPath, newAssetGuid, property, Settings);
            var newentry = Settings.FindAssetEntry(newAssetGuid);
            Assert.IsNull(newentry);
            Assert.AreEqual(m_AssetReferenceDrawer.m_AssetRefObject.Asset.name, newAssetGuid);

            // Cleanup
            Settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(newEntryPath));
            Settings.RemoveAssetEntry(folderGuid);
            Settings.RemoveGroup(newGroup);
            TearDownTestDir();
        }

        [Test]
        public void AssetReferenceDrawer_SetObject_CanSetObject()
        {
            // Setup AssetReference
            string assetPath = "";
            var testObject = SetupAssetReference(out assetPath);

            // Setup property
            TestObjectWithRef obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");
            AssetReferenceDrawerUtilities.GatherFilters(property);

            // Test
            string guid;
            
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Assert.IsTrue(AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame,property, testObject, propertyFieldInfo, "", out guid));
            Assert.AreEqual(m_AssetGUID, m_AssetReferenceDrawer.m_AssetRefObject.AssetGUID);
            Assert.AreEqual(m_AssetGUID, guid);
            Assert.AreEqual(testObject.name, m_AssetReferenceDrawer.m_AssetRefObject.editorAsset.name);
            m_AssetReferenceDrawer = null;
            TearDownTestDir();
        }
        
        [Test]
        public void AssetReferenceDrawer_SetObject_CanSetSpriteObject()
        {
            // Setup
            string assetPath = "";
            SetupAssetReference( out assetPath);
            var assetGuid = "";
            var testObject = SetUpSingleSprite(out assetGuid);

            // Setup property
            TestObjectWithRef obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference"); 
            AssetReferenceDrawerUtilities.GatherFilters(property);

            // Test
            string guid;
            
            FieldInfo propertyFieldInfo = typeof(TestObjectWithRef).GetField("Reference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            SetupDefaultSettings();
            Assert.IsTrue(AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame,property, testObject, propertyFieldInfo, "", out guid));
            Assert.AreEqual(assetGuid, m_AssetReferenceDrawer.m_AssetRefObject.AssetGUID);
            Assert.AreEqual("testAtlas", m_AssetReferenceDrawer.m_AssetRefObject.editorAsset.name);
            Assert.AreEqual("testSprite", m_AssetReferenceDrawer.m_AssetRefObject.SubObjectName);
            
            // Cleanup
            TearDownTestDir();
        }

        [Test]
        public void SetObject_WhenTargetIsSubAsset_IsSetAsSubObject()
        {
            // Prepare test fbx
            m_fbxAssetPath = GetAssetPath("testFBX.fbx");
            if (!File.Exists(m_fbxAssetPath))
            {
                string fbxResourcePath = null;
                var repoRoot = Directory.GetParent(Application.dataPath).Parent?.FullName;
                if (!string.IsNullOrEmpty(repoRoot))
                    fbxResourcePath = Path.Combine(repoRoot, "Projects", "TestsResources", "testFBX.fbx");

                if (string.IsNullOrEmpty(fbxResourcePath) || !File.Exists(fbxResourcePath))
                    Assert.Ignore($"Unable to find required FBX file to run this test. Ignoring.");

                File.Copy(fbxResourcePath, m_fbxAssetPath, true);
                AssetDatabase.Refresh();
            }

            Assert.IsTrue(File.Exists(m_fbxAssetPath));
            var fbxAsset = AssetDatabase.LoadAssetAtPath<Object>(m_fbxAssetPath);
            var meshSubAsset = AssetDatabase.LoadAllAssetRepresentationsAtPath(m_fbxAssetPath).First(o => o is Mesh);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(fbxAsset, out string fbxAssetGuid, out long _);
            Assert.IsFalse(string.IsNullOrEmpty(fbxAssetGuid));

            // Setup property
            var ar = new AssetReferenceT<Mesh>("");
            var obj = ScriptableObject.CreateInstance<TestObjectWithRef>();
            obj.Reference = ar;
            var so = new SerializedObject(obj);
            var property = so.FindProperty("Reference");

            // Test
            string guid;
            m_AssetReferenceDrawer = new AssetReferenceDrawer();
            m_AssetReferenceDrawer.m_AssetRefObject = ar;
            AssetReferenceDrawerUtilities.GatherFilters(property);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var success = AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, meshSubAsset, propertyFieldInfo, "", out guid);

            // Assert
            Assert.IsTrue(success);
            Assert.AreEqual(fbxAssetGuid, guid);
            Assert.AreEqual(meshSubAsset.name, m_AssetReferenceDrawer.m_AssetRefObject.SubObjectName);
            Assert.AreEqual(meshSubAsset.GetType(), m_AssetReferenceDrawer.m_AssetRefObject.SubOjbectType);
        }

        [Test]
        public void AssetReferenceDrawer_SetObject_CanSetToNull()
        {
            // Setup Original AssetReference to not be null and property
            var property = SetupForSetObjectNullTests(out var newAssetGuid);

            // Test
            string guid;
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, null, propertyFieldInfo, "", out guid);
            Assert.AreEqual(null, m_AssetReferenceDrawer.m_AssetRefObject.SubObjectName);
            Assert.AreEqual(string.Empty, m_AssetReferenceDrawer.m_AssetRefObject.AssetGUID);
            
            // Cleanup
            Settings.RemoveAssetEntry(newAssetGuid);
            TearDownTestDir();
        }

        [Test]
        public void AssetReference_WhenCachedGUIDIsNotEqualToAssetGUID_CachedAssetIsNull()
        {
            // Setup
            string guid = "8888888888888888888";
            var assetRef = new TestAssetReference(guid)
            {
                CachedAssetProperty = new Object()
            };

            // Test
            Assert.IsTrue(assetRef.CachedAssetProperty == null);
            assetRef.ReleaseAsset();
            Settings.RemoveAssetEntry(assetRef.AssetGUID);
            TearDownTestDir();
        }

#if UNITY_2019_2_OR_NEWER

        [Test]
        public void AssetReferenceDrawer_SetObject_SetToNullDirtiesObject()
        {
            // Setup Original AssetReference to not be null and property
            var newAssetGuid = "";
            var property = SetupForSetObjectNullTests(out newAssetGuid);

            // Test
            string guid;
            EditorUtility.ClearDirty(property.serializedObject.targetObject);
            var prevDirty = EditorUtility.IsDirty(property.serializedObject.targetObject);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            AssetReferenceDrawerUtilities.SetObject(ref m_AssetReferenceDrawer.m_AssetRefObject, ref m_AssetReferenceDrawer.m_ReferencesSame, property, null, propertyFieldInfo, "", out guid);
            Assert.IsFalse(prevDirty);
            Assert.IsTrue(EditorUtility.IsDirty(property.serializedObject.targetObject));
            
            // Cleanup
            Settings.RemoveAssetEntry(newAssetGuid);
            TearDownTestDir();
        }

        [TestCase(1, 0, 1)]
        [TestCase(20, 10, 8)]
        [Ignore("The sub asset is returning the default name of null instead of the proper name for the selected element.")]
        public void AssetReferenceDrawer_SetSubAssets_CanSetSubAssets(int numAtlasObjects, int selectedId, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences, true);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // Test
            AssetReferenceDrawerUtilities.SetSubAssets(property, subAssets[selectedId], propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            foreach (var obj in property.serializedObject.targetObjects)
            {
                Assert.AreEqual(((TestSubObjectsSpriteAtlas)obj).testSpriteReference.SubObjectName, subAssets[selectedId].name);
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 1, 1, 0, 0)]
        [TestCase(4, 3, 3, 1, 1)]
        [Ignore("The sub asset is returning the default name of 'test' instead of the proper name for the selected element.")]
        public void AssetReferenceDrawer_SetSubAssets_CanSetInList(int numAtlasObjects, int numReferences, int numElements, int selectedElement, int selectedSubAsset)
        {
            // Setup
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out var subAssets);
            var property = SetupForSetSubAssetsList(atlas, numReferences, selectedElement, numElements, true, numReferences);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlasList).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            m_AssetReferenceDrawer.m_label = new GUIContent("Element " + selectedElement);

            // Test
            AssetReferenceDrawerUtilities.SetSubAssets(property, subAssets[selectedSubAsset], propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);

            // Check that only the selected element of each object's reference list was set to null
            foreach (var obj in property.serializedObject.targetObjects)
            {
                var checkList = ((TestSubObjectsSpriteAtlasList)obj).testSpriteReference;
                for (int currElement = checkList.Length-1; currElement >= 0; currElement--)
                {
                    if (currElement == selectedElement)
                        Assert.AreEqual(subAssets[selectedSubAsset].name, checkList[selectedElement].SubObjectName);
                    else
                        Assert.AreEqual("test", checkList[currElement].SubObjectName);
                }
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 0, 1)]
        [TestCase(20, 10, 8)]
        public void AssetReferenceDrawer_SetSubAssets_CanSetSubAssetsToNull(int numAtlasObjects, int selectedId, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences, true);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // Test
            AssetReferenceDrawerUtilities.SetSubAssets(property, subAssets[selectedId], propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            AssetReferenceDrawerUtilities.SetSubAssets(property, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            foreach (var obj in property.serializedObject.targetObjects)
            {
                Assert.AreEqual( null,((TestSubObjectsSpriteAtlas)obj).testSpriteReference.SubObjectName);
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 1, 1, 0)]
        [TestCase(2, 3, 3, 1)]
        public void AssetReferenceDrawer_SetSubAssets_CanSetToNullInList(int numAtlasObjects, int numReferences, int numElements, int selectedElement)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssetsList(atlas, numReferences, selectedElement, numElements, true, numReferences);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlasList).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            m_AssetReferenceDrawer.m_label = new GUIContent("Element " + selectedElement);

            // Test
            AssetReferenceDrawerUtilities.SetSubAssets(property, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);

            // Check that only the selected element of each object's reference list was set to null
            foreach (var obj in property.serializedObject.targetObjects)
            {
                var checkList = ((TestSubObjectsSpriteAtlasList)obj).testSpriteReference;
                for (int currElement = 0; currElement < checkList.Length; currElement++)
                {
                    if (currElement == selectedElement)
                        Assert.AreEqual(null, checkList[selectedElement].SubObjectName);
                    else
                        Assert.AreEqual("test", checkList[currElement].SubObjectName);
                }
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }
        
        [TestCase(1, 0, 1)]
        [TestCase(20, 10, 8)]
        public void AssetReferenceDrawer_GetSubAssetsList_CanGetSubAssetsList(int numAtlasObjects, int selectedId, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences, true);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // Test
            AssetReferenceDrawerUtilities.SetSubAssets(property, subAssets[selectedId], propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            var subassetsList = AssetReferenceDrawerUtilities.GetSubAssetsList(m_AssetReferenceDrawer.m_AssetRefObject);
            Assert.AreEqual(subassetsList.Count,subAssets.Count + 1);
            foreach (var obj in subassetsList)
            {
                if (obj != null)
                {
                    Assert.IsTrue(FindSubassetNameInList(obj.name, subAssets));
                }
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        bool FindSubassetNameInList(string objName, List<Object> objects)
        {
            foreach (var obj in objects)
            {
                if (objName.Contains(obj.name))
                    return true;
            }
            return false;
        }

        [TestCase(1, 1)]
        [TestCase(20, 8)]
        public void AssetReferenceDrawer_SetMainAssets_CanSetMultipleAssets(int numAtlasObjects, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);

            // Test
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, property, atlas, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            foreach (var obj in property.serializedObject.targetObjects)
            {
                Assert.AreEqual(((TestSubObjectsSpriteAtlas)obj).testSpriteReference.AssetGUID, atlasGuid);
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 1, 1, 0)]
        [TestCase(2, 3, 3, 1)]
        public void AssetReferenceDrawer_SetMainAssets_CanSetMultipleAssetReferencesInList(int numAtlasObjects, int numReferences, int numElements, int selectedElement)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssetsList(atlas, numReferences, selectedElement, numElements, false);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlasList).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("Element " + selectedElement);

            // Test
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, property, atlas, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);

            // Check that only the selected element of each object's reference list was set
            foreach (var obj in property.serializedObject.targetObjects)
            {
                var checkList = ((TestSubObjectsSpriteAtlasList)obj).testSpriteReference;
                for (int currElement = 0; currElement < checkList.Length; currElement++)
                {
                    if (currElement == selectedElement)
                        Assert.AreEqual(atlasGuid, checkList[selectedElement].AssetGUID);
                    else
                        Assert.AreEqual(null, checkList[currElement].AssetGUID);
                }
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [Test]
        public void AssetReferenceDrawer_SetMainAssets_CanSetToNull()
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(1, out subAssets);
            var property = SetupForSetSubAssets(atlas, 1);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);

            // Test
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, property, null, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            Assert.AreEqual(((TestSubObjectsSpriteAtlas)property.serializedObject.targetObject).testSpriteReference.Asset, null);

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 1, 1, 0)]
        [TestCase(2, 3, 3, 1)]
        public void AssetReferenceDrawer_SetMainAssets_CanSetToNullInList(int numAtlasObjects, int numReferences, int numElements, int selectedElement)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssetsList(atlas, numReferences, selectedElement, numElements, true, numReferences);
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlasList).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("Element " + selectedElement);

            // Test
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, property, null, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);

            // Check that only the selected element of each object's reference list was set to null
            foreach (var obj in property.serializedObject.targetObjects)
            {
                var checkList = ((TestSubObjectsSpriteAtlasList)obj).testSpriteReference;
                for (int currElement = 0; currElement < checkList.Length; currElement++)
                {
                    if (currElement == selectedElement)
                        Assert.AreEqual(String.Empty, checkList[selectedElement].AssetGUID);
                    else
                        Assert.AreEqual(atlasGuid, checkList[currElement].AssetGUID);
                }
            }

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [Test]
        public void AssetReferenceDrawer_SetMainAssets_SetToNullDirtiesObject()
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(1, out subAssets);
            var property = SetupForSetSubAssets(atlas, 1);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // Test
            EditorUtility.ClearDirty(property.serializedObject.targetObject);
            var prevDirty = EditorUtility.IsDirty(property.serializedObject.targetObject);
            AssetReferenceDrawerUtilities.SetMainAssets(ref m_AssetReferenceDrawer.m_ReferencesSame, property, null, null, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            Assert.IsFalse(prevDirty);
            Assert.IsTrue(EditorUtility.IsDirty(property.serializedObject.targetObject));
            
            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(1, 1)]
        [TestCase(20, 8)]
        [Ignore("The get name for asset is returning only the type of asset, not the asset subname.")]
        public void AssetReferenceDrawer_GetNameForAsset_CanGetAssetNameWhenAllSame(int numAtlasObjects, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences, true);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            m_AssetReferenceDrawer.m_AssetName = atlas.name;

            // Test
            var nameforAsset = AssetReferenceDrawerUtilities.GetNameForAsset(ref m_AssetReferenceDrawer.m_ReferencesSame, property, false, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            Assert.AreEqual(atlas.name, nameforAsset);

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

        [TestCase(10, 4, 8)]
        public void AssetReferenceDrawer_GetNameForAsset_CanGetAssetNameWhenDifferent(int numAtlasObjects, int numToSet, int numReferences)
        {
            // Setup
            var subAssets = new List<Object>();
            var atlas = SetUpSpriteAtlas(numAtlasObjects, out subAssets);
            var property = SetupForSetSubAssets(atlas, numReferences, true, numToSet);
            var assetPath = AssetDatabase.GetAssetOrScenePath(atlas);
            var atlasGuid = AssetDatabase.AssetPathToGUID(assetPath);
            m_AssetReferenceDrawer.m_label = new GUIContent("testSpriteReference");
            FieldInfo propertyFieldInfo = typeof(TestSubObjectsSpriteAtlas).GetField("testSpriteReference", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            m_AssetReferenceDrawer.m_AssetName = atlas.name;

            // Test
            var nameforAsset = AssetReferenceDrawerUtilities.GetNameForAsset(ref m_AssetReferenceDrawer.m_ReferencesSame, property, false, propertyFieldInfo, m_AssetReferenceDrawer.m_label.text);
            Assert.AreEqual("--", nameforAsset);

            // Cleanup
            TearDownTestDir();
            Settings.RemoveAssetEntry(atlasGuid);
        }

#endif
    }
}
