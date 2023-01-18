using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.ResourceProviders
{
    /// <summary>
    /// Provider for content catalogs.  This provider makes use of a hash file to determine if a newer version of the catalog needs to be downloaded.
    /// </summary>
    [DisplayName("Content Catalog Provider")]
    public class ContentCatalogProvider : ResourceProviderBase
    {
        /// <summary>
        /// Options for specifying which entry in the catalog dependencies should hold each hash item.
        ///  The Remote should point to the hash on the server.  The Cache should point to the
        ///  local cache copy of the remote data.
        /// </summary>
        public enum DependencyHashIndex
        {
            /// <summary>
            /// Use to represent the index of the remote entry in the dependencies list.
            /// </summary>
            Remote = 0,
            /// <summary>
            /// Use to represent the index of the cache entry in the dependencies list.
            /// </summary>
            Cache,
            /// <summary>
            /// Use to represent the number of entries in the dependencies list.
            /// </summary>
            Count
        }

        /// <summary>
        /// Use to indicate if the updating the catalog on startup should be disabled.
        /// </summary>
        public bool DisableCatalogUpdateOnStart = false;

        /// <summary>
        /// Use to indicate if the local catalog is in a bundle.
        /// </summary>
        public bool IsLocalCatalogInBundle = false;

        internal Dictionary<IResourceLocation, InternalOp> m_LocationToCatalogLoadOpMap = new Dictionary<IResourceLocation, InternalOp>();
        ResourceManager m_RM;
        /// <summary>
        /// Constructor for this provider.
        /// </summary>
        /// <param name="resourceManagerInstance">The resource manager to use.</param>
        public ContentCatalogProvider(ResourceManager resourceManagerInstance)
        {
            m_RM = resourceManagerInstance;
            m_BehaviourFlags = ProviderBehaviourFlags.CanProvideWithFailedDependencies;
        }

        /// <inheritdoc/>
        public override void Release(IResourceLocation location, object obj)
        {
            if (m_LocationToCatalogLoadOpMap.ContainsKey(location))
            {
                m_LocationToCatalogLoadOpMap[location].Release();
                m_LocationToCatalogLoadOpMap.Remove(location);
            }
            base.Release(location, obj);
        }

        internal class InternalOp
        {
            //   int m_StartFrame;
            string m_LocalDataPath;
            string m_RemoteHashValue;
            internal string m_LocalHashValue;
            ProvideHandle m_ProviderInterface;
            internal ContentCatalogData m_ContentCatalogData;
            AsyncOperationHandle<ContentCatalogData> m_ContentCatalogDataLoadOp;
            private BundledCatalog m_BundledCatalog;
            private bool m_Retried;
            private bool m_DisableCatalogUpdateOnStart;
            private bool m_IsLocalCatalogInBundle;

            public void Start(ProvideHandle providerInterface, bool disableCatalogUpdateOnStart, bool isLocalCatalogInBundle)
            {
                m_ProviderInterface = providerInterface;
                m_DisableCatalogUpdateOnStart = disableCatalogUpdateOnStart;
                m_IsLocalCatalogInBundle = isLocalCatalogInBundle;
                m_ProviderInterface.SetWaitForCompletionCallback(WaitForCompletionCallback);
                m_LocalDataPath = null;
                m_RemoteHashValue = null;

                List<object> deps = new List<object>(); // TODO: garbage. need to pass actual count and reuse the list
                m_ProviderInterface.GetDependencies(deps);
                string idToLoad = DetermineIdToLoad(m_ProviderInterface.Location, deps, disableCatalogUpdateOnStart);

                Addressables.LogFormat("Addressables - Using content catalog from {0}.", idToLoad);

                bool loadCatalogFromLocalBundle = isLocalCatalogInBundle && CanLoadCatalogFromBundle(idToLoad, m_ProviderInterface.Location);

                LoadCatalog(idToLoad, loadCatalogFromLocalBundle);
            }

            bool WaitForCompletionCallback()
            {
                if (m_ContentCatalogData != null)
                    return true;
                bool ccComplete;
                if (m_BundledCatalog != null)
                {
                    ccComplete = m_BundledCatalog.WaitForCompletion();
                }
                else
                {
                    ccComplete = m_ContentCatalogDataLoadOp.IsDone;
                    if (!ccComplete)
                        m_ContentCatalogDataLoadOp.WaitForCompletion();
                }

                //content catalog op needs the Update to be pumped so we can invoke completion callbacks
                if (ccComplete && m_ContentCatalogData == null)
                    m_ProviderInterface.ResourceManager.Update(Time.unscaledDeltaTime);

                return ccComplete;
            }

            /// <summary>
            /// Clear all content catalog data.
            /// </summary>
            public void Release()
            {
                m_ContentCatalogData?.CleanData();
            }

            internal bool CanLoadCatalogFromBundle(string idToLoad, IResourceLocation location)
            {
                return Path.GetExtension(idToLoad) == ".bundle" &&
                    idToLoad.Equals(GetTransformedInternalId(location));
            }

            internal void LoadCatalog(string idToLoad, bool loadCatalogFromLocalBundle)
            {
                try
                {
                    ProviderLoadRequestOptions providerLoadRequestOptions = null;
                    if (m_ProviderInterface.Location.Data is ProviderLoadRequestOptions providerData)
                        providerLoadRequestOptions = providerData.Copy();

                    if (loadCatalogFromLocalBundle)
                    {
                        int webRequestTimeout = providerLoadRequestOptions?.WebRequestTimeout ?? 0;
                        m_BundledCatalog = new BundledCatalog(idToLoad, webRequestTimeout);
                        m_BundledCatalog.OnLoaded += ccd =>
                        {
                            m_ContentCatalogData = ccd;
                            OnCatalogLoaded(ccd);
                        };
                        m_BundledCatalog.LoadCatalogFromBundleAsync();
                    }
                    else
                    {
                        ResourceLocationBase location = new ResourceLocationBase(idToLoad, idToLoad,
                            typeof(JsonAssetProvider).FullName, typeof(ContentCatalogData));
                        location.Data = providerLoadRequestOptions;

                        m_ContentCatalogDataLoadOp = m_ProviderInterface.ResourceManager.ProvideResource<ContentCatalogData>(location);
                        m_ContentCatalogDataLoadOp.Completed += CatalogLoadOpCompleteCallback;
                    }
                }
                catch (Exception ex)
                {
                    m_ProviderInterface.Complete<ContentCatalogData>(null, false, ex);
                }
            }

            void CatalogLoadOpCompleteCallback(AsyncOperationHandle<ContentCatalogData> op)
            {
                m_ContentCatalogData = op.Result;
                m_ProviderInterface.ResourceManager.Release(op);
                OnCatalogLoaded(m_ContentCatalogData);
            }

            internal class BundledCatalog
            {
                private readonly string m_BundlePath;
                private bool m_OpInProgress;
                private AssetBundleCreateRequest m_LoadBundleRequest;
                internal AssetBundle m_CatalogAssetBundle;
                private AssetBundleRequest m_LoadTextAssetRequest;
                private ContentCatalogData m_CatalogData;
                private WebRequestQueueOperation m_WebRequestQueueOperation;
                private AsyncOperation m_RequestOperation;
                private int m_WebRequestTimeout;

                public event Action<ContentCatalogData> OnLoaded;

                public bool OpInProgress => m_OpInProgress;
                public bool OpIsSuccess => !m_OpInProgress && m_CatalogData != null;

                public BundledCatalog(string bundlePath, int webRequestTimeout = 0)
                {
                    if (string.IsNullOrEmpty(bundlePath))
                    {
                        throw new ArgumentNullException(nameof(bundlePath), "Catalog bundle path is null.");
                    }
                    else if (!bundlePath.EndsWith(".bundle"))
                    {
                        throw new ArgumentException("You must supply a valid bundle file path.");
                    }

                    m_BundlePath = bundlePath;
                    m_WebRequestTimeout = webRequestTimeout;
                }

                ~BundledCatalog()
                {
                    Unload();
                }

                private void Unload()
                {
                    m_CatalogAssetBundle?.Unload(true);
                    m_CatalogAssetBundle = null;
                }

                public void LoadCatalogFromBundleAsync()
                {
                    //Debug.Log($"LoadCatalogFromBundleAsync frame : {Time.frameCount}");
                    if (m_OpInProgress)
                    {
                        Addressables.LogError($"Operation in progress : A catalog is already being loaded. Please wait for the operation to complete.");
                        return;
                    }

                    m_OpInProgress = true;

                    if (ResourceManagerConfig.ShouldPathUseWebRequest(m_BundlePath))
                    {
                        var req = UnityWebRequestAssetBundle.GetAssetBundle(m_BundlePath);
                        if (m_WebRequestTimeout > 0)
                            req.timeout = m_WebRequestTimeout;

                        m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(req);
                        if (m_WebRequestQueueOperation.IsDone)
                        {
                            m_RequestOperation = m_WebRequestQueueOperation.Result;
                            if (m_RequestOperation.isDone)
                                WebRequestOperationCompleted(m_RequestOperation);
                            else
                                m_RequestOperation.completed += WebRequestOperationCompleted;
                        }
                        else
                        {
                            m_WebRequestQueueOperation.OnComplete += asyncOp =>
                            {
                                m_RequestOperation = asyncOp;
                                m_RequestOperation.completed += WebRequestOperationCompleted;
                            };
                        }
                    }
                    else
                    {
                        m_LoadBundleRequest = AssetBundle.LoadFromFileAsync(m_BundlePath);
                        m_LoadBundleRequest.completed += loadOp =>
                        {
                            if (loadOp is AssetBundleCreateRequest createRequest && createRequest.assetBundle != null)
                            {
                                m_CatalogAssetBundle = createRequest.assetBundle;
                                m_LoadTextAssetRequest = m_CatalogAssetBundle.LoadAllAssetsAsync<TextAsset>();
                                if (m_LoadTextAssetRequest.isDone)
                                    LoadTextAssetRequestComplete(m_LoadTextAssetRequest);
                                m_LoadTextAssetRequest.completed += LoadTextAssetRequestComplete;
                            }
                            else
                            {
                                Addressables.LogError($"Unable to load dependent bundle from location : {m_BundlePath}");
                                m_OpInProgress = false;
                            }
                        };
                    }
                }

                private void WebRequestOperationCompleted(AsyncOperation op)
                {
                    UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
                    var webReq = remoteReq.webRequest;
                    DownloadHandlerAssetBundle downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                    if (!UnityWebRequestUtilities.RequestHasErrors(webReq, out UnityWebRequestResult uwrResult))
                    {
                        m_CatalogAssetBundle = downloadHandler.assetBundle;
                        m_LoadTextAssetRequest = m_CatalogAssetBundle.LoadAllAssetsAsync<TextAsset>();
                        if (m_LoadTextAssetRequest.isDone)
                            LoadTextAssetRequestComplete(m_LoadTextAssetRequest);
                        m_LoadTextAssetRequest.completed += LoadTextAssetRequestComplete;
                    }
                    else
                    {
                        Addressables.LogError($"Unable to load dependent bundle from location : {m_BundlePath}");
                        m_OpInProgress = false;
                    }
                    webReq.Dispose();
                }

                void LoadTextAssetRequestComplete(AsyncOperation op)
                {
                    if (op is AssetBundleRequest loadRequest
                        && loadRequest.asset is TextAsset textAsset
                        && textAsset.text != null)
                    {
                        m_CatalogData = JsonUtility.FromJson<ContentCatalogData>(textAsset.text);
                        OnLoaded?.Invoke(m_CatalogData);
                    }
                    else
                    {
                        Addressables.LogError($"No catalog text assets where found in bundle {m_BundlePath}");
                    }
                    Unload();
                    m_OpInProgress = false;
                }

                public bool WaitForCompletion()
                {
                    if (m_LoadBundleRequest.assetBundle == null)
                        return false;

                    return m_LoadTextAssetRequest.asset != null || m_LoadTextAssetRequest.allAssets != null;
                }
            }

            string GetTransformedInternalId(IResourceLocation loc)
            {
                if (m_ProviderInterface.ResourceManager == null)
                    return loc.InternalId;
                return m_ProviderInterface.ResourceManager.TransformInternalId(loc);
            }

            internal string DetermineIdToLoad(IResourceLocation location, IList<object> dependencyObjects, bool disableCatalogUpdateOnStart = false)
            {
                //default to load actual local source catalog
                string idToLoad = GetTransformedInternalId(location);
                if (dependencyObjects != null &&
                    location.Dependencies != null &&
                    dependencyObjects.Count == (int)DependencyHashIndex.Count &&
                    location.Dependencies.Count == (int)DependencyHashIndex.Count)
                {
                    var remoteHash = dependencyObjects[(int)DependencyHashIndex.Remote] as string;
                    m_LocalHashValue = dependencyObjects[(int)DependencyHashIndex.Cache] as string;
                    Addressables.LogFormat("Addressables - ContentCatalogProvider CachedHash = {0}, RemoteHash = {1}.", m_LocalHashValue, remoteHash);

                    if (string.IsNullOrEmpty(remoteHash) || disableCatalogUpdateOnStart) //offline
                    {
                        if (!string.IsNullOrEmpty(m_LocalHashValue) && !m_Retried) //cache exists and not forcing a retry state
                        {
                            idToLoad = GetTransformedInternalId(location.Dependencies[(int)DependencyHashIndex.Cache]).Replace(".hash", ".json");
                        }
                        else
                        {
                            m_LocalHashValue = Hash128.Compute(idToLoad).ToString();
                        }
                    }
                    else //online
                    {
                        if (remoteHash == m_LocalHashValue && !m_Retried) //cache of remote is good and not forcing a retry state
                        {
                            idToLoad = GetTransformedInternalId(location.Dependencies[(int)DependencyHashIndex.Cache]).Replace(".hash", ".json");
                        }
                        else //remote is different than cache, or no cache
                        {
                            idToLoad = GetTransformedInternalId(location.Dependencies[(int)DependencyHashIndex.Remote]).Replace(".hash", ".json");
                            m_LocalDataPath = GetTransformedInternalId(location.Dependencies[(int)DependencyHashIndex.Cache]).Replace(".hash", ".json");
                            m_RemoteHashValue = remoteHash;
                        }
                    }
                }
                return idToLoad;
            }

            private void OnCatalogLoaded(ContentCatalogData ccd)
            {
                Addressables.LogFormat("Addressables - Content catalog load result = {0}.", ccd);
                if (ccd != null)
                {
                    ccd.location = m_ProviderInterface.Location;
                    ccd.localHash = m_LocalHashValue;
                    if (!string.IsNullOrEmpty(m_RemoteHashValue) && !string.IsNullOrEmpty(m_LocalDataPath))
                    {
#if ENABLE_CACHING
                        var dir = Path.GetDirectoryName(m_LocalDataPath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        var localCachePath = m_LocalDataPath;
                        Addressables.LogFormat("Addressables - Saving cached content catalog to {0}.", localCachePath);
                        try
                        {
                            File.WriteAllText(localCachePath, JsonUtility.ToJson(ccd));
                            File.WriteAllText(localCachePath.Replace(".json", ".hash"), m_RemoteHashValue);
                        }
                        catch (Exception e)
                        {
                            string remoteInternalId = GetTransformedInternalId(m_ProviderInterface.Location.Dependencies[(int)DependencyHashIndex.Remote]);
                            var errorMessage = $"Unable to load ContentCatalogData from location {remoteInternalId}. Failed to cache catalog to location {localCachePath}.";
                            ccd = null;
                            m_ProviderInterface.Complete(ccd, false, new Exception(errorMessage, e));
                            return;
                        }
#endif
                        ccd.localHash = m_RemoteHashValue;
                    }
                    m_ProviderInterface.Complete(ccd, true, null);
                }
                else
                {
                    var errorMessage = $"Unable to load ContentCatalogData from location {m_ProviderInterface.Location}";
                    if (!m_Retried)
                    {
                        m_Retried = true;

                        //if the prev load path is cache, try to remove cache and reload from remote
                        var cachePath = GetTransformedInternalId(m_ProviderInterface.Location.Dependencies[(int)DependencyHashIndex.Cache]);
                        if (m_ContentCatalogDataLoadOp.LocationName == cachePath.Replace(".hash", ".json"))
                        {
                            try
                            {
#if ENABLE_CACHING
                                File.Delete(cachePath);
#endif
                            }
                            catch (Exception)
                            {
                                errorMessage += $". Unable to delete cache data from location {cachePath}";
                                m_ProviderInterface.Complete(ccd, false, new Exception(errorMessage));
                                return;
                            }
                        }

                        Addressables.LogWarning(errorMessage + ". Attempting to retry...");
                        Start(m_ProviderInterface, m_DisableCatalogUpdateOnStart, m_IsLocalCatalogInBundle);
                    }
                    else
                    {
                        m_ProviderInterface.Complete(ccd, false, new Exception(errorMessage + " on second attempt."));
                    }
                }
            }
        }

        ///<inheritdoc/>
        public override void Provide(ProvideHandle providerInterface)
        {
            if (!m_LocationToCatalogLoadOpMap.ContainsKey(providerInterface.Location))
                m_LocationToCatalogLoadOpMap.Add(providerInterface.Location, new InternalOp());
            m_LocationToCatalogLoadOpMap[providerInterface.Location].Start(providerInterface, DisableCatalogUpdateOnStart, IsLocalCatalogInBundle);
        }
    }
}
