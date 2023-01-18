using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace UnityEditor.AddressableAssets.Tests
{
    public class BuildScriptPackedTests : AddressableAssetTestBase
    {
        private AddressablesDataBuilderInput m_BuilderInput;
        private ResourceManagerRuntimeData m_RuntimeData;
        private AddressableAssetsBuildContext m_BuildContext;
        private BuildScriptPackedMode m_BuildScript;
        private AssetBundle m_AssetBundle;

        protected override bool PersistSettings => false;

        private AddressableAssetSettings m_PersistedSettings = null;
        protected new AddressableAssetSettings Settings => m_PersistedSettings != null ? m_PersistedSettings : base.Settings;

        [SetUp]
        protected void Setup()
        {
            using (new IgnoreFailingLogMessage())
            {
                m_BuilderInput = new AddressablesDataBuilderInput(Settings);
                m_BuildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
                m_BuildScript.InitializeBuildContext(m_BuilderInput, out m_BuildContext);
                m_RuntimeData = m_BuildContext.runtimeData;
            }
        }

        [TearDown]
        protected void TearDown()
        {
            m_BuilderInput = null;
            Object.DestroyImmediate(m_BuildScript);
            m_BuildScript = null;
            m_AssetBundle?.Unload(true);
            m_AssetBundle = null;
            Object.DestroyImmediate(m_PersistedSettings, true);
            m_PersistedSettings = null;
        }

        [Test]
        [TestCase(ShaderBundleNaming.ProjectName, "")]
        [TestCase(ShaderBundleNaming.DefaultGroupGuid, "")]
        [TestCase(ShaderBundleNaming.Custom, "custom name")]
        public void ShaderBundleNaming_GeneratesCorrectShaderBundlePrefix(ShaderBundleNaming shaderBundleNaming, string customName)
        {
            //Setup
            string savedCustomName = m_BuildContext.Settings.ShaderBundleCustomNaming;
            ShaderBundleNaming savedBundleNaming = m_BuildContext.Settings.ShaderBundleNaming;
            m_BuildContext.Settings.ShaderBundleCustomNaming = customName;
            m_BuildContext.Settings.ShaderBundleNaming = shaderBundleNaming;
            string expectedValue = "";
            switch (shaderBundleNaming)
            {
                case ShaderBundleNaming.ProjectName:
                    expectedValue = Hash128.Compute(BuildScriptPackedMode.GetProjectName()).ToString();
                    break;
                case ShaderBundleNaming.DefaultGroupGuid:
                    expectedValue = m_BuildContext.Settings.DefaultGroup.Guid;
                    break;
                case ShaderBundleNaming.Custom:
                    expectedValue = customName;
                    break;
            }

            //Test
            string bundleName = BuildScriptPackedMode.GetBuiltInShaderBundleNamePrefix(m_BuildContext);

            //Assert
            Assert.AreEqual(expectedValue, bundleName);

            //Cleanup
            m_BuildContext.Settings.ShaderBundleCustomNaming = savedCustomName;
            m_BuildContext.Settings.ShaderBundleNaming = savedBundleNaming;

        }
        
        [Test]
        [TestCase(MonoScriptBundleNaming.Disabled, "")]
        [TestCase(MonoScriptBundleNaming.ProjectName, "")]
        [TestCase(MonoScriptBundleNaming.DefaultGroupGuid, "")]
        [TestCase(MonoScriptBundleNaming.Custom, "custom name")]
        public void MonoScriptBundleNaming_GeneratesCorrectMonoScriptBundlePrefix(MonoScriptBundleNaming monoScriptBundleNaming, string customName)
        {
            //Setup
            string savedCustomName = m_BuildContext.Settings.MonoScriptBundleCustomNaming;
            MonoScriptBundleNaming savedBundleNaming = m_BuildContext.Settings.MonoScriptBundleNaming;
            m_BuildContext.Settings.MonoScriptBundleCustomNaming = customName;
            m_BuildContext.Settings.MonoScriptBundleNaming = monoScriptBundleNaming;
            string expectedValue = "";
            switch (monoScriptBundleNaming)
            {
                case MonoScriptBundleNaming.ProjectName:
                    expectedValue = Hash128.Compute(BuildScriptPackedMode.GetProjectName()).ToString();
                    break;
                case MonoScriptBundleNaming.DefaultGroupGuid:
                    expectedValue = m_BuildContext.Settings.DefaultGroup.Guid;
                    break;
                case MonoScriptBundleNaming.Custom:
                    expectedValue = customName;
                    break;
                case MonoScriptBundleNaming.Disabled:
                    expectedValue = null;
                    break;
            }

            //Test
            string bundleName = BuildScriptPackedMode.GetMonoScriptBundleNamePrefix(m_BuildContext);

            //Assert
            Assert.AreEqual(expectedValue, bundleName);

            //Cleanup
            m_BuildContext.Settings.MonoScriptBundleCustomNaming = savedCustomName;
            m_BuildContext.Settings.MonoScriptBundleNaming = savedBundleNaming;

        }

        [Test]
        public void SettingsWithMaxConcurrentWebRequests_InitializeBuildContext_SetsMaxConcurrentWebRequestsInRuntimeData()
        {
            Settings.MaxConcurrentWebRequests = 23;
            var builderInput = new AddressablesDataBuilderInput(Settings);
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
            buildScript.InitializeBuildContext(builderInput, out var buildContext);
            Assert.AreEqual(Settings.MaxConcurrentWebRequests, buildContext.runtimeData.MaxConcurrentWebRequests);
        }
        
        [Test]
        public void SettingsWithCatalogTimeout_InitializeBuildContext_SetsCatalogTimeoutInRuntimeData()
        {
            Settings.CatalogRequestsTimeout = 23;
            var builderInput = new AddressablesDataBuilderInput(Settings);
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
            buildScript.InitializeBuildContext(builderInput, out var buildContext);
            Assert.AreEqual(Settings.CatalogRequestsTimeout, buildContext.runtimeData.CatalogRequestsTimeout);
        }

        [Test]
        public void PackedModeScript_CannotBuildPlayContent()
        {
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();

            Assert.IsFalse(buildScript.CanBuildData<AddressablesPlayModeBuildResult>());

            Assert.IsTrue(buildScript.CanBuildData<AddressableAssetBuildResult>());
            Assert.IsTrue(buildScript.CanBuildData<AddressablesPlayerBuildResult>());
        }

        [Test]
        public void WarningIsLogged_WhenAddressableGroupDoesNotContainSchema()
        {
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
            AddressablesDataBuilderInput input = m_BuilderInput;
            var group = input.AddressableSettings.CreateGroup("Invalid Group", false, false, false,
                new List<AddressableAssetGroupSchema>());

            buildScript.BuildData<AddressableAssetBuildResult>(input);

            LogAssert.Expect(LogType.Warning, $"{group.Name} does not have any associated AddressableAssetGroupSchemas. " +
                                              $"Data from this group will not be included in the build. " +
                                              $"If this is unexpected the AddressableGroup may have become corrupted.");

            input.AddressableSettings.RemoveGroup(group);
        }
        
        [Test]
        [TestCase(MonoScriptBundleNaming.Disabled, ShaderBundleNaming.ProjectName, "")]
        [TestCase(MonoScriptBundleNaming.ProjectName, ShaderBundleNaming.ProjectName, "")]
        [TestCase(MonoScriptBundleNaming.DefaultGroupGuid, ShaderBundleNaming.DefaultGroupGuid, "")]
        [TestCase(MonoScriptBundleNaming.Custom, ShaderBundleNaming.Custom, "custom_name")]
        public void GlobalSharedBundles_BuiltWithCorrectName(MonoScriptBundleNaming monoScriptBundleNaming, ShaderBundleNaming shaderNaming, string customName)
        {
            m_PersistedSettings = AddressableAssetSettings.Create(ConfigFolder, k_TestConfigName, true, true);
            m_PersistedSettings.MonoScriptBundleNaming = monoScriptBundleNaming;
            m_PersistedSettings.MonoScriptBundleCustomNaming = customName;
            Setup();

            string assetNamePrefix = "bundlePrefixTest_";
            AddressableAssetGroup assetGroup = null;
            BuildScriptPackedMode buildScript = null;
            
            try
            {
                buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();

                assetGroup = Settings.CreateGroup("TestGroup", false, false, false,
                    new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
                var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
                schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
                Settings.DefaultGroup = assetGroup;

                var testObject = UnityEngine.AddressableAssets.Tests.TestObject.Create("TestScriptableObject", GetAssetPath(assetNamePrefix+"TestScriptableObject.asset"));
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(testObject, out string guid, out long id))
                    return;
                Settings.CreateOrMoveEntry(guid, assetGroup, false, false);

                var a = base.CreateAsset(GetAssetPath(assetNamePrefix + "prefabWithMaterial.prefab"));
                Settings.CreateOrMoveEntry(a, assetGroup, false, false);
                
                buildScript.BuildData<AddressableAssetBuildResult>(m_BuilderInput);
                
                // test
                string monoBundle = BuildScriptPackedMode.GetMonoScriptBundleNamePrefix(Settings);
                if (monoScriptBundleNaming != MonoScriptBundleNaming.Disabled)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(monoBundle), "MonoScript Bundle is enabled but no name recieved");
                    monoBundle = Path.Combine(schema.BuildPath.GetValue(assetGroup.Settings), monoBundle + "_monoscripts.bundle");
                    Assert.IsTrue(File.Exists(monoBundle), "MonoScript bundle not found at " + monoBundle);
                }
                else
                    Assert.IsTrue(string.IsNullOrEmpty(monoBundle), "MonoScript Bundle is disabled but name recieved");
                
                string shaderBundle = BuildScriptPackedMode. GetBuiltInShaderBundleNamePrefix(assetGroup.Settings) + "_unitybuiltinshaders.bundle";
                shaderBundle = Path.Combine(schema.BuildPath.GetValue(assetGroup.Settings), shaderBundle);
                Assert.IsTrue(File.Exists(shaderBundle), "Built in Shaders bundle not found at " + shaderBundle);
            }
            finally
            {
                // cleanup
                Settings.RemoveGroup(assetGroup);
                UnityEngine.Object.DestroyImmediate(buildScript);
            }
        }
        
        [Test]
        public void CatalogBuiltWithDifferentGroupOrder_AreEqualWhenOrderEnabled()
        {
            m_PersistedSettings = AddressableAssetSettings.Create(ConfigFolder, k_TestConfigName, true, true);
            Setup();
            var buildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
            
            AddressableAssetGroup group1 = Settings.CreateGroup("simpleGroup1", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(Path.Combine(TestFolder, "test 1.prefab")),
                group1, false, false);
            AddressableAssetGroup group2 = Settings.CreateGroup("simpleGroup2", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            Settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(Path.Combine(TestFolder, "test 2.prefab")),
                group2, false, false);
            
            var r1 = buildScript.BuildData<AddressableAssetBuildResult>(m_BuilderInput);
            string p = r1.FileRegistry.GetFilePathForBundle("catalog");
            Assert.IsFalse(string.IsNullOrEmpty(p));
            string catalogjson1 = File.ReadAllText(p);
            Assert.IsFalse(string.IsNullOrEmpty(catalogjson1));
            
            Settings.groups.Remove(group1);
            Settings.groups.Remove(group2);
            Settings.groups.Add(group2);
            Settings.groups.Add(group1);
            
            var r2 = buildScript.BuildData<AddressableAssetBuildResult>(m_BuilderInput);
            p = r2.FileRegistry.GetFilePathForBundle("catalog");
            Assert.IsFalse(string.IsNullOrEmpty(p));
            string catalogjson2 = File.ReadAllText(p);
            Assert.IsFalse(string.IsNullOrEmpty(catalogjson2));

            int h1 = catalogjson1.GetHashCode();
            int h2 = catalogjson2.GetHashCode();

            Settings.RemoveGroup(group1);
            Settings.RemoveGroup(group2);
            
            Assert.AreEqual(h1,h2);
        }

        [Test]
        public void SetAssetEntriesBundleFileIdToCatalogEntryBundleFileId_ModifiedOnlyAssetEntries_ThatAreIncludedInBuildWriteData()
        {
            GUID entry1Guid = GUID.Generate();
            GUID entry2Guid = GUID.Generate();
            string bundleFile = "bundle";
            string internalBundleName = "bundlepath";
            string finalBundleName = "finalBundlePath";
            string bundleCatalogEntryInternalId = "catalogentrybundlefileid";

            AddressableAssetEntry entry1 = new AddressableAssetEntry(entry1Guid.ToString(), "123", null, false);
            AddressableAssetEntry entry2 = new AddressableAssetEntry(entry2Guid.ToString(), "456", null, false);
            ICollection<AddressableAssetEntry> entries = new List<AddressableAssetEntry>()
            {
                entry1, entry2
            };

            Dictionary<string, string> bundleToIdMap = new Dictionary<string, string>()
            {
                {internalBundleName, finalBundleName}
            };

            IBundleWriteData writeData = new BundleWriteData();
            writeData.AssetToFiles.Add(entry1Guid, new List<string>() { bundleFile });
            writeData.FileToBundle.Add(bundleFile, internalBundleName);

            Dictionary<string, ContentCatalogDataEntry> catalogMap = new Dictionary<string, ContentCatalogDataEntry>()
            {
                {
                    finalBundleName,
                    new ContentCatalogDataEntry(typeof(IAssetBundleResource), bundleCatalogEntryInternalId,
                        typeof(AssetBundleProvider).FullName, new[] {"catalogentry"})
                }
            };

            BuildScriptPackedMode.SetAssetEntriesBundleFileIdToCatalogEntryBundleFileId(entries, bundleToIdMap, writeData, catalogMap);

            Assert.AreEqual(bundleCatalogEntryInternalId, entry1.BundleFileId);
            Assert.IsNull(entry2.BundleFileId);
        }

        [Test]
        public void SetAssetEntriesBundleFileIdToCatalogEntryBundleFileId_SetsBundleFileIdToBundleNameOnly_WhenGroupSchemaNamingIsSetToFilename()
        {
            //Setup
            GUID entry1Guid = GUID.Generate();
            string bundleFile = "bundle";
            string internalBundleName = "bundlepath";
            string finalBundleName = "finalBundlePath";
            string bundleCatalogEntryInternalIdHashed = "catalogentrybundlefileid_1234567890.bundle";
            string bundleCatalogEntryInternalIdUnHashed = "catalogentrybundlefileid.bundle";

            AddressableAssetEntry entry1 = new AddressableAssetEntry(entry1Guid.ToString(), "123", null, false);
            AddressableAssetGroup group = Settings.CreateGroup("testGroup", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            group.AddAssetEntry(entry1, false);

            ICollection<AddressableAssetEntry> entries = new List<AddressableAssetEntry>()
            {
                entry1
            };

            Dictionary<string, string> bundleToIdMap = new Dictionary<string, string>()
            {
                {internalBundleName, finalBundleName}
            };

            IBundleWriteData writeData = new BundleWriteData();
            writeData.AssetToFiles.Add(entry1Guid, new List<string>() { bundleFile });
            writeData.FileToBundle.Add(bundleFile, internalBundleName);

            Dictionary<string, ContentCatalogDataEntry> catalogMap = new Dictionary<string, ContentCatalogDataEntry>()
            {
                {
                    finalBundleName,
                    new ContentCatalogDataEntry(typeof(IAssetBundleResource), bundleCatalogEntryInternalIdHashed,
                        typeof(AssetBundleProvider).FullName, new[] {"catalogentry"})
                }
            };

            //Test
            BuildScriptPackedMode.SetAssetEntriesBundleFileIdToCatalogEntryBundleFileId(entries, bundleToIdMap, writeData, catalogMap);

            //Assert
            Assert.AreEqual(bundleCatalogEntryInternalIdUnHashed, entry1.BundleFileId);

            //Cleanup
            Settings.RemoveGroup(group);
        }

        [Test]
        public void AddPostCatalogUpdates_AddsCallbackToUpdateBundleLocation_WhenNamingSchemaIsSetToFilenameOnly()
        {
            //Setup
            AddressableAssetGroup group = Settings.CreateGroup("TestAddPostCatalogUpdate", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            List<Action> callbacks = new List<Action>();
            string targetBundlePathHashed = "LocalPathToFile/testbundle_123456.bundle";
            string targetBundlePathUnHashed = "LocalPathToFile/testbundle.bundle";
            string targetBundleInternalIdHashed = "{runtime_val}/testbundle_123456.bundle";
            string targetBundleInternalIdUnHashed = "{runtime_val}/testbundle.bundle";
            ContentCatalogDataEntry dataEntry = new ContentCatalogDataEntry(typeof(ContentCatalogData), targetBundleInternalIdHashed, typeof(BundledAssetProvider).FullName, new List<object>());
            FileRegistry registry = new FileRegistry();
            registry.AddFile(targetBundlePathHashed);
            m_BuildScript.AddPostCatalogUpdatesInternal(group, callbacks, dataEntry, targetBundlePathHashed, registry);

            //Assert setup
            Assert.AreEqual(1, callbacks.Count);
            Assert.AreEqual(targetBundleInternalIdHashed, dataEntry.InternalId);

            //Test
            callbacks[0].Invoke();

            //Assert
            Assert.AreEqual(targetBundleInternalIdUnHashed, dataEntry.InternalId);
            Assert.AreEqual(registry.GetFilePathForBundle("testbundle"), targetBundlePathUnHashed);

            //Cleanup
            Settings.RemoveGroup(group);
        }

        [Test]
        public void AddPostCatalogUpdatesInternal_DoesNotAttemptToRemoveHashUnnecessarily()
        {
            AddressableAssetGroup group = Settings.CreateGroup("TestAddPostCatalogUpdate", false, false, false,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
            List<Action> callbacks = new List<Action>();
            string targetBundlePathHashed = "LocalPathToFile/testbundle_test_123456.bundle";
            string targetBundlePathUnHashed = "LocalPathToFile/testbundle_test.bundle";
            string targetBundleInternalId= "{runtime_val}/testbundle_test.bundle";
            ContentCatalogDataEntry dataEntry = new ContentCatalogDataEntry(typeof(ContentCatalogData), targetBundleInternalId, typeof(BundledAssetProvider).FullName, new List<object>());
            FileRegistry registry = new FileRegistry();
            registry.AddFile(targetBundlePathHashed);
            m_BuildScript.AddPostCatalogUpdatesInternal(group, callbacks, dataEntry, targetBundlePathHashed, registry);
            
            //Assert Setup
            Assert.AreEqual(1, callbacks.Count);
            Assert.AreEqual(targetBundleInternalId, dataEntry.InternalId);
            
            //Test
            callbacks[0].Invoke();

            //Assert
            //InternalId should not have changed since it was already unhashed.
            Assert.AreEqual(targetBundleInternalId, dataEntry.InternalId);
            Assert.AreEqual(registry.GetFilePathForBundle("testbundle_test"), targetBundlePathUnHashed);
            
            //Cleanup
            Settings.RemoveGroup(group);
        }

        [Test]
        public void ErrorCheckBundleSettings_FindsNoProblemsInDefaultScema()
        {
            var group = Settings.CreateGroup("PackedTest", false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();

            var errorStr = BuildScriptPackedMode.ErrorCheckBundleSettings(schema, group, Settings);
            LogAssert.NoUnexpectedReceived();
            Assert.IsTrue(string.IsNullOrEmpty(errorStr));
        }

        [Test]
        public void ErrorCheckBundleSettings_WarnsOfMismatchedBuildPath()
        {
            var group = Settings.CreateGroup("PackedTest", false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BuildPath.Id = "BadPath";

            var errorStr = BuildScriptPackedMode.ErrorCheckBundleSettings(schema, group, Settings);
            LogAssert.NoUnexpectedReceived();
            Assert.IsTrue(errorStr.Contains("is set to the dynamic-lookup version of StreamingAssets, but BuildPath is not."));
        }

        [Test]
        public void ErrorCheckBundleSettings_WarnsOfMismatchedLoadPath()
        {
            var group = Settings.CreateGroup("PackedTest", false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.LoadPath.Id = "BadPath";

            var errorStr = BuildScriptPackedMode.ErrorCheckBundleSettings(schema, group, Settings);
            LogAssert.NoUnexpectedReceived();
            Assert.IsTrue(errorStr.Contains("is set to the dynamic-lookup version of StreamingAssets, but LoadPath is not."));
        }

        [Test]
        public void WhenUsingLocalContentAndCompressionIsLZMA_ErrorCheckBundleSettings_LogsWarning()
        {
            var group = Settings.CreateGroup("PackedTest", false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZMA;

            BuildScriptPackedMode.ErrorCheckBundleSettings(schema, group, Settings);
            LogAssert.Expect(LogType.Warning, $"Bundle compression is set to LZMA, but group {group.Name} uses local content.");
        }

        private static IEnumerable<List<AssetBundleBuild>> DuplicateBundleNamesCases()
        {
            var abb1 = new AssetBundleBuild() { assetBundleName = "name1.bundle" };
            var abb2 = new AssetBundleBuild() { assetBundleName = "name2.bundle" };
            yield return new List<AssetBundleBuild>();
            yield return new List<AssetBundleBuild>() { abb1 };
            yield return new List<AssetBundleBuild>() { abb1, abb1 };
            yield return new List<AssetBundleBuild>() { abb1, abb2 };
            yield return new List<AssetBundleBuild>() { abb1, abb1, abb1 };
        }

        [Test, TestCaseSource(nameof(DuplicateBundleNamesCases))]
        public void HandleBundlesNaming_NamesShouldAlwaysBeUnique(List<AssetBundleBuild> bundleBuilds)
        {
            var group = Settings.CreateGroup("PackedTest", false, false, false, null, typeof(BundledAssetGroupSchema));
            var bundleToAssetGroup = new Dictionary<string, string>();

            List<string> uniqueNames = BuildScriptPackedMode.HandleDuplicateBundleNames(bundleBuilds, bundleToAssetGroup, group.Guid);

            var uniqueNamesInBundleBuilds = bundleBuilds.Select(b => b.assetBundleName).Distinct();
            Assert.AreEqual(bundleBuilds.Count, uniqueNames.Count());
            Assert.AreEqual(bundleBuilds.Count, uniqueNamesInBundleBuilds.Count());
            Assert.AreEqual(bundleBuilds.Count, bundleToAssetGroup.Count);
        }

        [Test]
        public void CreateCatalogFiles_NullArgs_ShouldFail()
        {
            var jsonText = "Some text in catalog file";
            var result = m_BuildScript.CreateCatalogFiles(jsonText, m_BuilderInput, null);
            Assert.IsFalse(result);
            LogAssert.Expect(LogType.Error, new Regex("catalog", RegexOptions.IgnoreCase));

            result = m_BuildScript.CreateCatalogFiles(jsonText, null, m_BuildContext);
            Assert.IsFalse(result);
            LogAssert.Expect(LogType.Error, new Regex("catalog", RegexOptions.IgnoreCase));

            result = m_BuildScript.CreateCatalogFiles(null, m_BuilderInput, m_BuildContext);
            Assert.IsFalse(result);
            LogAssert.Expect(LogType.Error, new Regex("catalog", RegexOptions.IgnoreCase));
        }

        [Test]
        public void CreateCatalogFiles_DefaultOptions_ShouldCreateLocalJsonCatalogFile()
        {
            var fileName = m_BuilderInput.RuntimeCatalogFilename;
            var jsonText = "Some text in catalog file";

            var result = m_BuildScript.CreateCatalogFiles(jsonText, m_BuilderInput, m_BuildContext);

            Assert.IsTrue(result);

            // Assert locations
            Assert.IsTrue(m_RuntimeData.CatalogLocations.Count == 1);
            Assert.IsTrue(m_RuntimeData.CatalogLocations.Any(l => l.InternalId.EndsWith(fileName)));

            // Assert file paths
            var registryPaths = m_BuilderInput.Registry.GetFilePaths().ToList();
            Assert.IsTrue(registryPaths.Any(p => p.EndsWith(fileName)));
            Assert.IsTrue(File.Exists(registryPaths.First(p => p.EndsWith(fileName))));
        }

        [Test]
        public void CreateCatalogFiles_BundleLocalCatalog_ShouldCreateLocalCatalogFileInAssetBundle()
        {
            Settings.BundleLocalCatalog = true;

            var defaultFileName = m_BuilderInput.RuntimeCatalogFilename;
            var bundleFileName = defaultFileName.Replace(".json", ".bundle");
            var jsonText = "Some text in catalog file";

            var result = m_BuildScript.CreateCatalogFiles(jsonText, m_BuilderInput, m_BuildContext);

            Assert.IsTrue(result);

            // Assert locations
            Assert.AreEqual(1, m_RuntimeData.CatalogLocations.Count);
            Assert.AreEqual(1, m_RuntimeData.CatalogLocations.Count(l => l.InternalId.EndsWith(bundleFileName)));

            // Assert file paths
            var registryPaths = m_BuilderInput.Registry.GetFilePaths().ToList();
            var registryBundlePath = registryPaths.First(p => p.EndsWith(bundleFileName));
            Assert.AreEqual(1, registryPaths.Count(p => p.EndsWith(bundleFileName)));
            Assert.AreEqual(0, registryPaths.Count(p => p.EndsWith(defaultFileName)));
            Assert.IsTrue(File.Exists(registryBundlePath));

            // Assert catalogs
            m_AssetBundle = AssetBundle.LoadFromFile(registryBundlePath);
            Assert.IsNotNull(m_AssetBundle);

            var assets = m_AssetBundle.LoadAllAssets<TextAsset>();
            Assert.AreEqual(1, assets.Length);
            Assert.AreEqual(jsonText, assets.First().text);
        }

        [Test]
        public void CreateCatalogFiles_BundleLocalCatalog_BuildRemoteCatalog_ShouldCreateCatalogBundleAndRemoteJsonCatalog()
        {
            // Creating a bundle causes a domain reload and settings need to be persisted to be able to access profile variables.
            m_PersistedSettings = AddressableAssetSettings.Create(ConfigFolder, k_TestConfigName, true, true);
            Setup();

            Settings.BundleLocalCatalog = true;
            Settings.BuildRemoteCatalog = true;
            Settings.RemoteCatalogBuildPath = new ProfileValueReference();
            Settings.RemoteCatalogBuildPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteBuildPath);
            Settings.RemoteCatalogLoadPath = new ProfileValueReference();
            Settings.RemoteCatalogLoadPath.SetVariableByName(Settings, AddressableAssetSettings.kRemoteLoadPath);

            var defaultFileName = m_BuilderInput.RuntimeCatalogFilename;
            var bundleFileName = defaultFileName.Replace(".json", ".bundle");
            var jsonText = "Some text in catalog file";

            var result = m_BuildScript.CreateCatalogFiles(jsonText, m_BuilderInput, m_BuildContext);

            Assert.IsTrue(result);

            // Assert locations
            Assert.AreEqual(3, m_RuntimeData.CatalogLocations.Count);
            Assert.AreEqual(1, m_RuntimeData.CatalogLocations.Count(l => l.InternalId.EndsWith(bundleFileName)));
            Assert.AreEqual(2, m_RuntimeData.CatalogLocations.Count(l => l.InternalId.EndsWith(".hash")));

            // Assert file paths
            var remoteBuildFolder = Settings.RemoteCatalogBuildPath.GetValue(Settings);
            var registryPaths = m_BuilderInput.Registry.GetFilePaths().ToList();
            Assert.AreEqual(3, registryPaths.Count);
            Assert.AreEqual(1, registryPaths.Count(p => p.EndsWith(bundleFileName)));
            Assert.AreEqual(1, registryPaths.Count(p => p.Contains(remoteBuildFolder) && p.EndsWith(".json")));
            Assert.AreEqual(1, registryPaths.Count(p => p.Contains(remoteBuildFolder) && p.EndsWith(".hash")));

            var registryBundlePath = registryPaths.First(p => p.EndsWith(bundleFileName));
            var registryRemoteCatalogPath = registryPaths.First(p => p.Contains(remoteBuildFolder) && p.EndsWith(".json"));
            var registryRemoteHashPath = registryPaths.First(p => p.Contains(remoteBuildFolder) && p.EndsWith(".hash"));
            Assert.IsTrue(File.Exists(registryBundlePath));
            Assert.IsTrue(File.Exists(registryRemoteCatalogPath));
            Assert.IsTrue(File.Exists(registryRemoteHashPath));

            // Assert catalogs
            m_AssetBundle = AssetBundle.LoadFromFile(registryBundlePath);
            Assert.IsNotNull(m_AssetBundle);

            var assets = m_AssetBundle.LoadAllAssets<TextAsset>();
            Assert.AreEqual(1, assets.Length);
            Assert.AreEqual(jsonText, assets.First().text);

            var remoteCatalogText = File.ReadAllText(registryRemoteCatalogPath);
            Assert.AreEqual(jsonText, remoteCatalogText);

            File.Delete(registryRemoteCatalogPath);
            File.Delete(registryRemoteHashPath);
        }
    }

    class ProcessPlayerDataSchemaTests : EditorAddressableAssetsTestFixture
    {
        AddressablesDataBuilderInput m_BuilderInput;
        BuildScriptPackedMode m_BuildScript;
        AddressableAssetsBuildContext m_BuildContext;
        EditorBuildSettingsScene[] m_ScenesBkp;

        const string k_SchemaTestFolderPath = TempPath + "/SchemaTests";

        [SetUp]
        protected void PlayerDataSchemaTestsSetup()
        {
            m_BuilderInput = new AddressablesDataBuilderInput(m_Settings);
            m_BuildScript = ScriptableObject.CreateInstance<BuildScriptPackedMode>();
            m_BuildScript.InitializeBuildContext(m_BuilderInput, out m_BuildContext);

            m_ScenesBkp = EditorBuildSettings.scenes;
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
        }

        [TearDown]
        protected void PlayerDataSchemaTestsTearDown()
        {
            Object.DestroyImmediate(m_BuildScript);
            EditorBuildSettings.scenes = m_ScenesBkp;
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenIncludeBuildSettingsScenesIsFalse_ShouldNotGenerateAnySceneLocations()
        {
            var scenePath = k_SchemaTestFolderPath + "/testScene.unity";
            Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
            CreateScene(scenePath, Path.GetFileNameWithoutExtension(scenePath));

            var scenes = EditorBuildSettings.scenes;
            Assert.AreEqual(1, scenes.Length);

            var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
            var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
            bool includeBuildSettingsScenes = schema.IncludeBuildSettingsScenes;
            schema.IncludeBuildSettingsScenes = false;

            var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
            Assert.True(string.IsNullOrEmpty(errorStr));

            var actualLocations = m_BuildContext.locations.Where(l => l.ResourceType == typeof(SceneInstance)).ToList();
            Assert.AreEqual(0, actualLocations.Count);

            // TODO : Assert for existence of SceneProvider (not exposed)

            schema.IncludeBuildSettingsScenes = includeBuildSettingsScenes;
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenNoBuildSettingsScenesAreIncluded_ShouldNotGenerateAnySceneLocations()
        {
            var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
            var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
            bool includeBuildSettingsScenes = schema.IncludeBuildSettingsScenes;
            schema.IncludeBuildSettingsScenes = true;

            var scenes = EditorBuildSettings.scenes;
            Assert.AreEqual(0, scenes.Length);

            var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
            Assert.True(string.IsNullOrEmpty(errorStr));

            var actualLocations = m_BuildContext.locations.Where(l => l.ResourceType == typeof(SceneInstance)).ToList();
            Assert.AreEqual(0 , actualLocations.Count);

            // TODO : Assert for existence of SceneProvider (not exposed)

            schema.IncludeBuildSettingsScenes = includeBuildSettingsScenes;
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenMultipleBuildSettingsScenesAreIncluded_ShouldGenerateCorrectLocations()
        {
            var scenePaths = new[]
            {
                k_SchemaTestFolderPath + "/testScene.unity",
                k_SchemaTestFolderPath + "/OtherFolder/testScene2.unity"
            };
            foreach (string scenePath in scenePaths)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(scenePath));
                CreateScene(scenePath, Path.GetFileNameWithoutExtension(scenePath));
            }

            var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
            var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
            bool includeBuildSettingsScenes = schema.IncludeBuildSettingsScenes;
            schema.IncludeBuildSettingsScenes = true;

            var scenes = EditorBuildSettings.scenes;
            Assert.AreEqual(2, scenes.Length);

            var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
            Assert.True(string.IsNullOrEmpty(errorStr));

            var actualLocations = m_BuildContext.locations.Where(l => l.ResourceType == typeof(SceneInstance)).ToList();

            var expectedLocationIds = scenePaths.Select(s => s.Replace(".unity", "").Replace("Assets/", ""));
            Assert.AreEqual(expectedLocationIds.Count(), actualLocations.Count);
            Assert.AreEqual(expectedLocationIds, actualLocations.Select(l => l.InternalId));

            // TODO : Assert for existence of SceneProvider (not exposed)

            schema.IncludeBuildSettingsScenes = includeBuildSettingsScenes;
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenIncludeResourcesFoldersIsFalse_ShouldNotGenerateAnyResourceLocationsOrProviders()
        {
            var assetPath = k_SchemaTestFolderPath + "/Resources/testResource1.prefab";
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            CreateAsset(assetPath, Path.GetFileNameWithoutExtension(assetPath));
            Assert.IsTrue(File.Exists(assetPath));

            var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
            var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
            schema.IncludeResourcesFolders = false;

            var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
            Assert.True(string.IsNullOrEmpty(errorStr));

            var actualLocations = m_BuildContext.locations.Where(
                l => l.ResourceType == typeof(GameObject) && l.Provider == typeof(LegacyResourcesProvider).FullName).ToList();
            Assert.AreEqual(0, actualLocations.Count);

            var actualProviders = m_BuildScript.ResourceProviderData.Where(rpd => rpd.ObjectType.ClassName == typeof(LegacyResourcesProvider).FullName).ToList();
            Assert.AreEqual(0, actualProviders.Count);
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenNoResourcesAreIncluded_ShouldNotGenerateAnyResourceLocationsOrProviders()
        {
            using (new HideResourceFoldersScope())
            {
                var resourceFolder = k_SchemaTestFolderPath + "/Resources";
                int builtInPackagesResourceCount = ResourcesTestUtility.GetResourcesEntryCount(m_Settings, true);
                Directory.CreateDirectory(resourceFolder);

                var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
                var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
                schema.IncludeResourcesFolders = true;

                var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
                Assert.True(string.IsNullOrEmpty(errorStr));

                var actualLocations = m_BuildContext.locations.Where(
                        l => l.ResourceType == typeof(GameObject) &&
                             l.Provider == typeof(LegacyResourcesProvider).FullName)
                    .ToList();
                Assert.AreEqual(0, actualLocations.Count);

                var actualProviders = m_BuildScript.ResourceProviderData
                    .Where(rpd => rpd.ObjectType.ClassName == typeof(LegacyResourcesProvider).FullName).ToList();
                Assert.AreEqual(builtInPackagesResourceCount > 0 ? 1 : 0, actualProviders.Count);
            }
        }

        [Test]
        public void ProcessPlayerDataSchema_WhenMultipleResourcesAreIncluded_ShouldGenerateCorrectResourceLocationsAndProviders()
        {
            using (new HideResourceFoldersScope())
            {
                var resourcesPaths = new[]
                {
                    k_SchemaTestFolderPath + "/OtherFolder/Resources/testResource2.prefab",
                    k_SchemaTestFolderPath + "/Resources/testResource1.prefab"
                };
                foreach (string resourcePath in resourcesPaths)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(resourcePath));
                    CreateAsset(resourcePath, Path.GetFileNameWithoutExtension(resourcePath));
                    Assert.IsTrue(File.Exists(resourcePath));
                }

                var group = m_Settings.FindGroup(AddressableAssetSettings.PlayerDataGroupName);
                var schema = group.GetSchema(typeof(PlayerDataGroupSchema)) as PlayerDataGroupSchema;
                bool includeResourceFolders = schema.IncludeResourcesFolders;
                schema.IncludeResourcesFolders = true;

                var errorStr = m_BuildScript.ProcessPlayerDataSchema(schema, group, m_BuildContext);
                Assert.True(string.IsNullOrEmpty(errorStr));

                var actualLocations = m_BuildContext.locations.Where(
                        l => l.ResourceType == typeof(GameObject) &&
                             l.Provider == typeof(LegacyResourcesProvider).FullName)
                    .ToList();
                var expectedLocationIds = resourcesPaths.Distinct()
                    .Select(s => Path.GetFileName(s).Replace(".prefab", "")).ToList();
                Assert.AreEqual(expectedLocationIds.Count, actualLocations.Count);
                Assert.AreEqual(expectedLocationIds, actualLocations.Select(l => l.InternalId));

                var actualProviders = m_BuildScript.ResourceProviderData
                    .Where(rpd => rpd.ObjectType.ClassName == typeof(LegacyResourcesProvider).FullName).ToList();
                Assert.AreEqual(1, actualProviders.Count);

                schema.IncludeResourcesFolders = includeResourceFolders;
            }
        }
    }

    class PrepGroupBundlePackingTests : EditorAddressableAssetsTestFixture
    {
        void CreateGroupWithAssets(string groupName, int assetEntryCount, out AddressableAssetGroup group, out List<AddressableAssetEntry> entries)
        {
            group = m_Settings.CreateGroup(groupName, false, false, false, null, typeof(BundledAssetGroupSchema));
            entries = new List<AddressableAssetEntry>();
            for (int i = 0; i < assetEntryCount; i++)
            {
                AddressableAssetEntry e = new AddressableAssetEntry($"111{i}", $"addr{i}", group, false);
                e.m_cachedAssetPath = $"DummyPath{i}";
                group.AddAssetEntry(e);
                entries.Add(e);
            }
        }

        [Test]
        public void CalculateGroupHash_WithGroupGuidMode_GeneratesStableBundleNameWhenEntriesChange()
        {
            var group = m_Settings.CreateGroup(nameof(CalculateGroupHash_WithGroupGuidMode_GeneratesStableBundleNameWhenEntriesChange), false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            var expected = group.Guid;
            Assert.AreEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuid, group, group.entries));
            group.AddAssetEntry(new AddressableAssetEntry("test", "test", group, true));
            Assert.AreEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuid, group, group.entries));
            m_Settings.RemoveGroupInternal(group, true, false);
        }

        [Test]
        public void CalculateGroupHash_WithGroupGuidProjectIdMode_GeneratesStableBundleNameWhenEntriesChange()
        {
            var group = m_Settings.CreateGroup(nameof(CalculateGroupHash_WithGroupGuidProjectIdMode_GeneratesStableBundleNameWhenEntriesChange), false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            var expected = HashingMethods.Calculate(group.Guid, Application.cloudProjectId).ToString();
            Assert.AreEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdHash, group, group.entries));
            group.AddAssetEntry(new AddressableAssetEntry("test", "test", group, true));
            Assert.AreEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdHash, group, group.entries));
            m_Settings.RemoveGroupInternal(group, true, false);
        }

        [Test]
        public void CalculateGroupHash_WithGroupGuidProjectIdEntryHashMode_GeneratesNewBundleNameWhenEntriesChange()
        {
            var group = m_Settings.CreateGroup(nameof(CalculateGroupHash_WithGroupGuidProjectIdEntryHashMode_GeneratesNewBundleNameWhenEntriesChange), false, false, false, null, typeof(BundledAssetGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            var expected = HashingMethods.Calculate(group.Guid, Application.cloudProjectId, new HashSet<string>(group.entries.Select(e => e.guid))).ToString();
            Assert.AreEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash, group, group.entries));
            group.AddAssetEntry(new AddressableAssetEntry("test", "test", group, true));
            Assert.AreNotEqual(expected, BuildScriptPackedMode.CalculateGroupHash(BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash, group, group.entries));
            m_Settings.RemoveGroupInternal(group, true, false);
        }

        [Test]
        public void GenerateBuildInputDefinition_WithInternalIdModes_GeneratesExpectedAddresses()
        {
            var group = m_Settings.CreateGroup("DynamicInternalIdGroup", false, false, false, null, typeof(BundledAssetGroupSchema));
            var entries = new List<AddressableAssetEntry>();
            AddressableAssetEntry e = new AddressableAssetEntry($"abcde", $"addr0", group, false);
            e.m_cachedAssetPath = "Assets/DummyPath0.asset";
            entries.Add(e);

            e = new AddressableAssetEntry($"abcdf", $"addr0", group, false);
            e.m_cachedAssetPath = "Assets/DummyPath0.asset";
            entries.Add(e);

            e = new AddressableAssetEntry($"abcdg", $"addr0", group, false);
            e.m_cachedAssetPath = "Assets/DummyPath0.asset";
            entries.Add(e);

            e = new AddressableAssetEntry($"axcde", $"addr0", group, false);
            e.m_cachedAssetPath = "Assets/DummyPath0.asset";
            entries.Add(e);

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.FullPath;
            var bundleBuild = BuildScriptPackedMode.GenerateBuildInputDefinition(entries, "bundle");
            Assert.AreEqual("Assets/DummyPath0.asset", bundleBuild.addressableNames[0]);

            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Filename;
            bundleBuild = BuildScriptPackedMode.GenerateBuildInputDefinition(entries, "bundle");
            Assert.AreEqual("DummyPath0.asset", bundleBuild.addressableNames[0]);

            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
            bundleBuild = BuildScriptPackedMode.GenerateBuildInputDefinition(entries, "bundle");
            Assert.AreEqual("abcde", bundleBuild.addressableNames[0]);

            schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.Dynamic;
            bundleBuild = BuildScriptPackedMode.GenerateBuildInputDefinition(entries, "bundle");
            Assert.AreEqual("a", bundleBuild.addressableNames[0]);
            Assert.AreEqual("ab", bundleBuild.addressableNames[1]);
            Assert.AreEqual("abc", bundleBuild.addressableNames[2]);
            Assert.AreEqual("ax", bundleBuild.addressableNames[3]);
        }

        [Test]
        public void PrepGroupBundlePacking_WhenEntriesDontExpand_AllAssetEntriesAreReturned([Values] BundledAssetGroupSchema.BundlePackingMode mode)
        {
            int entryCount = 2;
            CreateGroupWithAssets("PrepGroup1", entryCount, out AddressableAssetGroup group, out List<AddressableAssetEntry> entries);
            for (int i = 0; i < entryCount; i++)
                entries[i].SetLabel($"label", true, true, false);
            List<AssetBundleBuild> buildInputDefs = new List<AssetBundleBuild>();
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.BundleMode = mode;
            List <AddressableAssetEntry> retEntries = BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs, schema);
            CollectionAssert.AreEquivalent(retEntries, entries);
        }

        [Test]
        public void PrepGroupBundlePacking_PackSeperate_GroupChangeDoesntAffectOtherAssetsBuildInput()
        {
            CreateGroupWithAssets("PrepGroup", 2, out AddressableAssetGroup group, out List<AddressableAssetEntry> entries);
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            List<AssetBundleBuild> buildInputDefs = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs, schema);

            group.RemoveAssetEntry(entries[1]);

            List<AssetBundleBuild> buildInputDefs2 = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs2, schema);

            Assert.AreEqual(buildInputDefs[0].assetBundleName, buildInputDefs2[0].assetBundleName);
        }

        [Test]
        public void PrepGroupBundlePacking_PackTogether_GroupChangeDoesAffectBuildInput()
        {
            CreateGroupWithAssets("PrepGroup", 2, out AddressableAssetGroup group, out List<AddressableAssetEntry> entries);
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash;

            List<AssetBundleBuild> buildInputDefs = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs, schema);

            group.RemoveAssetEntry(entries[1]);

            List<AssetBundleBuild> buildInputDefs2 = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs2, schema);

            Assert.AreNotEqual(buildInputDefs[0].assetBundleName, buildInputDefs2[0].assetBundleName);
        }

        [Test]
        public void PrepGroupBundlePacking_PackTogetherByLabel_GroupChangeDoesAffectBuildInput()
        {
            //Setup
            CreateGroupWithAssets("PrepGroup", 2, out AddressableAssetGroup group, out List<AddressableAssetEntry> entries);
            string label = "testlabel";
            entries[0].SetLabel(label, true, true, false);
            entries[1].SetLabel(label, true, true, false);
            var schema = ScriptableObject.CreateInstance<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
            schema.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdEntriesHash;

            List<AssetBundleBuild> buildInputDefs = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs, schema);

            entries[1].SetLabel(label, false, true, false);

            List<AssetBundleBuild> buildInputDefs2 = new List<AssetBundleBuild>();
            BuildScriptPackedMode.PrepGroupBundlePacking(group, buildInputDefs2, schema);

            Assert.AreNotEqual(buildInputDefs[0].assetBundleName, buildInputDefs2[0].assetBundleName);
        }
    }
}
