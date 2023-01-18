using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class BuildScriptTests : AddressableAssetTestBase
    {
        [TestFixture]
        class StreamingAssetTests : AddressableAssetTestBase
        {
            [SetUp]
            public void Setup()
            {
                DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, recursiveDelete: true);
            }

            [TearDown]
            public void TearDown()
            {
                DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, recursiveDelete: true);
            }

            [Test]
            public void ClearCachedData_CleansStreamingAssetFolder()
            {
                var context = new AddressablesDataBuilderInput(Settings);

                int builderCount = 0;
                for (int i = 0; i < Settings.DataBuilders.Count; i++)
                {
                    var builder = Settings.DataBuilders[i] as IDataBuilder;
                    if (builder.CanBuildData<AddressablesPlayerBuildResult>())
                    {
                        builderCount++;
                        var existingFiles = new HashSet<string>();
                        if (System.IO.Directory.Exists("Assets/StreamingAssets"))
                        {
                            foreach (var f in System.IO.Directory.GetFiles("Assets/StreamingAssets"))
                                existingFiles.Add(f);
                        }

                        builder.BuildData<AddressablesPlayerBuildResult>(context);
                        builder.ClearCachedData();
                        if (System.IO.Directory.Exists("Assets/StreamingAssets"))
                        {
                            foreach (var f in System.IO.Directory.GetFiles("Assets/StreamingAssets"))
                                Assert.IsTrue(existingFiles.Contains(f), string.Format("Data Builder {0} did not clean up file {1}", builder.Name, f));
                        }
                    }
                }

                Assert.IsTrue(builderCount > 0);
            }

            [Test]
            public void Folder_WithSubAssets_GetsBundleFileIdAssigned_DuringBuild()
            {
                var context = new AddressablesDataBuilderInput(Settings);
                string folderGuid = AssetDatabase.CreateFolder(TestFolder, "FolderAsset");
                string folderPath = $"{TestFolder}/FolderAsset";
                PrefabUtility.SaveAsPrefabAsset(new GameObject(), $"{folderPath}/subfolderprefab.prefab");

                AddressableAssetEntry folderEntry = Settings.CreateOrMoveEntry(folderGuid, Settings.DefaultGroup);

                Assert.IsTrue(string.IsNullOrEmpty(folderEntry.BundleFileId));

                Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

                Assert.IsTrue(folderEntry.IsFolder);
                Assert.IsFalse(string.IsNullOrEmpty(folderEntry.BundleFileId));

                //Cleanup
                AssetDatabase.DeleteAsset(folderPath);
                Settings.RemoveAssetEntry(folderEntry);
            }

            [Test]
            public void Folder_WithNoSubAssets_DoesNotThrowErrorDuringBuild()
            {
                var context = new AddressablesDataBuilderInput(Settings);
                string folderGuid = AssetDatabase.CreateFolder(TestFolder, "FolderAsset");
                string folderPath = $"{TestFolder}/FolderAsset";

                AddressableAssetEntry folderEntry = Settings.CreateOrMoveEntry(folderGuid, Settings.DefaultGroup);

                Assert.IsTrue(string.IsNullOrEmpty(folderEntry.BundleFileId));

                Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

                Assert.IsTrue(folderEntry.IsFolder);
                Assert.IsTrue(string.IsNullOrEmpty(folderEntry.BundleFileId));

                //Cleanup
                AssetDatabase.DeleteAsset(folderPath);
                Settings.RemoveAssetEntry(folderEntry);
            }

            [Test]
            public void Folder_DoesNotAssignBundleFileId_ForDynamicallyCreatedSubEntries()
            {
                var context = new AddressablesDataBuilderInput(Settings);
                string folderGuid = AssetDatabase.CreateFolder(TestFolder, "FolderAsset");
                string folderPath = $"{TestFolder}/FolderAsset";
                PrefabUtility.SaveAsPrefabAsset(new GameObject(), $"{folderPath}/subfolderprefab.prefab");

                AddressableAssetEntry folderEntry = Settings.CreateOrMoveEntry(folderGuid, Settings.DefaultGroup);

                Settings.ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(context);

                Assert.True(string.IsNullOrEmpty(folderEntry.SubAssets[0].BundleFileId));

                //Cleanup
                AssetDatabase.DeleteAsset(folderPath);
                Settings.RemoveAssetEntry(folderEntry);
            }

