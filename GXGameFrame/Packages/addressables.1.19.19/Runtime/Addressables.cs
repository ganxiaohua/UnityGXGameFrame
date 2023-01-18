using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AddressableAssets
{
    /// <summary>
    /// Exception to encapsulate invalid key errors.
    /// </summary>
    public class InvalidKeyException : Exception
    {
        /// <summary>
        /// The key used to generate the exception.
        /// </summary>
        public object Key { get; private set; }

        /// <summary>
        /// The type of the key used to generate the exception.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// MergeMode if used, else null.
        /// </summary>
        public Addressables.MergeMode? MergeMode { get; }

        /// <summary>
        /// Construct a new InvalidKeyException.
        /// </summary>
        /// <param name="key">The key that caused the exception.</param>
        public InvalidKeyException(object key) : this(key, typeof(object)) {}

        private AddressablesImpl m_Addressables;

        /// <summary>
        /// Construct a new InvalidKeyException.
        /// </summary>
        /// <param name="key">The key that caused the exception.</param>
        /// <param name="type">The type of the key that caused the exception.</param>
        public InvalidKeyException(object key, Type type)
        {
            Key = key;
            Type = type;
        }
        
        internal InvalidKeyException(object key, Type type, AddressablesImpl addr)
        {
            Key = key;
            Type = type;
            m_Addressables = addr;
        }

        /// <summary>
        /// Construct a new InvalidKeyException.
        /// </summary>
        /// <param name="key">The key that caused the exception.</param>
        /// <param name="type">The type of the key that caused the exception.</param>
        /// <param name="mergeMode">The mergeMode of the input that caused the exception.</param>
        public InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode)
        {
            Key = key;
            Type = type;
            MergeMode = mergeMode;
        }
        
        internal InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode, AddressablesImpl addr)
        {
            Key = key;
            Type = type;
            MergeMode = mergeMode;
            m_Addressables = addr;
        }

        ///<inheritdoc cref="InvalidKeyException"/>
        public InvalidKeyException() {}

        ///<inheritdoc/>
        public InvalidKeyException(string message) : base(message) {}

        ///<inheritdoc/>
        public InvalidKeyException(string message, Exception innerException) : base(message, innerException) {}

        ///<inheritdoc/>
        protected InvalidKeyException(SerializationInfo message, StreamingContext context) : base(message, context) {}

        const string BaseInvalidKeyMessageFormat = "{0}, Key={1}, Type={2}";

        /// <summary>
        /// Stores information about the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                string stringKey = Key as string;
                if (!string.IsNullOrEmpty(stringKey))
                {
                    if (m_Addressables == null)
                        return string.Format(BaseInvalidKeyMessageFormat, base.Message, stringKey, Type);
                    return GetMessageForSingleKey(stringKey);
                }

                IEnumerable enumerableKey = Key as IEnumerable;
                if (enumerableKey != null)
                {
                    int keyCount = 0;
                    List<string> stringKeys = new List<string>();
                    HashSet<string> keyTypeNames = new HashSet<string>();
                    foreach (object keyObj in enumerableKey)
                    {
                        keyCount++;
                        keyTypeNames.Add(keyObj.GetType().ToString());
                        if (keyObj is string)
                            stringKeys.Add(keyObj as string);
                    }

                    if (!MergeMode.HasValue)
                    {
                        string keysCSV = GetCSVString(stringKeys, "Key=", "Keys=");
                        return $"{base.Message} No MergeMode is set to merge the multiple keys requested. {keysCSV}, Type={Type}";
                    }
                    if (keyCount != stringKeys.Count)
                    {
                        string types = GetCSVString(keyTypeNames, "Type=", "Types=");
                        return $"{base.Message} Enumerable key contains multiple Types. {types}, all Keys are expected to be strings";
                    }
                    if (keyCount == 1)
                        return GetMessageForSingleKey(stringKeys[0]);
                    return GetMessageforMergeKeys(stringKeys);
                }

                return string.Format(BaseInvalidKeyMessageFormat, base.Message, Key, Type);
            }
        }

        string GetMessageForSingleKey(string keyString)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GUIDToAssetPath(keyString);
            if (!string.IsNullOrEmpty(path))
            {
                Type directType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (directType != null)
                    return $"{base.Message} Could not load Asset with GUID={keyString}, Path={path}. Asset exists with main Type={directType}, which is not assignable from the requested Type={Type}";
                return string.Format(BaseInvalidKeyMessageFormat, base.Message, keyString, Type);
            }
