using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;

namespace UnityEditor.AddressableAssets.Build.DataBuilders
{
    /// <summary>
    /// Base class for build script assets
    /// </summary>
    public class BuildScriptBase : ScriptableObject, IDataBuilder
    {
        /// <summary>
        /// The type of instance provider to create for the Addressables system.
        /// </summary>
        [FormerlySerializedAs("m_InstanceProviderType")]
        [SerializedTypeRestrictionAttribute(type = typeof(IInstanceProvider))]
        public SerializedType instanceProviderType = new SerializedType() { Value = typeof(InstanceProvider) };
        /// <summary>
        /// The type of scene provider to create for the addressables system.
        /// </summary>
        [FormerlySerializedAs("m_SceneProviderType")]
        [SerializedTypeRestrictionAttribute(type = typeof(ISceneProvider))]
        public SerializedType sceneProviderType = new SerializedType() { Value = typeof(SceneProvider) };

        /// <summary>
        /// Stores the logged information of all the build tasks.
        /// </summary>
        public IBuildLogger Log { get { return m_Log; } }

        [NonSerialized]
        internal IBuildLogger m_Log;

        /// <summary>
        /// The descriptive name used in the UI.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return "Undefined";
            }
        }
        internal static void WriteBuildLog(BuildLog log, string directory)
        {
            Directory.CreateDirectory(directory);
            PackageManager.PackageInfo info = PackageManager.PackageInfo.FindForAssembly(typeof(BuildScriptBase).Assembly);
            log.AddMetaData(info.name, info.version);
            File.WriteAllText(Path.Combine(directory, "AddressablesBuildTEP.json"), log.FormatForTraceEventProfiler());
        }

        /// <summary>
        /// Build the specified data with the provided builderInput.  This is the public entry point.
        ///  Child class overrides should use <see cref="BuildDataImplementation{TResult}"/>
        /// </summary>
        /// <typeparam name="TResult">The type of data to build.</typeparam>
        /// <param name="builderInput">The builderInput object used in the build.</param>
        /// <returns>The build data result.</returns>
        public TResult BuildData<TResult>(AddressablesDataBuilderInput builderInput) where TResult : IDataBuilderResult
        {
            if (!CanBuildData<TResult>())
            {
                var message = "Data builder " + Name + " cannot build requested type: " + typeof(TResult);
                Debug.LogError(message);
                return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, message);
            }

            m_Log = (builderInput.Logger != null) ? builderInput.Logger : new BuildLog();

            AddressablesRuntimeProperties.ClearCachedPropertyValues();

            TResult result = default;
            // Append the file registry to the results
            using (m_Log.ScopedStep(LogLevel.Info, $"Building {this.Name}"))
            {
                try
                {
                    result = BuildDataImplementation<TResult>(builderInput);
                }
                catch (Exception e)
                {
                    string errMessage;
                    if (e.Message == "path")
                        errMessage = "Invalid path detected during build. Check for unmatched brackets in your active profile's variables.";
                    else
                        errMessage = e.Message;
                    
                    Debug.LogError(errMessage);
                    return AddressableAssetBuildResult.CreateResult<TResult>(null, 0, errMessage);
                }
                if (result != null)
                    result.FileRegistry = builderInput.Registry;
            }

            if (builderInput.Logger == null && m_Log != null)
                WriteBuildLog((BuildLog)m_Log, Path.GetDirectoryName(Application.dataPath) + "/" + Addressables.LibraryPath);

            return result;
        }

        /// <summary>
        /// The implementation of <see cref="BuildData{TResult}"/>.  That is the public entry point,
        ///  this is the home for child class overrides.
        /// </summary>
        /// <param name="builderInput">The builderInput object used in the build</param>
        /// <typeparam name="TResult">The type of data to build</typeparam>
        /// <returns>The build data result</returns>
        protected virtual TResult BuildDataImplementation<TResult>(AddressablesDataBuilderInput builderInput) where TResult : IDataBuilderResult
        {
            return default(TResult);
        }

        /// <summary>
        /// Loops over each group, after doing some data checking.
        /// </summary>
        /// <param name="aaContext">The Addressables builderInput object to base the group processing on</param>
        /// <returns>An error string if there were any problems processing the groups</returns>
        protected virtual string ProcessAllGroups(AddressableAssetsBuildContext aaContext)
        {
            if (aaContext == null ||
                aaContext.Settings == null ||
                aaContext.Settings.groups == null)
            {
                return "No groups found to process in build script " + Name;
            }
            //intentionally for not foreach so groups can be added mid-loop.
            for (int index = 0; index < aaContext.Settings.groups.Count; index++)
            {
                AddressableAssetGroup assetGroup = aaContext.Settings.groups[index];
                if (assetGroup == null)
                    continue;

                if (assetGroup.Schemas.Find((x) => x.GetType() == typeof(PlayerDataGroupSchema)) &&
                    assetGroup.Schemas.Find((x) => x.GetType() == typeof(BundledAssetGroupSchema)))
                {
                    EditorUtility.ClearProgressBar();
                    return $"Addressable group {assetGroup.Name} cannot have both a {typeof(PlayerDataGroupSchema).Name} and a {typeof(BundledAssetGroupSchema).Name}";
                }

                EditorUtility.DisplayProgressBar($"Processing Addressable Group", assetGroup.Name, (float)index / aaContext.Settings.groups.Count);
                var errorString = ProcessGroup(assetGroup, aaContext);
                if (!string.IsNullOrEmpty(errorString))
                {
                    EditorUtility.ClearProgressBar();
                    return errorString;
                }
            }

            EditorUtility.ClearProgressBar();
            return string.Empty;
        }

        /// <summary>
        /// Build processing of an individual group.
        /// </summary>
        /// <param name="assetGroup">The group to process</param>
        /// <param name="aaContext">The Addressables builderInput object to base the group processing on</param>
        /// <returns>An error string if there were any problems processing the groups</returns>
        protected virtual string ProcessGroup(AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            return string.Empty;
        }

        /// <summary>
        /// Used to determine if this builder is capable of building a specific type of data.
        /// </summary>
        /// <typeparam name="T">The type of data needed to be built.</typeparam>
        /// <returns>True if this builder can build this data.</returns>
        public virtual bool CanBuildData<T>() where T : IDataBuilderResult
        {
            return false;
        }

        /// <summary>
        /// Utility method for creating locations from player data.
        /// </summary>
        /// <param name="playerDataSchema">The schema for the group.</param>
        /// <param name="assetGroup">The group to extract the locations from.</param>
        /// <param name="locations">The list of created locations to fill in.</param>
        /// <param name="providerTypes">Any unknown provider types are added to this set in order to ensure they are not stripped.</param>
        /// <returns>True if any legacy locations were created.  This is used by the build scripts to determine if a legacy provider is needed.</returns>
        protected bool CreateLocationsForPlayerData(PlayerDataGroupSchema playerDataSchema, AddressableAssetGroup assetGroup, List<ContentCatalogDataEntry> locations, HashSet<Type> providerTypes)
        {
            bool needsLegacyProvider = false;
            if (playerDataSchema != null && (playerDataSchema.IncludeBuildSettingsScenes || playerDataSchema.IncludeResourcesFolders))
            {
                var entries = new List<AddressableAssetEntry>();
                assetGroup.GatherAllAssets(entries, true, true, false);
                foreach (var a in entries.Where(e => e.IsInSceneList || e.IsInResources))
                {
                    if (!playerDataSchema.IncludeBuildSettingsScenes && a.IsInSceneList)
                        continue;
                    if (!playerDataSchema.IncludeResourcesFolders && a.IsInResources)
                        continue;
                    a.CreateCatalogEntries(locations, false, a.IsScene ? "" : typeof(LegacyResourcesProvider).FullName, null, null, providerTypes);
                    if (!a.IsScene)
                        needsLegacyProvider = true;
                }
            }
            return needsLegacyProvider;
        }

        /// <summary>
        /// Utility method for deleting files.
        /// </summary>
        /// <param name="path">The file path to delete.</param>
        protected static void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Utility method to write a file.  The directory will be created if it does not exist.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <param name="content">The content of the file.</param>
        /// <param name="registry">The file registry used to track all produced artifacts.</param>
        /// <returns>True if the file was written.</returns>
        protected static bool WriteFile(string path, string content, FileRegistry registry)
        {
            try
            {
                registry.AddFile(path);
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, content);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                registry.RemoveFile(path);
                return false;
            }
        }

        /// <summary>
        /// Used to clean up any cached data created by this builder.
        /// </summary>
        public virtual void ClearCachedData()
        {
        }

        /// <summary>
        /// Checks to see if the data is built for the given builder.
        /// </summary>
        /// <returns>Returns true if the data is built. Returns false otherwise.</returns>
        public virtual bool IsDataBuilt()
        {
            return false;
        }
    }
}
