using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;

namespace UnityEditor.AddressableAssets.Settings.GroupSchemas
{
    /// <summary>
    /// Schema used for bundled asset groups.
    /// </summary>
//    [CreateAssetMenu(fileName = "BundledAssetGroupSchema.asset", menuName = "Addressables/Group Schemas/Bundled Assets")]
    [DisplayName("Content Packing & Loading")]
    public class BundledAssetGroupSchema : AddressableAssetGroupSchema, IHostingServiceConfigurationProvider, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Defines how bundles are created.
        /// </summary>
        public enum BundlePackingMode
        {
            /// <summary>
            /// Creates a bundle for all non-scene entries and another for all scenes entries.
            /// </summary>
            PackTogether,
            /// <summary>
            /// Creates a bundle per entry.  This is useful if each entry is a folder as all sub entries will go to the same bundle.
            /// </summary>
            PackSeparately,
            /// <summary>
            /// Creates a bundle per unique set of labels
            /// </summary>
            PackTogetherByLabel
        }

        /// <summary>
        /// Defines how internal bundles are named. This is used for both caching and for inter-bundle dependecies.  If possible, GroupGuidProjectIdHash should be used as it is stable and unique between projects.
        /// </summary>
        public enum BundleInternalIdMode
        {
            /// <summary>
            /// Use the guid of the group asset
            /// </summary>
            GroupGuid,
            /// <summary>
            /// Use the hash of the group asset guid and the project id
            /// </summary>
            GroupGuidProjectIdHash,
            /// <summary>
            /// Use the hash of the group asset, the project id and the guids of the entries in the group
            /// </summary>
            GroupGuidProjectIdEntriesHash
        }

        /// <summary>
        /// Options for compressing bundles in this group.
        /// </summary>
        public enum BundleCompressionMode
        {
            /// <summary>
            /// Use to indicate that bundles will not be compressed.
            /// </summary>
            Uncompressed,
            /// <summary>
            /// Use to indicate that bundles will be compressed using the LZ4 compression algorithm.
            /// </summary>
            LZ4,
            /// <summary>
            /// Use to indicate that bundles will be compressed using the LZMA compression algorithm.
            /// </summary>
            LZMA
        }

        [SerializeField]
        BundleInternalIdMode m_InternalBundleIdMode = BundleInternalIdMode.GroupGuidProjectIdHash;
        /// <summary>
        /// Internal bundle naming mode
        /// </summary>
        public BundleInternalIdMode InternalBundleIdMode
        {
            get => m_InternalBundleIdMode;
            set
            {
                if (m_InternalBundleIdMode != value)
                {
                    m_InternalBundleIdMode = value;
                    SetDirty(true);
                }
            }
        }

        [SerializeField]
        BundleCompressionMode m_Compression = BundleCompressionMode.LZ4;

        /// <summary>
        /// Build compression.
        /// </summary>
        public BundleCompressionMode Compression
        {
            get => m_Compression;
            set
            {
                if (m_Compression != value)
                {
                    m_Compression = value;
                    SetDirty(true);
                }
            }
        }

        /// <summary>
        /// Options for internal id of assets in bundles.
        /// </summary>
        public enum AssetNamingMode
        {
            /// <summary>
            /// Use to identify assets by their full path.
            /// </summary>
            FullPath,
            /// <summary>
            /// Use to identify assets by their filename only.  There is a risk of collisions when assets in different folders have the same filename.
            /// </summary>
            Filename,
            /// <summary>
            /// Use to identify assets by their asset guid.  This will save space over using the full path and will be stable if assets move in the project.
            /// </summary>
            GUID,
            /// <summary>
            /// This method attempts to use the smallest identifier for internal asset ids.  For asset bundles with very few items, this can save a significant amount of space in the content catalog.
            /// </summary>
            Dynamic
        }

        [SerializeField]
        bool m_IncludeAddressInCatalog = true;
        [SerializeField]
        bool m_IncludeGUIDInCatalog = true;
        [SerializeField]
        bool m_IncludeLabelsInCatalog = true;

        /// <summary>
        /// If enabled, addresses are included in the content catalog.  This is required if assets are to be loaded via their main address.
        /// </summary>
        public bool IncludeAddressInCatalog
        {
            get => m_IncludeAddressInCatalog;
            set
            {
                if (m_IncludeAddressInCatalog != value)
                {
                    m_IncludeAddressInCatalog = value;
                    SetDirty(true);
                }
            }
        }

        /// <summary>
        /// If enabled, guids are included in content catalogs.  This is required if assets are to be loaded via AssetReference.
        /// </summary>
        public bool IncludeGUIDInCatalog
        {
            get => m_IncludeGUIDInCatalog;
            set
            {
                if (m_IncludeGUIDInCatalog != value)
                {
                    m_IncludeGUIDInCatalog = value;
                    SetDirty(true);
                }
            }
        }

        /// <summary>
        /// If enabled, labels are included in the content catalogs.  This is required if labels are used at runtime load load assets.
        /// </summary>
        public bool IncludeLabelsInCatalog
        {
            get => m_IncludeLabelsInCatalog;
            set
            {
                if (m_IncludeLabelsInCatalog != value)
                {
                    m_IncludeLabelsInCatalog = value;
                    SetDirty(true);
                }
            }
        }

        /// <summary>
        /// Internal Id mode for assets in bundles.
        /// </summary>
        public AssetNamingMode InternalIdNamingMode
        {
            get { return m_InternalIdNamingMode; }
            set { m_InternalIdNamingMode = value; SetDirty(true); }
        }

        [SerializeField]
        [Tooltip("Indicates how the internal asset name will be generated.")]
        AssetNamingMode m_InternalIdNamingMode = AssetNamingMode.FullPath;


        /// <summary>
        /// Behavior for clearing old bundles from the cache.
        /// </summary>
        public enum CacheClearBehavior
        {
            /// <summary>
            /// Bundles are only removed from the cache when space is needed.
            /// </summary>
            ClearWhenSpaceIsNeededInCache,
            /// <summary>
            /// Bundles are removed from the cache when a newer version has been loaded successfully.
            /// </summary>
            ClearWhenWhenNewVersionLoaded,
        }

