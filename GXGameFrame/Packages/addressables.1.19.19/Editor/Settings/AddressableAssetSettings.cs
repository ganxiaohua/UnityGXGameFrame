using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.HostingServices;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;
using static UnityEditor.AddressableAssets.Settings.AddressablesFileEnumeration;
using System.Threading.Tasks;

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
using Unity.Services.CCD.Management;
using Unity.Services.CCD.Management.Apis.Entries;
using Unity.Services.CCD.Management.Http;
using Unity.Services.CCD.Management.Apis.Content;
using Unity.Services.CCD.Management.Apis.Releases;
using Unity.Services.CCD.Management.Entries;
using Unity.Services.CCD.Management.Content;
using System.Net;
using Unity.Services.CCD.Management.Apis.Badges;
using Unity.Services.CCD.Management.Releases;
using Unity.Services.CCD.Management.Badges;
using Unity.Services.CCD.Management.Models;
#endif

namespace UnityEditor.AddressableAssets.Settings
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Contains editor data for the addressables system.
    /// </summary>
    public class AddressableAssetSettings : ScriptableObject
    {
        [InitializeOnLoadMethod]
        static void RegisterWithAssetPostProcessor()
        {
            //if the Library folder has been deleted, this will be null and it will have to be set on the first access of the settings object
            if (AddressableAssetSettingsDefaultObject.Settings != null)
                AddressablesAssetPostProcessor.OnPostProcess.Register(AddressableAssetSettingsDefaultObject.Settings.OnPostprocessAllAssets, 0);
            else
                EditorApplication.update += TryAddAssetPostprocessorOnNextUpdate;
        }

        private static void TryAddAssetPostprocessorOnNextUpdate()
        {
            if (AddressableAssetSettingsDefaultObject.Settings != null)
                AddressablesAssetPostProcessor.OnPostProcess.Register(AddressableAssetSettingsDefaultObject.Settings.OnPostprocessAllAssets, 0);
            EditorApplication.update -= TryAddAssetPostprocessorOnNextUpdate;
        }

        /// <summary>
        /// Build Path Name
        /// </summary>
        public const string kBuildPath = "BuildPath";
        /// <summary>
        /// Load Path Name
        /// </summary>
        public const string kLoadPath = "LoadPath";
        /// <summary>
        /// Default name of a newly created group.
        /// </summary>
        public const string kNewGroupName = "New Group";
        /// <summary>
        /// Default name of local build path.
        /// </summary>
        public const string kLocalBuildPath = "Local.BuildPath";
        /// <summary>
        /// Default name of local load path.
        /// </summary>
        public const string kLocalLoadPath = "Local.LoadPath";
        /// <summary>
        /// Default name of remote build path.
        /// </summary>
        public const string kRemoteBuildPath = "Remote.BuildPath";
        /// <summary>
        /// Default name of remote load path.
        /// </summary>
        public const string kRemoteLoadPath = "Remote.LoadPath";

        private const string kLocalGroupTypePrefix = "Built-In";
        internal static string LocalGroupTypePrefix => kLocalGroupTypePrefix; 
        /// <summary>
        /// Default value of local build path.
        /// </summary>
        public const string kLocalBuildPathValue = "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]";
        /// <summary>
        /// Default value of local load path.
        /// </summary>
        public const string kLocalLoadPathValue = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]";

        private const string kEditorHostedGroupTypePrefix = "Editor Hosted";
        internal static string EditorHostedGroupTypePrefix => kEditorHostedGroupTypePrefix; 
        /// <summary>
        /// Default value of remote build path.
        /// </summary>
        public const string kRemoteBuildPathValue = "ServerData/[BuildTarget]";
        /// <summary>
        /// Default value of remote load path.
        /// </summary>
        public const string kRemoteLoadPathValue = "http://localhost/[BuildTarget]";
        internal static string RemoteLoadPathValue
        {
            get
            {
                // Fix for case ADDR-2314. kRemoteLoadPathValue is incorrect, "http://localhost/[BuildTarget]" does not work with local hosting service
                return "http://[PrivateIpAddress]:[HostingServicePort]";
                // kRemoteLoadPathValue will be fixed to the correct path in Addressables 1.20.0
            }
        }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
        /// <summary>
        /// Default path of build assets that are uploaded to CCD.
        /// </summary>
        public const string kCCDBuildDataPath = "CCDBuildData";
        /// <summary>
        /// CCD Package Name
        /// </summary>
        public const string kCCDPackageName = "com.unity.services.ccd.management";
#endif


        private const string kImportAssetEntryCollectionOptOutKey = "com.unity.addressables.importAssetEntryCollections.optOut";
        internal bool DenyEntryCollectionPermission { get; set; }

        /// <summary>
        /// Options for building Addressables when building a player.
        /// </summary>
        public enum PlayerBuildOption
        {
            /// <summary>
            /// Use to indicate that the global settings (stored in preferences) will determine if building a player will also build Addressables.
            /// </summary>
            PreferencesValue,
            /// <summary>
            /// Use to indicate that building a player will also build Addressables.
            /// </summary>
            BuildWithPlayer,
            /// <summary>
            /// Use to indicate that building a player won't build Addressables.
            /// </summary>
            DoNotBuildWithPlayer
        }

        /// <summary>
        /// Options for labeling all the different generated events.
        /// </summary>
        public enum ModificationEvent
        {
            /// <summary>
            /// Use to indicate that a group was added to the settings object.
            /// </summary>
            GroupAdded,
            /// <summary>
            /// Use to indicate that a group was removed from the the settings object.
            /// </summary>
            GroupRemoved,
            /// <summary>
            /// Use to indicate that a group in the settings object was renamed.
            /// </summary>
            GroupRenamed,
            /// <summary>
            /// Use to indicate that a schema was added to a group.
            /// </summary>
            GroupSchemaAdded,
            /// <summary>
            /// Use to indicate that a schema was removed from a group.
            /// </summary>
            GroupSchemaRemoved,
            /// <summary>
            /// Use to indicate that a schema was modified.
            /// </summary>
            GroupSchemaModified,
            /// <summary>
            /// Use to indicate that a group template was added to the settings object.
            /// </summary>
            GroupTemplateAdded,
            /// <summary>
            /// Use to indicate that a group template was removed from the settings object.
            /// </summary>
            GroupTemplateRemoved,
            /// <summary>
            /// Use to indicate that a schema was added to a group template.
            /// </summary>
            GroupTemplateSchemaAdded,
            /// <summary>
            /// Use to indicate that a schema was removed from a group template.
            /// </summary>
            GroupTemplateSchemaRemoved,
            /// <summary>
            /// Use to indicate that an asset entry was created.
            /// </summary>
            EntryCreated,
            /// <summary>
            /// Use to indicate that an asset entry was added to a group.
            /// </summary>
            EntryAdded,
            /// <summary>
            /// Use to indicate that an asset entry moved from one group to another.
            /// </summary>
            EntryMoved,
            /// <summary>
            /// Use to indicate that an asset entry was removed from a group.
            /// </summary>
            EntryRemoved,
            /// <summary>
            /// Use to indicate that an asset label was added to the settings object.
            /// </summary>
            LabelAdded,
            /// <summary>
            /// Use to indicate that an asset label was removed from the settings object.
            /// </summary>
            LabelRemoved,
            /// <summary>
            /// Use to indicate that a profile was added to the settings object.
            /// </summary>
            ProfileAdded,
            /// <summary>
            /// Use to indicate that a profile was removed from the settings object.
            /// </summary>
            ProfileRemoved,
            /// <summary>
            /// Use to indicate that a profile was modified.
            /// </summary>
            ProfileModified,
            /// <summary>
            /// Use to indicate that a profile has been set as the active profile.
            /// </summary>
            ActiveProfileSet,
            /// <summary>
            /// Use to indicate that an asset entry was modified.
            /// </summary>
            EntryModified,
            /// <summary>
            /// Use to indicate that the build settings object was modified.
            /// </summary>
            BuildSettingsChanged,
            /// <summary>
            /// Use to indicate that a new build script is being used as the active build script.
            /// </summary>
            ActiveBuildScriptChanged,
            /// <summary>
            /// Use to indicate that a new data builder script was added to the settings object.
            /// </summary>
            DataBuilderAdded,
            /// <summary>
            /// Use to indicate that a data builder script was removed from the settings object.
            /// </summary>
            DataBuilderRemoved,
            /// <summary>
            /// Use to indicate a new initialization object was added to the settings object.
            /// </summary>
            InitializationObjectAdded,
            /// <summary>
            /// Use to indicate a initialization object was removed from the settings object.
            /// </summary>
            InitializationObjectRemoved,
            /// <summary>
            /// Use to indicate that a new script is being used as the active playmode data builder.
            /// </summary>
            ActivePlayModeScriptChanged,
            /// <summary>
            /// Use to indicate that a batch of asset entries was modified. Note that the posted object will be null.
            /// </summary>
            BatchModification,
            /// <summary>
            /// Use to indicate that the hosting services manager was modified.
            /// </summary>
            HostingServicesManagerModified,
            /// <summary>
            /// Use to indicate that a group changed its order placement within the list of groups in the settings object.
            /// </summary>
            GroupMoved,
            /// <summary>
            /// Use to indicate that a new certificate handler is being used for the initialization object provider.
            /// </summary>
            CertificateHandlerChanged
        }

        /// <summary>
        /// The path of the settings asset.
        /// </summary>
        public string AssetPath
        {
            get
            {
                string guid;
                long localId;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out guid, out localId))
                    throw new Exception($"{nameof(AddressableAssetSettings)} is not persisted.  Unable to determine AssetPath.");
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                    throw new Exception($"{nameof(AddressableAssetSettings)} - Unable to determine AssetPath from guid {guid}.");
                return assetPath;
            }
        }

        /// <summary>
        /// The folder of the settings asset.
        /// </summary>
        public string ConfigFolder
        {
            get
            {
                return Path.GetDirectoryName(AssetPath);
            }
        }

        /// <summary>
        /// The folder for the group assets.
        /// </summary>
        public string GroupFolder
        {
            get
            {
                return ConfigFolder + "/AssetGroups";
            }
        }
        /// <summary>
        /// The folder for the script assets.
        /// </summary>
        public string DataBuilderFolder
        {
            get
            {
                return ConfigFolder + "/DataBuilders";
            }
        }
        /// <summary>
        /// The folder for the asset group schema assets.
        /// </summary>
        public string GroupSchemaFolder
        {
            get
            {
                return GroupFolder + "/Schemas";
            }
        }

        /// <summary>
        /// The default folder for the group template assets.
        /// </summary>
        public string GroupTemplateFolder
        {
            get
            {
                return ConfigFolder + "/AssetGroupTemplates";
            }
        }

        /// <summary>
        /// Event for handling settings changes.  The object passed depends on the event type.
        /// </summary>
        public Action<AddressableAssetSettings, ModificationEvent, object> OnModification { get; set; }

        /// <summary>
        /// Event for handling settings changes on all instances of AddressableAssetSettings.  The object passed depends on the event type.
        /// </summary>
        public static event Action<AddressableAssetSettings, ModificationEvent, object> OnModificationGlobal;

        /// <summary>
        /// Event for handling the result of a DataBuilder.Build call.
        /// </summary>
        public Action<AddressableAssetSettings, IDataBuilder, IDataBuilderResult> OnDataBuilderComplete { get; set; }

        [FormerlySerializedAs("m_defaultGroup")]
        [SerializeField]
        string m_DefaultGroup;
        [FormerlySerializedAs("m_cachedHash")]
        [SerializeField]
        Hash128 m_CachedHash;

        bool m_IsTemporary;
        /// <summary>
        /// Returns whether this settings object is persisted to an asset.
        /// </summary>
        public bool IsPersisted { get { return !m_IsTemporary; } }

        [SerializeField]
        bool m_OptimizeCatalogSize = false;

        [SerializeField]
        bool m_BuildRemoteCatalog = false;

        [SerializeField]
        bool m_BundleLocalCatalog = false;

        [SerializeField]
        int m_CatalogRequestsTimeout = 0;

        [SerializeField]
        bool m_DisableCatalogUpdateOnStart = false;

        [SerializeField]
        bool m_IgnoreUnsupportedFilesInBuild = false;

        [SerializeField]
        bool m_UniqueBundleIds = false;

        [SerializeField]