#endif

            HashSet<Type> typesAvailableForKey = GetTypesForKey(keyString);
            if (typesAvailableForKey.Count == 0)
                return $"{base.Message} No Location found for Key={keyString}";
            
            if (typesAvailableForKey.Count == 1)
            {
                Type availableType = null;
                foreach (Type type in typesAvailableForKey)
                    availableType = type;
                if (availableType == null)
                    return string.Format(BaseInvalidKeyMessageFormat, base.Message, keyString, Type);
                return $"{base.Message} No Asset found with for Key={keyString}. Key exists as Type={availableType}, which is not assignable from the requested Type={Type}";
            }
            
            StringBuilder csv = new StringBuilder(512);
            int count = 0;
            foreach (Type type in typesAvailableForKey)
            {
                count++;
                csv.Append(count > 1 ? $", {type}" : type.ToString());
            }

            return $"{base.Message} No Asset found with for Key={keyString}. Key exists as multiple Types={csv}, which is not assignable from the requested Type={Type}";
        }

        string GetMessageforMergeKeys(List<string> keys)
        {
            string keysCSV = GetCSVString(keys, "Key=", "Keys=");
            string NoLocationLineMessage = "\nNo Location found for Key={0}";
            StringBuilder messageBuilder = null;
            switch (MergeMode)
            {
                case Addressables.MergeMode.Union:
                {
                    messageBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} of Assets between {keysCSV} with Type={Type}");
                    
                    Dictionary<Type, List<string>> typeToKeys = new Dictionary<Type, List<string>>();
                    foreach (string key in keys)
                    {
                        if (!GetTypeToKeys(key, typeToKeys))
                            messageBuilder.Append(string.Format(NoLocationLineMessage, key));
                    }

                    foreach (KeyValuePair<Type, List<string>> pair in typeToKeys)
                    {
                        string availableKeysString = GetCSVString(pair.Value, "Key=", "Keys=");
                        List<string> unavailableKeys = new List<string>();
                        foreach (string key in keys)
                        {
                            if (!pair.Value.Contains(key))
                                unavailableKeys.Add(key);
                        }

                        if (unavailableKeys.Count == 0)
                            messageBuilder.Append($"\nUnion of Type={pair.Key} found with {availableKeysString}");
                        else
                        {
                            string unavailableKeysString = GetCSVString(unavailableKeys, "Key=", "Keys=");
                            messageBuilder.Append($"\nUnion of Type={pair.Key} found with {availableKeysString}. Without {unavailableKeysString}");
                        }
                    }
                }
                    break;

                case Addressables.MergeMode.Intersection:
                {
                    messageBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} of Assets between {keysCSV} with Type={Type}");

                    bool hasInvalidKeys = false;
                    Dictionary<Type, List<string>> typeToKeys = new Dictionary<Type, List<string>>();
                    foreach (string key in keys)
                    {
                        if (!GetTypeToKeys(key, typeToKeys))
                        {
                            hasInvalidKeys = true;
                            messageBuilder.Append(string.Format(NoLocationLineMessage, key));
                        }
                    }
                    if (hasInvalidKeys)
                        break;
                    
                    foreach (KeyValuePair<Type,List<string>> pair in typeToKeys)
                    {
                        if (pair.Value.Count == keys.Count)
                            messageBuilder.Append($"\nAn Intersection exists for Type={pair.Key}");
                    }
                }
                    break;
                
                case Addressables.MergeMode.UseFirst:
                {
                    messageBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} Asset within {keysCSV} with Type={Type}");

                    Dictionary<Type, List<string>> typeToKeys = new Dictionary<Type, List<string>>();
                    foreach (string key in keys)
                    {
                        if (!GetTypeToKeys(key, typeToKeys))
                            messageBuilder.Append(string.Format(NoLocationLineMessage, key));
                    }

                    string keyCSV;
                    foreach (KeyValuePair<Type,List<string>> pair in typeToKeys)
                    {
                        keyCSV = GetCSVString(pair.Value, "Key=", "Keys=");
                        messageBuilder.Append($"\nType={pair.Key} exists for {keyCSV}");
                    }
                }
                    break;
            }
            return messageBuilder.ToString();
        }
        
        HashSet<Type> GetTypesForKey(string keyString)
        {
            HashSet<Type> typesAvailableForKey = new HashSet<Type>();
            foreach (var locator in  m_Addressables.ResourceLocators)
            {
                if (!locator.Locate(keyString, null, out var locations))
                    continue;
                        
                foreach (IResourceLocation location in locations)
                    typesAvailableForKey.Add(location.ResourceType);
            }

            return typesAvailableForKey;
        }
        
        bool GetTypeToKeys(string key, Dictionary<Type, List<string>> typeToKeys)
        {
            HashSet<Type> types = GetTypesForKey(key);
            if (types.Count == 0)
                return false;

            foreach (Type type in types)
            {
                if (!typeToKeys.TryGetValue(type, out List<string> keysForType))
                    typeToKeys.Add(type, new List<string>() {key});
                else
                    keysForType.Add(key);
            }
            return true;
        }
        
        string GetCSVString(IEnumerable<string> enumerator, string prefixSingle, string prefixPlural)
        {
            StringBuilder keysCSVBuilder = new StringBuilder(prefixPlural);
            int count = 0;
            foreach (var key in enumerator)
            {
                count++;
                keysCSVBuilder.Append(count > 1 ? $", {key}" : key);
            }
            if (count == 1 && !string.IsNullOrEmpty(prefixPlural) && !string.IsNullOrEmpty(prefixSingle))
                keysCSVBuilder.Replace(prefixPlural, prefixSingle);
            return keysCSVBuilder.ToString();
        }
    }

    /// <summary>
    /// Entry point for Addressable API, this provides a simpler interface than using ResourceManager directly as it assumes string address type.
    /// </summary>
    public static class Addressables
    {
        internal static bool reinitializeAddressables = true;
        internal static AddressablesImpl m_AddressablesInstance = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
        static AddressablesImpl m_Addressables
        {
            get
            {
#if UNITY_EDITOR
                if (EditorSettings.enterPlayModeOptionsEnabled && reinitializeAddressables)
                {
                    reinitializeAddressables = false;
                    m_AddressablesInstance.ReleaseSceneManagerOperation();
                    m_AddressablesInstance = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
                }
#endif
                return m_AddressablesInstance;
            }
        }
        /// <summary>
        /// Stores the ResourceManager associated with this Addressables instance.
        /// </summary>
        public static ResourceManager ResourceManager { get { return m_Addressables.ResourceManager; } }
        internal static AddressablesImpl Instance { get { return m_Addressables; } }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void RegisterPlayModeStateChange()
        {
            EditorApplication.playModeStateChanged += SetAddressablesReInitFlagOnExitPlayMode;
        }

        static void SetAddressablesReInitFlagOnExitPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode || change == PlayModeStateChange.ExitingPlayMode)
                reinitializeAddressables = true;
        }

