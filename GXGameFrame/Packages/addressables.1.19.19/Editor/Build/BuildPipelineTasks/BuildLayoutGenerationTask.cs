using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.Build.BuildPipelineTasks
{
    /// <summary>
    /// The BuildTask used to generate the bundle layout.
    /// </summary>
    public class BuildLayoutGenerationTask : IBuildTask
    {
        const int k_Version = 1;

        internal static Action<BuildLayout> s_LayoutCompleteCallback;

        /// <summary>
        /// The GenerateLocationListsTask version.
        /// </summary>
        public int Version { get { return k_Version; } }

        /// <summary>
        /// The mapping of the old to new bundle names. 
        /// </summary>
        public Dictionary<string, string> BundleNameRemap { get { return m_BundleNameRemap; } set { m_BundleNameRemap = value; }}

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IAddressableAssetsBuildContext m_AaBuildContext;

        [InjectContext(ContextUsage.In)]
        IBuildParameters m_Parameters;

        [InjectContext]
        IBundleWriteData m_WriteData;

        [InjectContext(ContextUsage.In, true)]
        IBuildLogger m_Log;

        [InjectContext]
        IBuildResults m_Results;

        [InjectContext(ContextUsage.In)]
        IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.In)]
        IBundleBuildResults m_BuildBundleResults;