#if UNITY_2021_1_OR_NEWER
        bool m_NonRecursiveBuilding = true;
#else
        bool m_NonRecursiveBuilding = false;
#endif

#if UNITY_2019_4_OR_NEWER
        [SerializeField]
#if !ENABLE_CCD
        bool m_CCDEnabled = false;
#else
        bool m_CCDEnabled = true;
#endif

        public bool CCDEnabled
        {
            get { return m_CCDEnabled; }
            set { m_CCDEnabled = value; }
        }
#endif

        [SerializeField]
        int m_maxConcurrentWebRequests = 500;

        /// <summary>
        /// The maximum time to download hash and json catalog files before a timeout error.
        /// </summary>
        public int CatalogRequestsTimeout
        {
            get { return m_CatalogRequestsTimeout; }
            set { m_CatalogRequestsTimeout = value < 0 ? 0 : value; }
        }

        /// <summary>
        /// The maximum number of concurrent web requests.  This value will be clamped from 1 to 1024.
        /// </summary>
        public int MaxConcurrentWebRequests
        {
            get { return m_maxConcurrentWebRequests; }
            set { m_maxConcurrentWebRequests = Mathf.Clamp(value, 1, 1024); }
        }

        /// <summary>
        /// Set this to true to ensure unique bundle ids. Set to false to allow duplicate bundle ids.
        /// </summary>
        public bool UniqueBundleIds
        {
            get { return m_UniqueBundleIds; }
            set { m_UniqueBundleIds = value; }
        }

        [SerializeField]
#if UNITY_2021_1_OR_NEWER
        bool m_ContiguousBundles = true;
#else
        bool m_ContiguousBundles = false;