#endif

        /// <summary>
        /// The Instance Provider used by the Addressables System.
        /// </summary>
        public static IInstanceProvider InstanceProvider { get { return m_Addressables.InstanceProvider; } }

        /// <summary>
        /// Used to resolve a string using addressables config values
        /// </summary>
        /// <param name="id">The internal id to resolve.</param>
        /// <returns>Returns the string that the internal id represents.</returns>
        public static string ResolveInternalId(string id)
        {
            return m_Addressables.ResolveInternalId(id);
        }

        /// <summary>
        /// Functor to transform internal ids before being used by the providers.
        /// </summary>
        /// <remarks>
        /// The <see cref="ResourceManager"/> calls your transorm function when it looks up an asset,
        /// passing the <see cref="IResourceLocation"/> instance for the asset to your function.
        /// You can change the <see cref="IResourceLocation.InternalId"/> property of this instance
        /// and return the modified object as the return value of your function.
        ///
        /// See also: [Transforming resource URLs](xref:addressables-api-transform-internal-id)
        /// </remarks>
        static public Func<IResourceLocation, string> InternalIdTransformFunc
        {
            get { return m_Addressables.InternalIdTransformFunc; }
            set { m_Addressables.InternalIdTransformFunc = value; }
        }

        /// <summary>
        /// Delegate that can be used to override the web request options before being sent.
        /// </summary>
        /// <remarks>
        /// The web request passed to this delegate has already been preconfigured internally. Override at your own risk.
        /// </remarks>
        public static Action<UnityWebRequest> WebRequestOverride
        {
            get { return m_Addressables.WebRequestOverride; }
            set { m_Addressables.WebRequestOverride = value; }
        }

        /// <summary>
        /// Options for merging the results of requests.
        /// If keys (A, B) mapped to results ([1,2,4],[3,4,5])...
        ///  - UseFirst (or None) takes the results from the first key
        ///  -- [1,2,4]
        ///  - Union takes results of each key and collects items that matched any key.
        ///  -- [1,2,3,4,5]
        ///  - Intersection takes results of each key, and collects items that matched every key.
        ///  -- [4]
        /// </summary>
        public enum MergeMode
        {
            /// <summary>
            /// Use to indicate that no merge should occur. The first set of results will be used.
            /// </summary>
            None = 0,
            /// <summary>
            /// Use to indicate that the merge should take the first set of results.
            /// </summary>
            UseFirst = 0,
            /// <summary>
            /// Use to indicate that the merge should take the union of the results.
            /// </summary>
            Union,
            /// <summary>
            /// Use to indicate that the merge should take the intersection of the results.
            /// </summary>
            Intersection
        }

        /// <summary>
        /// The name of the PlayerPrefs value used to set the path to load the addressables runtime data file.
        /// </summary>
        public const string kAddressablesRuntimeDataPath = "AddressablesRuntimeDataPath";
        const string k_AddressablesLogConditional = "ADDRESSABLES_LOG_ALL";

        /// <summary>
        /// The name of the PlayerPrefs value used to set the path to check for build logs that need to be shown in the runtime.
        /// </summary>
        public const string kAddressablesRuntimeBuildLogPath = "AddressablesRuntimeBuildLog";

        /// <summary>
        /// The subfolder used by the Addressables system for its initialization data.
        /// </summary>
        public static string StreamingAssetsSubFolder { get { return m_Addressables.StreamingAssetsSubFolder; } }

        /// <summary>
        /// The path to the Addressables Library subfolder
        /// </summary>
        public static string LibraryPath = "Library/com.unity.addressables/";

        /// <summary>
        /// The path used by the Addressables system for its initialization data.
        /// </summary>
        public static string BuildPath { get { return m_Addressables.BuildPath; } }

        /// <summary>
        /// The path that addressables player data gets copied to during a player build.
        /// </summary>
        public static string PlayerBuildDataPath { get { return m_Addressables.PlayerBuildDataPath; } }

        /// <summary>
        /// The path used by the Addressables system to load initialization data.
        /// </summary>
        public static string RuntimePath { get { return m_Addressables.RuntimePath; } }


        /// <summary>
        /// Gets the collection of configured <see cref="IResourceLocator"/> objects. Resource Locators are used to find <see cref="IResourceLocation"/> objects from user-defined typed keys.
        /// </summary>
        /// <value>The resource locators collection.</value>
        public static IEnumerable<IResourceLocator> ResourceLocators { get { return m_Addressables.ResourceLocators; } }

        [Conditional(k_AddressablesLogConditional)]
        internal static void InternalSafeSerializationLog(string msg, LogType logType = LogType.Log)
        {
            if (m_AddressablesInstance == null)
                return;
            switch (logType)
            {
                case LogType.Warning:
                    m_AddressablesInstance.LogWarning(msg);
                    break;
                case LogType.Error:
                    m_AddressablesInstance.LogError(msg);
                    break;
                case LogType.Log:
                    m_AddressablesInstance.Log(msg);
                    break;
            }
        }

        [Conditional(k_AddressablesLogConditional)]
        internal static void InternalSafeSerializationLogFormat(string format, LogType logType = LogType.Log, params object[] args)
        {
            if (m_AddressablesInstance == null)
                return;
            switch (logType)
            {
                case LogType.Warning:
                    m_AddressablesInstance.LogWarningFormat(format, args);
                    break;
                case LogType.Error:
                    m_AddressablesInstance.LogErrorFormat(format, args);
                    break;
                case LogType.Log:
                    m_AddressablesInstance.LogFormat(format, args);
                    break;
            }
        }

        /// <summary>
        /// Debug.Log wrapper method that is contional on the ADDRESSABLES_LOG_ALL symbol definition.  This can be set in the Player preferences in the 'Scripting Define Symbols'.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        [Conditional(k_AddressablesLogConditional)]
        public static void Log(string msg)
        {
            m_Addressables.Log(msg);
        }

        /// <summary>
        /// Debug.LogFormat wrapper method that is contional on the ADDRESSABLES_LOG_ALL symbol definition.  This can be set in the Player preferences in the 'Scripting Define Symbols'.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        [Conditional(k_AddressablesLogConditional)]
        public static void LogFormat(string format, params object[] args)
        {
            m_Addressables.LogFormat(format, args);
        }

        /// <summary>
        /// Debug.LogWarning wrapper method.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        public static void LogWarning(string msg)
        {
            m_Addressables.LogWarning(msg);
        }

        /// <summary>
        /// Debug.LogWarningFormat wrapper method.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        public static void LogWarningFormat(string format, params object[] args)
        {
            m_Addressables.LogWarningFormat(format, args);
        }

        /// <summary>
        /// Debug.LogError wrapper method.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        public static void LogError(string msg)
        {
            m_Addressables.LogError(msg);
        }

        /// <summary>
        /// Debug.LogException wrapper method.
        /// </summary>
        /// <param name="op">The operation handle.</param>
        /// <param name="ex">The exception.</param>
        public static void LogException(AsyncOperationHandle op, Exception ex)
        {
            m_Addressables.LogException(op, ex);
        }

        /// <summary>
        /// Debug.LogException wrapper method.
        /// </summary>
        /// <param name="ex">The exception.</param>
        public static void LogException(Exception ex)
        {
            m_Addressables.LogException(ex);
        }

        /// <summary>
        /// Debug.LogErrorFormat wrapper method.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
            m_Addressables.LogErrorFormat(format, args);
        }

        /// <summary>
        /// Initialize Addressables system.  Addressables will be initialized on the first API call if this is not called explicitly.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InitializeAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> Initialize()
        {
            return InitializeAsync();
        }

        /// <summary>
        /// Initialize the Addressables system, if needed.
        /// </summary>
        /// <remarks>
        /// The Addressables system initializes itself at runtime the first time you call an Addressables API function.
        /// You can call this function explicitly to initialize Addressables earlier. This function does nothing if
        /// initialization has already occurred.
        ///
        /// The initialization process:
        /// * Sets up the <see cref="ResourceManager"/> and <see cref="ResourceLocators"/>
        /// * Loads the <see cref="Initialization.ResourceManagerRuntimeData"/> object, which is created by the Addressables build
        /// * Executes <see cref="IInitializableObject"/> operations
        /// * Optionally, checks for an updated content catalog (`true` by default)
        /// * Loads the content catalog
        ///
        /// The `Result` object contained in the <see cref="AsyncOperationHandle{TObject}"/> returned by this function
        /// contains a list of Addressable keys and a method that can be used to gather the <see cref="IResourceLocation"/>
        /// instances for a given key and asset type. You must access the `Result` object in a <see cref="AsyncOperationHandle{TObject}.Completed"/>
        /// event handler. To access the handle in a coroutine or Task-based function, pass `false` to the
        /// <see cref="InitializeAsync(bool)"/> overload of this function. Otherwise, the Addressables system
        /// releases the <see cref="AsyncOperationHandle{TObject}"/> object before control returns to your code.
        ///
        /// Initializing Addressables manually can improve performance of your first loading operations since they do not
        /// need to wait for initialization to complete. In addition, it can help when debugging early loading operations
        /// by separating out the initialization process.
        ///
        /// See also:
        /// * [Customizing initialization](xref:addressables-api-initialize-async)
        /// * [Managing catalogs at runtime](xref:addressables-api-load-content-catalog-async)
        /// </remarks>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> InitializeAsync()
        {
            return m_Addressables.InitializeAsync();
        }

        /// <summary>
        /// Initialize the Addressables system, if needed.
        /// </summary>
        /// <remarks>
        /// The Addressables system initializes itself at runtime the first time you call an Addressables API function.
        /// You can call this function explicitly to initialize Addressables earlier. This function does nothing if
        /// initialization has already occurred.
        ///
        /// The initialization process:
        /// * Sets up the <see cref="ResourceManager"/> and <see cref="ResourceLocators"/>
        /// * Loads the <see cref="Initialization.ResourceManagerRuntimeData"/> object, which is created by the Addressables build
        /// * Executes <see cref="IInitializableObject"/> operations
        /// * Optionally, checks for an updated content catalog (`true` by default)
        /// * Loads the content catalog
        ///
        /// The `Result` object contained in the <see cref="AsyncOperationHandle{TObject}"/> returned by this function
        /// contains a list of Addressable keys and a method that can be used to gather the <see cref="IResourceLocation"/>
        /// instances for a given key and asset type. To access the `Result` object, use a <see cref="AsyncOperationHandle{TObject}.Completed"/>
        /// event handler or set <paramref name="autoReleaseHandle"/> to `false`.
        ///
        /// Initializing Addressables manually can improve performance of your first loading operations since they do not
        /// need to wait for initialization to complete. In addition, it can help when debugging early loading operations
        /// by separating out the initialization process.
        ///
        /// See also:
        /// * [Customizing initialization](xref:addressables-api-initialize-async)
        /// * [Managing catalogs at runtime](xref:addressables-api-load-content-catalog-async)
        /// </remarks>
        /// <param name="autoReleaseHandle">If true, the handle is automatically released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> InitializeAsync(bool autoReleaseHandle)
        {
            return m_Addressables.InitializeAsync(autoReleaseHandle);
        }

        /// <summary>
        /// Additively load catalogs from runtime data.  The settings are not used.
        /// </summary>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadContentCatalogAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalog(string catalogPath, string providerSuffix = null)
        {
            return LoadContentCatalogAsync(catalogPath, providerSuffix);
        }

        /// <summary>
        /// Additively load catalogs from runtime data.
        /// </summary>
        /// <remarks>
        /// You can cache content catalog by providing the hash file created for the catalog by the Addressables content build
        /// at the same URL as the catalog JSON file. The Addressables system uses this hash file to determine if the cached catalog
        /// needs to be updated. If the value in the hash file has not changed since the last time you loaded the same catalog,
        /// this function loads the cached version instead of downloading the catalog. If the hash value has changed or if no
        /// hash file is provided, Addressables downloads the catalog from the specified path before loading it into memory.
        ///
        /// See also: [Managing catalogs at runtime](xref:addressables-api-load-content-catalog-async)
        /// </remarks>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, string providerSuffix = null)
        {
            return m_Addressables.LoadContentCatalogAsync(catalogPath, false, providerSuffix);
        }

        /// <summary>
        /// Additively load catalogs from runtime data.
        /// </summary>
        /// <remarks>
        /// You can cache content catalog by providing the hash file created for the catalog by the Addressables content build
        /// at the same URL as the catalog JSON file. The Addressables system uses this hash file to determine if the cached catalog
        /// needs to be updated. If the value in the hash file has not changed since the last time you loaded the same catalog,
        /// this function loads the cached version instead of downloading the catalog. If the hash value has changed or if no
        /// hash file is provided, Addressables downloads the catalog from the specified path before loading it into memory.
        ///
        /// See also: [Managing catalogs at runtime](xref:addressables-api-load-content-catalog-async)
        /// </remarks>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="autoReleaseHandle">If true, the async operation handle will be automatically released on completion. Typically,
        /// there is no reason to hold on to the handle for this operation.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, bool autoReleaseHandle, string providerSuffix = null)
        {
            return m_Addressables.LoadContentCatalogAsync(catalogPath, autoReleaseHandle, providerSuffix);
        }

        /// <summary>
        /// Initialization operation.  You can register a callback with this if you need to run code after Addressables is ready.  Any requests made before this operaton completes will automatically cahin to its result.
        /// </summary>
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> InitializationOperation => default;

        /// <summary>
        /// Load a single asset
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="location">The location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<TObject> LoadAsset<TObject>(IResourceLocation location)
        {
            return LoadAssetAsync<TObject>(location);
        }

        /// <summary>
        /// Load a single asset
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<TObject> LoadAsset<TObject>(object key)
        {
            return LoadAssetAsync<TObject>(key);
        }

        /// <summary>
        /// Loads a single Addressable asset identified by an <see cref="IResourceLocation"/>.
        /// </summary>
        /// <remarks>
        /// When you load an Addressable asset, the system:
        /// * Gathers the asset's dependencies
        /// * Downloads any remote AssetBundles needed to load the asset or its dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function.
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// See [Loading Addressable Assets](xref:addressables-api-load-asset-async) for more information and examples.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="location">The location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        public static AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(IResourceLocation location)
        {
            return m_Addressables.LoadAssetAsync<TObject>(location);
        }

        /// <summary>
        /// Loads a single Addressable asset identified by a key such as an address or label.
        /// </summary>
        /// <remarks>
        /// When you load an Addressable asset, the system:
        /// * Gathers the asset's dependencies
        /// * Downloads any remote AssetBundles needed to load the asset or its dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function.
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// Note that if you provide a key, such as a label, that maps to more than one asset, only the first object encountered by the
        /// loading operation is returned. Use <see cref="LoadAssetsAsync{TObject}(object, Action{TObject})"/> or one of its overloads
        /// to load multiple assets in a single operation.
        ///
        /// See [Loading Addressable Assets](xref:addressables-api-load-asset-async) for more information and examples.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        public static AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            return m_Addressables.LoadAssetAsync<TObject>(key);
        }

        /// <summary>
        /// Loads the resource locations specified by the keys.
        /// The method will always return success, with a valid IList of results. If nothing matches keys, IList will be empty
        /// </summary>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadResourceLocationsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocations(IList<object> keys, MergeMode mode, Type type = null)
        {
            return LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Loads the resource locations specified by a list of keys.
        /// </summary>
        /// <remarks>
        /// The operation always completes successfully and the operation handle's `Result` object always contains a valid IList instance.
        /// If no assets matched the specified keys, the list in `Result` is empty.
        ///
        /// See [Loading assets by location](xref:addressables-api-load-asset-async#loading-assets-by-location) for more information.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IList<object> keys, MergeMode mode, Type type = null)
        {
            return m_Addressables.LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Loads the resource locations specified by a set of keys.
        /// </summary>
        /// <remarks>
        /// The operation always completes successfully and the operation handle's `Result` object always contains a valid IList instance.
        /// If no assets matched the specified keys, the list in `Result` is empty.
        ///
        /// See [Loading assets by location](xref:addressables-api-load-asset-async#loading-assets-by-location) for more information.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IEnumerable keys, MergeMode mode, Type type = null)
        {
            return m_Addressables.LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Request the locations for a given key.
        /// The method will always return success, with a valid IList of results. If nothing matches key, IList will be empty
        /// </summary>
        /// <param name="key">The key for the locations.</param>
        /// <param name="type">A type restriction for the lookup. Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadResourceLocationsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocations(object key, Type type = null)
        {
            return LoadResourceLocationsAsync(key, type);
        }

        /// <summary>
        /// Loads the resource locations specified by a key.
        /// </summary>
        /// <remarks>
        /// The operation always completes successfully and the operation handle's `Result` object always contains a valid IList instance.
        /// If no assets matched the specified key, the list in `Result` is empty.
        ///
        /// See [Loading assets by location](xref:addressables-api-load-asset-async#loading-assets-by-location) for more information.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <param name="key">The key for the locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null)
        {
            return m_Addressables.LoadResourceLocationsAsync(key, type);
        }

        /// <summary>
        /// Load multiple assets
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(IList<IResourceLocation> locations, Action<TObject> callback)
        {
            return LoadAssetsAsync(locations, callback);
        }

        /// <summary>
        /// Loads multiple assets, based on the list of locations provided.
        /// </summary>
        /// <remarks>
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the asset
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback)
        {
            return m_Addressables.LoadAssetsAsync(locations, callback, true);
        }

        /// <summary>
        /// Loads multiple assets, based on the list of locations provided.
        /// </summary>
        /// <remarks>
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// When false, if any matching location fails, the `Result` instance in the returned operation handle contains an IList of size equal to the number of
        /// locations that the operation attempted to load. The entry in the result list corresponding to a location that failed to load is null.
        /// The entries for locations that successfully loaded are set to a valid TObject. The `Status` of the operation handle is still <see cref="AsyncOperationStatus.Failed"/>
        /// if any single asset failed to load.
        ///
        /// When <paramref name="releaseDependenciesOnFailure"/> is true, you do not need to release the <see cref="AsyncOperationHandle"/> instance on failure.
        /// When false, you must always release it.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback, bool releaseDependenciesOnFailure)
        {
            return m_Addressables.LoadAssetsAsync(locations, callback, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load multiple assets
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode)
        {
            return LoadAssetsAsync(keys, callback, mode);
        }

        /// <summary>
        /// Loads multiple assets identified by a list of keys.
        /// </summary>
        /// <remarks>
        /// The keys in <paramref name="keys"/> are translated into lists of locations, which are merged into a single list based on
        /// the value in <paramref name="mode"/>.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode)
        {
            return m_Addressables.LoadAssetsAsync(keys, callback, mode, true);
        }

        /// <summary>
        /// Loads multiple assets identified by a list of keys.
        /// </summary>
        /// <remarks>
        /// The keys in <paramref name="keys"/> are translated into lists of locations, which are merged into a single list based on
        /// the value in <paramref name="mode"/>.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode)
        {
            return m_Addressables.LoadAssetsAsync(keys, callback, mode, true);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        ///
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        ///
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        ///
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode, bool releaseDependenciesOnFailure)
        {
            return m_Addressables.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Loads multiple assets, identified by a set of keys.
        /// </summary>
        /// <remarks>
        /// The keys in <paramref name="keys"/> are translated into lists of locations, which are merged into a single list based on
        /// the value in <paramref name="mode"/>.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        ///
        /// When true, if any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// When false, if any matching location fails, the `Result` instance in the returned operation handle contains an IList of size equal to the number of
        /// locations that the operation attempted to load. The entry in the result list corresponding to a location that failed to load is null.
        /// The entries for locations that successfully loaded are set to a valid TObject. The `Status` of the operation handle is still <see cref="AsyncOperationStatus.Failed"/>
        /// if any single asset failed to load.
        ///
        /// When <paramref name="releaseDependenciesOnFailure"/> is true, you do not need to release the <see cref="AsyncOperationHandle"/> instance on failure.
        /// When false, you must always release it.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode, bool releaseDependenciesOnFailure)
        {
            return m_Addressables.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load mutliple assets
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all async methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(object key, Action<TObject> callback)
        {
            return LoadAssetsAsync(key, callback);
        }

        /// <summary>
        /// Loads multiple assets identified by a single key.
        /// </summary>
        /// <remarks>
        /// The key in <paramref name="key"/> is translated into a list of locations that are then loaded.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// If any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback)
        {
            return m_Addressables.LoadAssetsAsync(key, callback, true);
        }

        /// <summary>
        /// Loads multiple assets identified by a single key.
        /// </summary>
        /// <remarks>
        /// The key in <paramref name="key"/> is translated into a list of locations that are then loaded.
        ///
        /// When you load Addressable assets, the system:
        /// * Gathers the dependencies of the assets
        /// * Downloads any remote AssetBundles needed to load the assets or their dependencies
        /// * Loads the AssetBundles into memory
        /// * Populates the `Result` object of the <see cref="AsyncOperationHandle{TObject}"/> instance returned by this function
        ///
        /// Use the `Result` object to access the loaded assets.
        ///
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        ///
        /// See [Operations](xref:addressables-async-operation-handling) for information on handling the asynchronous operations used
        /// to load Addressable assets.
        /// </remarks>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation (per loaded asset).</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        ///
        /// When true, if any assets cannot be loaded, the entire operation fails. The operation releases any assets and dependencies it had already loaded.
        /// The `Status` of the operation handle is set to <see cref="AsyncOperationStatus.Failed"/> and the `Result` is set to null.
        ///
        /// When false, if any matching location fails, the `Result` instance in the returned operation handle contains an IList of size equal to the number of
        /// locations that the operation attempted to load. The entry in the result list corresponding to a location that failed to load is null.
        /// The entries for locations that successfully loaded are set to a valid TObject. The `Status` of the operation handle is still <see cref="AsyncOperationStatus.Failed"/>
        /// if any single asset failed to load.
        ///
        /// When <paramref name="releaseDependenciesOnFailure"/> is true, you do not need to release the <see cref="AsyncOperationHandle"/> instance on failure.
        /// When false, you must always release it.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure)
        {
            return m_Addressables.LoadAssetsAsync(key, callback, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Release asset.
        /// </summary>
        /// <typeparam name="TObject">The type of the object being released</typeparam>
        /// <param name="obj">The asset to release.</param>
        public static void Release<TObject>(TObject obj)
        {
            m_Addressables.Release(obj);
        }

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <typeparam name="TObject">The type of the AsyncOperationHandle being released</typeparam>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release<TObject>(AsyncOperationHandle<TObject> handle)
        {
            m_Addressables.Release(handle);
        }

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release(AsyncOperationHandle handle)
        {
            m_Addressables.Release(handle);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="instance">The GameObject instance to be released and destroyed.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(GameObject instance)
        {
            return m_Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(AsyncOperationHandle handle)
        {
            m_Addressables.Release(handle);
            return true;
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            m_Addressables.Release(handle);
            return true;
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> GetDownloadSizeAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<long> GetDownloadSize(object key)
        {
            return GetDownloadSizeAsync(key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(object key)
        {
            return m_Addressables.GetDownloadSizeAsync(key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(string key)
        {
            return m_Addressables.GetDownloadSizeAsync((object)key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="keys"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="keys">The keys of the asset(s) to get the download size of.</param>
        [Obsolete]
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(IList<object> keys)
        {
            return m_Addressables.GetDownloadSizeAsync(keys);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="keys"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="keys">The keys of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(IEnumerable keys)
        {
            return m_Addressables.GetDownloadSizeAsync(keys);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified label or address.
        /// </summary>
        /// <param name="key">The key of the asset(s) to load dependencies for.</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> DownloadDependenciesAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle DownloadDependencies(object key)
        {
            return DownloadDependenciesAsync(key);
        }

        /// <summary>
        /// Downloads dependencies of assets identified with the specified label or address.
        /// </summary>
        /// <remarks>
        /// Call this function to make sure that the dependencies of assets you plan to load in
        /// performance-critical sections of code are downloaded and available beforehand.
        ///
        /// You can use the <see cref="AsyncOperationHandle"/> returned by this function to monitor and
        /// provide feedback on the download progress.
        ///
        /// See [Preloading dependencies](xref:addressables-api-download-dependencies-async) for more details.
        /// </remarks>
        /// <param name="key">The key of the assets to load dependencies for.</param>
        /// <param name="autoReleaseHandle">If true, the Addressables system automatically releases the handle on completion.</param>
        /// <returns>The AsyncOperationHandle for the dependency load operation.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(object key, bool autoReleaseHandle = false)
        {
            return m_Addressables.DownloadDependenciesAsync(key, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets at the specified locations.
        /// </summary>
        /// <remarks>
        /// Call this function to make sure that the dependencies of assets you plan to load in
        /// performance-critical sections of code are downloaded and available beforehand.
        ///
        /// You can use the <see cref="AsyncOperationHandle"/> returned by this function to monitor and
        /// provide feedback on the download progress.
        ///
        /// See [Preloading dependencies](xref:addressables-api-download-dependencies-async) for more details.
        /// </remarks>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="autoReleaseHandle">If true, the Addressables system automatically releases the handle on completion.</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(IList<IResourceLocation> locations, bool autoReleaseHandle = false)
        {
            return m_Addressables.DownloadDependenciesAsync(locations, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified labels or addresses.
        /// See the [DownloadDependenciesAsync](xref:addressables-api-download-dependencies-async) documentation for more details.
        /// </summary>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="autoReleaseHandle">Automatically releases the handle on completion</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        [Obsolete]
        public static AsyncOperationHandle DownloadDependenciesAsync(IList<object> keys, MergeMode mode, bool autoReleaseHandle = false)
        {
            return m_Addressables.DownloadDependenciesAsync(keys, mode, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets identified by a list of keys.
        /// </summary>
        /// <remarks>
        /// The keys in <paramref name="keys"/> are translated into lists of locations, which are merged into a single list based on
        /// the value in <paramref name="mode"/>.
        ///
        /// Call this function to make sure that the dependencies of assets you plan to load in
        /// performance-critical sections of code are downloaded and available beforehand.
        ///
        /// You can use the <see cref="AsyncOperationHandle"/> returned by this function to monitor and
        /// provide feedback on the download progress.
        ///
        /// See [Preloading dependencies](xref:addressables-api-download-dependencies-async) for more details.
        /// </remarks>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="autoReleaseHandle">If true, the Addressables system automatically releases the handle on completion.</param>
        /// <returns>The AsyncOperationHandle for the dependency load operation.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(IEnumerable keys, MergeMode mode, bool autoReleaseHandle = false)
        {
            return m_Addressables.DownloadDependenciesAsync(keys, mode, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a given key.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="key">The key to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(object key)
        {
            m_Addressables.ClearDependencyCacheAsync(key, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable locations.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="locations">The locations to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(IList<IResourceLocation> locations)
        {
            m_Addressables.ClearDependencyCacheAsync(locations, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="keys">The keys to clear the cache for.</param>
        [Obsolete]
        public static void ClearDependencyCacheAsync(IList<object> keys)
        {
            m_Addressables.ClearDependencyCacheAsync(keys, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="keys">The keys to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(IEnumerable keys)
        {
            m_Addressables.ClearDependencyCacheAsync(keys, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="key">The key to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(string key)
        {
            m_Addressables.ClearDependencyCacheAsync((object)key, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a given key.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="key">The key to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(object key, bool autoReleaseHandle)
        {
            return m_Addressables.ClearDependencyCacheAsync(key, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable locations.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="locations">The locations to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IList<IResourceLocation> locations, bool autoReleaseHandle)
        {
            return m_Addressables.ClearDependencyCacheAsync(locations, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="keys">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IList<object> keys, bool autoReleaseHandle)
        {
            return m_Addressables.ClearDependencyCacheAsync(keys, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="keys">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IEnumerable keys, bool autoReleaseHandle)
        {
            return m_Addressables.ClearDependencyCacheAsync(keys, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <remarks>
        /// Clear all cached AssetBundles
        /// WARNING: This will cause all asset bundles represented by the passed-in
        /// parameters to be cleared and require re-downloading.
        /// </remarks>
        /// <param name="key">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(string key, bool autoReleaseHandle)
        {
            return m_Addressables.ClearDependencyCacheAsync((object)key, autoReleaseHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return InstantiateAsync(location, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return InstantiateAsync(location, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return InstantiateAsync(key, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return InstantiateAsync(key, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return InstantiateAsync(location, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(location, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(location, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(key, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(key, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object.
        /// </summary>
        /// <remarks>
        /// Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        ///
        /// See [Instantiating objects from Addressables](xref:addressables-api-load-asset-async#instantiate) documentation for more details.
        /// </remarks>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return m_Addressables.InstantiateAsync(location, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Load scene.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> LoadScene(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Load scene.
        /// </summary>
        /// <param name="location">The location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> LoadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> LoadScene(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Loads an Addressable Scene asset.
        /// </summary>
        /// <remarks>
        /// The <paramref name="loadMode"/>, <paramref name="activateOnLoad"/>, and <paramref name="priority"/> parameters correspond to
        /// the parameters used in the Unity [SceneManager.LoadSceneAsync](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html)
        /// method.
        ///
        /// See [Loading Scenes](xref:addressables-api-load-asset-async#loading-scenes) for more details.
        /// </remarks>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Loads an Addressable Scene asset.
        /// </summary>
        /// <remarks>
        /// The <paramref name="loadMode"/>, <paramref name="activateOnLoad"/>, and <paramref name="priority"/> parameters correspond to
        /// the parameters used in the Unity [SceneManager.LoadSceneAsync](https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html)
        /// method.
        ///
        /// See [Loading Scenes](xref:addressables-api-load-asset-async#loading-scenes) for more details.
        /// </remarks>
        /// <param name="location">The location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_Addressables.LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(scene, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asynchronous methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="unloadOptions">If true, assets embedded in the scene will be unloaded as part of the scene unload process.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, UnloadSceneOptions unloadOptions, bool autoReleaseHandle = true)
        {
            return m_Addressables.UnloadSceneAsync(scene, unloadOptions, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="unloadOptions">If true, assets embedded in the scene will be unloaded as part of the scene unload process.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle = true)
        {
            return m_Addressables.UnloadSceneAsync(handle, unloadOptions, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="unloadOptions">If true, assets embedded in the scene will be unloaded as part of the scene unload process.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(AsyncOperationHandle<SceneInstance> handle, UnloadSceneOptions unloadOptions, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(handle, unloadOptions, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return m_Addressables.UnloadSceneAsync(scene, UnloadSceneOptions.None, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return m_Addressables.UnloadSceneAsync(handle, UnloadSceneOptions.None, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return m_Addressables.UnloadSceneAsync(handle, UnloadSceneOptions.None, autoReleaseHandle);
        }

        /// <summary>
        /// Checks all updatable content catalogs for a new version.
        /// </summary>
        /// <param name="autoReleaseHandle">If true, the handle will automatically be released when the operation completes.</param>
        /// <returns>The operation containing the list of catalog ids that have an available update.  This can be used to filter which catalogs to update with the UpdateContent.</returns>
        public static AsyncOperationHandle<List<string>> CheckForCatalogUpdates(bool autoReleaseHandle = true)
        {
            return m_Addressables.CheckForCatalogUpdates(autoReleaseHandle);
        }

        /// <summary>
        /// Update the specified catalogs.
        /// </summary>
        /// <remarks>
        /// When you call the UpdateCatalog function, all other Addressable requests are blocked until the operation is finished.
        /// You can release the operation handle returned by UpdateCatalogs immediately after the operation finishes (or set the
        /// autoRelease parameter to true).
        ///
        /// If you call UpdateCatalog without providing a list of catalogs, the Addressables system checks all of the currently
        /// loaded catalogs for updates.
        ///
        /// If you update a catalog when you have already loaded content from the related AssetBundles, you can encounter conflicts
        /// between the loaded AssetBundles and the updated versions. To avoid conflicts, update the catalog before loading assets or unload
        /// the AssetBundles before the updating the catalog. You can enable the
        /// [Unique Bundle Ids](xref:addressables-content-update-builds#unique-bundle-ids-setting)
        /// option in your Addressable settings to avoid conflicts, but that can increase memory consumption since you will still
        /// have the original AssetBundles in memory after loading the updated ones. Enabling this option can also make the download size of content
        /// updates larger because typically more AssetBundles must be rebuilt.
        ///
        /// See [Updating catalogs](xref:addressables-api-load-content-catalog-async#updating-catalogs) for more details.
        /// </remarks>
        /// <param name="catalogs">The set of catalogs to update.  If null, all catalogs that have an available update will be updated.</param>
        /// <param name="autoReleaseHandle">If true, the handle will automatically be released when the operation completes.</param>
        /// <returns>The operation with the list of updated content catalog data.</returns>
        public static AsyncOperationHandle<List<IResourceLocator>> UpdateCatalogs(IEnumerable<string> catalogs = null, bool autoReleaseHandle = true)
        {
            return m_Addressables.UpdateCatalogs(catalogs, autoReleaseHandle, false);
        }

        /// <summary>
        /// Update the specified catalogs.
        /// </summary>
        /// <remarks>
        /// When you call the UpdateCatalog function, all other Addressable requests are blocked until the operation is finished.
        /// You can release the operation handle returned by UpdateCatalogs immediately after the operation finishes (or set the
        /// autoRelease parameter to true).
        ///
        /// If you call UpdateCatalog without providing a list of catalogs, the Addressables system checks all of the currently
        /// loaded catalogs for updates.
        ///
        /// If you update a catalog when you have already loaded content from the related AssetBundles, you can encounter conflicts
        /// between the loaded AssetBundles and the updated versions. To avoid conflicts, update the catalog before loading assets or unload
        /// the AssetBundles before the updating the catalog. You can enable the
        /// [Unique Bundle Ids](xref:addressables-content-update-builds#unique-bundle-ids-setting)
        /// option in your Addressable settings to avoid conflicts, but that can increase memory consumption since you will still
        /// have the original AssetBundles in memory after loading the updated ones. Enabling this option can also make the download size of content
        /// updates larger because typically more AssetBundles must be rebuilt.
        ///
        /// See [Updating catalogs](xref:addressables-api-load-content-catalog-async#updating-catalogs) for more details.
        /// </remarks>
        /// <param name="autoCleanBundleCache">If true, removes any nonreferenced bundles in the cache.</param>
        /// <param name="catalogs">The set of catalogs to update.  If null, all catalogs that have an available update will be updated.</param>
        /// <param name="autoReleaseHandle">If true, the handle will automatically be released when the operation completes.</param>
        /// <returns>The operation with the list of updated content catalog data.</returns>
        public static AsyncOperationHandle<List<IResourceLocator>> UpdateCatalogs(bool autoCleanBundleCache, IEnumerable<string> catalogs = null, bool autoReleaseHandle = true) // autoCleanBundleCache must be listed first to avoid breaking API
        {
            return m_Addressables.UpdateCatalogs(catalogs, autoReleaseHandle, autoCleanBundleCache);
        }

        /// <summary>
        /// Add a resource locator.
        /// </summary>
        /// <param name="locator">The locator object.</param>
        /// <param name="localCatalogHash">The hash of the local catalog. This can be null if the catalog cannot be updated.</param>
        /// <param name="remoteCatalogLocation">The location of the remote catalog. This can be null if the catalog cannot be updated.</param>
        public static void AddResourceLocator(IResourceLocator locator, string localCatalogHash = null, IResourceLocation remoteCatalogLocation = null)
        {
            m_Addressables.AddResourceLocator(locator, localCatalogHash, remoteCatalogLocation);
        }

        /// <summary>
        /// Remove a locator;
        /// </summary>
        /// <param name="locator">The locator to remove.</param>
        public static void RemoveResourceLocator(IResourceLocator locator)
        {
            m_Addressables.RemoveResourceLocator(locator);
        }

        /// <summary>
        /// Remove all locators.
        /// </summary>
        public static void ClearResourceLocators()
        {
            m_Addressables.ClearResourceLocators();
        }

        /// <summary>
        /// Removes any AssetBundles that are no longer referenced in the bundle cache. This can occur when a new, updated catalog excludes entries present in an older catalog.
        /// </summary>
        /// <remarks>
        /// Note, that only AssetBundles loaded through UnityWebRequest are cached. If you want to purge the entire cache, use Caching.ClearCache instead.
        /// In the Editor CleanBundleCache should only be called when using the "Use Existing Build (requires built groups)" playmode script as it loads content from bundles.
        ///
        /// See [AssetBundle caching](xref:addressables-remote-content-distribution#assetbundle-caching) for more details.
        /// </remarks>
        /// <param name="catalogsIds">The ids of catalogs whose bundle cache entries we want to preserve. If null, entries for all currently loaded catalogs will be preserved.</param>
        /// <returns>The operation handle for the request. Note, that it is user's responsibility to release the returned operation; this can be done before or after the operation completes.</returns>
        public static AsyncOperationHandle<bool> CleanBundleCache(IEnumerable<string> catalogsIds = null)
        {
            return m_Addressables.CleanBundleCache(catalogsIds, false);
        }
    }
}