#if !UNITY_2021_2_OR_NEWER
            [Test]
            public void CopiedStreamingAssetAreCorrectlyDeleted_DirectoriesWithoutImport()
            {
                var context = new AddressablesDataBuilderInput(Settings);

                int builderCount = 0;
                for (int i = 0; i < Settings.DataBuilders.Count; i++)
                {
                    var builder = Settings.DataBuilders[i] as IDataBuilder;
                    if (builder.CanBuildData<AddressablesPlayerBuildResult>())
                    {
                        builderCount++;

                        // confirm that StreamingAssets does not exists before the test
                        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets"));
                        builder.BuildData<AddressablesPlayerBuildResult>(context);

                        Assert.IsTrue(Directory.Exists(Addressables.BuildPath));
                        AddressablesPlayerBuildProcessor.CopyTemporaryPlayerBuildData();
                        builder.ClearCachedData();

                        Assert.IsTrue(Directory.Exists(Addressables.PlayerBuildDataPath));
                        AddressablesPlayerBuildProcessor.CleanTemporaryPlayerBuildData();
                        Assert.IsFalse(Directory.Exists(Addressables.PlayerBuildDataPath));
                        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets"));
                    }
                }

                Assert.IsTrue(builderCount > 0);
            }

            [Test]
            public void CopiedStreamingAssetAreCorrectlyDeleted_MetaFilesWithImport()
            {
                var context = new AddressablesDataBuilderInput(Settings);

                int builderCount = 0;
                for (int i = 0; i < Settings.DataBuilders.Count; i++)
                {
                    var builder = Settings.DataBuilders[i] as IDataBuilder;
                    if (builder.CanBuildData<AddressablesPlayerBuildResult>())
                    {
                        builderCount++;

                        // confirm that StreamingAssets does not exists before the test
                        DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, recursiveDelete: true);
                        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets"));
                        builder.BuildData<AddressablesPlayerBuildResult>(context);

                        Assert.IsTrue(Directory.Exists(Addressables.BuildPath));
                        AddressablesPlayerBuildProcessor.CopyTemporaryPlayerBuildData();
                        builder.ClearCachedData();

                        // confirm that PlayerBuildDataPath is imported to AssetDatabase
                        AssetDatabase.Refresh();
                        Assert.IsTrue(Directory.Exists(Addressables.PlayerBuildDataPath));
                        Assert.IsTrue(File.Exists(Addressables.PlayerBuildDataPath + ".meta"));
                        string relativePath = Addressables.PlayerBuildDataPath.Replace(Application.dataPath, "Assets");
                        Assert.IsTrue(AssetDatabase.IsValidFolder(relativePath), "Copied StreamingAssets folder was not importer as expected");

                        AddressablesPlayerBuildProcessor.CleanTemporaryPlayerBuildData();
                        Assert.IsFalse(Directory.Exists(Addressables.PlayerBuildDataPath));
                        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets"));
                    }
                }

                Assert.IsTrue(builderCount > 0);
            }

            [Test]
            public void CopiedStreamingAssetAreCorrectlyDeleted_WithExistingFiles()
            {
                var context = new AddressablesDataBuilderInput(Settings);

                int builderCount = 0;
                for (int i = 0; i < Settings.DataBuilders.Count; i++)
                {
                    var builder = Settings.DataBuilders[i] as IDataBuilder;
                    if (builder.CanBuildData<AddressablesPlayerBuildResult>())
                    {
                        builderCount++;

                        // confirm that StreamingAssets does not exists before the test
                        DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, recursiveDelete: true);
                        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets"));

                        // create StreamingAssets and an extra folder as existing content
                        AssetDatabase.CreateFolder("Assets", "StreamingAssets");
                        AssetDatabase.CreateFolder("Assets/StreamingAssets", "extraFolder");

                        builder.BuildData<AddressablesPlayerBuildResult>(context);

                        Assert.IsTrue(Directory.Exists(Addressables.BuildPath));
                        AddressablesPlayerBuildProcessor.CopyTemporaryPlayerBuildData();
                        builder.ClearCachedData();

                        Assert.IsTrue(Directory.Exists(Addressables.PlayerBuildDataPath));
                        AddressablesPlayerBuildProcessor.CleanTemporaryPlayerBuildData();
                        Assert.IsFalse(Directory.Exists(Addressables.PlayerBuildDataPath));
                        Assert.IsTrue(Directory.Exists("Assets/StreamingAssets"));
                        Assert.IsTrue(Directory.Exists("Assets/StreamingAssets/extraFolder"));

                        AssetDatabase.DeleteAsset("Assets/StreamingAssets");
                    }
                }

                Assert.IsTrue(builderCount > 0);
            }

