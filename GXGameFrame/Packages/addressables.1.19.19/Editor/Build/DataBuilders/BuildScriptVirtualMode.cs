using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;
using UnityEngine.ResourceManagement.Util;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    /// <summary>
    /// Build script for creating virtual asset bundle dat for running in the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildScriptVirtual.asset", menuName = "Addressables/Content Builders/Simulate Groups (advanced)")]
    public class BuildScriptVirtualMode : BuildScriptBase
    {
        /// <inheritdoc />
        public override string Name
        {
            get { return "Simulate Groups (advanced)"; }
        }

        /// <inheritdoc />
        public override bool CanBuildData<T>()
        {
            return typeof(T).IsAssignableFrom(typeof(AddressablesPlayModeBuildResult));
        }

        /// <inheritdoc />
        public override void ClearCachedData()
        {
            DeleteFile(string.Format(m_PathFormat, "", "catalog"));
            DeleteFile(string.Format(m_PathFormat, "", "settings"));
        }

        /// <inheritdoc />
        public override bool IsDataBuilt()
        {
            var catalogPath = string.Format(m_PathFormat, "", "catalog");
            var settingsPath = string.Format(m_PathFormat, "", "settings");
            return File.Exists(catalogPath) &&
                File.Exists(settingsPath);
        }

        private string m_pathFormatStore;
        private string m_PathFormat
        {
            get
            {
                if (string.IsNullOrEmpty(m_pathFormatStore))
                    m_pathFormatStore = "{0}" + Addressables.LibraryPath + "{1}_BuildScriptVirtualMode.json";
                return m_pathFormatStore;
            }
            set { m_pathFormatStore = value; }
        }

        List<ObjectInitializationData> m_ResourceProviderData;
        List<AssetBundleBuild> m_AllBundleInputDefinitions;
        Dictionary<string, VirtualAssetBundleRuntimeData> m_CreatedProviderIds;

        /// <inheritdoc />
        protected override TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput)
        {
            TResult result = default(TResult);

            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            var aaSettings = builderInput.AddressableSettings;

            m_PathFormat = builderInput.PathFormat;


            //gather entries
            var aaContext = new AddressableAssetsBuildContext
            {
                Settings = aaSettings,
                runtimeData = new ResourceManagerRuntimeData(),
                bundleToAssetGroup = new Dictionary<string, string>(),
                locations = new List<ContentCatalogDataEntry>(),
                providerTypes = new HashSet<Type>(),
                assetEntries = new List<AddressableAssetEntry>()
            };
            m_AllBundleInputDefinitions = new List<AssetBundleBuild>();
            aaContext.runtimeData.BuildTarget = builderInput.Target.ToString();
            aaContext.runtimeData.ProfileEvents = ProjectConfigData.PostProfilerEvents;
            aaContext.runtimeData.LogResourceManagerExceptions = aaSettings.buildSettings.LogResourceManagerExceptions;
            aaContext.runtimeData.ProfileEvents = ProjectConfigData.PostProfilerEvents;
            aaContext.runtimeData.MaxConcurrentWebRequests = aaSettings.MaxConcurrentWebRequests;
            aaContext.runtimeData.CatalogRequestsTimeout = aaSettings.CatalogRequestsTimeout;
            aaContext.runtimeData.CatalogLocations.Add(new ResourceLocationData(
                new[] { ResourceManagerRuntimeData.kCatalogAddress },
                string.Format(m_PathFormat, "file://{UnityEngine.Application.dataPath}/../", "catalog"),
                typeof(ContentCatalogProvider), typeof(ContentCatalogData)));
            aaContext.runtimeData.AddressablesVersion = PackageManager.PackageInfo.FindForAssembly(typeof(Addressables).Assembly)?.version;
            m_CreatedProviderIds = new Dictionary<string, VirtualAssetBundleRuntimeData>();
            m_ResourceProviderData = new List<ObjectInitializationData>();

            var errorString = ProcessAllGroups(aaContext);
            if (!string.IsNullOrEmpty(errorString))
                result = AddressableAssetBuildResult.CreateResult<TResult>(null, 0, errorString);

            if (result == null)
            {
                result = DoBuild<TResult>(builderInput, aaSettings, aaContext);
            }

            if (result != null)
                result.Duration = timer.Elapsed.TotalSeconds;
            return result;
        }

        TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetSettings aaSettings, AddressableAssetsBuildContext aaContext) where TResult : IDataBuilderResult
        {
            if (m_AllBundleInputDefinitions.Count > 0)
            {
                if (!BuildUtility.CheckModifiedScenesAndAskToSave())
                    return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, "Unsaved scenes");

                var buildTarget = builderInput.Target;
                var buildTargetGroup = builderInput.TargetGroup;
                var buildParams = new AddressableAssetsBundleBuildParameters(aaSettings, aaContext.bundleToAssetGroup, buildTarget, buildTargetGroup, aaSettings.buildSettings.bundleBuildPath);
                var builtinShaderBundleName = aaSettings.DefaultGroup.Name.ToLower().Replace(" ", "").Replace('\\', '/').Replace("//", "/") + "_unitybuiltinshaders.bundle";
                var buildTasks = RuntimeDataBuildTasks(aaSettings.buildSettings.compileScriptsInVirtualMode, builtinShaderBundleName);
                ExtractDataTask extractData = new ExtractDataTask();
                buildTasks.Add(extractData);

                string aaPath = aaSettings.AssetPath;
                IBundleBuildResults results;
                var exitCode = ContentPipeline.BuildAssetBundles(buildParams, new BundleBuildContent(m_AllBundleInputDefinitions), out results, buildTasks, aaContext);

                if (exitCode < ReturnCode.Success)
                    return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, "SBP Error" + exitCode);

                if (aaSettings == null && !string.IsNullOrEmpty(aaPath))
                    aaSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(aaPath);
            }

            var bundledAssets = new Dictionary<object, HashSet<string>>();
            foreach (var loc in aaContext.locations)
            {
                if (loc.Dependencies != null && loc.Dependencies.Count > 0)
                {
                    for (int i = 0; i < loc.Dependencies.Count; i++)
                    {
                        var dep = loc.Dependencies[i];
                        HashSet<string> assetsInBundle;
                        if (!bundledAssets.TryGetValue(dep, out assetsInBundle))
                            bundledAssets.Add(dep, assetsInBundle = new HashSet<string>());
                        if (i == 0 && !assetsInBundle.Contains(loc.InternalId)) //only add the asset to the first bundle...
                            assetsInBundle.Add(loc.InternalId);
                    }
                }
            }

            foreach (var bd in bundledAssets)
            {
                AddressableAssetGroup group = aaSettings.DefaultGroup;
                string groupGuid;
                if (aaContext.bundleToAssetGroup.TryGetValue(bd.Key as string, out groupGuid))
                    group = aaSettings.FindGroup(g => g.Guid == groupGuid);

                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema != null)
                {
                    var bundleLocData = aaContext.locations.First(s => s.Keys[0] == bd.Key);
                    var isLocalBundle = IsInternalIdLocal(bundleLocData.InternalId);
                    uint crc = (uint)UnityEngine.Random.Range(0, int.MaxValue);
                    var hash = Guid.NewGuid().ToString();

                    string originalBundleName = bd.Key as string;
                    string newBundleName = BuildUtility.GetNameWithHashNaming(schema.BundleNaming, hash, originalBundleName);
                    bundleLocData.InternalId = bundleLocData.InternalId.Remove(bundleLocData.InternalId.Length - originalBundleName.Length) + newBundleName;
                    var abb = m_AllBundleInputDefinitions.FirstOrDefault(a => a.assetBundleName == originalBundleName);
                    var virtualBundleName = AddressablesRuntimeProperties.EvaluateString(bundleLocData.InternalId);
                    var bundleData = new VirtualAssetBundle(virtualBundleName, isLocalBundle, crc, hash);

                    long dataSize = 0;
                    long headerSize = 0;
                    foreach (var a in bd.Value)
                    {
                        var i = Array.IndexOf(abb.addressableNames, a);
                        var assetPath = abb.assetNames[i];
                        var size = ComputeSize(assetPath);
                        var vab = new VirtualAssetBundleEntry(a, size);
                        vab.m_AssetPath = assetPath;
                        bundleData.Assets.Add(vab);
                        dataSize += size;
                        headerSize += a.Length * 5; //assume 5x path length overhead size per item, probably much less
                    }
                    if (bd.Value.Count == 0)
                    {
                        dataSize = 100 * 1024;
                        headerSize = 1024;
                    }
                    bundleData.SetSize(dataSize, headerSize);


                    var requestOptions = new VirtualAssetBundleRequestOptions
                    {
                        Crc = schema.UseAssetBundleCrc ? crc : 0,
                        Hash = schema.UseAssetBundleCache ? hash : "",
                        ChunkedTransfer = schema.ChunkedTransfer,
                        RedirectLimit = schema.RedirectLimit,
                        RetryCount = schema.RetryCount,
                        Timeout = schema.Timeout,
                        BundleName = Path.GetFileName(bundleLocData.InternalId),
                        AssetLoadMode = schema.AssetLoadMode,
                        BundleSize = dataSize + headerSize
                    };
                    bundleLocData.Data = requestOptions;

                    var bundleProviderId = schema.GetBundleCachedProviderId();
                    var virtualBundleRuntimeData = m_CreatedProviderIds[bundleProviderId];
                    virtualBundleRuntimeData.AssetBundles.Add(bundleData);
                }
            }
            foreach (var kvp in m_CreatedProviderIds)
            {
                if (kvp.Value != null)
                {
                    var bundleProviderData = ObjectInitializationData.CreateSerializedInitializationData<VirtualAssetBundleProvider>(kvp.Key, kvp.Value);
                    m_ResourceProviderData.Add(bundleProviderData);
                }
            }

            var contentCatalog = new ContentCatalogData(ResourceManagerRuntimeData.kCatalogAddress);
            contentCatalog.SetData(aaContext.locations.OrderBy(f => f.InternalId).ToList(), aaContext.Settings.OptimizeCatalogSize);

            contentCatalog.ResourceProviderData.AddRange(m_ResourceProviderData);
            foreach (var t in aaContext.providerTypes)
                contentCatalog.ResourceProviderData.Add(ObjectInitializationData.CreateSerializedInitializationData(t));

            contentCatalog.InstanceProviderData = ObjectInitializationData.CreateSerializedInitializationData(instanceProviderType.Value);
            contentCatalog.SceneProviderData = ObjectInitializationData.CreateSerializedInitializationData(sceneProviderType.Value);

            //save catalog
            WriteFile(string.Format(m_PathFormat, "", "catalog"), JsonUtility.ToJson(contentCatalog), builderInput.Registry);

            foreach (var io in aaSettings.InitializationObjects)
            {
                if (io is IObjectInitializationDataProvider)
                    aaContext.runtimeData.InitializationObjects.Add((io as IObjectInitializationDataProvider).CreateObjectInitializationData());
            }

            var settingsPath = string.Format(m_PathFormat, "", "settings");
            WriteFile(settingsPath, JsonUtility.ToJson(aaContext.runtimeData), builderInput.Registry);

            //inform runtime of the init data path
            var runtimeSettingsPath = string.Format(m_PathFormat, "file://{UnityEngine.Application.dataPath}/../", "settings");
            PlayerPrefs.SetString(Addressables.kAddressablesRuntimeDataPath, runtimeSettingsPath);
            var result = AddressableAssetBuildResult.CreateResult<TResult>(settingsPath, aaContext.locations.Count);
            return result;
        }

        /// <inheritdoc />
        protected override string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            var errorString = string.Empty;
            PlayerDataGroupSchema playerSchema = assetGroup.GetSchema<PlayerDataGroupSchema>();
            if (playerSchema != null)
            {
                if (CreateLocationsForPlayerData(playerSchema, assetGroup, aaContext.locations, aaContext.providerTypes))
                {
                    if (!m_CreatedProviderIds.ContainsKey(typeof(LegacyResourcesProvider).Name))
                    {
                        m_CreatedProviderIds.Add(typeof(LegacyResourcesProvider).Name, null);
                        m_ResourceProviderData.Add(ObjectInitializationData.CreateSerializedInitializationData(typeof(LegacyResourcesProvider)));
                    }
                }
                return errorString;
            }

            var schema = assetGroup.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
                return errorString;

            var bundledProviderId = schema.GetBundleCachedProviderId();
            var assetProviderId = schema.GetAssetCachedProviderId();
            if (!m_CreatedProviderIds.ContainsKey(bundledProviderId))
            {
                //TODO: pull from schema instead of ProjectConfigData
                var virtualBundleRuntimeData = new VirtualAssetBundleRuntimeData(ProjectConfigData.LocalLoadSpeed, ProjectConfigData.RemoteLoadSpeed);
                //save virtual runtime data to collect assets into virtual bundles
                m_CreatedProviderIds.Add(bundledProviderId, virtualBundleRuntimeData);
            }

            if (!m_CreatedProviderIds.ContainsKey(assetProviderId))
            {
                m_CreatedProviderIds.Add(assetProviderId, null);

                var assetProviderData = ObjectInitializationData.CreateSerializedInitializationData<VirtualBundledAssetProvider>(assetProviderId);
                m_ResourceProviderData.Add(assetProviderData);
            }


            var bundleInputDefs = new List<AssetBundleBuild>();
            List<AddressableAssetEntry> list = BuildScriptPackedMode.PrepGroupBundlePacking(assetGroup, bundleInputDefs, schema);
            aaContext.assetEntries.AddRange(list);
            for (int i = 0; i < bundleInputDefs.Count; i++)
            {
                if (aaContext.bundleToAssetGroup.ContainsKey(bundleInputDefs[i].assetBundleName))
                {
                    var bid = bundleInputDefs[i];
                    int count = 1;
                    var newName = bid.assetBundleName;
                    while (aaContext.bundleToAssetGroup.ContainsKey(newName) && count < 1000)
                        newName = bid.assetBundleName.Replace(".bundle", string.Format("{0}.bundle", count++));
                    bundleInputDefs[i] = new AssetBundleBuild { assetBundleName = newName, addressableNames = bid.addressableNames, assetBundleVariant = bid.assetBundleVariant, assetNames = bid.assetNames };
                }

                aaContext.bundleToAssetGroup.Add(bundleInputDefs[i].assetBundleName, assetGroup.Guid);
            }

            m_AllBundleInputDefinitions.AddRange(bundleInputDefs);

            return errorString;
        }

        static bool IsInternalIdLocal(string path)
        {
            return path.StartsWith("{UnityEngine.AddressableAssets.Addressables.RuntimePath}");
        }

        static string OutputLibraryPathForAsset(string a)
        {
            var guid = AssetDatabase.AssetPathToGUID(a);
            var legacyPath = string.Format("Library/metadata/{0}{1}/{2}", guid[0], guid[1], guid);
#if UNITY_2020_2_OR_NEWER            
            var artifactID = Experimental.AssetDatabaseExperimental.ProduceArtifact(new ArtifactKey(new GUID(guid)));
            if (Experimental.AssetDatabaseExperimental.GetArtifactPaths(artifactID, out var paths))
                return Path.GetFullPath(paths[0]);
            else
                legacyPath = String.Empty;
#elif UNITY_2020_1_OR_NEWER
            var hash = Experimental.AssetDatabaseExperimental.GetArtifactHash(guid);
            if (Experimental.AssetDatabaseExperimental.GetArtifactPaths(hash, out var paths))
                return Path.GetFullPath(paths[0]);
            else
                legacyPath = String.Empty; // legacy path is never valid in 2020.1+
#else
            if (IsAssetDatabaseV2Enabled()) // AssetDatabase V2 is optional in 2019.3 and 2019.4
            {
                var hash = Experimental.AssetDatabaseExperimental.GetArtifactHash(guid);
                if (Experimental.AssetDatabaseExperimental.GetArtifactPaths(hash, out var paths))
                    return Path.GetFullPath(paths[0]);
            }
#endif
            return legacyPath;
        }

        static bool IsAssetDatabaseV2Enabled()
        {
            // method is internal
            var methodInfo = typeof(AssetDatabase).GetMethod("IsV2Enabled", BindingFlags.Static | BindingFlags.NonPublic);
            return methodInfo != null && (bool)methodInfo.Invoke(null, null);
        }

        static long ComputeSize(string a)
        {
            var path = OutputLibraryPathForAsset(a);
            if (!File.Exists(path))
            {
                return 1024 * 1024;
            }
            return new FileInfo(path).Length;
        }

        static IList<IBuildTask> RuntimeDataBuildTasks(bool compileScripts, string builtinShaderBundleName)
        {
            var buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            if (compileScripts)
            {
                buildTasks.Add(new BuildPlayerScripts());
                buildTasks.Add(new PostScriptsCallback());
            }

            // Dependency
            buildTasks.Add(new PreviewSceneDependencyData());
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle(builtinShaderBundleName));
            buildTasks.Add(new PostDependencyCallback());

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new UpdateBundleObjectLayout());
            buildTasks.Add(new GenerateLocationListsTask());
            buildTasks.Add(new PostPackingCallback());

            return buildTasks;
        }
    }
}