#endif

        /// <summary>
        /// If set, packs assets in bundles contiguously based on the ordering of the source asset which results in improved asset loading times. Disable this if you've built bundles with a version of Addressables older than 1.12.1 and you want to minimize bundle changes.
        /// </summary>
        public bool ContiguousBundles
        {
            get { return m_ContiguousBundles; }
            set { m_ContiguousBundles = value; }
        }

        /// <summary>
        /// If set, Calculates and build asset bundles using Non-Recursive Dependency calculation methods. This approach helps reduce asset bundle rebuilds and runtime memory consumption.
        /// </summary>
        public bool NonRecursiveBuilding
        {
            get { return m_NonRecursiveBuilding; }
            set { m_NonRecursiveBuilding = value; }
        }

        /// <summary>
        /// Enables size optimization of content catalogs.  This may increase the cpu usage of loading the catalog.
        /// </summary>
        public bool OptimizeCatalogSize
        {
            get { return m_OptimizeCatalogSize; }
            set { m_OptimizeCatalogSize = value; }
        }

        /// <summary>
        /// Determine if a remote catalog should be built-for and loaded-by the app.
        /// </summary>
        public bool BuildRemoteCatalog
        {
            get { return m_BuildRemoteCatalog; }
            set { m_BuildRemoteCatalog = value; }
        }

        /// <summary>
        /// Whether the local catalog should be serialized in an asset bundle or as json.
        /// </summary>
        public bool BundleLocalCatalog
        {
            get { return m_BundleLocalCatalog; }
            set { m_BundleLocalCatalog = value; }
        }

        /// <summary>
        /// Tells Addressables if it should check for a Content Catalog Update during the initialization step.
        /// </summary>
        public bool DisableCatalogUpdateOnStartup
        {
            get { return m_DisableCatalogUpdateOnStart; }
            set { m_DisableCatalogUpdateOnStart = value; }
        }

        [SerializeField]
        bool m_StripUnityVersionFromBundleBuild = false;
        /// <summary>
        /// If true, this option will strip the Unity Editor Version from the header of the AssetBundle during a build.
        /// </summary>
        internal bool StripUnityVersionFromBundleBuild
        {
            get { return m_StripUnityVersionFromBundleBuild; }
            set { m_StripUnityVersionFromBundleBuild = value; }
        }

        [SerializeField]
        bool m_DisableVisibleSubAssetRepresentations = false;
        /// <summary>
        /// If true, the build will assume that sub Assets have no visible asset representations (are not visible in the Project view) which results in improved build times.
        /// However sub assets in the built bundles cannot be accessed by AssetBundle.LoadAsset&lt;T&gt; or AssetBundle.LoadAllAssets&lt;T&gt;.
        /// </summary>
        public bool DisableVisibleSubAssetRepresentations
        {
            get { return m_DisableVisibleSubAssetRepresentations; }
            set { m_DisableVisibleSubAssetRepresentations = value; }
        }

        /// <summary>
        /// Whether unsupported files during build should be ignored or treated as an error.
        /// </summary>
        public bool IgnoreUnsupportedFilesInBuild
        {
            get { return m_IgnoreUnsupportedFilesInBuild; }
            set { m_IgnoreUnsupportedFilesInBuild = value; }
        }

        [SerializeField]
        ShaderBundleNaming m_ShaderBundleNaming = ShaderBundleNaming.ProjectName;
        /// <summary>
        /// Sets the naming convention used for the Unity built in shader bundle at build time.
        /// The recommended setting is Project Name.
        /// </summary>
        public ShaderBundleNaming ShaderBundleNaming
        {
            get { return m_ShaderBundleNaming; }
            set { m_ShaderBundleNaming = value; }
        }

        [SerializeField]
        string m_ShaderBundleCustomNaming = "";
        /// <summary>
        /// Custom Unity built in shader bundle prefix that is used if AddressableAssetSettings.ShaderBundleNaming is set to ShaderBundleNaming.Custom.
        /// </summary>
        public string ShaderBundleCustomNaming
        {
            get { return m_ShaderBundleCustomNaming; }
            set { m_ShaderBundleCustomNaming = value; }
        }

        [SerializeField]
        MonoScriptBundleNaming m_MonoScriptBundleNaming = MonoScriptBundleNaming.Disabled;
        /// <summary>
        /// Sets the naming convention used for the MonoScript bundle at build time. Or disabled MonoScript bundle generation.
        /// The recommended setting is Project Name.
        /// </summary>
        public MonoScriptBundleNaming MonoScriptBundleNaming
        {
            get { return m_MonoScriptBundleNaming; }
            set { m_MonoScriptBundleNaming = value; }
        }

        [SerializeField]
        string m_MonoScriptBundleCustomNaming = "";
        /// <summary>
        /// Custom MonoScript bundle prefix that is used if AddressableAssetSettings.MonoScriptBundleNaming is set to MonoScriptBundleNaming.Custom.
        /// </summary>
        public string MonoScriptBundleCustomNaming
        {
            get { return m_MonoScriptBundleCustomNaming; }
            set { m_MonoScriptBundleCustomNaming = value; }
        }

        [SerializeField]
        ProfileValueReference m_RemoteCatalogBuildPath;
        /// <summary>
        /// The path to place a copy of the content catalog for online retrieval.  To do any content updates
        /// to an existing built app, there must be a remote catalog. Overwriting the catalog is how the app
        /// gets informed of the updated content.
        /// </summary>
        public ProfileValueReference RemoteCatalogBuildPath
        {
            get
            {
                if (m_RemoteCatalogBuildPath.Id == null)
                {
                    m_RemoteCatalogBuildPath = new ProfileValueReference();
                    m_RemoteCatalogBuildPath.SetVariableByName(this, kRemoteBuildPath);
                }
                return m_RemoteCatalogBuildPath;
            }
            set { m_RemoteCatalogBuildPath = value; }
        }

        [SerializeField]
        ProfileValueReference m_RemoteCatalogLoadPath;
        /// <summary>
        /// The path to load the remote content catalog from.  This is the location the app will check to
        /// look for updated catalogs, which is the only indication the app has for updated content.
        /// </summary>
        public ProfileValueReference RemoteCatalogLoadPath
        {
            get
            {
                if (m_RemoteCatalogLoadPath.Id == null)
                {
                    m_RemoteCatalogLoadPath = new ProfileValueReference();
                    m_RemoteCatalogLoadPath.SetVariableByName(this, kRemoteLoadPath);
                }
                return m_RemoteCatalogLoadPath;
            }
            set { m_RemoteCatalogLoadPath = value; }
        }

        [SerializeField]
        private string m_ContentStateBuildPath = "";
        /// <summary>
        /// The path used for saving the addressables_content_state.bin file.  If empty, this will be the addressable settings config folder in your project.
        /// </summary>
        public string ContentStateBuildPath
        {
            get { return m_ContentStateBuildPath; }
            set { m_ContentStateBuildPath = value; }
        }
        
        [SerializeField]
        private PlayerBuildOption m_BuildAddressablesWithPlayerBuild = PlayerBuildOption.DoNotBuildWithPlayer;
        
        /// <summary>
        /// Defines if Addressables content will be built along with a Player build. (Requires 2021.2 or above)
        /// </summary>
        /// <remarks>
        /// Build with Player, will build Addressables with a Player build, this overrides preferences value.
        /// Do not Build with Player, will not build Addressables with a Player build, this overrides preferences value.
        /// Preferences value, will build with the Player dependant on is the user preferences value for "Build Addressables on Player build" is set.
        /// </remarks>
        public PlayerBuildOption BuildAddressablesWithPlayerBuild
        {
            get { return m_BuildAddressablesWithPlayerBuild; }
            set { m_BuildAddressablesWithPlayerBuild = value;  }
        }

        internal string GetContentStateBuildPath()
        {
            string p = ConfigFolder;
            if (!string.IsNullOrEmpty(m_ContentStateBuildPath))
                p = m_ContentStateBuildPath;
            p = Path.Combine(p, PlatformMappingService.GetPlatformPathSubFolder());
            return p;
        }

        /// <summary>
        /// Hash of the current settings.  This value is recomputed if anything changes.
        /// </summary>
        public Hash128 currentHash
        {
            get
            {
                if (m_CachedHash.isValid)
                    return m_CachedHash;
                var stream = new MemoryStream();
                var formatter = new BinaryFormatter();
                m_BuildSettings.SerializeForHash(formatter, stream);
                formatter.Serialize(stream, activeProfileId);
                formatter.Serialize(stream, m_LabelTable);
                formatter.Serialize(stream, m_ProfileSettings);
                formatter.Serialize(stream, m_GroupAssets.Count);
                foreach (var g in m_GroupAssets)
                    g.SerializeForHash(formatter, stream);
                return (m_CachedHash = HashingMethods.Calculate(stream).ToHash128());
            }
        }

        internal void DataBuilderCompleted(IDataBuilder builder, IDataBuilderResult result)
        {
            if (OnDataBuilderComplete != null)
                OnDataBuilderComplete(this, builder, result);
        }

        /// <summary>
        /// Create an AssetReference object.  If the asset is not already addressable, it will be added.
        /// </summary>
        /// <param name="guid">The guid of the asset reference.</param>
        /// <returns>Returns the newly created AssetReference.</returns>
        public AssetReference CreateAssetReference(string guid)
        {
            CreateOrMoveEntry(guid, DefaultGroup);
            return new AssetReference(guid);
        }

        [SerializeField]
        string m_overridePlayerVersion = "";
        /// <summary>
        /// Allows for overriding the player version used to generated catalog names.
        /// </summary>
        public string OverridePlayerVersion
        {
            get { return m_overridePlayerVersion; }
            set { m_overridePlayerVersion = value; }
        }
        /// <summary>
        /// The version of the player build.  This is implemented as a timestamp int UTC of the form  string.Format("{0:D4}.{1:D2}.{2:D2}.{3:D2}.{4:D2}.{5:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).
        /// </summary>
        public string PlayerBuildVersion
        {
            get
            {
                if (!string.IsNullOrEmpty(m_overridePlayerVersion))
                    return profileSettings.EvaluateString(activeProfileId, m_overridePlayerVersion);
                var now = DateTime.UtcNow;
                return string.Format("{0:D4}.{1:D2}.{2:D2}.{3:D2}.{4:D2}.{5:D2}", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            }
        }

        [FormerlySerializedAs("m_groupAssets")]
        [SerializeField]
        List<AddressableAssetGroup> m_GroupAssets = new List<AddressableAssetGroup>();
        /// <summary>
        /// List of asset groups.
        /// </summary>
        public List<AddressableAssetGroup> groups { get { return m_GroupAssets; } }

        [FormerlySerializedAs("m_buildSettings")]
        [SerializeField]
        AddressableAssetBuildSettings m_BuildSettings = new AddressableAssetBuildSettings();
        /// <summary>
        /// Build settings object.
        /// </summary>
        public AddressableAssetBuildSettings buildSettings { get { return m_BuildSettings; } }

        [FormerlySerializedAs("m_profileSettings")]
        [SerializeField]
        AddressableAssetProfileSettings m_ProfileSettings = new AddressableAssetProfileSettings();
        /// <summary>
        /// Profile settings object.
        /// </summary>
        public AddressableAssetProfileSettings profileSettings { get { return m_ProfileSettings; } }

        [FormerlySerializedAs("m_labelTable")]
        [SerializeField]
        LabelTable m_LabelTable = new LabelTable();
        /// <summary>
        /// LabelTable object.
        /// </summary>
        internal LabelTable labelTable { get { return m_LabelTable; } }
        [FormerlySerializedAs("m_schemaTemplates")]
        [SerializeField]
        List<AddressableAssetGroupSchemaTemplate> m_SchemaTemplates = new List<AddressableAssetGroupSchemaTemplate>();

        /// <summary>
        /// Remove  the schema at the specified index.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the schema was removed.</returns>
        [Obsolete("GroupSchemaTemplates are deprecated, use GroupTemplateObjects")]
        public bool RemoveSchemaTemplate(int index, bool postEvent = true)
        {
            Debug.LogError("GroupSchemaTemplates are deprecated, use GroupTemplateObjects");
            return false;
        }

        [SerializeField]
        List<ScriptableObject> m_GroupTemplateObjects = new List<ScriptableObject>();

        /// <summary>
        /// List of ScriptableObjects that implement the IGroupTemplate interface for providing new templates.
        /// For use in the AddressableAssetsWindow to display new groups to create
        /// </summary>
        public List<ScriptableObject> GroupTemplateObjects
        {
            get { return m_GroupTemplateObjects; }
        }

        /// <summary>
        /// Get the IGroupTemplate at the specified index.
        /// </summary>
        /// <param name="index">The index of the template object.</param>
        /// <returns>The AddressableAssetGroupTemplate object at the specified index.</returns>
        public IGroupTemplate GetGroupTemplateObject(int index)
        {
            if (m_GroupTemplateObjects.Count == 0)
                return null;
            if (index < 0 || index >= m_GroupTemplateObjects.Count)
            {
                Debug.LogWarningFormat("Invalid index for group template: {0}.", index);
                return null;
            }
            return m_GroupTemplateObjects[Mathf.Clamp(index, 0, m_GroupTemplateObjects.Count)] as IGroupTemplate;
        }

        /// <summary>
        /// Adds a AddressableAssetsGroupTemplate object.
        /// </summary>
        /// <param name="templateObject">The AddressableAssetGroupTemplate object to add.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was added.</returns>
        public bool AddGroupTemplateObject(IGroupTemplate templateObject, bool postEvent = true)
        {
            if (templateObject == null)
            {
                Debug.LogWarning("Cannot add null IGroupTemplate");
                return false;
            }
            var so = templateObject as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("Group Template objects must inherit from ScriptableObject.");
                return false;
            }

            m_GroupTemplateObjects.Add(so);
            SetDirty(ModificationEvent.GroupTemplateAdded, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Remove the AddressableAssetGroupTemplate object at the specified index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        /// <param name="postEvent">Indicates if an event should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was removed.</returns>
        public bool RemoveGroupTemplateObject(int index, bool postEvent = true)
        {
            if (m_GroupTemplateObjects.Count <= index)
                return false;
            var so = m_GroupTemplateObjects[index];
            m_GroupTemplateObjects.RemoveAt(index);
            SetDirty(ModificationEvent.GroupTemplateRemoved, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Sets the initialization object at the specified index.
        /// </summary>
        /// <param name="index">The index to set the initialization object.</param>
        /// <param name="templateObject">The rroup template object to set.  This must be a valid scriptable object that implements the IGroupTemplate interface.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was set, false otherwise.</returns>
        public bool SetGroupTemplateObjectAtIndex(int index, IGroupTemplate templateObject, bool postEvent = true)
        {
            if (m_GroupTemplateObjects.Count <= index)
                return false;
            if (templateObject == null)
            {
                Debug.LogWarning("Cannot set null IGroupTemplate");
                return false;
            }
            var so = templateObject as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("AddressableAssetGroupTemplate objects must inherit from ScriptableObject.");
                return false;
            }

            m_GroupTemplateObjects[index] = so;
            SetDirty(ModificationEvent.GroupTemplateAdded, so, postEvent, true);
            return true;
        }

        [FormerlySerializedAs("m_initializationObjects")]
        [SerializeField]
        List<ScriptableObject> m_InitializationObjects = new List<ScriptableObject>();
        /// <summary>
        /// List of ScriptableObjects that implement the IObjectInitializationDataProvider interface for providing runtime initialization.
        /// </summary>
        public List<ScriptableObject> InitializationObjects
        {
            get { return m_InitializationObjects; }
        }

        /// <summary>
        /// Get the IObjectInitializationDataProvider at a specifc index.
        /// </summary>
        /// <param name="index">The index of the initialization object.</param>
        /// <returns>The initialization object at the specified index.</returns>
        public IObjectInitializationDataProvider GetInitializationObject(int index)
        {
            if (m_InitializationObjects.Count == 0)
                return null;
            if (index < 0 || index >= m_InitializationObjects.Count)
            {
                Debug.LogWarningFormat("Invalid index for data builder: {0}.", index);
                return null;
            }
            return m_InitializationObjects[Mathf.Clamp(index, 0, m_InitializationObjects.Count)] as IObjectInitializationDataProvider;
        }

        /// <summary>
        /// Adds an initialization object.
        /// </summary>
        /// <param name="initObject">The initialization object to add.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was added.</returns>
        public bool AddInitializationObject(IObjectInitializationDataProvider initObject, bool postEvent = true)
        {
            if (initObject == null)
            {
                Debug.LogWarning("Cannot add null IObjectInitializationDataProvider");
                return false;
            }
            var so = initObject as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("Initialization objects must inherit from ScriptableObject.");
                return false;
            }

            m_InitializationObjects.Add(so);
            SetDirty(ModificationEvent.InitializationObjectAdded, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Remove the initialization object at the specified index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was removed.</returns>
        public bool RemoveInitializationObject(int index, bool postEvent = true)
        {
            if (m_InitializationObjects.Count <= index)
                return false;
            var so = m_InitializationObjects[index];
            m_InitializationObjects.RemoveAt(index);
            SetDirty(ModificationEvent.InitializationObjectRemoved, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Sets the initialization object at the specified index.
        /// </summary>
        /// <param name="index">The index to set the initialization object.</param>
        /// <param name="initObject">The initialization object to set.  This must be a valid scriptable object that implements the IInitializationObject interface.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the initialization object was set, false otherwise.</returns>
        public bool SetInitializationObjectAtIndex(int index, IObjectInitializationDataProvider initObject, bool postEvent = true)
        {
            if (m_InitializationObjects.Count <= index)
                return false;
            if (initObject == null)
            {
                Debug.LogWarning("Cannot add null IObjectInitializationDataProvider");
                return false;
            }
            var so = initObject as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("Initialization objects must inherit from ScriptableObject.");
                return false;
            }

            m_InitializationObjects[index] = so;
            SetDirty(ModificationEvent.InitializationObjectAdded, so, postEvent, true);
            return true;
        }

        [SerializeField]
        [SerializedTypeRestriction(type = typeof(UnityEngine.Networking.CertificateHandler))]
        SerializedType m_CertificateHandlerType;

        /// <summary>
        /// The type of CertificateHandler to use for this provider.
        /// </summary>
        public Type CertificateHandlerType
        {
            get
            {
                return m_CertificateHandlerType.Value;
            }
            set
            {
                m_CertificateHandlerType.Value = value;
                SetDirty(ModificationEvent.CertificateHandlerChanged, value, true, true);
            }
        }

        [FormerlySerializedAs("m_activePlayerDataBuilderIndex")]
        [SerializeField]
        int m_ActivePlayerDataBuilderIndex = 3;
        [FormerlySerializedAs("m_dataBuilders")]
        [SerializeField]
        List<ScriptableObject> m_DataBuilders = new List<ScriptableObject>();
        /// <summary>
        /// List of ScriptableObjects that implement the IDataBuilder interface.  These are used to create data for editor play mode and for player builds.
        /// </summary>
        public List<ScriptableObject> DataBuilders { get { return m_DataBuilders; } }
        /// <summary>
        /// Get The data builder at a specifc index.
        /// </summary>
        /// <param name="index">The index of the builder.</param>
        /// <returns>The data builder at the specified index.</returns>
        public IDataBuilder GetDataBuilder(int index)
        {
            if (m_DataBuilders.Count == 0)
                return null;
            if (index < 0 || index >= m_DataBuilders.Count)
            {
                Debug.LogWarningFormat("Invalid index for data builder: {0}.", index);
                return null;
            }
            return m_DataBuilders[Mathf.Clamp(index, 0, m_DataBuilders.Count)] as IDataBuilder;
        }

        /// <summary>
        /// Adds a data builder.
        /// </summary>
        /// <param name="builder">The data builder to add.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the data builder was added.</returns>
        public bool AddDataBuilder(IDataBuilder builder, bool postEvent = true)
        {
            if (builder == null)
            {
                Debug.LogWarning("Cannot add null IDataBuilder");
                return false;
            }
            var so = builder as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("Data builders must inherit from ScriptableObject.");
                return false;
            }

            m_DataBuilders.Add(so);
            SetDirty(ModificationEvent.DataBuilderAdded, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Remove the data builder at the sprcified index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the builder was removed.</returns>
        public bool RemoveDataBuilder(int index, bool postEvent = true)
        {
            if (m_DataBuilders.Count <= index)
                return false;
            var so = m_DataBuilders[index];
            m_DataBuilders.RemoveAt(index);
            SetDirty(ModificationEvent.DataBuilderRemoved, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Sets the data builder at the specified index.
        /// </summary>
        /// <param name="index">The index to set the builder.</param>
        /// <param name="builder">The builder to set.  This must be a valid scriptable object that implements the IDataBuilder interface.</param>
        /// <param name="postEvent">Indicates if an even should be posted to the Addressables event system for this change.</param>
        /// <returns>True if the builder was set, false otherwise.</returns>
        public bool SetDataBuilderAtIndex(int index, IDataBuilder builder, bool postEvent = true)
        {
            if (m_DataBuilders.Count <= index)
                return false;
            if (builder == null)
            {
                Debug.LogWarning("Cannot add null IDataBuilder");
                return false;
            }
            var so = builder as ScriptableObject;
            if (so == null)
            {
                Debug.LogWarning("Data builders must inherit from ScriptableObject.");
                return false;
            }

            m_DataBuilders[index] = so;
            SetDirty(ModificationEvent.DataBuilderAdded, so, postEvent, true);
            return true;
        }

        /// <summary>
        /// Get the active data builder for player data.
        /// </summary>
        public IDataBuilder ActivePlayerDataBuilder
        {
            get
            {
                return GetDataBuilder(m_ActivePlayerDataBuilderIndex);
            }
        }

        /// <summary>
        /// Get the active data builder for editor play mode data.
        /// </summary>
        public IDataBuilder ActivePlayModeDataBuilder
        {
            get
            {
                return GetDataBuilder(ProjectConfigData.ActivePlayModeIndex);
            }
        }

        /// <summary>
        /// Get the index of the active player data builder.
        /// </summary>
        public int ActivePlayerDataBuilderIndex
        {
            get
            {
                return m_ActivePlayerDataBuilderIndex;
            }
            set
            {
                if (m_ActivePlayerDataBuilderIndex != value)
                {
                    m_ActivePlayerDataBuilderIndex = value;
                    SetDirty(ModificationEvent.ActiveBuildScriptChanged, ActivePlayerDataBuilder, true, true);
                }
            }
        }

        /// <summary>
        /// Get the index of the active play mode data builder.
        /// </summary>
        public int ActivePlayModeDataBuilderIndex
        {
            get
            {
                return ProjectConfigData.ActivePlayModeIndex;
            }
            set
            {
                ProjectConfigData.ActivePlayModeIndex = value;
                SetDirty(ModificationEvent.ActivePlayModeScriptChanged, ActivePlayModeDataBuilder, true, false);
            }
        }

        /// <summary>
        /// Gets the list of all defined labels.
        /// </summary>
        /// <returns>Returns a list of all defined labels.</returns>
        public List<string> GetLabels()
        {
            return m_LabelTable.labelNames.ToList();
        }

        /// <summary>
        /// Add a new label.
        /// </summary>
        /// <param name="label">The label name.</param>
        /// <param name="postEvent">Send modification event.</param>
        public void AddLabel(string label, bool postEvent = true)
        {
            if (m_LabelTable.AddLabelName(label))
                SetDirty(ModificationEvent.LabelAdded, label, postEvent, true);
        }

        internal void RenameLabel(string oldLabelName, string newLabelName)
        {
            int index = m_LabelTable.GetIndexOfLabel(oldLabelName);
            if (index < 0)
                return;

            if (!m_LabelTable.AddLabelName(newLabelName, index))
                return;

            foreach (var group in groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.labels.Contains(oldLabelName))
                    {
                        entry.labels.Remove(oldLabelName);
                        entry.SetLabel(newLabelName, true);
                    }
                }
            }

            m_LabelTable.RemoveLabelName(oldLabelName);
        }

        /// <summary>
        /// Remove a label by name.
        /// </summary>
        /// <param name="label">The label name.</param>
        /// <param name="postEvent">Send modification event.</param>
        public void RemoveLabel(string label, bool postEvent = true)
        {
            m_LabelTable.RemoveLabelName(label);
            SetDirty(ModificationEvent.LabelRemoved, label, postEvent, true);
            Debug.LogWarningFormat("Label \"{0}\" removed. If you re-add the label before building, it will be restored in entries that had it. " +
                "Building Addressables content will clear this label from all entries. That action cannot be undone.", label);
        }

        [FormerlySerializedAs("m_activeProfileId")]
        [SerializeField]
        string m_ActiveProfileId;
        /// <summary>
        /// The active profile id.
        /// </summary>
        public string activeProfileId
        {
            get
            {
                if (string.IsNullOrEmpty(m_ActiveProfileId))
                    m_ActiveProfileId = m_ProfileSettings.CreateDefaultProfile();
                return m_ActiveProfileId;
            }
            set
            {
                var oldVal = m_ActiveProfileId;
                m_ActiveProfileId = value;

                if (oldVal != value)
                {
                    SetDirty(ModificationEvent.ActiveProfileSet, value, true, true);
                }
            }
        }

        [FormerlySerializedAs("m_hostingServicesManager")]
        [SerializeField]
        HostingServicesManager m_HostingServicesManager;
        /// <summary>
        /// Get the HostingServicesManager object.
        /// </summary>
        public HostingServicesManager HostingServicesManager
        {
            get
            {
                if (m_HostingServicesManager == null)
                    m_HostingServicesManager = new HostingServicesManager();

                if (!m_HostingServicesManager.IsInitialized)
                    m_HostingServicesManager.Initialize(this);

                return m_HostingServicesManager;
            }

            // For unit tests
            internal set { m_HostingServicesManager = value; }
        }

        /// <summary>
        /// Gets all asset entries from all groups.
        /// </summary>
        /// <param name="assets">The list of asset entries.</param>
        /// <param name="includeSubObjects">Determines if sub objects such as sprites should be included.</param>
        /// <param name="groupFilter">A method to filter groups.  Groups will be processed if filter is null, or it returns TRUE</param>
        /// <param name="entryFilter">A method to filter entries.  Entries will be processed if filter is null, or it returns TRUE</param>
        public void GetAllAssets(List<AddressableAssetEntry> assets, bool includeSubObjects, Func<AddressableAssetGroup, bool> groupFilter = null, Func<AddressableAssetEntry, bool> entryFilter = null)
        {
            using (var cache = new AddressablesFileEnumerationCache(this, false, null))
            {
                foreach (var g in groups)
                    if (g != null && (groupFilter == null || groupFilter(g)))
                        g.GatherAllAssets(assets, true, true, includeSubObjects, entryFilter);
            }
        }

        internal void GatherAllAssetReferenceDrawableEntries(List<IReferenceEntryData> assets)
        {
            using (var cache = new AddressablesFileEnumerationCache(this, false, null))
            {
                foreach (var g in groups)
                {
                    if (g != null)
                        g.GatherAllAssetReferenceDrawableEntries(assets);
                }
            }
        }

        /// <summary>
        /// Remove an asset entry.
        /// </summary>
        /// <param name="guid">The  guid of the asset.</param>
        /// <param name="postEvent">Send modifcation event.</param>
        /// <returns>True if the entry was found and removed.</returns>
        public bool RemoveAssetEntry(string guid, bool postEvent = true)
            => RemoveAssetEntry(FindAssetEntry(guid), postEvent);

        /// <summary>
        /// Remove an asset entry.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        /// <param name="postEvent">Send modifcation event.</param>
        /// <returns>True if the entry was found and removed.</returns>
        internal bool RemoveAssetEntry(AddressableAssetEntry entry, bool postEvent = true)
        {
            if (entry == null)
                return false;
            if (entry.parentGroup != null)
                entry.parentGroup.RemoveAssetEntry(entry, postEvent);
            return true;
        }

        void Awake()
        {
            profileSettings.OnAfterDeserialize(this);
            buildSettings.OnAfterDeserialize(this);
        }

        void OnEnable()
        {
            HostingServicesManager.OnEnable();
        }

        void OnDisable()
        {
            HostingServicesManager.OnDisable();
        }

        private string m_DefaultGroupTemplateName = "Packed Assets";
        void Validate()
        {
            // Begin update any SchemaTemplate to GroupTemplateObjects
            if (m_SchemaTemplates != null && m_SchemaTemplates.Count > 0)
            {
                Debug.LogError("Updating from GroupSchema version that is too old, deleting schemas");
                m_SchemaTemplates = null;
            }
            if (m_GroupTemplateObjects.Count == 0)
                CreateDefaultGroupTemplate(this);
            // End update of SchemaTemplate to GroupTemplates

            if (m_BuildSettings == null)
                m_BuildSettings = new AddressableAssetBuildSettings();
            if (m_ProfileSettings == null)
                m_ProfileSettings = new AddressableAssetProfileSettings();
            if (m_LabelTable == null)
                m_LabelTable = new LabelTable();
            if (string.IsNullOrEmpty(m_ActiveProfileId))
                m_ActiveProfileId = m_ProfileSettings.CreateDefaultProfile();
            if (m_DataBuilders == null || m_DataBuilders.Count == 0)
            {
                m_DataBuilders = new List<ScriptableObject>();
                m_DataBuilders.Add(CreateScriptAsset<BuildScriptFastMode>());
                m_DataBuilders.Add(CreateScriptAsset<BuildScriptVirtualMode>());
                m_DataBuilders.Add(CreateScriptAsset<BuildScriptPackedPlayMode>());
                m_DataBuilders.Add(CreateScriptAsset<BuildScriptPackedMode>());
            }

            if (ActivePlayerDataBuilder != null && !ActivePlayerDataBuilder.CanBuildData<AddressablesPlayerBuildResult>())
                ActivePlayerDataBuilderIndex = m_DataBuilders.IndexOf(m_DataBuilders.Find(s => s.GetType() == typeof(BuildScriptPackedMode)));
            if (ActivePlayModeDataBuilder != null && !ActivePlayModeDataBuilder.CanBuildData<AddressablesPlayModeBuildResult>())
                ActivePlayModeDataBuilderIndex = m_DataBuilders.IndexOf(m_DataBuilders.Find(s => s.GetType() == typeof(BuildScriptFastMode)));

            profileSettings.Validate(this);
            buildSettings.Validate(this);
        }

        T CreateScriptAsset<T>() where T : ScriptableObject
        {
            var script = CreateInstance<T>();
            if (!Directory.Exists(DataBuilderFolder))
                Directory.CreateDirectory(DataBuilderFolder);
            var path = DataBuilderFolder + "/" + typeof(T).Name + ".asset";
            if (!File.Exists(path))
                AssetDatabase.CreateAsset(script, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// The default name of the built in player data AddressableAssetGroup
        /// </summary>
        public const string PlayerDataGroupName = "Built In Data";
        /// <summary>
        /// The default name of the local data AddressableAsssetGroup
        /// </summary>
        public const string DefaultLocalGroupName = "Default Local Group";

        /// <summary>
        /// Create a new AddressableAssetSettings object.
        /// </summary>
        /// <param name="configFolder">The folder to store the settings object.</param>
        /// <param name="configName">The name of the settings object.</param>
        /// <param name="createDefaultGroups">If true, create groups for player data and local packed content.</param>
        /// <param name="isPersisted">If true, assets are created.</param>
        /// <returns></returns>
        public static AddressableAssetSettings Create(string configFolder, string configName, bool createDefaultGroups, bool isPersisted)
        {
            AddressableAssetSettings aa;
            var path = configFolder + "/" + configName + ".asset";
            aa = isPersisted ? AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path) : null;
            if (aa == null)
            {
                aa = CreateInstance<AddressableAssetSettings>();
                aa.m_IsTemporary = !isPersisted;
                aa.activeProfileId = aa.profileSettings.Reset();
                aa.name = configName;
                // TODO: Uncomment after initial opt-in testing period
                //aa.ContiguousBundles = true;
                aa.BuildAddressablesWithPlayerBuild = PlayerBuildOption.PreferencesValue;
                
                if (isPersisted)
                {
                    Directory.CreateDirectory(configFolder);
                    AssetDatabase.CreateAsset(aa, path);
                    aa = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
                    aa.Validate();
                }

                if (createDefaultGroups)
                {
                    CreateBuiltInData(aa);
                    CreateDefaultGroup(aa);
                }

                if (isPersisted)
                    AssetDatabase.SaveAssets();
            }
            return aa;
        }

        /// <summary>
        /// Creates a new AddressableAssetGroupTemplate Object with the set of schema types with default settings for use in the editor GUI.
        /// </summary>
        /// <param name="displayName">The display name of the template.</param>
        /// <param name="description">Description text use with the template.</param>
        /// <param name="types">The schema types for the template.</param>
        /// <returns>True if the template was added, false otherwise.</returns>
        public bool CreateAndAddGroupTemplate(string displayName, string description, params Type[] types)
        {
            string assetPath = GroupTemplateFolder + "/" + displayName + ".asset";

            if (!CanCreateGroupTemplate(displayName, assetPath, types))
                return false;

            if (!Directory.Exists(GroupTemplateFolder))
                Directory.CreateDirectory(GroupTemplateFolder);

            AddressableAssetGroupTemplate newAssetGroupTemplate = ScriptableObject.CreateInstance<AddressableAssetGroupTemplate>();
            newAssetGroupTemplate.Description = description;

            AssetDatabase.CreateAsset(newAssetGroupTemplate, assetPath);
            AssetDatabase.SaveAssets();

            AddGroupTemplateObject(newAssetGroupTemplate);

            foreach (Type type in types)
                newAssetGroupTemplate.AddSchema(type);


            return true;
        }

        private bool CanCreateGroupTemplate(string displayName, string assetPath, Type[] types)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template must have a valid name.");
                return false;
            }
            if (types.Length == 0)
            {
                Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template {0} must contain at least 1 schema type.", displayName);
                return false;
            }
            bool typesAreValid = true;
            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];
                if (t == null)
                {
                    Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template {0} schema type at index {1} is null.", displayName, i);
                    typesAreValid = false;
                }
                else if (!typeof(AddressableAssetGroupSchema).IsAssignableFrom(t))
                {
                    Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template {0} schema type at index {1} must inherit from AddressableAssetGroupSchema.  Specified type was {2}.", displayName, i, t.FullName);
                    typesAreValid = false;
                }
            }
            if (!typesAreValid)
            {
                Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template {0} must contains at least 1 invalid schema type.", displayName);
                return false;
            }

            if (File.Exists(assetPath))
            {
                Debug.LogWarningFormat("CreateAndAddGroupTemplate - Group template {0} already exists at location {1}.", displayName, assetPath);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Find asset group by functor.
        /// </summary>
        /// <param name="func">The functor to call on each group.  The first group that evaluates to true is returned.</param>
        /// <returns>The group found or null.</returns>
        public AddressableAssetGroup FindGroup(Func<AddressableAssetGroup, bool> func)
        {
            return groups.Find(g => g != null && func(g));
        }

        /// <summary>
        /// Find asset group by name.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        /// <returns>The group found or null.</returns>
        public AddressableAssetGroup FindGroup(string groupName)
        {
            return FindGroup(g => g != null && g.Name == groupName);
        }

        /// <summary>
        /// The default group.  This group is used when marking assets as addressable via the inspector.
        /// </summary>
        public AddressableAssetGroup DefaultGroup
        {
            get
            {
                AddressableAssetGroup group = null;
                if (string.IsNullOrEmpty(m_DefaultGroup))
                    group = groups.FirstOrDefault(s => s != null && s.CanBeSetAsDefault());
                else
                {
                    group = groups.FirstOrDefault(x => x != null && x.Guid == m_DefaultGroup);
                    if (group == null || !group.CanBeSetAsDefault())
                    {
                        group = groups.FirstOrDefault(s => s != null && s.CanBeSetAsDefault());
                        if (group != null)
                            m_DefaultGroup = group.Guid;
                    }
                }

                if (group == null)
                {
                    Addressables.LogWarning("A valid default group could not be found.  One will be created.");
                    group = CreateDefaultGroup(this);
                }

                return group;
            }
            set
            {
                if (value == null)
                    Addressables.LogError("Unable to set null as the Default Group.  Default Groups must not be ReadOnly.");

                else if (!value.CanBeSetAsDefault())
                    Addressables.LogError("Unable to set " + value.Name + " as the Default Group.  Default Groups must not be ReadOnly.");
                else
                    m_DefaultGroup = value.Guid;
            }
        }

        internal static AddressableAssetGroup CreateBuiltInData(AddressableAssetSettings aa)
        {
            var playerData = aa.CreateGroup(PlayerDataGroupName, false, true, false, null, typeof(PlayerDataGroupSchema));
            var resourceEntry = aa.CreateOrMoveEntry(AddressableAssetEntry.ResourcesName, playerData);
            resourceEntry.IsInResources = true;
            aa.CreateOrMoveEntry(AddressableAssetEntry.EditorSceneListName, playerData);
            return playerData;
        }

        private static AddressableAssetGroup CreateDefaultGroup(AddressableAssetSettings aa)
        {
            var localGroup = aa.CreateGroup(DefaultLocalGroupName, true, false, false, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
            var schema = localGroup.GetSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(aa, kLocalBuildPath);
            schema.LoadPath.SetVariableByName(aa, kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            aa.m_DefaultGroup = localGroup.Guid;
            return localGroup;
        }

        private static bool CreateDefaultGroupTemplate(AddressableAssetSettings aa)
        {
            string assetPath = aa.GroupTemplateFolder + "/" + aa.m_DefaultGroupTemplateName + ".asset";

            if (File.Exists(assetPath))
                return LoadGroupTemplateObject(aa, assetPath);

            return aa.CreateAndAddGroupTemplate(aa.m_DefaultGroupTemplateName, "Pack assets into asset bundles.", typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        }

        private static bool LoadGroupTemplateObject(AddressableAssetSettings aa, string assetPath)
        {
            return aa.AddGroupTemplateObject(AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject)) as IGroupTemplate);
        }

        internal AddressableAssetEntry CreateEntry(string guid, string address, AddressableAssetGroup parent, bool readOnly, bool postEvent = true)
        {
            AddressableAssetEntry entry = parent.GetAssetEntry(guid);
            if (entry == null)
                entry = new AddressableAssetEntry(guid, address, parent, readOnly);

            if (!readOnly)
                SetDirty(ModificationEvent.EntryCreated, entry, postEvent, false);

            return entry;
        }

        /// <summary>
        /// Marks the object as modified.
        /// </summary>
        /// <param name="modificationEvent">The event type that is changed.</param>
        /// <param name="eventData">The object data that corresponds to the event.</param>
        /// <param name="postEvent">If true, the event is propagated to callbacks.</param>
        /// <param name="settingsModified">If true, the settings asset will be marked as dirty.</param>
        public void SetDirty(ModificationEvent modificationEvent, object eventData, bool postEvent, bool settingsModified = false)
        {
            if (modificationEvent == ModificationEvent.ProfileRemoved && eventData as string == activeProfileId)
                activeProfileId = null;
            if (this != null)
            {
                if (postEvent)
                {
                    if (OnModificationGlobal != null)
                        OnModificationGlobal(this, modificationEvent, eventData);
                    if (OnModification != null)
                        OnModification(this, modificationEvent, eventData);
                }

                if (settingsModified && IsPersisted)
                    EditorUtility.SetDirty(this);
            }

            m_CachedHash = default(Hash128);
        }

        internal bool RemoveMissingGroupReferences()
        {
            List<int> missingGroupsIndices = new List<int>();
            for (int i = 0; i < groups.Count; i++)
            {
                var g = groups[i];
                if (g == null)
                    missingGroupsIndices.Add(i);
            }
            if (missingGroupsIndices.Count > 0)
            {
                Debug.Log("Addressable settings contains " + missingGroupsIndices.Count + " group reference(s) that are no longer there. Removing reference(s).");
                for (int i = missingGroupsIndices.Count - 1; i >= 0; i--)
                {
                    groups.RemoveAt(missingGroupsIndices[i]);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find and asset entry by guid.
        /// </summary>
        /// <param name="guid">The asset guid.</param>
        /// <returns>The found entry or null.</returns>
        public AddressableAssetEntry FindAssetEntry(string guid)
        {
            return FindAssetEntry(guid, false);
        }

        /// <summary>
        /// Find and asset entry by guid.
        /// </summary>
        /// <param name="guid">The asset guid.</param>
        /// <param name="includeImplicit">Whether or not to include implicit asset entries in the search.</param>
        /// <returns>The found entry or null.</returns>
        public AddressableAssetEntry FindAssetEntry(string guid, bool includeImplicit)
        {
            AddressableAssetEntry foundEntry = null;
            if (includeImplicit)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AddressableAssetUtility.IsPathValidForEntry(path))
                    return null;
                
                // try find non-implicit first
                foreach (var g in groups)
                {
                    if (g != null)
                    {
                        foundEntry = g.GetAssetEntry(guid);
                        if (foundEntry != null)
                            return foundEntry;
                    }
                }
                
                // go through all collections
                foreach (var g in groups)
                {
                    if (g != null)
                    {
                        foreach (AddressableAssetEntry addressableAssetEntry in g.AssetCollectionEntries)
                        {
                            var e = addressableAssetEntry.GetAssetCollectionSubEntry(guid);
                            if (e != null)
                                return e;
                        }
                    }
                }

                // find an explicit parent folder entry within groups
                string directory = Path.GetDirectoryName(path);
                AddressableAssetEntry folderEntry = null;
                while (!string.IsNullOrEmpty(directory))
                {
                    string folderGuid = AssetDatabase.AssetPathToGUID(directory);
                    foreach (var g in groups)
                    {
                        if (g != null)
                        {
                            folderEntry = g.GetAssetEntry(folderGuid);
                            if (folderEntry != null)
                                break;
                        }
                    }

                    if (folderEntry != null)
                    {
                        foundEntry = folderEntry.GetFolderSubEntry(guid, path);
                        if (foundEntry != null)
                            return foundEntry;
                        Debug.LogError($"Explicit AssetEntry for {directory} unable to find subEntry {path}");
                    }
                    directory = Path.GetDirectoryName(directory);
                }
            }
            else
            {
                foreach (var g in groups)
                {
                    if (g != null)
                    {
                        foundEntry = g.GetAssetEntry(guid);
                        if (foundEntry != null)
                            break;
                    }
                }
            }
            return foundEntry;
        }

        internal bool IsAssetPathInAddressableDirectory(string assetPath, out string assetName)
        {
            if (!string.IsNullOrEmpty(assetPath))
            {
                var dir = Path.GetDirectoryName(assetPath);
                while (!string.IsNullOrEmpty(dir))
                {
                    var dirEntry = FindAssetEntry(AssetDatabase.AssetPathToGUID(dir));
                    if (dirEntry != null)
                    {
                        assetName = dirEntry.address + assetPath.Remove(0, dir.Length);
                        return true;
                    }

                    dir = Path.GetDirectoryName(dir);
                }
            }
            assetName = "";
            return false;
        }

        internal void MoveAssetsFromResources(Dictionary<string, string> guidToNewPath, AddressableAssetGroup targetParent)
        {
            if (guidToNewPath == null || targetParent == null)
            {
                return;
            }

            var entries = new List<AddressableAssetEntry>();
            var createdDirs = new List<string>();
            AssetDatabase.StartAssetEditing();
            foreach (var item in guidToNewPath)
            {
                var dirInfo = new FileInfo(item.Value).Directory;
                if (dirInfo != null && !dirInfo.Exists)
                {
                    dirInfo.Create();
                    createdDirs.Add(dirInfo.FullName);
                    AssetDatabase.StopAssetEditing();
                    AssetDatabase.Refresh();
                    AssetDatabase.StartAssetEditing();
                }

                var oldPath = AssetDatabase.GUIDToAssetPath(item.Key);
                var errorStr = AssetDatabase.MoveAsset(oldPath, item.Value);
                if (!string.IsNullOrEmpty(errorStr))
                {
                    Addressables.LogError("Error moving asset: " + errorStr);
                }
                else
                {
                    AddressableAssetEntry e = FindAssetEntry(item.Key);
                    if (e != null)
                        e.IsInResources = false;

                    var newEntry = CreateOrMoveEntry(item.Key, targetParent, false, false);
                    var index = oldPath.ToLower().LastIndexOf("resources/");
                    if (index >= 0)
                    {
                        var newAddress = oldPath.Substring(index + 10);
                        if (Path.HasExtension(newAddress))
                        {
                            newAddress = newAddress.Replace(Path.GetExtension(oldPath), "");
                        }

                        if (!string.IsNullOrEmpty(newAddress))
                        {
                            newEntry.SetAddress(newAddress, false);
                        }
                    }
                    entries.Add(newEntry);
                }
            }

            foreach (var dir in createdDirs)
                DirectoryUtility.DeleteDirectory(dir, onlyIfEmpty: true);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
            SetDirty(ModificationEvent.EntryMoved, entries, true, true);
        }

        /// <summary>
        /// Move an existing entry to a group.
        /// </summary>
        /// <param name="entries">The entries to move.</param>
        /// <param name="targetParent">The group to add the entries to.</param>
        /// <param name="readOnly">Should the entries be read only.</param>
        /// <param name="postEvent">Send modification event.</param>
        public void MoveEntries(List<AddressableAssetEntry> entries, AddressableAssetGroup targetParent, bool readOnly = false, bool postEvent = true)
        {
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    MoveEntry(entry, targetParent, readOnly, false);
                }

                SetDirty(ModificationEvent.EntryMoved, entries, postEvent, false);
            }
        }

        /// <summary>
        /// Move an existing entry to a group.
        /// </summary>
        /// <param name="entry">The entry to move.</param>
        /// <param name="targetParent">The group to add the entry to.</param>
        /// <param name="readOnly">Should the entry be read only.</param>
        /// <param name="postEvent">Send modification event.</param>
        public void MoveEntry(AddressableAssetEntry entry, AddressableAssetGroup targetParent, bool readOnly = false, bool postEvent = true)
        {
            if (targetParent == null || entry == null)
                return;

            entry.ReadOnly = readOnly;

            if (entry.parentGroup != null && entry.parentGroup != targetParent)
                entry.parentGroup.RemoveAssetEntry(entry, postEvent);

            targetParent.AddAssetEntry(entry, postEvent);
        }

        /// <summary>
        /// Create a new entry, or if one exists in a different group, move it into the new group.
        /// </summary>
        /// <param name="guid">The asset guid.</param>
        /// <param name="targetParent">The group to add the entry to.</param>
        /// <param name="readOnly">Is the new entry read only.</param>
        /// <param name="postEvent">Send modification event.</param>
        /// <returns></returns>
        public AddressableAssetEntry CreateOrMoveEntry(string guid, AddressableAssetGroup targetParent, bool readOnly = false, bool postEvent = true)
        {
            if (targetParent == null || string.IsNullOrEmpty(guid))
                return null;

            AddressableAssetEntry entry = FindAssetEntry(guid);
            if (entry != null) //move entry to where it should go...
            {
                MoveEntry(entry, targetParent, readOnly, postEvent);
            }
            else //create entry
            {
                entry = CreateAndAddEntryToGroup(guid, targetParent, readOnly, postEvent);
            }

            return entry;
        }

        /// <summary>
        /// Create a new entries for each asset, or if one exists in a different group, move it into the targetParent group.
        /// </summary>
        /// <param name="guids">The asset guid's to move.</param>
        /// <param name="targetParent">The group to add the entries to.</param>
        /// <param name="createdEntries">List to add new entries to.</param>
        /// <param name="movedEntries">List to add moved entries to.</param>
        /// <param name="readOnly">Is the new entry read only.</param>
        /// <param name="postEvent">Send modification event.</param>
        /// <exception cref="ArgumentException"></exception>
        internal void CreateOrMoveEntries(IEnumerable guids, AddressableAssetGroup targetParent, List<AddressableAssetEntry> createdEntries, List<AddressableAssetEntry> movedEntries, bool readOnly = false, bool postEvent = true)
        {
            if (targetParent == null)
                throw new ArgumentException("targetParent must not be null");

            if (createdEntries == null)
                createdEntries = new List<AddressableAssetEntry>();
            if (movedEntries == null)
                movedEntries = new List<AddressableAssetEntry>();

            foreach (string guid in guids)
            {
                AddressableAssetEntry entry = FindAssetEntry(guid);
                if (entry != null)
                {
                    MoveEntry(entry, targetParent, readOnly, postEvent);
                    movedEntries.Add(entry);
                }
                else
                {
                    entry = CreateAndAddEntryToGroup(guid, targetParent, readOnly, postEvent);
                    if (entry != null)
                        createdEntries.Add(entry);
                }
            }
        }

        private AddressableAssetEntry CreateAndAddEntryToGroup(string guid, AddressableAssetGroup targetParent, bool readOnly = false, bool postEvent = true)
        {
            AddressableAssetEntry entry = null;
            var path = AssetDatabase.GUIDToAssetPath(guid);

            if (AddressableAssetUtility.IsPathValidForEntry(path))
            {
                entry = CreateEntry(guid, path, targetParent, readOnly, postEvent);
            }
            else
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(path) != null && BuildUtility.IsEditorAssembly(AssetDatabase.GetMainAssetTypeAtPath(path).Assembly))
                    return null;
                entry = CreateEntry(guid, guid, targetParent, true, postEvent);
            }

            targetParent.AddAssetEntry(entry, postEvent);
            return entry;
        }

        internal AddressableAssetEntry CreateSubEntryIfUnique(string guid, string address, AddressableAssetEntry parentEntry)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            AddressableAssetEntry entry = FindAssetEntry(guid);

            if (entry == null)
            {
                entry = new AddressableAssetEntry(guid, address, parentEntry.parentGroup, true);
                entry.IsSubAsset = true;
                entry.ParentEntry = parentEntry;
                entry.BundleFileId = parentEntry.BundleFileId;
                //parentEntry.parentGroup.AddAssetEntry(entry);
                return entry;
            }

            //if the sub-entry already exists update it's info.  This mainly covers the case of dragging folders around.
            if (entry.IsSubAsset)
            {
                entry.parentGroup = parentEntry.parentGroup;
                entry.IsInResources = parentEntry.IsInResources;
                entry.address = address;
                entry.ReadOnly = true;
                entry.BundleFileId = parentEntry.BundleFileId;
                return entry;
            }
            return null;
        }

        /// <summary>
        /// Create a new asset group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="setAsDefaultGroup">Set the new group as the default group.</param>
        /// <param name="readOnly">Is the new group read only.</param>
        /// <param name="postEvent">Post modification event.</param>
        /// <param name="schemasToCopy">Schema set to copy from.</param>
        /// <param name="types">Types of schemas to add.</param>
        /// <returns>The newly created group.</returns>
        public AddressableAssetGroup CreateGroup(string groupName, bool setAsDefaultGroup, bool readOnly, bool postEvent, List<AddressableAssetGroupSchema> schemasToCopy, params Type[] types)
        {
            if (string.IsNullOrEmpty(groupName))
                groupName = kNewGroupName;
            string validName = FindUniqueGroupName(groupName);
            var group = CreateInstance<AddressableAssetGroup>();
            group.Initialize(this, validName, GUID.Generate().ToString(), readOnly);

            if (IsPersisted)
            {
                if (!Directory.Exists(GroupFolder))
                    Directory.CreateDirectory(GroupFolder);
                AssetDatabase.CreateAsset(group, GroupFolder + "/" + group.Name + ".asset");
            }
            if (schemasToCopy != null)
            {
                foreach (var s in schemasToCopy)
                    group.AddSchema(s, false);
            }
            foreach (var t in types)
                group.AddSchema(t);

            if (!m_GroupAssets.Contains(group))
                groups.Add(group);

            if (setAsDefaultGroup)
                DefaultGroup = group;
            SetDirty(ModificationEvent.GroupAdded, group, postEvent, true);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(this);
            return group;
        }

        internal string FindUniqueGroupName(string potentialName)
        {
            var cleanedName = potentialName.Replace('/', '-');
            cleanedName = cleanedName.Replace('\\', '-');
            if (cleanedName != potentialName)
                Addressables.Log("Group names cannot include '\\' or '/'.  Replacing with '-'. " + cleanedName);
            var validName = cleanedName;
            int index = 1;
            bool foundExisting = true;
            while (foundExisting)
            {
                if (index > 1000)
                {
                    Addressables.LogError("Unable to create valid name for new Addressable Assets group.");
                    return cleanedName;
                }
                foundExisting = IsNotUniqueGroupName(validName);
                if (foundExisting)
                {
                    validName = cleanedName + index;
                    index++;
                }
            }

            return validName;
        }

        internal bool IsNotUniqueGroupName(string groupName)
        {
            bool foundExisting = false;
            foreach (var g in groups)
            {
                if (g != null && g.Name == groupName)
                {
                    foundExisting = true;
                    break;
                }
            }
            return foundExisting;
        }

        /// <summary>
        /// Remove an asset group.
        /// </summary>
        /// <param name="g"></param>
        public void RemoveGroup(AddressableAssetGroup g)
        {
            RemoveGroupInternal(g, true, true);
        }

        internal void RemoveGroupInternal(AddressableAssetGroup g, bool deleteAsset, bool postEvent)
        {
            g?.ClearSchemas(true);
            groups.Remove(g);
            SetDirty(ModificationEvent.GroupRemoved, g, postEvent, true);
            if (g != null && deleteAsset)
            {
                string guidOfGroup;
                long localId;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(g, out guidOfGroup, out localId))
                {
                    var groupPath = AssetDatabase.GUIDToAssetPath(guidOfGroup);
                    if (!string.IsNullOrEmpty(groupPath))
                        AssetDatabase.DeleteAsset(groupPath);
                }
            }
        }

        internal void SetLabelValueForEntries(List<AddressableAssetEntry> entries, string label, bool value, bool postEvent = true)
        {
            var addedNewLabel = value && m_LabelTable.AddLabelName(label);

            foreach (var e in entries)
            {
                e.SetLabel(label, value, false, false);
                AddressableAssetUtility.OpenAssetIfUsingVCIntegration(e.parentGroup);
            }

            SetDirty(ModificationEvent.EntryModified, entries, postEvent, addedNewLabel);
            AddressableAssetUtility.OpenAssetIfUsingVCIntegration(this);
        }

        internal void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<string> assetEntryCollections = new List<string>();
            var aa = this;
            bool relatedAssetChanged = false;
            bool settingsChanged = false;
            foreach (string str in importedAssets)
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(str);
                if (typeof(AddressableAssetSettings).IsAssignableFrom(assetType))
                {
                    var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(str);
                    if (settings != null)
                        settings.Validate();
                }
                if (typeof(AddressableAssetGroup).IsAssignableFrom(assetType))
                {
                    AddressableAssetGroup group = aa.FindGroup(Path.GetFileNameWithoutExtension(str));
                    if (group == null)
                    {
                        var foundGroup = AssetDatabase.LoadAssetAtPath<AddressableAssetGroup>(str);
                        if (!aa.groups.Contains(foundGroup))
                        {
                            aa.groups.Add(foundGroup);
                            group = aa.FindGroup(Path.GetFileNameWithoutExtension(str));
                            relatedAssetChanged = true;
                            settingsChanged = true;
                        }
                    }
                    if (group != null)
                        group.DedupeEnteries();
                }

#pragma warning disable 0618
                if (typeof(AddressableAssetEntryCollection).IsAssignableFrom(assetType))
                    assetEntryCollections.Add(str);
#pragma warning restore 0618

                var guid = AssetDatabase.AssetPathToGUID(str);
                if (aa.FindAssetEntry(guid) != null)
                    relatedAssetChanged = true;

                if (AddressableAssetUtility.IsInResources(str))
                    relatedAssetChanged = true;
            }

            if (assetEntryCollections.Count > 0)
                relatedAssetChanged = ConvertAssetEntryCollectionsWithPermissionRequest(assetEntryCollections) || relatedAssetChanged;

            if (deletedAssets.Length > 0)
            {
                // if any directly referenced assets were deleted while Unity was closed, the path isn't useful, so Remove(null) is our only option
                //  this can lead to orphaned schema files.
                if (groups.Remove(null) ||
                    DataBuilders.Remove(null) ||
                    GroupTemplateObjects.Remove(null) ||
                    InitializationObjects.Remove(null))
                {
                    relatedAssetChanged = true;
                }
            }

            foreach (string str in deletedAssets)
            {
                if (AddressableAssetUtility.IsInResources(str))
                    relatedAssetChanged = true;
                else
                {
                    if (CheckForGroupDataDeletion(str))
                    {
                        relatedAssetChanged = true;
                        settingsChanged = true;
                        continue;
                    }

                    var guidOfDeletedAsset = AssetDatabase.AssetPathToGUID(str);
                    if (aa.RemoveAssetEntry(guidOfDeletedAsset))
                    {
                        relatedAssetChanged = true;
                    }
                }
            }
            for (int i = 0; i < movedAssets.Length; i++)
            {
                var str = movedAssets[i];
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(str);
                if (typeof(AddressableAssetGroup).IsAssignableFrom(assetType))
                {
                    var oldGroupName = Path.GetFileNameWithoutExtension(movedFromAssetPaths[i]);
                    var group = aa.FindGroup(oldGroupName);
                    if (group != null)
                    {
                        var newGroupName = Path.GetFileNameWithoutExtension(str);
                        group.Name = newGroupName;
                        relatedAssetChanged = true;
                    }
                }
                else
                {
                    var guid = AssetDatabase.AssetPathToGUID(str);
                    AddressableAssetEntry entry = aa.FindAssetEntry(guid);

                    bool isAlreadyAddressable = entry != null;
                    bool startedInResources = AddressableAssetUtility.IsInResources(movedFromAssetPaths[i]);
                    bool endedInResources = AddressableAssetUtility.IsInResources(str);
                    bool inEditorSceneList = BuiltinSceneCache.Contains(new GUID(guid));

                    //update entry cached path
                    entry?.SetCachedPath(str);

                    //move to Resources
                    if (isAlreadyAddressable && endedInResources)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(str);
                        Addressables.Log("You have moved addressable asset " + fileName + " into a Resources directory.  It has been unmarked as addressable, but can still be loaded via the Addressables API via its Resources path.");
                        aa.RemoveAssetEntry(guid, false);
                    }
                    else if (inEditorSceneList)
                        BuiltinSceneCache.ClearState();

                    //any addressables move or resources move (even resources to within resources) needs to refresh the UI.
                    relatedAssetChanged = isAlreadyAddressable || startedInResources || endedInResources || inEditorSceneList;
                }
            }

            if (relatedAssetChanged || settingsChanged)
                aa.SetDirty(ModificationEvent.BatchModification, null, true, settingsChanged);
        }