#else
            [Test]
            public void AddressablesBuildPlayerProcessor_IncludeAdditionalStreamingAssetsWhenExist()
            {
                string path = Addressables.BuildPath;
                Directory.CreateDirectory(path);
                var paths = AddressablesPlayerBuildProcessor.GetStreamingAssetPaths();
                Assert.AreEqual(1, paths.Count, "StreamingAssets paths expected to include Addressables.BuildPath");

                // cleanup
                Directory.Delete(path);
            }

            [Test]
            public void AddressablesBuildPlayerProcessor_DoesntIncludeAdditionalStreamingAssetsWhenDontExist()
            {
                if (Directory.Exists(Addressables.BuildPath))
                    DirectoryUtility.DeleteDirectory(Addressables.BuildPath, false);
                var paths = AddressablesPlayerBuildProcessor.GetStreamingAssetPaths();
                Assert.AreEqual(0, paths.Count, "StreamingAssets paths are expected to be empty");

                // cleanup
            }

#endif
        }

        [Test]
        public void BuildScriptBase_FailsCanBuildData()
        {
            var buildScript = ScriptableObject.CreateInstance<BuildScriptBase>();
            Assert.IsFalse(buildScript.CanBuildData<IDataBuilderResult>());
            Assert.IsFalse(buildScript.CanBuildData<AddressableAssetBuildResult>());
            Assert.IsFalse(buildScript.CanBuildData<AddressablesPlayModeBuildResult>());
            Assert.IsFalse(buildScript.CanBuildData<AddressablesPlayerBuildResult>());
        }

        class BuildScriptTestClass : BuildScriptBase
        {
            public override string Name
            {
                get
                {
                    return "Test Script";
                }
            }

            public override bool CanBuildData<T>()
            {
                return typeof(T).IsAssignableFrom(typeof(AddressablesPlayModeBuildResult));
            }

            protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
            {
                Debug.LogError("Inside BuildDataInternal for test script!");
                return base.BuildDataImplementation<TResult>(builderInput);
            }
        }

        [Test]
        public void BuildScript_DoesNotBuildWrongDataType()
        {
            var context = new AddressablesDataBuilderInput(Settings);

            var baseScript = ScriptableObject.CreateInstance<BuildScriptTestClass>();
            baseScript.BuildData<AddressablesPlayerBuildResult>(context);
            LogAssert.Expect(LogType.Error, new Regex("Data builder Test Script cannot build requested type.*"));

            baseScript.BuildData<AddressablesPlayModeBuildResult>(context);
            LogAssert.Expect(LogType.Error, "Inside BuildDataInternal for test script!");
        }

        [Test]
        public void GetNameWithHashNaming_ReturnsNoChangeIfNoHash()
        {
            string source = "x/y.bundle";
            string hash = "123abc";
            string expected = "x/y.bundle";

            var actual = BuildUtility.GetNameWithHashNaming(BundledAssetGroupSchema.BundleNamingStyle.NoHash, hash, source);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetNameWithHashNaming_CanAppendHash()
        {
            string source = "x/y.bundle";
            string hash = "123abc";
            string expected = "x/y_123abc.bundle";

            var actual = BuildUtility.GetNameWithHashNaming(BundledAssetGroupSchema.BundleNamingStyle.AppendHash, hash, source);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetNameWithHashNaming_CanReplaceFileNameWithHash()
        {
            string source = "x/y.bundle";
            string hash = "123abc";
            string expected = "123abc.bundle";

            var actual = BuildUtility.GetNameWithHashNaming(BundledAssetGroupSchema.BundleNamingStyle.OnlyHash, hash, source);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetNameWithHashNaming_CanReplaceFileNameWithFileNameHash()
        {
            string source = "x/y.bundle";
            string hash = HashingMethods.Calculate(source).ToString();
            string expected = hash + ".bundle";

            var actual = BuildUtility.GetNameWithHashNaming(BundledAssetGroupSchema.BundleNamingStyle.FileNameHash, hash, source);

            Assert.AreEqual(expected, actual);
        }

        // regression test for https://jira.unity3d.com/browse/ADDR-1292
        [Test]
        public void BuildScriptBaseWriteBuildLog_WhenDirectoryDoesNotExist_DirectoryCreated()
        {
            string dirName = "SomeTestDir";
            string logFile = Path.Combine(dirName, "AddressablesBuildTEP.json");
            try
            {
                BuildLog log = new BuildLog();
                BuildScriptBase.WriteBuildLog(log, dirName);
                FileAssert.Exists(logFile);
            }
            finally
            {
                Directory.Delete(dirName, true);
            }
        }

        [Test]
        public void Building_CreatesPerformanceReportWithMetaData()
        {
            Settings.BuildPlayerContentImpl();
            string path = Addressables.LibraryPath + "AddressablesBuildTEP.json";
            FileAssert.Exists(path);
            string text = File.ReadAllText(path);
            StringAssert.Contains("com.unity.addressables", text);
        }

        [Test]
        public void Build_WithInvalidAssetInResourcesFolder_Succeeds()
        {
            var path = GetAssetPath("Resources/unknownAsset.plist");
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(path, "nothing");
            AssetDatabase.ImportAsset(path);
            var context = new AddressablesDataBuilderInput(Settings);
            foreach (IDataBuilder db in Settings.DataBuilders)
                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                    db.BuildData<AddressablesPlayerBuildResult>(context);
        }

        [Test]
        public void Build_GroupWithPlayerDataGroupSchemaAndBundledAssetGroupSchema_LogsError()
        {
            const string groupName = "NewGroup";
            var schemas = new List<AddressableAssetGroupSchema> { ScriptableObject.CreateInstance<PlayerDataGroupSchema>(), ScriptableObject.CreateInstance<BundledAssetGroupSchema>() };
            AddressableAssetGroup group = Settings.CreateGroup(groupName, false, false, false, schemas);

            var context = new AddressablesDataBuilderInput(Settings);
            foreach (IDataBuilder db in Settings.DataBuilders)
            {
                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                {
                    AddressablesPlayerBuildResult result = db.BuildData<AddressablesPlayerBuildResult>(context);
                    Assert.AreEqual(result.Error, $"Addressable group {groupName} cannot have both a {typeof(PlayerDataGroupSchema).Name} and a {typeof(BundledAssetGroupSchema).Name}");
                }
            }

            Settings.RemoveGroup(group);
        }

        // ADDR-1755
        [Test]
        public void WhenBundleLocalCatalogEnabled_BuildScriptPacked_DoesNotCreatePerformanceLogReport()
        {
            string logPath = $"Library/com.unity.addressables/aa/{PlatformMappingService.GetPlatformPathSubFolder()}/buildlogtep.json";

            if (File.Exists(logPath))
                File.Delete(logPath);

            Settings.BundleLocalCatalog = true;

            var context = new AddressablesDataBuilderInput(Settings);
            BuildScriptBase db = (BuildScriptBase)Settings.DataBuilders.Find(x => x.GetType() == typeof(BuildScriptPackedMode));

            Assert.IsFalse(File.Exists(logPath)); // make sure file does not exist before build

            var res = db.BuildData<AddressablesPlayerBuildResult>(context);
            Assert.IsFalse(File.Exists(logPath));
        }

        [Test]
        public void Build_WithDeletedAsset_Succeeds()
        {
            var context = new AddressablesDataBuilderInput(Settings);

            //make an entry with no actual AssetPath
            Settings.CreateOrMoveEntry("abcde", Settings.DefaultGroup);
            Settings.CreateOrMoveEntry(m_AssetGUID, Settings.DefaultGroup);
            foreach (BuildScriptBase db in Settings.DataBuilders.OfType<BuildScriptBase>())
            {
                if (db is BuildScriptPackedPlayMode)
                    continue;

                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                {
                    var res = db.BuildData<AddressablesPlayerBuildResult>(context);
                    Assert.IsTrue(db.IsDataBuilt());
                    Assert.IsTrue(string.IsNullOrEmpty(res.Error));
                }
                else if (db.CanBuildData<AddressablesPlayModeBuildResult>())
                {
                    var res = db.BuildData<AddressablesPlayModeBuildResult>(context);
                    Assert.IsTrue(db.IsDataBuilt());
                    Assert.IsTrue(string.IsNullOrEmpty(res.Error));
                }
            }

            Settings.RemoveAssetEntry("abcde", false);
            Settings.RemoveAssetEntry(m_AssetGUID, false);
        }

        [Test]
        public void WhenAddressHasSquareBrackets_AndContentCatalogsAreCreated_BuildFails()
        {
            var context = new AddressablesDataBuilderInput(Settings);

            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(m_AssetGUID, Settings.DefaultGroup);
            entry.address = "[test]";
            LogAssert.Expect(LogType.Error, $"Address '{entry.address}' cannot contain '[ ]'.");
            foreach (IDataBuilder db in Settings.DataBuilders)
            {
                if (db.GetType() == typeof(BuildScriptFastMode) || db.GetType() == typeof(BuildScriptPackedPlayMode))
                    continue;

                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                    db.BuildData<AddressablesPlayerBuildResult>(context);
                else if (db.CanBuildData<AddressablesPlayModeBuildResult>())
                    db.BuildData<AddressablesPlayModeBuildResult>(context);
                LogAssert.Expect(LogType.Error, "Address '[test]' cannot contain '[ ]'.");
            }

            Settings.RemoveAssetEntry(m_AssetGUID, false);
        }

        [Test]
        public void WhenFileTypeIsInvalid_AndContentCatalogsAreCreated_BuildFails()
        {
            var context = new AddressablesDataBuilderInput(Settings);

            string path = GetAssetPath("fake.file");
            FileStream fs = File.Create(path);
            fs.Close();
            AssetDatabase.ImportAsset(path);
            string guid = AssetDatabase.AssetPathToGUID(path);

            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, Settings.DefaultGroup);

            foreach (IDataBuilder db in Settings.DataBuilders)
            {
                if (db.GetType() == typeof(BuildScriptFastMode) ||
                    db.GetType() == typeof(BuildScriptPackedPlayMode))
                    continue;

                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                    db.BuildData<AddressablesPlayerBuildResult>(context);
                else if (db.CanBuildData<AddressablesPlayModeBuildResult>())
                    db.BuildData<AddressablesPlayModeBuildResult>(context);
                LogAssert.Expect(LogType.Error, "Cannot recognize file type for entry located at 'Assets/UnityEditor.AddressableAssets.Tests.BuildScriptTests_Tests/fake.file'. Asset import failed for using an unsupported file type.");
            }
            Settings.RemoveAssetEntry(guid, false);
            AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void WhenFileTypeIsInvalid_AndContentCatalogsAreCreated_IgnoreUnsupportedFilesInBuildIsSet_BuildSucceedWithWarning()
        {
            bool oldValue = Settings.IgnoreUnsupportedFilesInBuild;
            Settings.IgnoreUnsupportedFilesInBuild = true;

            var context = new AddressablesDataBuilderInput(Settings);

            string path = GetAssetPath("fake.file");
            FileStream fs = File.Create(path);
            fs.Close();
            AssetDatabase.ImportAsset(path);
            string guid = AssetDatabase.AssetPathToGUID(path);

            AddressableAssetEntry entry = Settings.CreateOrMoveEntry(guid, Settings.DefaultGroup);

            foreach (BuildScriptBase db in Settings.DataBuilders.OfType<BuildScriptBase>())
            {
                if (db.GetType() == typeof(BuildScriptFastMode) || db.GetType() == typeof(BuildScriptPackedPlayMode))
                    continue;

                if (db.CanBuildData<AddressablesPlayerBuildResult>())
                {
                    var res = db.BuildData<AddressablesPlayerBuildResult>(context);
                    Assert.IsTrue(db.IsDataBuilt());
                    Assert.IsTrue(string.IsNullOrEmpty(res.Error));
                }
                else if (db.CanBuildData<AddressablesPlayModeBuildResult>())
                {
                    var res = db.BuildData<AddressablesPlayModeBuildResult>(context);
                    Assert.IsTrue(db.IsDataBuilt());
                    Assert.IsTrue(string.IsNullOrEmpty(res.Error));
                }

                LogAssert.Expect(LogType.Warning, new Regex($".*{path}.*ignored"));
                LogAssert.Expect(LogType.Warning, new Regex($".*{path}.*stripped"));
            }

            Settings.RemoveAssetEntry(guid, false);
            AssetDatabase.DeleteAsset(path);
            Settings.IgnoreUnsupportedFilesInBuild = oldValue;
        }

        [Test]
        public void WhenLoadPathUsesHttp_AndInsecureHttpNotAllowed_BuildLogsWarning()
        {
#if UNITY_2022_1_OR_NEWER
            InsecureHttpOption oldHttpOption = PlayerSettings.insecureHttpOption;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.NotAllowed;

            string remoteLoadPathId = Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteLoadPath).Id;
            string oldRemoteLoadPath = Settings.profileSettings.GetValueById(Settings.activeProfileId, remoteLoadPathId);
            Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, "http://insecureconnection/");

            AddressableAssetGroup assetGroup = Settings.CreateGroup("InsecureConnections", false, false, false, null, typeof(BundledAssetGroupSchema));
            assetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableById(Settings, Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteBuildPath).Id);
            assetGroup.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableById(Settings, Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteLoadPath).Id);

            string assetPath = Path.Combine(TestFolder, "insecureConnectionsTest.prefab");
            PrefabUtility.SaveAsPrefabAsset(new GameObject(), assetPath);
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), assetGroup, false, false);

            var context = new AddressablesDataBuilderInput(Settings);
            BuildScriptBase db = (BuildScriptBase)Settings.DataBuilders.Find(x => x.GetType() == typeof(BuildScriptPackedMode));
            db.BuildData<AddressablesPlayerBuildResult>(context);
            LogAssert.Expect(LogType.Warning, $"Addressable group {assetGroup.Name} uses insecure http for its load path.  To allow http connections for UnityWebRequests, change your settings in Edit > Project Settings > Player > Other Settings > Configuration > Allow downloads over HTTP.");

            Settings.RemoveGroup(assetGroup);
            Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, oldRemoteLoadPath);
            AssetDatabase.DeleteAsset(assetPath);
            PlayerSettings.insecureHttpOption = oldHttpOption;
