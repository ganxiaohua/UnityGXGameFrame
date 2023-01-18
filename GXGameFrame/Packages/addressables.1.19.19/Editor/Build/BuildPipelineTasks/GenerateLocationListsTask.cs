using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using static UnityEditor.AddressableAssets.Settings.AddressablesFileEnumeration;

namespace UnityEditor.AddressableAssets.Build.BuildPipelineTasks
{
    /// <summary>
    /// The BuildTask used to create location lists for Addressable assets.
    /// </summary>
    public class GenerateLocationListsTask : IBuildTask
    {
        const int k_Version = 1;

        /// <summary>
        /// The GenerateLocationListsTask version.
        /// </summary>
        public int Version { get { return k_Version; } }

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IAddressableAssetsBuildContext m_AaBuildContext;

        [InjectContext]
        IBundleWriteData m_WriteData;

        [InjectContext]
        IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.In, true)]
        IBuildLogger m_Log;

        [InjectContext(ContextUsage.In)]
        IBuildParameters m_Parameters;

#pragma warning restore 649

        /// <summary>
        /// Runs the build task with the injected context.
        /// </summary>
        /// <returns>The success or failure ReturnCode</returns>
        public ReturnCode Run()
        {
            Input input = new Input();
            var aaContext = (AddressableAssetsBuildContext)m_AaBuildContext;
            input.FileToBundle = m_WriteData.FileToBundle;
            input.AssetToFiles = m_WriteData.AssetToFiles;
            input.AssetToAssetInfo = m_DependencyData != null ? m_DependencyData.AssetInfo : null;
            input.Logger = m_Log;
            input.Settings = aaContext.Settings;
            input.BundleToAssetGroup = aaContext.bundleToAssetGroup;
            input.AddressableAssetEntries = aaContext.assetEntries;
            input.Target = m_Parameters.Target;

            Output output = ProcessInput(input);

            if (aaContext.locations == null)
                aaContext.locations = output.Locations;
            else
                aaContext.locations.AddRange(output.Locations);
            aaContext.assetGroupToBundles = output.AssetGroupToBundles;
            if (aaContext.providerTypes == null)
                aaContext.providerTypes = output.ProviderTypes;
            else
                aaContext.providerTypes.UnionWith(output.ProviderTypes);
            aaContext.bundleToExpandedBundleDependencies = output.BundleToExpandedBundleDependencies;
            aaContext.bundleToImmediateBundleDependencies = output.BundleToImmediateBundleDependencies;

            return ReturnCode.Success;
        }

        /// <summary>
        /// Storage for data gathered by the build pipeline.
        /// </summary>
        public struct Input
        {
            /// <summary>
            /// Mapping from serialized filename to the bundle name 
            /// </summary>
            public Dictionary<string, string> FileToBundle;
            /// <summary>
            /// Mapping of an asset to all the serialized files needed to load it. The first entry is the file that contains the asset itself.
            /// </summary>
            public Dictionary<GUID, List<string>> AssetToFiles;
            /// <summary>
            /// Map of Guid to AssetLoadInfo
            /// </summary>
            public Dictionary<GUID, AssetLoadInfo> AssetToAssetInfo;
            /// <summary>
            /// The logger used during the build.
            /// </summary>
            public IBuildLogger Logger;
            /// <summary>
            /// The current AddressableAssetSettings to be processed.
            /// </summary>
            public AddressableAssetSettings Settings;
            /// <summary>
            /// Mapping of the AssetBundle to the AddressableAssetGroup it was derived from
            /// </summary>
            public Dictionary<string, string> BundleToAssetGroup;
            /// <summary>
            /// All the AddressableAssetEntries to process
            /// </summary>
            public List<AddressableAssetEntry> AddressableAssetEntries;
            /// <summary>
            /// The BuildTarget to build for.
            /// </summary>
            public BuildTarget Target;
        }

        /// <summary>
        /// Storage for location data, including: dependencies, locations, and provider types.
        /// </summary>
        public struct Output
        {
            /// <summary>
            /// Content Catalog entries that were built into the Catalog.
            /// </summary>
            public List<ContentCatalogDataEntry> Locations;
            /// <summary>
            /// A mapping of AddressableAssetGroups to the AssetBundles generated from its data.
            /// </summary>
            public Dictionary<AddressableAssetGroup, List<string>> AssetGroupToBundles;
            /// <summary>
            /// A hash set of all the provider types included in the build.
            /// </summary>
            public HashSet<Type> ProviderTypes;
            /// <summary>
            /// A mapping of AssetBundles to the direct dependencies
            /// </summary>
            public Dictionary<string, List<string>> BundleToImmediateBundleDependencies;
            /// <summary>
            /// A mapping of AssetBundles to their expanded dependencies.
            /// </summary>
            public Dictionary<string, List<string>> BundleToExpandedBundleDependencies;
        }

        static AddressableAssetGroup GetGroupFromBundle(string bundleName, Dictionary<string, string> bundleToAssetGroupGUID , AddressableAssetSettings settings)
        {
            if (!bundleToAssetGroupGUID.TryGetValue(bundleName, out string groupGuid))
                return settings.DefaultGroup;
            return settings.FindGroup(g => g != null && g.Guid == groupGuid);
        }

        static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        class BundleEntry
        {
            public string BundleName;
            public HashSet<BundleEntry> Dependencies = new HashSet<BundleEntry>();
            public HashSet<BundleEntry> ExpandedDependencies;
            public List<GUID> Assets = new List<GUID>();
            public AddressableAssetGroup Group;
            public HashSet<string> AssetInternalIds = new HashSet<string>();
        }

        static private void ExpandDependencies(BundleEntry entry)
        {
            HashSet<BundleEntry> visited = new HashSet<BundleEntry>();
            Queue<BundleEntry> toVisit = new Queue<BundleEntry>();
            toVisit.Enqueue(entry);
            while (toVisit.Count > 0)
            {
                BundleEntry cur = toVisit.Dequeue();
                visited.Add(cur);
                foreach (BundleEntry dep in cur.Dependencies)
                    if (!visited.Contains(dep))
                        toVisit.Enqueue(dep);
            }
            entry.ExpandedDependencies = visited;
        }

        static BundleEntry GetOrCreateBundleEntry(string bundleName, Dictionary<string, BundleEntry> bundleToEntry)
        {
            if (!bundleToEntry.TryGetValue(bundleName, out BundleEntry e))
                bundleToEntry.Add(bundleName, e = new BundleEntry() { BundleName = bundleName });
            return e;
        }

        /// <summary>
        /// Processes the Input data from the build and returns an organized struct of information, including dependencies and catalog loctions.
        /// </summary>
        /// <param name="input">Data captured as part of the build process.</param>
        /// <returns>An object that contains organized information about dependencies and catalog locations.</returns>
        public static Output ProcessInput(Input input)
        {
            var locations = new List<ContentCatalogDataEntry>();
            var assetGroupToBundles = new Dictionary<AddressableAssetGroup, List<string>>();
            var bundleToEntry = new Dictionary<string, BundleEntry>();
            var providerTypes = new HashSet<Type>();

            // Create a bundle entry for every bundle that our assets could reference
            foreach (List<string> files in input.AssetToFiles.Values)
                files.ForEach(x => GetOrCreateBundleEntry(input.FileToBundle[x], bundleToEntry));
            
            // build list of assets each bundle has as well as the dependent bundles
            using (input.Logger.ScopedStep(LogLevel.Info, "Calculate Bundle Dependencies"))
            {
                foreach (KeyValuePair<GUID, List<string>> k in input.AssetToFiles)
                {
                    string bundle = input.FileToBundle[k.Value[0]];
                    BundleEntry bundleEntry = bundleToEntry[bundle];

                    bundleEntry.Assets.Add(k.Key);
                    bundleEntry.Dependencies.UnionWith(k.Value.Select(x => bundleToEntry[input.FileToBundle[x]]));
                }
            }

            using (input.Logger.ScopedStep(LogLevel.Info, "ExpandDependencies"))
            {
                foreach (BundleEntry bEntry in bundleToEntry.Values)
                    ExpandDependencies(bEntry);
            }

            // Assign each bundle a group
            foreach (BundleEntry bEntry in bundleToEntry.Values)
                bEntry.Group = GetGroupFromBundle(bEntry.BundleName, input.BundleToAssetGroup, input.Settings);

            // Create a location for each bundle
            foreach (BundleEntry bEntry in bundleToEntry.Values)
            {
                string bundleProvider = GetBundleProviderName(bEntry.Group);
                string bundleInternalId = GetLoadPath(bEntry.Group, bEntry.BundleName, input.Target);
                locations.Add(new ContentCatalogDataEntry(typeof(IAssetBundleResource), bundleInternalId, bundleProvider, new object[] { bEntry.BundleName }));
            }

            using (input.Logger.ScopedStep(LogLevel.Info, "Calculate Locations"))
            {
                // build a mapping of asset guid to AddressableAssetEntry
                Dictionary<string, AddressableAssetEntry> guidToEntry = input.AddressableAssetEntries.ToDictionary(x => x.guid, x => x);

                foreach (BundleEntry bEntry in bundleToEntry.Values)
                {
                    string assetProvider = GetAssetProviderName(bEntry.Group);
                    var schema = bEntry.Group.GetSchema<BundledAssetGroupSchema>();
                    foreach (GUID assetGUID in bEntry.Assets)
                    {
                        if (guidToEntry.TryGetValue(assetGUID.ToString(), out AddressableAssetEntry entry)) 
                            entry.CreateCatalogEntries(locations, true, assetProvider, bEntry.ExpandedDependencies.Select(x => x.BundleName), null, input.AssetToAssetInfo, providerTypes, schema.IncludeAddressInCatalog, schema.IncludeGUIDInCatalog, schema.IncludeLabelsInCatalog, bEntry.AssetInternalIds);
                    }
                }
            }

            // create the assetGroupToBundles mapping
            foreach (BundleEntry bEntry in bundleToEntry.Values)
                GetOrCreate(assetGroupToBundles, bEntry.Group).Add(bEntry.BundleName);

            var output = new Output();
            output.Locations = locations;
            output.ProviderTypes = providerTypes;
            output.AssetGroupToBundles = assetGroupToBundles;
            output.BundleToImmediateBundleDependencies = bundleToEntry.Values.ToDictionary(x => x.BundleName, x => x.Dependencies.Select(y => y.BundleName).ToList());
            output.BundleToExpandedBundleDependencies = bundleToEntry.Values.ToDictionary(x => x.BundleName, x => x.ExpandedDependencies.Where(y => !x.Dependencies.Contains(y)).Select(y => y.BundleName).ToList());
            return output;
        }

        /// <summary>
        /// Runs the build task with a give context and write data.
        /// </summary>
        /// <param name="aaBuildContext">The addressables build context.</param>
        /// <param name="writeData">The write data used to generate the location lists.</param>
        /// <returns>The success or failure ReturnCode</returns>
        [Obsolete("This method uses nonoptimized code. Use nonstatic version Run() instead.")]
        public static ReturnCode Run(IAddressableAssetsBuildContext aaBuildContext, IBundleWriteData writeData)
        {
            var task = new GenerateLocationListsTask();
            task.m_AaBuildContext = aaBuildContext;
            task.m_WriteData = writeData;
            return task.Run();
        }

        internal static string GetBundleProviderName(AddressableAssetGroup group)
        {
            return group.GetSchema<BundledAssetGroupSchema>().GetBundleCachedProviderId();
        }

        internal static string GetAssetProviderName(AddressableAssetGroup group)
        {
            return group.GetSchema<BundledAssetGroupSchema>().GetAssetCachedProviderId();
        }

        internal static string GetLoadPath(AddressableAssetGroup group, string name, BuildTarget target)
        {
            var bagSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bagSchema == null || bagSchema.LoadPath == null)
            {
                Debug.LogError("Unable to determine load path for " + name + ". Check that your default group is not '" + AddressableAssetSettings.PlayerDataGroupName + "'");
                return string.Empty;
            }

            string loadPath = bagSchema.LoadPath.GetValue(group.Settings);
            loadPath = loadPath.Replace('\\', '/');
            if (loadPath.EndsWith("/"))
                loadPath += name;
            else
                loadPath = loadPath + "/" + name;
            
            if (!string.IsNullOrEmpty(bagSchema.UrlSuffix))
                loadPath += bagSchema.UrlSuffix;
            if (!ResourceManagerConfig.ShouldPathUseWebRequest(loadPath) && !bagSchema.UseUnityWebRequestForLocalBundles)
            {
                char separator = PathSeparatorForPlatform(target);
                if (separator != '/')
                    loadPath = loadPath.Replace('/', separator);
            }
            return loadPath;
        }

        internal static char PathSeparatorForPlatform(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.XboxOne:
                    return '\\';
                default:
                    return '/';
            }
        }
    }
}