#pragma warning disable 0618
        internal bool ConvertAssetEntryCollectionsWithPermissionRequest(List<string> assetEntryCollections)
        {
            if (assetEntryCollections == null || assetEntryCollections.Count == 0 || DenyEntryCollectionPermission)
                return false;

            bool allowConvertCollectionToEntries = EditorUtility.GetDialogOptOutDecision(DialogOptOutDecisionType.ForThisMachine, kImportAssetEntryCollectionOptOutKey);
            if (!allowConvertCollectionToEntries)
            {
                allowConvertCollectionToEntries = EditorUtility.DisplayDialog("AssetEntryCollection Found",
                    "AssetEntryCollection is obsolete, do you want create AddressableAssetEntries from the AssetEntryCollection in the Default Group and remove the AssetEntryCollection from the project?",
                    "Yes", "No",
                    DialogOptOutDecisionType.ForThisMachine, kImportAssetEntryCollectionOptOutKey);
            }
            return allowConvertCollectionToEntries ? ConvertAssetEntryCollections(assetEntryCollections) : false;
        }

        internal bool ConvertAssetEntryCollections(List<string> assetEntryCollections)
        {
            if (assetEntryCollections == null || assetEntryCollections.Count == 0)
                return false;

            bool changesMade = false;
            foreach (string collectionPath in assetEntryCollections)
            {
                var collection = AssetDatabase.LoadAssetAtPath<AddressableAssetEntryCollection>(collectionPath);
                if (collection == null)
                {
                    Debug.LogError("Could not load and convert AssetEntryCollection at " + collectionPath);
                    continue;
                }
                if (!AddressableAssetEntryCollection.ConvertEntryCollectionToEntries(collection, this))
                {
                    Debug.LogError("Failed to convert AssetEntryCollection to AddressableAssetEntries at " + collectionPath);
                    continue;
                }

                if (collectionPath.StartsWith("Assets"))
                {
                    if (!AssetDatabase.DeleteAsset(collectionPath))
                        Debug.LogError("Failed to Delete AssetEntryCollection at " + collectionPath);
                }
                else
                {
                    Debug.LogWarning($"Imported AssetEntryCollection is in a Package, deletion of Asset at {collectionPath} aborted.");
                }
                changesMade = true;
            }
            return changesMade;
        }
