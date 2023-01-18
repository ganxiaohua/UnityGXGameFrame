using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build.BuildPipelineTasks;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace UnityEditor.AddressableAssets.Build.AnalyzeRules
{
    /// <summary>
    /// Base class for handling analyzing bundle rules tasks and checking dependencies
    /// </summary>
    public class BundleRuleBase : AnalyzeRule
    {
        [NonSerialized]
        internal List<GUID> m_AddressableAssets = new List<GUID>();
        [NonSerialized]
        internal Dictionary<string, List<GUID>> m_ResourcesToDependencies = new Dictionary<string, List<GUID>>();
        [NonSerialized]
        internal readonly List<ContentCatalogDataEntry> m_Locations = new List<ContentCatalogDataEntry>();
        [NonSerialized]
        internal readonly List<AssetBundleBuild> m_AllBundleInputDefs = new List<AssetBundleBuild>();
        [NonSerialized]
        internal readonly Dictionary<string, string> m_BundleToAssetGroup = new Dictionary<string, string>();
        [NonSerialized]
        internal readonly List<AddressableAssetEntry> m_AssetEntries = new List<AddressableAssetEntry>();
        [NonSerialized]
        internal ExtractDataTask m_ExtractData = new ExtractDataTask();

        internal IList<IBuildTask> RuntimeDataBuildTasks(string builtinShaderBundleName)
        {
            IList<IBuildTask> buildTasks = new List<IBuildTask>();

            // Setup
            buildTasks.Add(new SwitchToBuildPlatform());
            buildTasks.Add(new RebuildSpriteAtlasCache());

            // Player Scripts
            buildTasks.Add(new BuildPlayerScripts());

            // Dependency
            buildTasks.Add(new CalculateSceneDependencyData());
            buildTasks.Add(new CalculateAssetDependencyData());
            buildTasks.Add(new StripUnusedSpriteSources());
            buildTasks.Add(new CreateBuiltInShadersBundle(builtinShaderBundleName));

            // Packing
            buildTasks.Add(new GenerateBundlePacking());
            buildTasks.Add(new UpdateBundleObjectLayout());

            buildTasks.Add(new GenerateBundleCommands());
            buildTasks.Add(new GenerateSubAssetPathMaps());
            buildTasks.Add(new GenerateBundleMaps());

            buildTasks.Add(new GenerateLocationListsTask());

            return buildTasks;
        }

        /// <summary>
        /// Get context for current Addressables settings
        /// </summary>
        /// <param name="settings"> The current Addressables settings object </param>
        /// <returns> The build context information </returns>
        protected internal AddressableAssetsBuildContext GetBuildContext(AddressableAssetSettings settings)
        {
            ResourceManagerRuntimeData runtimeData = new ResourceManagerRuntimeData();
            runtimeData.LogResourceManagerExceptions = settings.buildSettings.LogResourceManagerExceptions;

            var aaContext = new AddressableAssetsBuildContext
            {
                Settings = settings,
                runtimeData = runtimeData,
                bundleToAssetGroup = m_BundleToAssetGroup,
                locations = m_Locations,
                providerTypes = new HashSet<Type>(),
                assetEntries = m_AssetEntries,
                assetGroupToBundles = new Dictionary<AddressableAssetGroup, List<string>>()
            };
            return aaContext;
        }

        /// <summary>
        /// Check path is valid path for Addressables entry
        /// </summary>
        /// <param name="path"> The path to check</param>
        /// <returns>Whether path is valid</returns>
        protected bool IsValidPath(string path)
        {
            return AddressableAssetUtility.IsPathValidForEntry(path) &&
                !path.ToLower().Contains("/resources/") &&
                !path.ToLower().StartsWith("resources/");
        }

        /// <summary>
        /// Refresh build to check bundles against current rules
        /// </summary>
        /// <param name="buildContext"> Context information for building</param>
        /// <returns> The return code of whether analyze build was successful, </returns>
        protected internal ReturnCode RefreshBuild(AddressableAssetsBuildContext buildContext)
        {
            var settings = buildContext.Settings;
            var context = new AddressablesDataBuilderInput(settings);

            var buildTarget = context.Target;
            var buildTargetGroup = context.TargetGroup;
            var buildParams = new AddressableAssetsBundleBuildParameters(settings, m_BundleToAssetGroup, buildTarget,
                buildTargetGroup, settings.buildSettings.bundleBuildPath);
            var builtinShaderBundleName =
                settings.DefaultGroup.Name.ToLower().Replace(" ", "").Replace('\\', '/').Replace("//", "/") +
                "_unitybuiltinshaders.bundle";
            var buildTasks = RuntimeDataBuildTasks(builtinShaderBundleName);
            buildTasks.Add(m_ExtractData);

            IBundleBuildResults buildResults;
            var exitCode = ContentPipeline.BuildAssetBundles(buildParams, new BundleBuildContent(m_AllBundleInputDefs),
                out buildResults, buildTasks, buildContext);

            return exitCode;
        }

        /// <summary>
        /// Get dependencies from bundles
        /// </summary>
        /// <returns> The list of GUIDs of bundle dependencies</returns>
        protected List<GUID> GetAllBundleDependencies()
        {
            var explicitGuids = m_ExtractData.WriteData.AssetToFiles.Keys;
            var implicitGuids = GetImplicitGuidToFilesMap().Keys;
            var allBundleGuids = explicitGuids.Union(implicitGuids);

            return allBundleGuids.ToList();
        }

        /// <summary>
        /// Add Resource and Bundle dependencies in common to map of resources to dependencies
        /// </summary>
        /// <param name="bundleDependencyGuids"> GUID list of bundle dependencies</param>
        protected internal void IntersectResourcesDepedenciesWithBundleDependencies(List<GUID> bundleDependencyGuids)
        {
            foreach (var key in m_ResourcesToDependencies.Keys)
            {
                var bundleDependencies = bundleDependencyGuids.Intersect(m_ResourcesToDependencies[key]).ToList();

                m_ResourcesToDependencies[key].Clear();
                m_ResourcesToDependencies[key].AddRange(bundleDependencies);
            }
        }

        /// <summary>
        /// Build map of resources to corresponding dependencies
        /// </summary>
        /// <param name="resourcePaths"> Array of resource paths</param>
        protected internal virtual void BuiltInResourcesToDependenciesMap(string[] resourcePaths)
        {
            for (int sceneIndex=0; sceneIndex<resourcePaths.Length; ++sceneIndex)
            {
                string path = resourcePaths[sceneIndex];
                if (EditorUtility.DisplayCancelableProgressBar("Generating built-in resource dependency map",
                    "Checking " + path + " for duplicates with Addressables content.",
                    (float) sceneIndex / resourcePaths.Length))
                {
                    m_ResourcesToDependencies.Clear();
                    EditorUtility.ClearProgressBar();
                    return;
                }
                string[] dependencies;
                if (path.EndsWith(".unity"))
                {
                    using (var w = new BuildInterfacesWrapper())
                    {
                        var usageTags = new BuildUsageTagSet();
                        BuildSettings settings = new BuildSettings
                        {
                            group = EditorUserBuildSettings.selectedBuildTargetGroup,
                            target = EditorUserBuildSettings.activeBuildTarget,
                            typeDB = null,
                            buildFlags = ContentBuildFlags.None
                        };

                        SceneDependencyInfo sceneInfo =
                            ContentBuildInterface.CalculatePlayerDependenciesForScene(path, settings, usageTags);
                        dependencies = new string[sceneInfo.referencedObjects.Count];
                        for (int i = 0; i < sceneInfo.referencedObjects.Count; ++i)
                        {
                            if (string.IsNullOrEmpty(sceneInfo.referencedObjects[i].filePath))
                                dependencies[i] = AssetDatabase.GUIDToAssetPath(sceneInfo.referencedObjects[i].guid.ToString());
                            else
                                dependencies[i] = sceneInfo.referencedObjects[i].filePath;
                        }
                    }
                }
                else
                    dependencies = AssetDatabase.GetDependencies(path);

                if (!m_ResourcesToDependencies.ContainsKey(path))
                    m_ResourcesToDependencies.Add(path, new List<GUID>(dependencies.Length));
                else
                    m_ResourcesToDependencies[path].Capacity += dependencies.Length;
                
                foreach (string dependency in dependencies)
                {
                    if (dependency.EndsWith(".cs") || dependency.EndsWith(".dll"))
                        continue;
                    m_ResourcesToDependencies[path].Add(new GUID(AssetDatabase.AssetPathToGUID(dependency)));
                }
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Use bundle names to create group names for AssetBundleBuild
        /// </summary>
        /// <param name="buildContext">Context information for building</param>
        protected internal void ConvertBundleNamesToGroupNames(AddressableAssetsBuildContext buildContext)
        {
            Dictionary<string, string> bundleNamesToUpdate = new Dictionary<string, string>();

            foreach (var assetGroup in buildContext.Settings.groups)
            {
                if (assetGroup == null)
                    continue;

                List<string> bundles;
                if (buildContext.assetGroupToBundles.TryGetValue(assetGroup, out bundles))
                {
                    foreach (string bundle in bundles)
                    {
                        var keys = m_ExtractData.WriteData.FileToBundle.Keys.Where(key => m_ExtractData.WriteData.FileToBundle[key] == bundle);
                        foreach (string key in keys)
                            bundleNamesToUpdate.Add(key, assetGroup.Name);
                    }
                }
            }

            foreach (string key in bundleNamesToUpdate.Keys)
            {
                var bundle = m_ExtractData.WriteData.FileToBundle[key];
                var inputDef = m_AllBundleInputDefs.FirstOrDefault(b => b.assetBundleName == bundle);
                int index = m_AllBundleInputDefs.IndexOf(inputDef);
                if (index >= 0)
                {
                    inputDef.assetBundleName = ConvertBundleName(inputDef.assetBundleName, bundleNamesToUpdate[key]);
                    m_AllBundleInputDefs[index] = inputDef;
                    m_ExtractData.WriteData.FileToBundle[key] = inputDef.assetBundleName;
                }
            }
        }
        
        /// <summary>
        /// Generate input definitions and entries for AssetBundleBuild
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        protected internal void CalculateInputDefinitions(AddressableAssetSettings settings)
        {
            int updateFrequency = Mathf.Max(settings.groups.Count / 10, 1);
            bool progressDisplayed = false;
            for (int groupIndex = 0; groupIndex < settings.groups.Count; ++groupIndex)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];
                if (group == null)
                    continue;
                if (!progressDisplayed || groupIndex % updateFrequency == 0)
                {
                    progressDisplayed = true;
                    if (EditorUtility.DisplayCancelableProgressBar("Calculating Input Definitions", "",
                        (float) groupIndex / settings.groups.Count))
                    {
                        m_AssetEntries.Clear();
                        m_BundleToAssetGroup.Clear();
                        m_AllBundleInputDefs.Clear();
                        break;
                    }
                }
        
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema != null && schema.IncludeInBuild)
                {
                    List<AssetBundleBuild> bundleInputDefinitions = new List<AssetBundleBuild>();
                    m_AssetEntries.AddRange(BuildScriptPackedMode.PrepGroupBundlePacking(group, bundleInputDefinitions, schema));
        
                    for (int i = 0; i < bundleInputDefinitions.Count; i++)
                    {
                        if (m_BundleToAssetGroup.ContainsKey(bundleInputDefinitions[i].assetBundleName))
                            bundleInputDefinitions[i] = CreateUniqueBundle(bundleInputDefinitions[i]);
        
                        m_BundleToAssetGroup.Add(bundleInputDefinitions[i].assetBundleName, schema.Group.Guid);
                    }
        
                    m_AllBundleInputDefs.AddRange(bundleInputDefinitions);
                }
            }
            if (progressDisplayed)
                EditorUtility.ClearProgressBar();
        }
        
        internal AssetBundleBuild CreateUniqueBundle(AssetBundleBuild bid)
        {
            return CreateUniqueBundle(bid, m_BundleToAssetGroup);
        }

        /// <summary>
        /// Create new AssetBundleBuild
        /// </summary>
        /// <param name="bid">ID for new AssetBundleBuild</param>
        /// <param name="bundleToAssetGroup"> Map of bundle names to asset group Guids</param>
        /// <returns></returns>
        protected internal static AssetBundleBuild CreateUniqueBundle(AssetBundleBuild bid, Dictionary<string, string> bundleToAssetGroup)
        {
            int count = 1;
            var newName = bid.assetBundleName;
            while (bundleToAssetGroup.ContainsKey(newName) && count < 1000)
                newName = bid.assetBundleName.Replace(".bundle", string.Format("{0}.bundle", count++));
            return new AssetBundleBuild
            {
                assetBundleName = newName,
                addressableNames = bid.addressableNames,
                assetBundleVariant = bid.assetBundleVariant,
                assetNames = bid.assetNames
            };
        }

        /// <summary>
        /// Get bundle's object ids that have no dependency file
        /// </summary>
        /// <param name="fileName"> Name of bundle file </param>
        /// <returns> List of GUIDS of objects in bundle with no dependency file</returns>
        protected List<GUID> GetImplicitGuidsForBundle(string fileName)
        {
            List<GUID> guids = (from id in m_ExtractData.WriteData.FileToObjects[fileName]
                where !m_ExtractData.WriteData.AssetToFiles.Keys.Contains(id.guid)
                select id.guid).ToList();
            return guids;
        }

        /// <summary>
        /// Build map of implicit guids to their bundle files
        /// </summary>
        /// <returns> Dictionary of implicit guids to their corresponding file</returns>
        protected internal Dictionary<GUID, List<string>> GetImplicitGuidToFilesMap()
        {
            Dictionary<GUID, List<string>> implicitGuids = new Dictionary<GUID, List<string>>();
            IEnumerable<KeyValuePair<ObjectIdentifier, string>> validImplicitGuids =
                from fileToObject in m_ExtractData.WriteData.FileToObjects
                from objectId in fileToObject.Value
                where !m_ExtractData.WriteData.AssetToFiles.Keys.Contains(objectId.guid)
                select new KeyValuePair<ObjectIdentifier, string>(objectId, fileToObject.Key);

            //Build our Dictionary from our list of valid implicit guids (guids not already in explicit guids)
            foreach (var objectIdToFile in validImplicitGuids)
            {
                if (!implicitGuids.ContainsKey(objectIdToFile.Key.guid))
                    implicitGuids.Add(objectIdToFile.Key.guid, new List<string>());
                implicitGuids[objectIdToFile.Key.guid].Add(objectIdToFile.Value);
            }

            return implicitGuids;
        }

        /// <summary>
        /// Calculate built in resources and corresponding bundle dependencies
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        /// <param name="builtInResourcesPaths">Array of resource paths</param>
        /// <returns>List of rule results after calculating resource and bundle dependency combined</returns>
        protected List<AnalyzeResult> CalculateBuiltInResourceDependenciesToBundleDependecies(AddressableAssetSettings settings, string[] builtInResourcesPaths)
        {
            List<AnalyzeResult> results = new List<AnalyzeResult>();

            if (!BuildUtility.CheckModifiedScenesAndAskToSave())
            {
                Debug.LogError("Cannot run Analyze with unsaved scenes");
                results.Add(new AnalyzeResult { resultName = ruleName + "Cannot run Analyze with unsaved scenes" });
                return results;
            }

            EditorUtility.DisplayProgressBar("Calculating Built-in dependencies", "Calculating dependencies between Built-in resources and Addressables", 0);
            m_AddressableAssets = (from aaGroup in settings.groups
                where aaGroup != null
                from entry in aaGroup.entries
                select new GUID(entry.guid)).ToList();

            // bulk of work and progress bars displayed in these methods
            BuiltInResourcesToDependenciesMap(builtInResourcesPaths);
            if (m_ResourcesToDependencies == null || m_ResourcesToDependencies.Count == 0)
            {
                results.Add(new AnalyzeResult {resultName = ruleName + " - No issues found."});
                return results;
            }

            CalculateInputDefinitions(settings);
            if (m_AllBundleInputDefs == null || m_AllBundleInputDefs.Count == 0)
            {
                results.Add(new AnalyzeResult {resultName = ruleName + " - No issues found."});
                return results;
            }
            EditorUtility.DisplayProgressBar("Calculating Built-in dependencies", "Calculating dependencies between Built-in resources and Addressables", 0.5f);

            var context = GetBuildContext(settings);
            ReturnCode exitCode = RefreshBuild(context);
            if (exitCode < ReturnCode.Success)
            {
                Debug.LogError("Analyze build failed. " + exitCode);
                results.Add(new AnalyzeResult { resultName = ruleName + "Analyze build failed. " + exitCode });
                EditorUtility.ClearProgressBar();
                return results;
            }

            EditorUtility.DisplayProgressBar("Calculating Built-in dependencies", "Calculating dependencies between Built-in resources and Addressables", 0.9f);
            IntersectResourcesDepedenciesWithBundleDependencies(GetAllBundleDependencies());
            ConvertBundleNamesToGroupNames(context);

            results = (from resource in m_ResourcesToDependencies.Keys
                from dependency in m_ResourcesToDependencies[resource]

                let assetPath = AssetDatabase.GUIDToAssetPath(dependency.ToString())
                    let files = m_ExtractData.WriteData.FileToObjects.Keys

                    from file in files
                    where m_ExtractData.WriteData.FileToObjects[file].Any(oid => oid.guid == dependency)
                    where m_ExtractData.WriteData.FileToBundle.ContainsKey(file)
                    let bundle = m_ExtractData.WriteData.FileToBundle[file]

                    select new AnalyzeResult
                {
                    resultName =
                        resource + kDelimiter +
                        bundle + kDelimiter +
                        assetPath,
                    severity = MessageType.Warning
                }).ToList();

            if (results.Count == 0)
                results.Add(new AnalyzeResult { resultName = ruleName + " - No issues found." });

            EditorUtility.ClearProgressBar();
            return results;
        }

        /// <summary>
        /// Convert bundle name to include group name 
        /// </summary>
        /// <param name="bundleName">Current bundle name</param>
        /// <param name="groupName">Group name of bundle's group</param>
        /// <returns>The new bundle name</returns>
        protected string ConvertBundleName(string bundleName, string groupName)
        {
            string[] bundleNameSegments = bundleName.Split('_');
            bundleNameSegments[0] = groupName.Replace(" ", "").ToLower();
            return string.Join("_", bundleNameSegments);
        }

        /// <summary>
        /// Clear all previously gathered bundle data and analysis
        /// </summary>
        public override void ClearAnalysis()
        {
            m_Locations.Clear();
            m_AddressableAssets.Clear();
            m_AssetEntries.Clear(); 
            m_AllBundleInputDefs.Clear();
            m_BundleToAssetGroup.Clear();
            m_ResourcesToDependencies.Clear();
            m_ExtractData = new ExtractDataTask();

            base.ClearAnalysis();
        }
    }
}