        [SerializeField]
        CacheClearBehavior m_CacheClearBehavior = CacheClearBehavior.ClearWhenSpaceIsNeededInCache;
        /// <summary>
        /// Determines how other cached versions of asset bundles are cleared.
        /// </summary>
        public CacheClearBehavior AssetBundledCacheClearBehavior { get { return m_CacheClearBehavior; } set { m_CacheClearBehavior = value; } }


        /// <summary>
        /// Gets the build compression settings for bundles in this group.
        /// </summary>
        /// <param name="bundleId">The bundle id.</param>
        /// <returns>The build compression.</returns>
        public virtual BuildCompression GetBuildCompressionForBundle(string bundleId)
        {
            //Unfortunately the BuildCompression struct is not serializable (nor is it settable), therefore this enum needs to be used to return the static members....
            switch (m_Compression)
            {
                case BundleCompressionMode.Uncompressed: return BuildCompression.Uncompressed;
                case BundleCompressionMode.LZ4: return BuildCompression.LZ4;
                case BundleCompressionMode.LZMA: return BuildCompression.LZMA;
            }
            return default(BuildCompression);
        }

        [FormerlySerializedAs("m_includeInBuild")]
        [SerializeField]
        [Tooltip("If true, the assets in this group will be included in the build of bundles.")]
        bool m_IncludeInBuild = true;
        /// <summary>
        /// If true, the assets in this group will be included in the build of bundles.
        /// </summary>
        public bool IncludeInBuild
        {
            get => m_IncludeInBuild;
            set
            {
                if (m_IncludeInBuild != value)
                {
                    m_IncludeInBuild = value;
                    SetDirty(true);
                }
            }
        }
        [SerializeField]
        [SerializedTypeRestriction(type = typeof(IResourceProvider))]
        [Tooltip("The provider type to use for loading assets from bundles.")]
        SerializedType m_BundledAssetProviderType;
        /// <summary>
        /// The provider type to use for loading assets from bundles.
        /// </summary>
        public SerializedType BundledAssetProviderType { get { return m_BundledAssetProviderType; } }

        [SerializeField]
        [Tooltip("If true, the bundle and asset provider for assets in this group will get unique provider ids and will only provide for assets in this group.")]
        bool m_ForceUniqueProvider = false;
        /// <summary>
        /// If true, the bundle and asset provider for assets in this group will get unique provider ids and will only provide for assets in this group.
        /// </summary>
        public bool ForceUniqueProvider
        {
            get => m_ForceUniqueProvider;
            set
            {
                if (m_ForceUniqueProvider != value)
                {
                    m_ForceUniqueProvider = value;
                    SetDirty(true);
                }
            }
        }

        [FormerlySerializedAs("m_useAssetBundleCache")]
        [SerializeField]
        [Tooltip("If true, the Hash value of the asset bundle is used to determine if a bundle can be loaded from the local cache instead of downloaded. (Only applies to remote asset bundles)")]
        bool m_UseAssetBundleCache = true;
        /// <summary>
        /// If true, the CRC and Hash values of the asset bundle are used to determine if a bundle can be loaded from the local cache instead of downloaded.
        /// </summary>
        public bool UseAssetBundleCache
        {
            get => m_UseAssetBundleCache;
            set
            {
                if (m_UseAssetBundleCache != value)
                {
                    m_UseAssetBundleCache = value;
                    SetDirty(true);
                }
            }
        }

        [SerializeField]
        [Tooltip("If true, the CRC (Cyclic Redundancy Check) of the asset bundle is used to check the integrity.  This can be used for both local and remote bundles.")]
        bool m_UseAssetBundleCrc = true;

        /// <summary>
        /// If true, the CRC and Hash values of the asset bundle are used to determine if a bundle can be loaded from the local cache instead of downloaded.
        /// </summary>
        public bool UseAssetBundleCrc
        {
            get => m_UseAssetBundleCrc;
            set
            {
                if (m_UseAssetBundleCrc != value)
                {
                    m_UseAssetBundleCrc = value;
                    SetDirty(true);
                }
            }
        }
        [SerializeField]
        [Tooltip("If true, the CRC (Cyclic Redundancy Check) of the asset bundle is used to check the integrity.")]
        bool m_UseAssetBundleCrcForCachedBundles = true;
        /// <summary>
        /// If true, the CRC and Hash values of the asset bundle are used to determine if a bundle can be loaded from the local cache instead of downloaded.
        /// </summary>
        public bool UseAssetBundleCrcForCachedBundles
        {
            get => m_UseAssetBundleCrcForCachedBundles;
            set
            {
                if (m_UseAssetBundleCrcForCachedBundles != value)
                {
                    m_UseAssetBundleCrcForCachedBundles = value;
                    SetDirty(true);
                }
            }
        }
        [SerializeField]
        [Tooltip("If true, local asset bundles will be loaded through UnityWebRequest.")]
        bool m_UseUWRForLocalBundles = false;
        /// <summary>
        /// If true, local asset bundles will be loaded through UnityWebRequest.
        /// </summary>
        public bool UseUnityWebRequestForLocalBundles
        {
            get => m_UseUWRForLocalBundles;
            set
            {
                if (m_UseUWRForLocalBundles != value)
                {
                    m_UseUWRForLocalBundles = value;
                    SetDirty(true);
                }
            }
        }
        [FormerlySerializedAs("m_timeout")]
        [SerializeField]
        [Tooltip("Attempt to abort after the number of seconds in timeout have passed, where the UnityWebRequest has received no data. (Only applies to remote asset bundles)")]
        int m_Timeout;
        /// <summary>
        /// Attempt to abort after the number of seconds in timeout have passed, where the UnityWebRequest has received no data.
        /// </summary>
        public int Timeout
        {
            get => m_Timeout;
            set
            {
                if (m_Timeout != value)
                {
                    m_Timeout = value;
                    SetDirty(true);
                }
            }
        }

        [FormerlySerializedAs("m_chunkedTransfer")]
        [SerializeField]
        [Tooltip("Deprecated in 2019.3+. Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method. (Only applies to remote asset bundles)")]
        bool m_ChunkedTransfer;
        /// <summary>
        /// Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method.
        /// </summary>
        public bool ChunkedTransfer
        {
            get => m_ChunkedTransfer;
            set
            {
                if (m_ChunkedTransfer != value)
                {
                    m_ChunkedTransfer = value;
                    SetDirty(true);
                }
            }
        }