#pragma warning restore 0618

        internal bool CheckForGroupDataDeletion(string str)
        {
            if (string.IsNullOrEmpty(str))
                return false;

            bool modified = false;
            AddressableAssetGroup groupToDelete = null;
            bool deleteGroup = false;
            foreach (var group in groups)
            {
                if (group != null)
                {
                    if (AssetDatabase.GUIDToAssetPath(group.Guid) == str)
                    {
                        groupToDelete = group;
                        deleteGroup = true;
                        break;
                    }

                    if (group.Schemas.Remove(null))
                        modified = true;
                }
            }

            if (deleteGroup)
            {
                RemoveGroupInternal(groupToDelete, false, true);
                modified = true;
            }

            return modified;
        }

        /// <summary>
        /// Runs the active player data build script to create runtime data.
        /// See the [BuildPlayerContent](xref:addressables-api-build-player-content) documentation for more details.
        /// </summary>
        public static void BuildPlayerContent()
        {
            BuildPlayerContent(out AddressablesPlayerBuildResult rst);
        }

#if (ENABLE_CCD && UNITY_2019_4_OR_NEWER)
        /// <summary>
        /// Runs the active player data build script to create runtime data.
        /// Any groups referencing CCD group type will have the produced bundles uploaded to the specified non-promotion only bucket.
        /// See the [BuildPlayerContent](xref:addressables-api-build-player-content) documentation for more details.
        /// </summary>
        public async static Task<AddressableAssetBuildResult> BuildAndReleasePlayerContent()
        {
            AddressableAssetBuildResult result = null;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                string error;
                if (EditorApplication.isUpdating)
                    error = "Addressable Asset Settings does not exist.  EditorApplication.isUpdating was true.";
                else if (EditorApplication.isCompiling)
                    error = "Addressable Asset Settings does not exist.  EditorApplication.isCompiling was true.";
                else
                    error = "Addressable Asset Settings does not exist.  Failed to create.";
                Addressables.LogError(error);
                result = new AddressablesPlayerBuildResult();
                result.Error = error;
                return result;
            }

            NullifyBundleFileIds(settings);

            //Processing groups, checking for promotion buckets
            bool promotionOnly = GroupsContainPromotionOnlyBucket(settings);
            if (promotionOnly)
            {
                result = new AddressablesPlayerBuildResult();
                result.Error = "Cannot upload to Promotion Only bucket.";
                return result;
            }

            //Build the player content
            result = settings.BuildPlayerContentImpl();

            //Getting files
            Addressables.Log("Creating and uploading entries");
            var startDirectory = new DirectoryInfo(kCCDBuildDataPath);
            var buckets = CreateBucketData(startDirectory);


            //Creating a release for each bucket
            var projectId = CloudProjectSettings.projectId;
            await CCDManagementAPIService.SetConfigurationAuthHeader(CloudProjectSettings.accessToken);
            var httpClient = new HttpClient();
            var ccdEntryClient = new EntriesApiClient(httpClient);
            var ccdContentClient = new ContentApiClient(httpClient);
            var ccdReleaseClient = new ReleasesApiClient(httpClient);
            var ccdBadgesClient = new BadgesApiClient(httpClient);

            await CreateReleaseForBuckets(buckets, ccdEntryClient, projectId, ccdContentClient, ccdReleaseClient, ccdBadgesClient);

            return result;

        }

        static Dictionary<DirectoryInfo, Dictionary<DirectoryInfo, List<FileInfo>>> CreateBucketData(DirectoryInfo startDirectory)
        {
            var buckets = new Dictionary<DirectoryInfo, Dictionary<DirectoryInfo, List<FileInfo>>>();
            var bucketDirs = startDirectory.GetDirectories().Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden));
            foreach (var bucketDir in bucketDirs)
            {
                var badgeDirs = bucketDir.GetDirectories().Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden));
                foreach (var badgeDir in badgeDirs)
                {
                    var files = badgeDir.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)).ToList();
                    if (!buckets.ContainsKey(bucketDir))
                    {
                        var badges = new Dictionary<DirectoryInfo, List<FileInfo>>();
                        badges.Add(badgeDir, files);
                        buckets.Add(bucketDir, badges);
                    }
                    else
                    {
                        buckets.TryGetValue(bucketDir, out var badges);
                        if (!badges.ContainsKey(badgeDir))
                        {
                            badges.Add(badgeDir, files);
                        }
                        else
                        {
                            badges.TryGetValue(badgeDir, out var existingFiles);
                            existingFiles.AddRange(files);
                        }
                    }
                }
            }

            return buckets;
        }

        async static Task CreateReleaseForBuckets(Dictionary<DirectoryInfo, Dictionary<DirectoryInfo, List<FileInfo>>> buckets, EntriesApiClient ccdEntryClient, string projectId,
            ContentApiClient ccdContentClient, ReleasesApiClient ccdReleaseClient, BadgesApiClient ccdBadgesClient)
        {
            foreach (var bucketKvp in buckets)
            {
                string bucketId = bucketKvp.Key.Name;

                foreach (var badgeKvp in bucketKvp.Value)
                {
                    string badgeName = badgeKvp.Key.Name;

                    //Creating entries and uploading files
                    await AddressableAssetUtility.ParallelForEachAsync(badgeKvp.Value, 5, async (path) =>
                    {
                        string contentHash = AddressableAssetUtility.GetMd5Hash(path.FullName);
                        using (var stream = File.OpenRead(path.FullName))
                        {
                            var entryPath = path.Name;
                            var entry = new CcdEntryCreateByPath(contentHash, (int)stream.Length);
                            var createEntryRequest = new CreateOrUpdateEntryByPathRequest(bucketId, entryPath, projectId, entry, true);
                            var createdEntry = (await ccdEntryClient.CreateOrUpdateEntryByPathAsync(createEntryRequest)).Result;

                            var uploadContentRequest = new UploadContentRequest(bucketId, createdEntry.Entryid.ToString(), projectId, stream);

                            var response = await ccdContentClient.UploadContentAsync(uploadContentRequest, (prog, total) =>
                            {
                                Addressables.Log($"Uploaded {prog}/{total} of {entryPath}");
                            });
                            if (response.Status == (long)HttpStatusCode.NoContent)
                            {
                                Addressables.Log($"File: {createdEntry.Path} has been successfully uploaded");
                            }
                        }
                    });

                    //Creating release
                    Addressables.Log("Creating release.");
                    var createRelease = new CcdReleaseCreate();
                    var createReleaseRequest = new CreateReleaseRequest(bucketId, projectId, createRelease);
                    var release = (await ccdReleaseClient.CreateReleaseAsync(createReleaseRequest)).Result;
                    Addressables.Log($"Release {release.Releaseid} created.");

                    //Don't update latest badge (as it always updates)
                    if (badgeName != "latest")
                    {
                        //Updating badge
                        Addressables.Log("Updating badge.");
                        var updateBadge = new CcdBadgeAssign("TestBadge", release.Releaseid);
                        var updateBadgeRequest = new UpdateBadgeRequest(bucketId, projectId, updateBadge);
                        var badge = (await ccdBadgesClient.UpdateBadgeAsync(updateBadgeRequest)).Result;
                        Addressables.Log($"Badge {badge.Name} updated.");
                    }
                }
            }
        }

        /// <summary>
        /// Check if groups contain promotion only buckets.
        /// </summary>
        /// <param name="settings">The Settings to process</param>
        /// <returns>True if any group points to a promotion only bucket.</returns>
        internal static bool GroupsContainPromotionOnlyBucket(AddressableAssetSettings settings)
        {
            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                    continue;

                var schema = group.GetSchema<BundledAssetGroupSchema>();
                if (schema != null)
                {
                    var buildPath = schema.BuildPath.GetValue(settings);
                    var loadPath = schema.LoadPath.GetValue(settings);
                    var groupType = new ProfileGroupType("temp");
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable(kBuildPath, buildPath));
                    groupType.AddVariable(new ProfileGroupType.GroupTypeVariable(kLoadPath, loadPath));
                    var foundGroupType = ProfileDataSourceSettings.GetSettings().FindGroupType(groupType);
                    if (foundGroupType != null && foundGroupType.GroupTypePrefix.StartsWith("CCD"))
                    {
                        if (bool.Parse(foundGroupType.GetVariableBySuffix(nameof(CcdBucket.Attributes.PromoteOnly)).Value) == true)
                        {
                            string error = "Cannot upload to Promotion Only bucket.";
                            Addressables.LogError(error);
                            return true;
                        }
                    }

                }
            }

            return false;
        }