#else
            Assert.Ignore($"Skipping test {nameof(WhenLoadPathUsesHttp_AndInsecureHttpNotAllowed_BuildLogsWarning)}.");
#endif
        }

        [Test]
        public void WhenLoadPathUsesHttp_AndInsecureHttpAllowed_BuildSucceeds()
        {
#if UNITY_2022_1_OR_NEWER
            InsecureHttpOption oldHttpOption = PlayerSettings.insecureHttpOption;
            PlayerSettings.insecureHttpOption = InsecureHttpOption.AlwaysAllowed;

            string remoteLoadPathId = Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteLoadPath).Id;
            string oldRemoteLoadPath = Settings.profileSettings.GetValueById(Settings.activeProfileId, remoteLoadPathId);
            Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, "http://insecureconnection/");

            AddressableAssetGroup assetGroup = Settings.CreateGroup("InsecureConnections", false, false, false, null, typeof(BundledAssetGroupSchema));
            assetGroup.GetSchema<BundledAssetGroupSchema>().BuildPath.SetVariableById(Settings, Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteBuildPath).Id);
            assetGroup.GetSchema<BundledAssetGroupSchema>().LoadPath.SetVariableById(Settings, Settings.profileSettings.GetProfileDataByName(AddressableAssetSettings.kRemoteLoadPath).Id);

            string assetPath = Path.Combine(TestFolder, "insecureConnectionsTest.prefab");
            PrefabUtility.SaveAsPrefabAsset(new GameObject(), assetPath);
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), assetGroup, false, false);

            var context = new AddressablesDataBuilderInput(Settings);
            BuildScriptBase db = (BuildScriptBase)Settings.DataBuilders.Find(x => x.GetType() == typeof(BuildScriptPackedMode));
            db.BuildData<AddressablesPlayerBuildResult>(context);
            LogAssert.NoUnexpectedReceived();

            Settings.RemoveGroup(assetGroup);
            Settings.profileSettings.SetValue(Settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath, oldRemoteLoadPath);
            AssetDatabase.DeleteAsset(assetPath);
            PlayerSettings.insecureHttpOption = oldHttpOption;
#else
            Assert.Ignore($"Skipping test {nameof(WhenLoadPathUsesHttp_AndInsecureHttpAllowed_BuildSucceeds)}.");
#endif
        }
    }
}