        [FormerlySerializedAs("m_redirectLimit")]
        [SerializeField]
        [Tooltip("Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error. (Only applies to remote asset bundles)")]
        int m_RedirectLimit = -1;
        /// <summary>
        /// Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error.
        /// </summary>
        public int RedirectLimit
        {
            get => m_RedirectLimit;
            set
            {
                if (m_RedirectLimit != value)
                {
                    m_RedirectLimit = value;
                    SetDirty(true);
                }
            }
        }
        [FormerlySerializedAs("m_retryCount")]
        [SerializeField]
        [Tooltip("Indicates the number of times the request will be retried.")]
        int m_RetryCount;
        /// <summary>
        /// Indicates the number of times the request will be retried.
        /// </summary>
        public int RetryCount
        {
            get => m_RetryCount;
            set
            {
                if (m_RetryCount != value)
                {
                    m_RetryCount = value;
                    SetDirty(true);
                }
            }
        }

        [FormerlySerializedAs("m_buildPath")]
        [SerializeField]
        [Tooltip("The path to copy asset bundles to.")]
        ProfileValueReference m_BuildPath = new ProfileValueReference();
        /// <summary>
        /// The path to copy asset bundles to.
        /// </summary>
        public ProfileValueReference BuildPath
        {
            get { return m_BuildPath; }
        }

        [FormerlySerializedAs("m_loadPath")]
        [SerializeField]
        [Tooltip("The path to load bundles from.")]
        ProfileValueReference m_LoadPath = new ProfileValueReference();
        /// <summary>
        /// The path to load bundles from.
        /// </summary>
        public ProfileValueReference LoadPath
        {
            get { return m_LoadPath; }
        }

        //placeholder for UrlSuffix support...
        internal string UrlSuffix
        {
            get { return string.Empty; }
        }

        [FormerlySerializedAs("m_bundleMode")]
        [SerializeField]
        [Tooltip("Controls how bundles are packed.  If set to PackTogether, a single asset bundle will be created for the entire group, with the exception of scenes, which are packed in a second bundle.  If set to PackSeparately, an asset bundle will be created for each entry in the group; in the case that an entry is a folder, one bundle is created for the folder and all of its sub entries.")]
        BundlePackingMode m_BundleMode = BundlePackingMode.PackTogether;
        /// <summary>
        /// Controls how bundles are packed.  If set to PackTogether, a single asset bundle will be created for the entire group, with the exception of scenes, which are packed in a second bundle.  If set to PackSeparately, an asset bundle will be created for each entry in the group; in the case that an entry is a folder, one bundle is created for the folder and all of its sub entries.
        /// </summary>
        public BundlePackingMode BundleMode
        {
            get => m_BundleMode;
            set
            {
                if (m_BundleMode != value)
                {
                    m_BundleMode = value;
                    SetDirty(true);
                }
            }
        }

        /// <inheritdoc/>
        public string HostingServicesContentRoot
        {
            get
            {
                return BuildPath?.GetValue(Group.Settings);
            }
        }

        [FormerlySerializedAs("m_assetBundleProviderType")]
        [SerializeField]
        [SerializedTypeRestriction(type = typeof(IResourceProvider))]
        [Tooltip("The provider type to use for loading asset bundles.")]
        SerializedType m_AssetBundleProviderType;
        /// <summary>
        /// The provider type to use for loading asset bundles.
        /// </summary>
        public SerializedType AssetBundleProviderType { get { return m_AssetBundleProviderType; } }

        /// <summary>
        /// Used to determine if dropdown should be custom
        /// </summary>
        private bool m_UseCustomPaths = false;


        /// <summary>
        /// Internal settings
        /// </summary>
        internal AddressableAssetSettings settings
        {
            get { return AddressableAssetSettingsDefaultObject.Settings; }
        }

        /// <summary>
        /// Set default values taken from the assigned group.
        /// </summary>
        /// <param name="group">The group this schema has been added to.</param>
        protected override void OnSetGroup(AddressableAssetGroup group)
        {
            //this can happen during the load of the addressables asset
        }

        internal void SetPathVariable(AddressableAssetSettings addressableAssetSettings, ref ProfileValueReference path, string newPathName, string oldPathName, List<string> variableNames)
        {
            if (path == null || !path.HasValue(addressableAssetSettings))
            {
                bool hasNewPath = variableNames.Contains(newPathName);
                bool hasOldPath = variableNames.Contains(oldPathName);

                if (hasNewPath && string.IsNullOrEmpty(path?.Id))
                {
                    path = new ProfileValueReference();
                    path.SetVariableByName(addressableAssetSettings, newPathName);
                    SetDirty(true);
                }
                else if (hasOldPath && string.IsNullOrEmpty(path?.Id))
                {
                    path = new ProfileValueReference();
                    path.SetVariableByName(addressableAssetSettings, oldPathName);
                    SetDirty(true);
                }
                else if (!hasOldPath && !hasNewPath)
                    Debug.LogWarning("Default path variable " + newPathName + " not found when initializing BundledAssetGroupSchema. Please manually set the path via the groups window.");
            }
        }

        internal override void Validate()
        {
            if (Group != null && Group.Settings != null)
            {
                List<string> variableNames = Group.Settings.profileSettings.GetVariableNames();
                SetPathVariable(Group.Settings, ref m_BuildPath, AddressableAssetSettings.kLocalBuildPath, "LocalBuildPath", variableNames);
                SetPathVariable(Group.Settings, ref m_LoadPath, AddressableAssetSettings.kLocalLoadPath, "LocalLoadPath", variableNames);
            }

            if (m_AssetBundleProviderType.Value == null)
                m_AssetBundleProviderType.Value = typeof(AssetBundleProvider);
            if (m_BundledAssetProviderType.Value == null)
                m_BundledAssetProviderType.Value = typeof(BundledAssetProvider);
        }