#pragma warning restore 649

        internal Dictionary<string, string> m_BundleNameRemap;

        internal static string m_LayoutTextFile = Addressables.LibraryPath + "/buildlayout.txt";

        static AssetBucket GetOrCreate(Dictionary<string, AssetBucket> buckets, string asset)
        {
            if (!buckets.TryGetValue(asset, out AssetBucket bucket))
            {
                bucket = new AssetBucket();
                bucket.guid = asset;
                buckets.Add(asset, bucket);
            }
            return bucket;
        }

        class AssetBucket
        {
            public string guid;
            public bool isFilePathBucket;
            public List<ObjectSerializedInfo> objs = new List<ObjectSerializedInfo>();
            public BuildLayout.ExplicitAsset ExplictAsset;

            public ulong CalcObjectSize() { return (ulong)objs.Sum(x => (long)x.header.size); }
            public ulong CalcStreamedSize() { return (ulong)objs.Sum(x => (long)x.rawData.size); }
        }

        private BuildLayout CreateBuildLayout()
        {
            LayoutLookupTables lookup = new LayoutLookupTables();

            foreach (string bundleName in m_WriteData.FileToBundle.Values.Distinct())
            {
                BuildLayout.Bundle bundle = new BuildLayout.Bundle();
                bundle.Name = bundleName;
                UnityEngine.BuildCompression compression = m_Parameters.GetCompressionForIdentifier(bundle.Name);
                bundle.Compression = compression.compression.ToString();
                lookup.Bundles.Add(bundle.Name, bundle);
            }

            // create files
            foreach (KeyValuePair<string, string> fileBundle in m_WriteData.FileToBundle)
            {
                BuildLayout.Bundle bundle = lookup.Bundles[fileBundle.Value];
                BuildLayout.File f = new BuildLayout.File();
                f.Name = fileBundle.Key;

                WriteResult result = m_Results.WriteResults[f.Name];
                foreach (ResourceFile rf in result.resourceFiles)
                {
                    var sf = new BuildLayout.SubFile();
                    sf.IsSerializedFile = rf.serializedFile;
                    sf.Name = rf.fileAlias;
                    sf.Size = (ulong)new FileInfo(rf.fileName).Length;
                    f.SubFiles.Add(sf);
                }

                bundle.Files.Add(f);
                lookup.Files.Add(f.Name, f);
            }

            // create assets
            foreach (KeyValuePair<GUID, List<string>> assetFile in m_WriteData.AssetToFiles)
            {
                BuildLayout.File file = lookup.Files[assetFile.Value[0]];
                BuildLayout.ExplicitAsset a = new BuildLayout.ExplicitAsset();
                a.Guid = assetFile.Key.ToString();
                a.AssetPath = AssetDatabase.GUIDToAssetPath(a.Guid);
                file.Assets.Add(a);
                lookup.GuidToExplicitAsset.Add(a.Guid, a);
            }

            Dictionary<string, List<BuildLayout.DataFromOtherAsset>> guidToPulledInBuckets = new Dictionary<string, List<BuildLayout.DataFromOtherAsset>>();

            foreach (BuildLayout.File file in lookup.Files.Values)
            {
                Dictionary<string, AssetBucket> buckets = new Dictionary<string, AssetBucket>();
                WriteResult writeResult = m_Results.WriteResults[file.Name];
                List<ObjectSerializedInfo> sceneObjects = new List<ObjectSerializedInfo>();

                foreach (ObjectSerializedInfo info in writeResult.serializedObjects)
                {
                    string sourceGuid = string.Empty;
                    if (info.serializedObject.guid.Empty())
                    {
                        if (info.serializedObject.filePath.Equals("temp:/assetbundle", StringComparison.OrdinalIgnoreCase))
                        {
                            file.BundleObjectInfo = new BuildLayout.AssetBundleObjectInfo();
                            file.BundleObjectInfo.Size = info.header.size;
                            continue;
                        }
                        else if (info.serializedObject.filePath.StartsWith("temp:/preloaddata", StringComparison.OrdinalIgnoreCase))
                        {
                            file.PreloadInfoSize = (int)info.header.size;
                            continue;
                        }
                        else if (info.serializedObject.filePath.StartsWith("temp:/", StringComparison.OrdinalIgnoreCase))
                        {
                            sceneObjects.Add(info);
                            continue;
                        }
                        else if (!string.IsNullOrEmpty(info.serializedObject.filePath))
                        {
                            AssetBucket pathBucket = GetOrCreate(buckets, info.serializedObject.filePath.ToString());
                            pathBucket.isFilePathBucket = true;
                            pathBucket.objs.Add(info);
                            continue;
                        }
                    }

                    AssetBucket bucket = GetOrCreate(buckets, info.serializedObject.guid.ToString());
                    bucket.objs.Add(info);
                }

                if (sceneObjects.Count > 0)
                {
                    BuildLayout.ExplicitAsset sceneAsset = file.Assets.First(x => x.AssetPath.EndsWith(".unity"));
                    AssetBucket bucket = GetOrCreate(buckets, sceneAsset.Guid);
                    bucket.objs.AddRange(sceneObjects);
                }

                // Update buckets with a reference to their explicit asset
                file.Assets.ForEach(eAsset =>
                {
                    if (!buckets.TryGetValue(eAsset.Guid, out AssetBucket b))
                        b = GetOrCreate(buckets, eAsset.Guid); // some assets might not pull in any objects
                    b.ExplictAsset = eAsset;
                });

                // Create entries for buckets that are implicitly pulled in
                Dictionary<string, BuildLayout.DataFromOtherAsset> guidToOtherData = new Dictionary<string, BuildLayout.DataFromOtherAsset>();
                foreach (AssetBucket bucket in buckets.Values.Where(x => x.ExplictAsset == null))
                {
                    string assetPath = bucket.isFilePathBucket ? bucket.guid : AssetDatabase.GUIDToAssetPath(bucket.guid);
                    if (assetPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || assetPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        file.MonoScriptCount++;
                        file.MonoScriptSize += bucket.CalcObjectSize();
                        continue;
                    }
                    var otherData = new BuildLayout.DataFromOtherAsset();
                    otherData.AssetPath = assetPath;
                    otherData.AssetGuid = bucket.guid;
                    otherData.SerializedSize = bucket.CalcObjectSize();
                    otherData.StreamedSize = bucket.CalcStreamedSize();
                    otherData.ObjectCount = bucket.objs.Count;
                    file.OtherAssets.Add(otherData);
                    guidToOtherData[otherData.AssetGuid] = otherData;

                    if (!guidToPulledInBuckets.TryGetValue(otherData.AssetGuid, out List<BuildLayout.DataFromOtherAsset> bucketList))
                        bucketList = guidToPulledInBuckets[otherData.AssetGuid] = new List<BuildLayout.DataFromOtherAsset>();
                    bucketList.Add(otherData);
                }

                // Add references
                foreach (BuildLayout.ExplicitAsset asset in file.Assets)
                {
                    AssetBucket bucket = buckets[asset.Guid];
                    asset.SerializedSize = bucket.CalcObjectSize();
                    asset.StreamedSize = bucket.CalcStreamedSize();

                    IEnumerable<ObjectIdentifier> refs = null;
                    if (m_DependencyData.AssetInfo.TryGetValue(new GUID(asset.Guid), out AssetLoadInfo info))
                        refs = info.referencedObjects;
                    else
                        refs = m_DependencyData.SceneInfo[new GUID(asset.Guid)].referencedObjects;
                    foreach (string refGUID in refs.Select(x => x.guid.Empty() ? x.filePath : x.guid.ToString()).Distinct())
                    {
                        if (guidToOtherData.TryGetValue(refGUID, out BuildLayout.DataFromOtherAsset dfoa))
                        {
                            dfoa.ReferencingAssets.Add(asset);
                            asset.InternalReferencedOtherAssets.Add(dfoa);
                        }
                        else if (buckets.TryGetValue(refGUID, out AssetBucket refBucket))
                        {
                            asset.InternalReferencedExplicitAssets.Add(refBucket.ExplictAsset);
                        }
                        else if (lookup.GuidToExplicitAsset.TryGetValue(refGUID, out BuildLayout.ExplicitAsset refAsset))
                        {
                            asset.ExternallyReferencedAssets.Add(refAsset);
                        }
                    }
                }
            }

            BuildLayout layout = new BuildLayout();

            // This is the addressables section. Everything above could technically be moved to SBP.
            {
                AddressableAssetsBuildContext aaContext = (AddressableAssetsBuildContext)m_AaBuildContext;
                // Map from GUID to AddrssableAssetEntry
                Dictionary<string, AddressableAssetEntry> guidToEntry = aaContext.assetEntries.ToDictionary(x => x.guid, x => x);
                Dictionary<string, string> groupNameToBuildPath = new Dictionary<string, string>();
                
                // create groups
                foreach (AddressableAssetGroup group in aaContext.Settings.groups)
                {
                    if (group.Name != group.name)
                    {
                        Debug.LogWarningFormat("Group name in settings does not match name in group asset, reset group name: \"{0}\" to \"{1}\"", group.name, group.Name);
                        group.Name = group.Name;
                    }
                    
                    var grp = new BuildLayout.Group();
                    grp.Name = group.Name;
                    grp.Guid = group.Guid;

                    foreach (AddressableAssetGroupSchema schema in group.Schemas)
                    {
                        var sd = new BuildLayout.SchemaData();
                        sd.Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(schema));
                        sd.Type = schema.GetType().Name.ToString();

                        BundledAssetGroupSchema bSchema = schema as BundledAssetGroupSchema;
                        if (bSchema != null)
                        {
                            sd.KvpDetails.Add(new Tuple<string, string>("PackingMode", bSchema.BundleMode.ToString()));
                            sd.KvpDetails.Add(new Tuple<string, string>("Compression", bSchema.Compression.ToString()));
                            groupNameToBuildPath[group.name] = bSchema.BuildPath.GetValue(aaContext.Settings);
                        }
                        grp.Schemas.Add(sd);
                    }

                    lookup.GroupLookup.Add(group.Guid, grp);
                    layout.Groups.Add(grp);
                }

                // go through all the bundles and put them in groups
                foreach (BuildLayout.Bundle b in lookup.Bundles.Values)
                {
                    if (aaContext.bundleToImmediateBundleDependencies.TryGetValue(b.Name, out List<string> deps))
                        b.Dependencies = deps.Select(x => lookup.Bundles[x]).Where(x => b != x).ToList();
                    if (aaContext.bundleToExpandedBundleDependencies.TryGetValue(b.Name, out List<string> deps2))
                        b.ExpandedDependencies = deps2.Select(x => lookup.Bundles[x]).Where(x => b != x).ToList();

                    if (aaContext.bundleToAssetGroup.TryGetValue(b.Name, out string grpName))
                    {
                        var assetGroup = lookup.GroupLookup[grpName];
                        b.Name = m_BundleNameRemap[b.Name];
                        b.FileSize = (ulong)new FileInfo(Path.Combine(groupNameToBuildPath[assetGroup.Name], b.Name)).Length;
                        assetGroup.Bundles.Add(b);
                    }
                    else
                    {
                        // will still be associated with a group and can be found in assetGroupToBundles
                        foreach (KeyValuePair<AddressableAssetGroup,List<string>> pair in aaContext.assetGroupToBundles)
                        {
                            foreach (string s in pair.Value)
                            {
                                if (s == b.Name)
                                {
                                    b.Name = m_BundleNameRemap[b.Name];
                                    b.FileSize = (ulong)new FileInfo(Path.Combine(groupNameToBuildPath[pair.Key.Name], b.Name)).Length;
                                    break;
                                }
                            }
                            if (b.FileSize > 0)
                                break;
                        }
                        layout.BuiltInBundles.Add(b);
                    }
                }

                // Apply the addressable name to the asset
                foreach (BuildLayout.ExplicitAsset a in BuildLayoutHelpers.EnumerateAssets(layout))
                    if (guidToEntry.TryGetValue(a.Guid, out AddressableAssetEntry entry))
                        a.AddressableName = entry.address;
            }

            return layout;
        }

        /// <summary>
        /// Runs the build task with the injected context.
        /// </summary>
        /// <returns>The success or failure ReturnCode</returns>
        public ReturnCode Run()
        {
            BuildLayout layout = CreateBuildLayout();

            Directory.CreateDirectory(Path.GetDirectoryName(m_LayoutTextFile));
            using (FileStream s = File.Open(m_LayoutTextFile, FileMode.Create))
                BuildLayoutPrinter.WriteBundleLayout(s, layout);

            UnityEngine.Debug.Log($"Build layout written to {m_LayoutTextFile}");

            s_LayoutCompleteCallback?.Invoke(layout);

            return ReturnCode.Success;
        }
    }
}
