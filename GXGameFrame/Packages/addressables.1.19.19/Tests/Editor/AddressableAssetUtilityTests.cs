using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Utilities;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityEditor.AddressableAssets.Tests
{
    public class AddressableAssetUtilityTests : AddressableAssetTestBase
    {
        static string CreateTestPrefabAsset(string assetPath, string objectName)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = objectName;
            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            UnityEngine.Object.DestroyImmediate(go, false);
            return AssetDatabase.AssetPathToGUID(assetPath);
        }

        [Test]
        public void GetPathAndGUIDFromTarget_FromPrefabAsset_ReturnsCorrectPathGUIDType()
        {
            var expectedGUID = CreateTestPrefabAsset(GetAssetPath("prefab1.prefab"), "prefab1");
            var expectedPath = AssetDatabase.GUIDToAssetPath(expectedGUID);
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(expectedPath);
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry(expectedPath), $"Asset is not a valid Addressable Entry path : {expectedPath}");
            Assert.IsTrue(AddressableAssetUtility.TryGetPathAndGUIDFromTarget(obj, out var actualPath, out var actualGUID), "Could not get Path and Guid from Target at expectedPath " + expectedPath);
            Assert.AreEqual(expectedPath, actualPath);
            Assert.AreEqual(expectedGUID, actualGUID);
            AssetDatabase.DeleteAsset(expectedPath);
        }

        [Test]
        public void GetPathAndGUIDFromTarget_FromPrefabObject_Fails()
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Assert.IsFalse(AddressableAssetUtility.TryGetPathAndGUIDFromTarget(obj, out var actualPath, out var actualGUID));
            Assert.IsEmpty(actualPath);
            Assert.IsEmpty(actualGUID);
            Assert.IsEmpty(actualGUID);
        }

        [Test]
        public void GetPathAndGUIDFromTarget_FromNullObject_Fails()
        {
            Assert.IsFalse(AddressableAssetUtility.TryGetPathAndGUIDFromTarget(null, out var actualPath, out var actualGUID));
            Assert.IsEmpty(actualPath);
            Assert.IsEmpty(actualGUID);
            Assert.IsEmpty(actualGUID);
        }

        public class TestBaseClass {}
        [System.ComponentModel.DisplayName("TestSubClass_DisplayName")]
        public class TestSubClass : TestBaseClass {}
        public abstract class TestAbstractSubClass : TestBaseClass {}

        [Test]
        public void GetTypesGeneric_ReturnsOnly_NonAbstractSubTypes()
        {
            var types = AddressableAssetUtility.GetTypes<TestBaseClass>();
            Assert.AreEqual(1, types.Count);
            Assert.AreEqual(types[0], typeof(TestSubClass));
        }

        [Test]
        public void GetTypes_ReturnsOnly_NonAbstractSubTypes()
        {
            var types = AddressableAssetUtility.GetTypes(typeof(TestBaseClass));
            Assert.AreEqual(1, types.Count);
            Assert.AreEqual(types[0], typeof(TestSubClass));
        }

        [Test]
        public void GetCachedTypeDisplayName_WithNoAttribute_ReturnsTypeName()
        {
            var name = AddressableAssetUtility.GetCachedTypeDisplayName(typeof(TestBaseClass));
            Assert.AreEqual(typeof(TestBaseClass).Name, name);
        }

        [Test]
        public void GetCachedTypeDisplayName_WithAttribute_ReturnsAttributeValue()
        {
            var subName = AddressableAssetUtility.GetCachedTypeDisplayName(typeof(TestSubClass));
            Assert.AreEqual("TestSubClass_DisplayName", subName);
        }

        [Test]
        public void GetCachedTypeDisplayName_WithNullType_ReturnsNONE()
        {
            var subName = AddressableAssetUtility.GetCachedTypeDisplayName(null);
            Assert.AreEqual("<none>", subName);
        }

        [Test]
        public void IsInResourcesProperlyHandlesCase()
        {
            Assert.IsTrue(AddressableAssetUtility.IsInResources("/rEsOurces/"));
            Assert.IsTrue(AddressableAssetUtility.IsInResources("/resources/"));
            Assert.IsTrue(AddressableAssetUtility.IsInResources("/RESOURCES/"));
        }

        [Test]
        public void IsInResourcesHandlesExtraPathing()
        {
            Assert.IsTrue(AddressableAssetUtility.IsInResources("path/path/resources/path"));
            Assert.IsTrue(AddressableAssetUtility.IsInResources("path/path/resources/"));
            Assert.IsTrue(AddressableAssetUtility.IsInResources("/resources/path"));
        }

        [Test]
        public void IsInResourcesHandlesResourcesInWrongContext()
        {
            Assert.IsFalse(AddressableAssetUtility.IsInResources("resources/"));
            Assert.IsFalse(AddressableAssetUtility.IsInResources("/resources"));
            Assert.IsFalse(AddressableAssetUtility.IsInResources("path/resourcesOther/path"));
            Assert.IsFalse(AddressableAssetUtility.IsInResources("/path/res/ources/path"));
        }

        [Test]
        public void IsPathValidBlocksCommonStrings()
        {
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry(string.Empty));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry(CommonStrings.UnityEditorResourcePath));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry(CommonStrings.UnityDefaultResourcePath));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry(CommonStrings.UnityBuiltInExtraPath));
        }

        [Test]
        [TestCase("Assets/Editor/Resources/fake_asset.png")]
        [TestCase("Assets/Editor/Resources/fake_asset.png")]
        [TestCase("Assets/Editor/Editor/fake_asset.png")]
        [TestCase("Assets\\Editor\\Resources\\fake_asset.png")]
        [TestCase("Assets\\Editor\\Editor\\Resources\\fake_asset.png")]
        [TestCase("Assets\\Editor\\Resources\\Editor\\Resources\\fake_asset.png")]
        public void IsPathValidBlocksEditorPaths(string path)
        {
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry(path));
        }

        [Test]
        public void IsPathValidBlocksBadExtensions()
        {
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.cs"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.js"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.boo"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.exe"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.dll"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.preset"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Assets/file.asmdef"));
        }

        [Test]
        public void IsPathValidAllowsBasicTypes()
        {
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.asset"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.png"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.bin"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.txt"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.prefab"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.mat"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.wav"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.jpg"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.avi"));
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Assets/file.controller"));
        }

        [Test]
        public void WhenPathIsInUnityAuthoredPackage_IsPathValidForEntry_ReturnsTrue()
        {
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Packages/com.unity.demo/file.asset"));
        }

        [Test]
        public void WhenPathIsPackageImportFile_IsPathValidForEntry_ReturnsFalse()
        {
            Assert.IsFalse(AddressableAssetUtility.IsPathValidForEntry("Packages/com.company.demo/package.json"));
        }

        public void WhenPathIsNotPackageImportFile_IsPathValidForEntry_ReturnsTrue()
        {
            Assert.IsTrue(AddressableAssetUtility.IsPathValidForEntry("Packages/com.company.demo/folder/package.json"));
        }

        [Test]
        public void WhenAssetIsNotInPackageFolder_IsPathValidPackageAsset_ReturnsFalse()
        {
            Assert.IsFalse(AddressableAssetUtility.IsPathValidPackageAsset("Assets/file.asset"));
            Assert.IsFalse(AddressableAssetUtility.IsPathValidPackageAsset("Packages/com.company.demo"));
        }

        [Test]
        public void IsEditorTypeRemappedToNull()
        {
            Assert.IsNull(AddressableAssetUtility.MapEditorTypeToRuntimeType(typeof(UnityEditor.AssetImporter), false));
        }

        [Test]
        public void IsRuntimeTypeNotRemapped()
        {
            Assert.AreEqual(AddressableAssetUtility.MapEditorTypeToRuntimeType(typeof(UnityEngine.Vector3), false), typeof(UnityEngine.Vector3));
        }

        [Test]
        public void AreConvertableEditorAssemblyTypesConverted()
        {
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == "UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            var conversionMapping = new Dictionary<Type, Type>()
            {
                { asm.GetType("UnityEditor.Audio.AudioMixerGroupController"), typeof(AudioMixerGroup) },
                { asm.GetType("UnityEditor.Audio.AudioMixerController"), typeof(AudioMixer) },
                { typeof(UnityEditor.SceneAsset),  typeof(UnityEngine.ResourceManagement.ResourceProviders.SceneInstance) },
                { typeof(UnityEditor.Animations.AnimatorController), typeof(RuntimeAnimatorController) }
            };

            foreach (Type key in conversionMapping.Keys)
            {
                var type = AddressableAssetUtility.MapEditorTypeToRuntimeType(key, false);
                Assert.AreEqual(type, conversionMapping[key]);
            }
        }

        [Test]
        public void SafeMoveResourcesToGroup_ResourcesMovedToNewFolderAndGroup()
        {
            var folderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(TestFolder, "Resources"));
            var g1 = CreateTestPrefabAsset(folderPath + "/p1.prefab", "p1");
            var g2 = CreateTestPrefabAsset(folderPath + "/p2.prefab", "p2");
            Assert.AreEqual(0, Settings.DefaultGroup.entries.Count);
            var result = AddressableAssetUtility.SafeMoveResourcesToGroup(Settings, Settings.DefaultGroup, new List<string> { AssetDatabase.GUIDToAssetPath(g1), AssetDatabase.GUIDToAssetPath(g2) }, null, false);
            Assert.IsTrue(result);
            Assert.AreEqual(2, Settings.DefaultGroup.entries.Count);
            var ap = $"{TestFolder}_Resources_moved";
            Assert.IsTrue(AssetDatabase.IsValidFolder($"{TestFolder}/Resources_moved"));
        }

        [Test]
        public void SafeMoveResourcesToGroup_WithInvalidParameters_Fails()
        {
            Assert.IsFalse(AddressableAssetUtility.SafeMoveResourcesToGroup(Settings, null, null, null, false));
            Assert.IsFalse(AddressableAssetUtility.SafeMoveResourcesToGroup(Settings, Settings.DefaultGroup, null, null, false));
        }

        HashSet<string> otherInternaIds = new HashSet<string>(new string[] { "a", "ab", "abc" });

        [TestCase(BundledAssetGroupSchema.AssetNamingMode.FullPath, "Assets/blah/something.asset", "", "Assets/blah/something.asset")]
        [TestCase(BundledAssetGroupSchema.AssetNamingMode.Filename, "Assets/blah/something.asset", "", "something.asset")]
        [TestCase(BundledAssetGroupSchema.AssetNamingMode.Filename, "Assets/blah/somescene.unity", "", "somescene")]
        [TestCase(BundledAssetGroupSchema.AssetNamingMode.GUID, "Assets/blah/something.asset", "guidstring", "guidstring")]
        [TestCase(BundledAssetGroupSchema.AssetNamingMode.Dynamic, "Assets/blah/something.asset", "guidstring", "g")]
        [TestCase(BundledAssetGroupSchema.AssetNamingMode.Dynamic, "Assets/blah/something.asset", "abcd_guidstring", "abcd")]
        [Test]
        public void BundledAssetGroupSchema_GetAssetLoadPath_Returns_ExpectedId(int imode, string assetPath, string guid, string expectedId)
        {
            var mode = (BundledAssetGroupSchema.AssetNamingMode)imode;
            var bas = Settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>();
            bas.InternalIdNamingMode = mode;
            var actualId  = bas.GetAssetLoadPath(assetPath, otherInternaIds, s => guid);
            Assert.AreEqual(expectedId, actualId);
        }

        [Test]
        public void InspectorGUI_GatherTargetinfo_AllAddressable()
        {
            string path1 = GetAssetPath("test.prefab");
            string guid1 = AssetDatabase.AssetPathToGUID(path1);
            Settings.CreateOrMoveEntry(guid1, Settings.DefaultGroup);
            string path2 = GetAssetPath("test 1.prefab");
            string guid2 = AssetDatabase.AssetPathToGUID(path2);
            Settings.CreateOrMoveEntry(guid2, Settings.DefaultGroup);

            UnityEngine.Object[] targets = new UnityEngine.Object[2];
            targets[0] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path1);
            targets[1] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path2);
            var infos = UnityEditor.AddressableAssets.GUI.AddressableAssetInspectorGUI.GatherTargetInfos(targets, Settings);
            
            Assert.AreEqual(2, infos.Count);
            Assert.NotNull(infos[0].MainAssetEntry);
            Assert.NotNull(infos[1].MainAssetEntry);
           
            // clean up
            Settings.RemoveAssetEntry(guid1);
            Settings.RemoveAssetEntry(guid2);
        }
        
        [Test]
        public void InspectorGUI_GatherTargetinfo_MixedAddressable()
        {
            string path1 = GetAssetPath("test.prefab");
            string guid1 = AssetDatabase.AssetPathToGUID(path1);
            Settings.CreateOrMoveEntry(guid1, Settings.DefaultGroup);
            string path2 = GetAssetPath("test 1.prefab");

            UnityEngine.Object[] targets = new UnityEngine.Object[2];
            targets[0] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path1);
            targets[1] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path2);
            var infos = UnityEditor.AddressableAssets.GUI.AddressableAssetInspectorGUI.GatherTargetInfos(targets, Settings);
            
            Assert.AreEqual(2, infos.Count);
            Assert.NotNull(infos[0].MainAssetEntry);
            Assert.IsNull(infos[1].MainAssetEntry);
           
            // clean up
            Settings.RemoveAssetEntry(guid1);
        }
        
        [Test]
        public void InspectorGUI_FindUniqueAssetGuids_CorrectAssetCount()
        {
            string path1 = GetAssetPath("test.prefab");
            string guid1 = AssetDatabase.AssetPathToGUID(path1);
            Settings.CreateOrMoveEntry(guid1, Settings.DefaultGroup);
            string path2 = GetAssetPath("test 1.prefab");

            UnityEngine.Object[] targets = new UnityEngine.Object[2];
            targets[0] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path1);
            targets[1] = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path2);
            var infos = UnityEditor.AddressableAssets.GUI.AddressableAssetInspectorGUI.GatherTargetInfos(targets, Settings);
            
            Assert.AreEqual(2, infos.Count);
            Assert.NotNull(infos[0].MainAssetEntry);
            Assert.IsNull(infos[1].MainAssetEntry);
            
            infos.Add(infos[0]);
            infos.Add(infos[1]);
            UnityEditor.AddressableAssets.GUI.AddressableAssetInspectorGUI.FindUniqueAssetGuids(infos, out var uniqueAssetGuids, out var uniqueAddressableAssetGuids);
            
            Assert.AreEqual(2, uniqueAssetGuids.Count);
            Assert.AreEqual(1, uniqueAddressableAssetGuids.Count);
           
            // clean up
            Settings.RemoveAssetEntry(guid1);
        }

        [Test]
        public void GivenFunction_ParallelForEachAsync_ReturnsCompletedTask()
        {
            //two identical lists
            var originalNums = new List<int>() { 1, 2, 3, 4, 5 };
            var nums = new List<int>() { 1, 2, 3, 4, 5 };

            //function modifies original list by adding one 
            AddressableAssetUtility.ParallelForEachAsync(nums, 5, (num) =>
            {
                originalNums[num - 1] += 1;
                return Task.FromResult(originalNums[num - 1]);
            }).GetAwaiter().GetResult();

            //validate that the modified number matches
            for (var i = 0; i < originalNums.Count; i++)
            {
                Assert.AreEqual(nums[i] + 1, originalNums[i]);
            }
        }

        [Test]
        public void GetMD5Hash_ReturnsValidMD5Hash()
        {
            const string FilePath = "test_file";
            var file = File.Create(FilePath);
            var content = "12345";
            var contentBytes = Encoding.ASCII.GetBytes(content);
            file.Write(contentBytes, 0, contentBytes.Length);
            file.Close();

            var hashString = AddressableAssetUtility.GetMd5Hash(FilePath);
            File.Delete(FilePath);

            Assert.NotNull(hashString);
            Assert.AreEqual("827ccb0eea8a706c4c34a16891f84e7b", hashString);

        }
        
        [Test]
        public void GetPackageVersion_ReturnsAValidVersion()
        {
            string v = AddressableAssetUtility.GetVersionFromPackageData();
            Assert.IsTrue(v.StartsWith("1."), $"Failed to get a valid version from package.json data. Expected 1.X but was {v}");
            Assert.IsTrue(v.Split('.').Length == 2, 
                $"Format from GetVersionFromPackageData, produced {v} which is incorrectly formatted");
        }
        
        [Test]
        public void SortedDelegate_InvokesMultipleRegisters()
        {
            AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]> handler =
                new AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]>();
            
            calledOrder.Clear();
            handler.Register(Callback1, 0);
            handler.Register(Callback2, 0);
            handler.Invoke(null,null,null,null);
            Assert.AreEqual(2, calledOrder.Count, "Expected to have had two callbacks triggered");
        }

        private List<int> calledOrder = new List<int>();
        [Test]
        public void SortedDelegate_InvokesMultipleRegisters_CorrectOrder()
        {
            AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]> handler =
                new AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]>();

            calledOrder.Clear();
            handler.Register(Callback1, 1);
            handler.Register(Callback2, 0);
            
            handler.Invoke(null,null,null,null);
            Assert.AreEqual(2, calledOrder.Count, "Expected to have had two callbacks triggered");
            Assert.AreEqual(2, calledOrder[0], "Callback2 was expected to be called first, but was not");
            Assert.AreEqual(1, calledOrder[1], "Callback1 was expected to be called second, but was not");
        }
        
        [Test]
        public void SortedDelegate_InvokesMultipleRegisters_RemovesCallbacks()
        {
            AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]> handler =
                new AddressableAssetUtility.SortedDelegate<string[], string[], string[], string[]>();

            calledOrder.Clear();
            handler.Register(Callback1, 0);
            handler.Register(Callback2, 0);
            handler.Invoke(null,null,null,null);
            Assert.AreEqual(2, calledOrder.Count, "Expected to have two callbacks Invoked");
            
            calledOrder.Clear();
            handler.Unregister(Callback1);
            handler.Invoke(null,null,null,null);
            Assert.AreEqual(1, calledOrder.Count, "Expected to have one callbacks Invoked");
            
            calledOrder.Clear();
            handler -= Callback2;
            handler.Invoke(null,null,null,null);
            Assert.AreEqual(0, calledOrder.Count, "Expected to have no callbacks Invoked");
            Assert.IsTrue(handler == null, "Equalto to return true to null when no callbacks are available"); // doesn't work with Assert.IsNull
        }

        private void Callback1(string[] arg1, string[] arg2, string[] arg3, string[] arg4)
        {
            calledOrder.Add(1);
        }

        private void Callback2(string[] arg1, string[] arg2, string[] arg3, string[] arg4)
        {
            calledOrder.Add(2);
        }
    }
}