        internal string GetAssetLoadPath(string assetPath, HashSet<string> otherLoadPaths, Func<string, string> pathToGUIDFunc)
        {
            switch (InternalIdNamingMode)
            {
                case AssetNamingMode.FullPath: return assetPath;
                case AssetNamingMode.Filename: return assetPath.EndsWith(".unity") ? System.IO.Path.GetFileNameWithoutExtension(assetPath) : System.IO.Path.GetFileName(assetPath);
                case AssetNamingMode.GUID: return pathToGUIDFunc(assetPath);
                case AssetNamingMode.Dynamic:
                {
                    var g = pathToGUIDFunc(assetPath);
                    if (otherLoadPaths == null)
                        return g;
                    var len = 1;
                    var p = g.Substring(0, len);
                    while (otherLoadPaths.Contains(p))
                        p = g.Substring(0, ++len);
                    otherLoadPaths.Add(p);
                    return p;
                }
            }
            return assetPath;
        }

        /// <summary>
        /// Impementation of ISerializationCallbackReceiver, does nothing.
        /// </summary>
        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Impementation of ISerializationCallbackReceiver, used to set callbacks for ProfileValueReference changes.
        /// </summary>
        public void OnAfterDeserialize()
        {
            BuildPath.OnValueChanged += s => SetDirty(true);
            LoadPath.OnValueChanged += s => SetDirty(true);
            if (m_AssetBundleProviderType.Value == null)
                m_AssetBundleProviderType.Value = typeof(AssetBundleProvider);
            if (m_BundledAssetProviderType.Value == null)
                m_BundledAssetProviderType.Value = typeof(BundledAssetProvider);
        }

        /// <summary>
        /// Returns the id of the asset provider needed to load from this group.
        /// </summary>
        /// <returns>The id of the cached provider needed for this group.</returns>
        public string GetAssetCachedProviderId()
        {
            return ForceUniqueProvider ? string.Format("{0}_{1}", BundledAssetProviderType.Value.FullName, Group.Guid) : BundledAssetProviderType.Value.FullName;
        }

        /// <summary>
        /// Returns the id of the bundle provider needed to load from this group.
        /// </summary>
        /// <returns>The id of the cached provider needed for this group.</returns>
        public string GetBundleCachedProviderId()
        {
            return ForceUniqueProvider ? string.Format("{0}_{1}", AssetBundleProviderType.Value.FullName, Group.Guid) : AssetBundleProviderType.Value.FullName;
        }

        /// <summary>
        /// Used to determine how the final bundle name should look.
        /// </summary>
        public enum BundleNamingStyle
        {
            /// <summary>
            /// Use to indicate that the hash should be appended to the bundle name.
            /// </summary>
            AppendHash,
            /// <summary>
            /// Use to indicate that the bundle name should not contain the hash.
            /// </summary>
            NoHash,
            /// <summary>
            /// Use to indicate that the bundle name should only contain the given hash.
            /// </summary>
            OnlyHash,
            /// <summary>
            /// Use to indicate that the bundle name should only contain the hash of the file name.
            /// </summary>
            FileNameHash
        }

        /// <summary>
        /// Used to draw the Bundle Naming popup
        /// </summary>
        [CustomPropertyDrawer(typeof(BundleNamingStyle))]
        class BundleNamingStylePropertyDrawer : PropertyDrawer
        {
            /// <summary>
            /// Custom Drawer for the BundleNamingStyle in order to display easier to understand display names.
            /// </summary>
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                bool showMixedValue = EditorGUI.showMixedValue;
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.showMixedValue = showMixedValue;

                GUIContent[] contents = new GUIContent[4];
                contents[0] = new GUIContent("Filename", "Leave filename unchanged.");
                contents[1] = new GUIContent("Append Hash to Filename", "Append filename with the AssetBundle content hash.");
                contents[2] = new GUIContent("Use Hash of AssetBundle", "Replace filename with AssetBundle hash.");
                contents[3] = new GUIContent("Use Hash of Filename", "Replace filename with hash of filename.");

                int enumValue = property.enumValueIndex;
                enumValue = enumValue == 0 ? 1 : enumValue == 1 ? 0 : enumValue;

                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUI.Popup(position, new GUIContent(label.text, "Controls how the output AssetBundle's will be named."), enumValue, contents);
                if (EditorGUI.EndChangeCheck())
                {
                    newValue = newValue == 0 ? 1 : newValue == 1 ? 0 : newValue;
                    property.enumValueIndex = newValue;
                }