#endif

        /// <summary>
        /// Runs the active player data build script to create runtime data.
        /// See the [BuildPlayerContent](xref:addressables-api-build-player-content) documentation for more details.
        /// </summary>
        /// <param name="result">Results from running the active player data build script.</param>
        public static void BuildPlayerContent(out AddressablesPlayerBuildResult result)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                string error;
                if (EditorApplication.isUpdating)
                    error = "Addressable Asset Settings does not exist.  EditorApplication.isUpdating was true.";
                else if (EditorApplication.isCompiling)
                    error = "Addressable Asset Settings does not exist.  EditorApplication.isCompiling was true.";
                else
                    error = "Addressable Asset Settings does not exist.  Failed to create.";
                Debug.LogError(error);
                result = new AddressablesPlayerBuildResult();
                result.Error = error;
                return;
            }

            NullifyBundleFileIds(settings);

            result = settings.BuildPlayerContentImpl();
        }

        internal static void NullifyBundleFileIds(AddressableAssetSettings settings)
        {
            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (group == null)
                    continue;
                foreach (AddressableAssetEntry entry in group.entries)
                    entry.BundleFileId = null;
            }
        }

        internal AddressablesPlayerBuildResult BuildPlayerContentImpl()
        {
            if (Directory.Exists(Addressables.BuildPath))
            {
                try
                {
                    Directory.Delete(Addressables.BuildPath, true);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var buildContext = new AddressablesDataBuilderInput(this);
            var result = ActivePlayerDataBuilder.BuildData<AddressablesPlayerBuildResult>(buildContext);
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError(result.Error);
                Debug.LogError($"Addressable content build failure (duration : {TimeSpan.FromSeconds(result.Duration).ToString("g")})");
            }
            else
                Debug.Log($"Addressable content successfully built (duration : {TimeSpan.FromSeconds(result.Duration).ToString("g")})");
            AddressableAnalytics.Report(this);
            if (BuildScript.buildCompleted != null)
                BuildScript.buildCompleted(result);
            AssetDatabase.Refresh();
            return result;
        }

        /// <summary>
        /// Deletes all created runtime data for the active player data builder.
        /// </summary>
        /// <param name="builder">The builder to call ClearCachedData on.  If null, all builders will be cleaned</param>
        public static void CleanPlayerContent(IDataBuilder builder = null)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                if (EditorApplication.isUpdating)
                    Debug.LogError("Addressable Asset Settings does not exist.  EditorApplication.isUpdating was true.");
                else if (EditorApplication.isCompiling)
                    Debug.LogError("Addressable Asset Settings does not exist.  EditorApplication.isCompiling was true.");
                else
                    Debug.LogError("Addressable Asset Settings does not exist.  Failed to create.");
                return;
            }
            settings.CleanPlayerContentImpl(builder);
        }

        internal void CleanPlayerContentImpl(IDataBuilder builder = null)
        {
            if (builder != null)
            {
                builder.ClearCachedData();
            }
            else
            {
                for (int i = 0; i < DataBuilders.Count; i++)
                {
                    var m = GetDataBuilder(i);
                    m.ClearCachedData();
                }
            }
            AssetDatabase.Refresh();
        }

        internal AsyncOperationHandle<IResourceLocator> CreatePlayModeInitializationOperation(AddressablesImpl addressables)
        {
            return addressables.ResourceManager.StartOperation(new FastModeInitializationOperation(addressables, this), default);
        }

        static Dictionary<string, Action<IEnumerable<AddressableAssetEntry>>> s_CustomAssetEntryCommands = new Dictionary<string, Action<IEnumerable<AddressableAssetEntry>>>();
        /// <summary>
        /// Register a custom command to process asset entries.  These commands will be shown in the context menu of the groups window.
        /// </summary>
        /// <param name="cmdId">The id of the command.  This will be used for the display name of the context menu item.</param>
        /// <param name="cmdFunc">The command handler function.</param>
        /// <returns>Returns true if the command was registered.</returns>
        public static bool RegisterCustomAssetEntryCommand(string cmdId, Action<IEnumerable<AddressableAssetEntry>> cmdFunc)
        {
            if (string.IsNullOrEmpty(cmdId))
            {
                Debug.LogError("RegisterCustomAssetEntryCommand - invalid command id.");
                return false;
            }
            if (cmdFunc == null)
            {
                Debug.LogError($"RegisterCustomAssetEntryCommand - command functor for id '{cmdId}'.");
                return false;
            }
            s_CustomAssetEntryCommands[cmdId] = cmdFunc;
            return true;
        }

        /// <summary>
        /// Removes a registered custom entry command.
        /// </summary>
        /// <param name="cmdId">The command id.</param>
        /// <returns>Returns true if the command was removed.</returns>
        public static bool UnregisterCustomAssetEntryCommand(string cmdId)
        {
            if (string.IsNullOrEmpty(cmdId))
            {
                Debug.LogError("UnregisterCustomAssetEntryCommand - invalid command id.");
                return false;
            }

            if (!s_CustomAssetEntryCommands.Remove(cmdId))
            {
                Debug.LogError($"UnregisterCustomAssetEntryCommand - command id '{cmdId}' is not registered.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Invoke a registered command for a set of entries.
        /// </summary>
        /// <param name="cmdId">The id of the command.</param>
        /// <param name="entries">The entries to run the command on.</param>
        /// <returns>Returns true if the command was executed without exceptions.</returns>
        public static bool InvokeAssetEntryCommand(string cmdId, IEnumerable<AddressableAssetEntry> entries)
        {
            try
            {
                if (string.IsNullOrEmpty(cmdId) || !s_CustomAssetEntryCommands.ContainsKey(cmdId))
                {
                    Debug.LogError($"Asset Entry Command '{cmdId}' not found.  Ensure that it is registered by calling RegisterCustomAssetEntryCommand.");
                    return false;
                }
                if (entries == null)
                {
                    Debug.LogError($"Asset Entry Command '{cmdId}' called with null entry collection.");
                    return false;
                }
                s_CustomAssetEntryCommands[cmdId](entries);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Encountered exception when running Asset Entry Command '{cmdId}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// The ids of the registered commands.
        /// </summary>
        public static IEnumerable<string> CustomAssetEntryCommands => s_CustomAssetEntryCommands.Keys;

        static Dictionary<string, Action<IEnumerable<AddressableAssetGroup>>> s_CustomAssetGroupCommands = new Dictionary<string, Action<IEnumerable<AddressableAssetGroup>>>();
        /// <summary>
        /// Register a custom command to process asset groups.  These commands will be shown in the context menu of the groups window.
        /// </summary>
        /// <param name="cmdId">The id of the command.  This will be used for the display name of the context menu item.</param>
        /// <param name="cmdFunc">The command handler function.</param>
        /// <returns>Returns true if the command was registered.</returns>
        public static bool RegisterCustomAssetGroupCommand(string cmdId, Action<IEnumerable<AddressableAssetGroup>> cmdFunc)
        {
            if (string.IsNullOrEmpty(cmdId))
            {
                Debug.LogError("RegisterCustomAssetGroupCommand - invalid command id.");
                return false;
            }
            if (cmdFunc == null)
            {
                Debug.LogError($"RegisterCustomAssetGroupCommand - command functor for id '{cmdId}'.");
                return false;
            }
            s_CustomAssetGroupCommands[cmdId] = cmdFunc;
            return true;
        }

        /// <summary>
        /// Removes a registered custom group command.
        /// </summary>
        /// <param name="cmdId">The command id.</param>
        /// <returns>Returns true if the command was removed.</returns>
        public static bool UnregisterCustomAssetGroupCommand(string cmdId)
        {
            if (string.IsNullOrEmpty(cmdId))
            {
                Debug.LogError("UnregisterCustomAssetGroupCommand - invalid command id.");
                return false;
            }
            if (!s_CustomAssetGroupCommands.Remove(cmdId))
            {
                Debug.LogError($"UnregisterCustomAssetGroupCommand - command id '{cmdId}' is not registered.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Invoke a registered command for a set of groups.
        /// </summary>
        /// <param name="cmdId">The id of the command.</param>
        /// <param name="groups">The groups to run the command on.</param>
        /// <returns>Returns true if the command was invoked successfully.</returns>
        public static bool InvokeAssetGroupCommand(string cmdId, IEnumerable<AddressableAssetGroup> groups)
        {
            try
            {
                if (string.IsNullOrEmpty(cmdId) || !s_CustomAssetGroupCommands.ContainsKey(cmdId))
                {
                    Debug.LogError($"Asset Group Command '{cmdId}' not found.  Ensure that it is registered by calling RegisterCustomAssetGroupCommand.");
                    return false;
                }
                if (groups == null)
                {
                    Debug.LogError($"Asset Group Command '{cmdId}' called with null group collection.");
                    return false;
                }
                s_CustomAssetGroupCommands[cmdId](groups);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Encountered exception when running Asset Group Command '{cmdId}': {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// The ids of the registered commands.
        /// </summary>
        public static IEnumerable<string> CustomAssetGroupCommands => s_CustomAssetGroupCommands.Keys;
    }
}