                EditorGUI.EndProperty();
            }
        }

        [SerializeField]
        BundleNamingStyle m_BundleNaming;
        /// <summary>
        /// Naming style to use for generated AssetBundle(s).
        /// </summary>
        public BundleNamingStyle BundleNaming
        {
            get { return m_BundleNaming; }
            set
            {
                m_BundleNaming = value;
                SetDirty(true);
            }
        }

        [SerializeField]
        AssetLoadMode m_AssetLoadMode;
        /// <summary>
        /// Will load all Assets into memory from the AssetBundle after the AssetBundle is loaded.
        /// </summary>
        public AssetLoadMode AssetLoadMode
        {
            get { return m_AssetLoadMode; }
            set
            {
                m_AssetLoadMode = value;
                SetDirty(true);
            }
        }

        private bool m_ShowPaths = true;

        /// <summary>
        /// Used for drawing properties in the inspector.
        /// </summary>
        public override void ShowAllProperties()
        {
            m_ShowPaths = true;
            AdvancedOptionsFoldout.IsActive = true;
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            var so = new SerializedObject(this);

            ShowSelectedPropertyPathPair(so);

            AdvancedOptionsFoldout.IsActive = GUI.AddressablesGUIUtility.FoldoutWithHelp(AdvancedOptionsFoldout.IsActive, new GUIContent("Advanced Options"), () =>
            {
                string url = AddressableAssetUtility.GenerateDocsURL("GroupSettings.html#advanced-options");
                Application.OpenURL(url);
            });
            if (AdvancedOptionsFoldout.IsActive)
                ShowAdvancedProperties(so);
            so.ApplyModifiedProperties();
        }

        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges = null;
            var so = new SerializedObject(this);

            List<BundledAssetGroupSchema> otherBundledSchemas = new List<BundledAssetGroupSchema>();
            foreach (var schema in otherSchemas)
            {
                otherBundledSchemas.Add(schema as BundledAssetGroupSchema);
            }

            foreach (var schema in otherBundledSchemas)
                schema.m_ShowPaths = m_ShowPaths;
            ShowSelectedPropertyPathPairMulti(so, otherSchemas, ref queuedChanges,
                (src, dst) => { dst.m_BuildPath.Id = src.BuildPath.Id; dst.m_LoadPath.Id = src.LoadPath.Id; dst.m_UseCustomPaths = src.m_UseCustomPaths;  dst.SetDirty(true); });

            EditorGUI.BeginChangeCheck();
            AdvancedOptionsFoldout.IsActive = GUI.AddressablesGUIUtility.BeginFoldoutHeaderGroupWithHelp(AdvancedOptionsFoldout.IsActive, new GUIContent("Advanced Options"), () =>
            {
                string url = AddressableAssetUtility.GenerateDocsURL("GroupSettings.html#advanced-options");
                Application.OpenURL(url);
            }, 10);
            if (AdvancedOptionsFoldout.IsActive)
            {
                ShowAdvancedPropertiesMulti(so, otherSchemas, ref queuedChanges);
            }
            EditorGUI.EndFoldoutHeaderGroup();

            so.ApplyModifiedProperties();
            if (queuedChanges != null)
            {
                Undo.SetCurrentGroupName("bundledAssetGroupSchemasUndos");
                foreach (var schema in otherBundledSchemas)
                    Undo.RecordObject(schema, "BundledAssetGroupSchema" + schema.name);

                foreach (var change in queuedChanges)
                {
                    foreach (var schema in otherBundledSchemas)
                        change.Invoke(this, schema);
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        void ShowPaths(SerializedObject so)
        {
            ShowSelectedPropertyPath(so, nameof(m_BuildPath), null, ref m_BuildPath);
            ShowSelectedPropertyPath(so, nameof(m_LoadPath), null, ref m_LoadPath);
        }

        void ShowPathsMulti(SerializedObject so, List<AddressableAssetGroupSchema> otherBundledSchemas, ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges)
        {
            ShowSelectedPropertyMulti(so, nameof(m_BuildPath), null, otherBundledSchemas, ref queuedChanges, (src, dst) => { dst.m_BuildPath.Id = src.BuildPath.Id; dst.SetDirty(true); }, m_BuildPath.Id, ref m_BuildPath);
            ShowSelectedPropertyMulti(so, nameof(m_LoadPath), null, otherBundledSchemas, ref queuedChanges, (src, dst) => { dst.m_LoadPath.Id = src.LoadPath.Id; dst.SetDirty(true); }, m_LoadPath.Id, ref m_LoadPath);
        }

        static GUI.FoldoutSessionStateValue AdvancedOptionsFoldout = new GUI.FoldoutSessionStateValue("Addressables.BundledAssetGroup.AdvancedOptions");

        GUIContent m_CompressionContent = new GUIContent("Asset Bundle Compression", "Compression method to use for asset bundles.");
        GUIContent m_IncludeInBuildContent = new GUIContent("Include in Build", "If disabled, these bundles will not be included in the build.");
        GUIContent m_ForceUniqueProviderContent = new GUIContent("Force Unique Provider", "If enabled, this option forces bundles loaded from this group to use a unique provider.");
        GUIContent m_UseAssetBundleCacheContent = new GUIContent("Use Asset Bundle Cache", "If enabled and supported, the device will cache  asset bundles.");
        GUIContent m_AssetBundleCrcContent = new GUIContent("Asset Bundle CRC", "Defines which Asset Bundles will have their CRC checked when loading to ensure correct content.");
        private GUIContent[] m_CrcPopupContent = new GUIContent[]
        {
            new GUIContent("Disabled", "Bundles will not have their CRC checked when loading."),
            new GUIContent("Enabled, Including Cached", "All Bundles will have their CRC checked when loading."),
            new GUIContent("Enabled, Excluding Cached", "Bundles that have already been downloaded and cached will not have their CRC check when loading, otherwise CRC check will be performed.")
        };
        GUIContent m_UseUWRForLocalBundlesContent = new GUIContent("Use UnityWebRequest for Local Asset Bundles", "If enabled, local asset bundles will load through UnityWebRequest.");
        GUIContent m_TimeoutContent = new GUIContent("Request Timeout", "The timeout with no download activity (in seconds) for the Http request.");
        GUIContent m_ChunkedTransferContent = new GUIContent("Use Http Chunked Transfer", "If enabled, the Http request will use chunked transfers.");
        GUIContent m_RedirectLimitContent = new GUIContent("Http Redirect Limit", "The redirect limit for the Http request.");
        GUIContent m_RetryCountContent = new GUIContent("Retry Count", "The number of times to retry the http request.");
        GUIContent m_IncludeAddressInCatalogContent = new GUIContent("Include Addresses in Catalog", "If disabled, addresses from this group will not be included in the catalog.  This is useful for reducing the size of the catalog if addresses are not needed.");
        GUIContent m_IncludeGUIDInCatalogContent = new GUIContent("Include GUIDs in Catalog", "If disabled, guids from this group will not be included in the catalog.  This is useful for reducing the size of the catalog if guids are not needed.");
        GUIContent m_IncludeLabelsInCatalogContent = new GUIContent("Include Labels in Catalog", "If disabled, labels from this group will not be included in the catalog.  This is useful for reducing the size of the catalog if labels are not needed.");
        GUIContent m_InternalIdNamingModeContent = new GUIContent("Internal Asset Naming Mode", "Mode for naming assets internally in bundles.  This can reduce the size of the catalog by replacing long paths with shorter strings.");
        GUIContent m_InternalBundleIdModeContent = new GUIContent("Internal Bundle Id Mode", $"Specifies how the internal id of the bundle is generated.  This must be set to {BundleInternalIdMode.GroupGuid} or {BundleInternalIdMode.GroupGuidProjectIdHash} to ensure proper caching on device.");
        GUIContent m_CacheClearBehaviorContent = new GUIContent("Cache Clear Behavior", "Controls how old cached asset bundles are cleared.");
        GUIContent m_BundleModeContent = new GUIContent("Bundle Mode", "Controls how bundles are created from this group.");
        GUIContent m_BundleNamingContent = new GUIContent("Bundle Naming Mode", "Controls the final file naming mode for bundles in this group.");
        GUIContent m_AssetLoadModeContent = new GUIContent("Asset Load Mode", "Determines how Assets are loaded when accessed." +
            "\n- Requested Asset And Dependencies, will only load the requested Asset (Recommended)." +
            "\n- All Packed Assets And Dependencies, will load all Assets that are packed together. Best used when loading all Assets into memory is required.");
        GUIContent m_AssetProviderContent = new GUIContent("Asset Provider", "The provider to use for loading assets out of AssetBundles");
        GUIContent m_BundleProviderContent = new GUIContent("Asset Bundle Provider", "The provider to use for loading AssetBundles (not the assets within bundles)");


        void ShowAdvancedProperties(SerializedObject so)
        {
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_Compression)), m_CompressionContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_IncludeInBuild)), m_IncludeInBuildContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_ForceUniqueProvider)), m_ForceUniqueProviderContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_UseAssetBundleCache)), m_UseAssetBundleCacheContent, true);
            CRCPropertyPopupField(so);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_UseUWRForLocalBundles)), m_UseUWRForLocalBundlesContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_Timeout)), m_TimeoutContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_ChunkedTransfer)), m_ChunkedTransferContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_RedirectLimit)), m_RedirectLimitContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_RetryCount)), m_RetryCountContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_IncludeAddressInCatalog)), m_IncludeAddressInCatalogContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_IncludeGUIDInCatalog)), m_IncludeGUIDInCatalogContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_IncludeLabelsInCatalog)), m_IncludeLabelsInCatalogContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_InternalIdNamingMode)), m_InternalIdNamingModeContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_InternalBundleIdMode)), m_InternalBundleIdModeContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_CacheClearBehavior)), m_CacheClearBehaviorContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_BundleMode)), m_BundleModeContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_BundleNaming)), m_BundleNamingContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_AssetLoadMode)), m_AssetLoadModeContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_BundledAssetProviderType)), m_AssetProviderContent, true);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(m_AssetBundleProviderType)), m_BundleProviderContent, true);
        }

        void CRCPropertyPopupField(SerializedObject so)
        {
            int enumIndex = 0;
            if (m_UseAssetBundleCrc)
                enumIndex = m_UseAssetBundleCrcForCachedBundles ? 1 : 2;

            int newEnumIndex = EditorGUILayout.Popup(m_AssetBundleCrcContent, enumIndex, m_CrcPopupContent);
            if (enumIndex != newEnumIndex)
            {
                if (newEnumIndex != 0)
                {
                    if (!m_UseAssetBundleCrc)
                        so.FindProperty("m_UseAssetBundleCrc").boolValue = true;
                    if (newEnumIndex == 1 && !m_UseAssetBundleCrcForCachedBundles)
                        so.FindProperty("m_UseAssetBundleCrcForCachedBundles").boolValue = true;
                    else if (newEnumIndex == 2 && m_UseAssetBundleCrcForCachedBundles)
                        so.FindProperty("m_UseAssetBundleCrcForCachedBundles").boolValue = false;
                }
                else
                    so.FindProperty("m_UseAssetBundleCrc").boolValue = false;
            }
        }

        void ShowAdvancedPropertiesMulti(SerializedObject so, List<AddressableAssetGroupSchema> otherBundledSchemas, ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges)
        {
            ShowSelectedPropertyMulti(so, nameof(m_Compression), m_CompressionContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.Compression = src.Compression, ref m_Compression);
            ShowSelectedPropertyMulti(so, nameof(m_IncludeInBuild), m_IncludeInBuildContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.IncludeInBuild = src.IncludeInBuild, ref m_IncludeInBuild);
            ShowSelectedPropertyMulti(so, nameof(m_ForceUniqueProvider), m_ForceUniqueProviderContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.ForceUniqueProvider = src.ForceUniqueProvider, ref m_ForceUniqueProvider);
            ShowSelectedPropertyMulti(so, nameof(m_UseAssetBundleCache), m_UseAssetBundleCacheContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.UseAssetBundleCache = src.UseAssetBundleCache, ref m_UseAssetBundleCache);
            ShowCustomGuiSelectedPropertyMulti(so, new string[] {nameof(m_UseAssetBundleCrc), nameof(m_UseAssetBundleCrcForCachedBundles)}, m_AssetBundleCrcContent, otherBundledSchemas, ref queuedChanges,
                schema => CRCPropertyPopupField(so), (src, dst) => { dst.UseAssetBundleCrc = src.UseAssetBundleCrc; dst.UseAssetBundleCrcForCachedBundles = src.UseAssetBundleCrcForCachedBundles; });
            ShowSelectedPropertyMulti(so, nameof(m_UseUWRForLocalBundles), m_UseUWRForLocalBundlesContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.UseUnityWebRequestForLocalBundles = src.UseUnityWebRequestForLocalBundles, ref m_UseUWRForLocalBundles);
            ShowSelectedPropertyMulti(so, nameof(m_Timeout), m_TimeoutContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.Timeout = src.Timeout, ref m_Timeout);
            ShowSelectedPropertyMulti(so, nameof(m_ChunkedTransfer), m_ChunkedTransferContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.ChunkedTransfer = src.ChunkedTransfer, ref m_ChunkedTransfer);
            ShowSelectedPropertyMulti(so, nameof(m_RedirectLimit), m_RedirectLimitContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.RedirectLimit = src.RedirectLimit, ref m_RedirectLimit);
            ShowSelectedPropertyMulti(so, nameof(m_RetryCount), m_RetryCountContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.RetryCount = src.RetryCount, ref m_RetryCount);
            ShowSelectedPropertyMulti(so, nameof(m_IncludeAddressInCatalog), m_IncludeAddressInCatalogContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.IncludeAddressInCatalog = src.IncludeAddressInCatalog, ref m_IncludeAddressInCatalog);
            ShowSelectedPropertyMulti(so, nameof(m_IncludeGUIDInCatalog), m_IncludeGUIDInCatalogContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.IncludeGUIDInCatalog = src.IncludeGUIDInCatalog, ref m_IncludeGUIDInCatalog);
            ShowSelectedPropertyMulti(so, nameof(m_IncludeLabelsInCatalog), m_IncludeLabelsInCatalogContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.IncludeLabelsInCatalog = src.IncludeLabelsInCatalog, ref m_IncludeLabelsInCatalog);
            ShowSelectedPropertyMulti(so, nameof(m_InternalIdNamingMode), m_InternalIdNamingModeContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.InternalIdNamingMode = src.InternalIdNamingMode, ref m_InternalIdNamingMode);
            ShowSelectedPropertyMulti(so, nameof(m_InternalBundleIdMode), m_InternalBundleIdModeContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.InternalBundleIdMode = src.InternalBundleIdMode, ref m_InternalBundleIdMode);
            ShowSelectedPropertyMulti(so, nameof(m_CacheClearBehavior), m_CacheClearBehaviorContent, otherBundledSchemas, ref queuedChanges, (src, dst) => { dst.AssetBundledCacheClearBehavior = src.AssetBundledCacheClearBehavior; dst.SetDirty(true); }, ref m_CacheClearBehavior);
            ShowSelectedPropertyMulti(so, nameof(m_BundleMode), m_BundleModeContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.BundleMode = src.BundleMode, ref m_BundleMode);
            ShowSelectedPropertyMulti(so, nameof(m_BundleNaming), m_BundleNamingContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.BundleNaming = src.BundleNaming, ref m_BundleNaming);
            ShowSelectedPropertyMulti(so, nameof(m_AssetLoadMode), m_AssetLoadModeContent, otherBundledSchemas, ref queuedChanges, (src, dst) => dst.AssetLoadMode = src.AssetLoadMode, ref m_AssetLoadMode);
            ShowSelectedPropertyMulti(so, nameof(m_BundledAssetProviderType), m_AssetProviderContent, otherBundledSchemas, ref queuedChanges, (src, dst) => { dst.m_BundledAssetProviderType = src.BundledAssetProviderType; dst.SetDirty(true); }, ref m_BundledAssetProviderType);
            ShowSelectedPropertyMulti(so, nameof(m_AssetBundleProviderType), m_BundleProviderContent, otherBundledSchemas, ref queuedChanges, (src, dst) => { dst.m_AssetBundleProviderType = src.AssetBundleProviderType; dst.SetDirty(true); }, ref m_AssetBundleProviderType);
        }

        void ShowSelectedPropertyMulti<T>(SerializedObject so, string propertyName, GUIContent label, List<AddressableAssetGroupSchema> otherSchemas, ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges, Action<BundledAssetGroupSchema, BundledAssetGroupSchema> a, ref T propertyValue)
        {
            var prop = so.FindProperty(propertyName);
            if (label == null)
                label = new GUIContent(prop.displayName);
            ShowMixedValue(prop, otherSchemas, typeof(T), propertyName);

            T newValue = default(T);

            EditorGUI.BeginChangeCheck();
            if (typeof(T) == typeof(bool))
            {
                newValue = (T)(object)EditorGUILayout.Toggle(label, (bool)(object)propertyValue);
            }
            else if (typeof(T).IsEnum)
            {
                newValue = (T)(object)(AssetNamingMode)EditorGUILayout.EnumPopup(label, (Enum)(object)propertyValue);
            }
            else if (typeof(T) == typeof(int))
            {
                newValue = (T)(object)EditorGUILayout.IntField(label, (int)(object)propertyValue);
            }
            else
            {
                EditorGUILayout.PropertyField(prop, label, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(so.targetObject, so.targetObject.name + propertyName);
                if (typeof(T) == typeof(bool) || typeof(T).IsEnum || typeof(T) == typeof(int))
                    propertyValue = newValue;
                if (queuedChanges == null)
                    queuedChanges = new List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>>();
                queuedChanges.Add(a);
                EditorUtility.SetDirty(this);
            }
            EditorGUI.showMixedValue = false;
        }

        void ShowCustomGuiSelectedPropertyMulti(SerializedObject so, string[] propertyNames, GUIContent label,
            List<AddressableAssetGroupSchema> otherSchemas,
            ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges,
            Action<BundledAssetGroupSchema> guiAction,
            Action<BundledAssetGroupSchema, BundledAssetGroupSchema> a)
        {
            if (label == null)
                return;

            SerializedProperty[] props = new SerializedProperty[propertyNames.Length];
            for (int i = 0; i < propertyNames.Length; ++i)
                props[i] = so.FindProperty(propertyNames[i]);

            for (int i = 0; i < propertyNames.Length; ++i)
            {
                if (EditorGUI.showMixedValue)
                    break;
                ShowMixedValue(props[i], otherSchemas, null, propertyNames[i]);
            }

            EditorGUI.BeginChangeCheck();
            guiAction.Invoke(this);
            if (EditorGUI.EndChangeCheck())
            {
                if (queuedChanges == null)
                    queuedChanges = new List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>>();
                queuedChanges.Add(a);
                EditorUtility.SetDirty(this);
            }
            EditorGUI.showMixedValue = false;
        }

        void ShowSelectedPropertyMulti(SerializedObject so, string propertyName, GUIContent label,
            List<AddressableAssetGroupSchema> otherSchemas,
            ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges,
            Action<BundledAssetGroupSchema, BundledAssetGroupSchema> a, string previousValue, ref ProfileValueReference currentValue)
        {
            var prop = so.FindProperty(propertyName);
            ShowMixedValue(prop, otherSchemas, typeof(ProfileValueReference), propertyName);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                var newValue = currentValue.Id;
                currentValue.Id = previousValue;
                Undo.RecordObject(so.targetObject, so.targetObject.name + propertyName);
                currentValue.Id = newValue;
                if (queuedChanges == null)
                    queuedChanges = new List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>>();
                queuedChanges.Add(a);
            }
            EditorGUI.showMixedValue = false;
        }

        void ShowSelectedPropertyPath(SerializedObject so, string propertyName, GUIContent label, ref ProfileValueReference currentValue)
        {
            var prop = so.FindProperty(propertyName);
            string previousValue = currentValue.Id;
            EditorGUI.BeginChangeCheck();
            //Current implementation using ProfileValueReferenceDrawer
            EditorGUILayout.PropertyField(prop, label, true);
            if (EditorGUI.EndChangeCheck())
            {
                var newValue = currentValue.Id;
                currentValue.Id = previousValue;
                Undo.RecordObject(so.targetObject, so.targetObject.name + propertyName);
                currentValue.Id = newValue;
                EditorUtility.SetDirty(this);
            }
            EditorGUI.showMixedValue = false;
        }

        void ShowSelectedPropertyPathPairMulti(SerializedObject so, List<AddressableAssetGroupSchema> otherSchemas, ref List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>> queuedChanges,
            Action<BundledAssetGroupSchema, BundledAssetGroupSchema> a)
        {
            var buildPathProperty = so.FindProperty(nameof(m_BuildPath));
            var loadPathProperty = so.FindProperty(nameof(m_LoadPath));
            ShowMixedValue(buildPathProperty, otherSchemas, typeof(ProfileValueReference), nameof(m_BuildPath));
            ShowMixedValue(loadPathProperty, otherSchemas, typeof(ProfileValueReference), nameof(m_LoadPath));

            List<ProfileGroupType> groupTypes = ProfileGroupType.CreateGroupTypes(settings.profileSettings.GetProfile(settings.activeProfileId));
            List<string> options = groupTypes.Select(group => group.GroupTypePrefix).ToList();
            //set selected to custom
            options.Add(AddressableAssetProfileSettings.customEntryString);
            int? selected = null;

            //Determine selection and whether to show custom
            if (!EditorGUI.showMixedValue)
            {
                //disregard custom value, want to check if valid pair
                selected = DetermineSelectedIndex(groupTypes, options.Count - 1);
                if (selected.HasValue && selected != options.Count - 1)
                {
                    m_UseCustomPaths = false;
                }
                else
                {
                    m_UseCustomPaths = true;
                }
            }

            //Dropdown selector
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup("Build & Load Paths", selected.HasValue ? selected.Value : -1, options.ToArray());
            if (EditorGUI.EndChangeCheck() && newIndex != selected)
            {
                selected = newIndex;
                SetPathPairOption(so, options, groupTypes, newIndex);

                if (queuedChanges == null)
                    queuedChanges = new List<Action<BundledAssetGroupSchema, BundledAssetGroupSchema>>();
                queuedChanges.Add(a);
                EditorGUI.showMixedValue = false;
            }

            if (m_UseCustomPaths && selected.HasValue)
            {
                ShowPathsMulti(so, otherSchemas, ref queuedChanges);
            }

            ShowPathsPreview(!selected.HasValue);
            EditorGUI.showMixedValue = false;
        }

        void ShowSelectedPropertyPathPair(SerializedObject so)
        {
            List<ProfileGroupType> groupTypes = ProfileGroupType.CreateGroupTypes(settings.profileSettings.GetProfile(settings.activeProfileId));
            List<string> options = groupTypes.Select(group => group.GroupTypePrefix).ToList();
            //Set selected to custom
            options.Add(AddressableAssetProfileSettings.customEntryString);
            int? selected = options.Count - 1;

            //Determine selection and whether to show custom
            selected = DetermineSelectedIndex(groupTypes, options.Count - 1);
            if (selected.HasValue && selected != options.Count - 1)
            {
                m_UseCustomPaths = false;
            }
            else
            {
                m_UseCustomPaths = true;
            }

            //Dropdown selector
            EditorGUI.BeginChangeCheck();
            var newIndex = EditorGUILayout.Popup("Build & Load Paths", selected.HasValue ? selected.Value : options.Count - 1, options.ToArray());
            if (EditorGUI.EndChangeCheck() && newIndex != selected)
            {
                SetPathPairOption(so, options, groupTypes, newIndex);
                EditorUtility.SetDirty(this);
            }

            if (m_UseCustomPaths)
            {
                ShowPaths(so);
            }

            ShowPathsPreview(false);
            EditorGUI.showMixedValue = false;
        }

        int? DetermineSelectedIndex(List<ProfileGroupType> groupTypes, int? defaultValue)
        {
            int? selected = defaultValue;

            HashSet<string> vars = settings.profileSettings.GetAllVariableIds();
            if (vars.Contains(m_BuildPath.Id) && vars.Contains(m_LoadPath.Id) && !m_UseCustomPaths)
            {
                for (int i = 0; i < groupTypes.Count; i++)
                {
                    ProfileGroupType.GroupTypeVariable buildPathVar = groupTypes[i].GetVariableBySuffix("BuildPath");
                    ProfileGroupType.GroupTypeVariable loadPathVar = groupTypes[i].GetVariableBySuffix("LoadPath");
                    if (m_BuildPath.GetName(settings) == groupTypes[i].GetName(buildPathVar) && m_LoadPath.GetName(settings) == groupTypes[i].GetName(loadPathVar))
                    {
                        selected = i;
                        break;
                    }
                }
            }
            return selected;
        }

        void SetPathPairOption(SerializedObject so, List<string> options, List<ProfileGroupType> groupTypes, int newIndex)
        {
            if (options[newIndex] != AddressableAssetProfileSettings.customEntryString)
            {
                Undo.RecordObject(so.targetObject, so.targetObject.name + "Path Pair");
                m_BuildPath.SetVariableByName(settings, groupTypes[newIndex].GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + "BuildPath");
                m_LoadPath.SetVariableByName(settings, groupTypes[newIndex].GroupTypePrefix + ProfileGroupType.k_PrefixSeparator + "LoadPath");
                m_UseCustomPaths = false;
            }
            else
            {
                Undo.RecordObject(so.targetObject, so.targetObject.name + "Path Pair");
                m_UseCustomPaths = true;
            }
        }

        void ShowPathsPreview(bool showMixedValue)
        {
            EditorGUI.indentLevel++;
            m_ShowPaths = EditorGUILayout.Foldout(m_ShowPaths, "Path Preview", true);
            if (m_ShowPaths)
            {
                EditorStyles.helpBox.fontSize = 12;
                var baseBuildPathValue = settings.profileSettings.GetValueById(settings.activeProfileId, m_BuildPath.Id);
                var baseLoadPathValue = settings.profileSettings.GetValueById(settings.activeProfileId, m_LoadPath.Id);
                EditorGUILayout.HelpBox(String.Format("Build Path: {0}", showMixedValue ? "-" : settings.profileSettings.EvaluateString(settings.activeProfileId, baseBuildPathValue)), MessageType.None);
                EditorGUILayout.HelpBox(String.Format("Load Path: {0}", showMixedValue ? "-" : settings.profileSettings.EvaluateString(settings.activeProfileId, baseLoadPathValue)), MessageType.None);
            }
            EditorGUI.indentLevel--;
        }
    }
}
