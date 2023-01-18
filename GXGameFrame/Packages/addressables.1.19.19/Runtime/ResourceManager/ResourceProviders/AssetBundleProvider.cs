#if UNITY_2022_1_OR_NEWER
#define UNLOAD_BUNDLE_ASYNC
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;


namespace UnityEngine.ResourceManagement.ResourceProviders
{
    internal class DownloadOnlyLocation : LocationWrapper
    {
        public DownloadOnlyLocation(IResourceLocation location) : base(location) {}
    }

    /// <summary>
    /// Used to indication how Assets are loaded from the AssetBundle on the first load request.
    /// </summary>
    public enum AssetLoadMode
    {
        /// <summary>
        /// Only load the requested Asset and Dependencies
        /// </summary>
        RequestedAssetAndDependencies = 0,
        /// <summary>
        /// Load all assets inside the AssetBundle
        /// </summary>
        AllPackedAssetsAndDependencies,
    }

    /// <summary>
    /// Wrapper for asset bundles.
    /// </summary>
    public interface IAssetBundleResource
    {
        /// <summary>
        /// Retrieves the asset bundle.
        /// </summary>
        /// <returns>Returns the asset bundle.</returns>
        AssetBundle GetAssetBundle();
    }

    /// <summary>
    /// Contains cache information to be used by the AssetBundleProvider
    /// </summary>
    [Serializable]
    public class AssetBundleRequestOptions : ILocationSizeData
    {
        [FormerlySerializedAs("m_hash")]
        [SerializeField]
        string m_Hash = "";
        /// <summary>
        /// Hash value of the asset bundle.
        /// </summary>
        public string Hash { get { return m_Hash; } set { m_Hash = value; } }
        [FormerlySerializedAs("m_crc")]
        [SerializeField]
        uint m_Crc;
        /// <summary>
        /// CRC value of the bundle.
        /// </summary>
        public uint Crc { get { return m_Crc; } set { m_Crc = value; } }
        [FormerlySerializedAs("m_timeout")]
        [SerializeField]
        int m_Timeout;
        /// <summary>
        /// Attempt to abort after the number of seconds in timeout have passed, where the UnityWebRequest has received no data.
        /// </summary>
        public int Timeout { get { return m_Timeout; } set { m_Timeout = value; } }
        [FormerlySerializedAs("m_chunkedTransfer")]
        [SerializeField]
        bool m_ChunkedTransfer;
        /// <summary>
        /// Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method.
        /// </summary>
        public bool ChunkedTransfer { get { return m_ChunkedTransfer; } set { m_ChunkedTransfer = value; } }
        [FormerlySerializedAs("m_redirectLimit")]
        [SerializeField]
        int m_RedirectLimit = -1;
        /// <summary>
        /// Indicates the number of redirects which this UnityWebRequest will follow before halting with a “Redirect Limit Exceeded” system error.
        /// </summary>
        public int RedirectLimit { get { return m_RedirectLimit; } set { m_RedirectLimit = value; } }
        [FormerlySerializedAs("m_retryCount")]
        [SerializeField]
        int m_RetryCount;
        /// <summary>
        /// Indicates the number of times the request will be retried.
        /// </summary>
        public int RetryCount { get { return m_RetryCount; } set { m_RetryCount = value; } }

        [SerializeField]
        string m_BundleName = null;
        /// <summary>
        /// The name of the original bundle.  This does not contain the appended hash.
        /// </summary>
        public string BundleName { get { return m_BundleName; } set { m_BundleName = value; } }

        [SerializeField]
        AssetLoadMode m_AssetLoadMode = AssetLoadMode.RequestedAssetAndDependencies;
        /// <summary>
        /// Determines how Assets are loaded when accessed.
        /// </summary>
        /// <remarks>
        /// Requested Asset And Dependencies, will only load the requested Asset (Recommended).
        /// All Packed Assets And Dependencies, will load all Assets that are packed together. Best used when loading all Assets into memory is required.
        ///</remarks>
        public AssetLoadMode AssetLoadMode { get { return m_AssetLoadMode; } set { m_AssetLoadMode = value; } }

        [SerializeField]
        long m_BundleSize;
        /// <summary>
        /// The size of the bundle, in bytes.
        /// </summary>
        public long BundleSize { get { return m_BundleSize; } set { m_BundleSize = value; } }

        [SerializeField]
        bool m_UseCrcForCachedBundles;
        /// <summary>
        /// If false, the CRC will not be used when loading bundles from the cache.
        /// </summary>
        public bool UseCrcForCachedBundle { get { return m_UseCrcForCachedBundles; } set { m_UseCrcForCachedBundles = value; } }

        [SerializeField]
        bool m_UseUWRForLocalBundles;
        /// <summary>
        /// If true, UnityWebRequest will be used even if the bundle is stored locally.
        /// </summary>
        public bool UseUnityWebRequestForLocalBundles { get { return m_UseUWRForLocalBundles; } set { m_UseUWRForLocalBundles = value; } }

        [SerializeField]
        bool m_ClearOtherCachedVersionsWhenLoaded;
        /// <summary>
        /// If false, the CRC will not be used when loading bundles from the cache.
        /// </summary>
        public bool ClearOtherCachedVersionsWhenLoaded { get { return m_ClearOtherCachedVersionsWhenLoaded; } set { m_ClearOtherCachedVersionsWhenLoaded = value; } }

        /// <summary>
        /// Computes the amount of data needed to be downloaded for this bundle.
        /// </summary>
        /// <param name="location">The location of the bundle.</param>
        /// <param name="resourceManager">The object that contains all the resource locations.</param>
        /// <returns>The size in bytes of the bundle that is needed to be downloaded.  If the local cache contains the bundle or it is a local bundle, 0 will be returned.</returns>
        public virtual long ComputeSize(IResourceLocation location, ResourceManager resourceManager)
        {
            var id = resourceManager == null ? location.InternalId : resourceManager.TransformInternalId(location);
            if (!ResourceManagerConfig.IsPathRemote(id))
                return 0;
            var locHash = Hash128.Parse(Hash);
#if ENABLE_CACHING
            if (locHash.isValid) //If we have a hash, ensure that our desired version is cached.
            {
                if (Caching.IsVersionCached(new CachedAssetBundle(BundleName, locHash)))
                    return 0;
                return BundleSize;
            }
#endif //ENABLE_CACHING
            return BundleSize;
        }
    }

    internal class AssetBundleResource : IAssetBundleResource, IUpdateReceiver
    {
        internal enum LoadType
        {
            None,
            Local,
            Web
        }

        AssetBundle m_AssetBundle;
        DownloadHandlerAssetBundle m_downloadHandler;
        AsyncOperation m_RequestOperation;
        WebRequestQueueOperation m_WebRequestQueueOperation;
        internal ProvideHandle m_ProvideHandle;
        internal AssetBundleRequestOptions m_Options;
        [NonSerialized]
        bool m_WebRequestCompletedCallbackCalled = false;
        int m_Retries;
        long m_BytesToDownload;
        long m_DownloadedBytes;
        bool m_Completed = false;
#if UNLOAD_BUNDLE_ASYNC
        AssetBundleUnloadOperation m_UnloadOperation;
#endif
        const int k_WaitForWebRequestMainThreadSleep = 1;
        string m_TransformedInternalId;
        AssetBundleRequest m_PreloadRequest;
        bool m_PreloadCompleted = false;
        ulong m_LastDownloadedByteCount = 0;
        float m_TimeoutTimer = 0;
        int m_TimeoutOverFrames = 0;

        private bool HasTimedOut => m_TimeoutTimer >= m_Options.Timeout && m_TimeoutOverFrames > 5;

        internal long BytesToDownload
        {
            get
            {
                if (m_BytesToDownload == -1)
                {
                    if (m_Options != null)
                        m_BytesToDownload = m_Options.ComputeSize(m_ProvideHandle.Location, m_ProvideHandle.ResourceManager);
                    else
                        m_BytesToDownload = 0;
                }
                return m_BytesToDownload;
            }
        }

        internal UnityWebRequest CreateWebRequest(IResourceLocation loc)
        {
            var url = m_ProvideHandle.ResourceManager.TransformInternalId(loc);
            return CreateWebRequest(url);
        }

        internal UnityWebRequest CreateWebRequest(string url)
        {
            if (m_Options == null)
                return UnityWebRequestAssetBundle.GetAssetBundle(url);
            UnityWebRequest webRequest;
            if (!string.IsNullOrEmpty(m_Options.Hash))
            {
                CachedAssetBundle cachedBundle = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
#if ENABLE_CACHING
                if (m_Options.UseCrcForCachedBundle || !Caching.IsVersionCached(cachedBundle))
                    webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle, m_Options.Crc);
                else
                    webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle);
#else
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, cachedBundle, m_Options.Crc);
#endif
            }
            else
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, m_Options.Crc);

            if (m_Options.RedirectLimit > 0)
                webRequest.redirectLimit = m_Options.RedirectLimit;
            if (m_ProvideHandle.ResourceManager.CertificateHandlerInstance != null)
            {
                webRequest.certificateHandler = m_ProvideHandle.ResourceManager.CertificateHandlerInstance;
                webRequest.disposeCertificateHandlerOnDispose = false;
            }

            m_ProvideHandle.ResourceManager.WebRequestOverride?.Invoke(webRequest);
            return webRequest;
        }

        internal AssetBundleRequest GetAssetPreloadRequest()
        {
            if (m_PreloadCompleted || GetAssetBundle() == null)
                return null;

            if (m_Options.AssetLoadMode == AssetLoadMode.AllPackedAssetsAndDependencies)
            {
#if !UNITY_2021_1_OR_NEWER
                if (AsyncOperationHandle.IsWaitingForCompletion)
                {
                    m_AssetBundle.LoadAllAssets();
                    m_PreloadCompleted = true;
                    return null;
                }
#endif
                if (m_PreloadRequest == null)
                {
                    m_PreloadRequest = m_AssetBundle.LoadAllAssetsAsync();
                    m_PreloadRequest.completed += operation => m_PreloadCompleted = true;
                }
                return m_PreloadRequest;
            }

            return null;
        }

        float PercentComplete() { return m_RequestOperation != null ? m_RequestOperation.progress : 0.0f; }

        DownloadStatus GetDownloadStatus()
        {
            if (m_Options == null)
                return default;
            var status = new DownloadStatus() { TotalBytes = BytesToDownload, IsDone = PercentComplete() >= 1f };
            if (BytesToDownload > 0)
            {
                if (m_WebRequestQueueOperation != null && string.IsNullOrEmpty(m_WebRequestQueueOperation.m_WebRequest.error))
                    m_DownloadedBytes = (long)(m_WebRequestQueueOperation.m_WebRequest.downloadedBytes);
                else if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation operation && string.IsNullOrEmpty(operation.webRequest.error))
                    m_DownloadedBytes = (long)operation.webRequest.downloadedBytes;
            }

            status.DownloadedBytes = m_DownloadedBytes;
            return status;
        }

        /// <summary>
        /// Get the asset bundle object managed by this resource.  This call may force the bundle to load if not already loaded.
        /// </summary>
        /// <returns>The asset bundle.</returns>
        public AssetBundle GetAssetBundle()
        {
            if (m_AssetBundle == null)
            {
                if (m_downloadHandler != null)
                {
                    m_AssetBundle = m_downloadHandler.assetBundle;
                    m_downloadHandler.Dispose();
                    m_downloadHandler = null;
                }
                else if (m_RequestOperation is AssetBundleCreateRequest)
                {
                    m_AssetBundle = (m_RequestOperation as AssetBundleCreateRequest).assetBundle;
                }
            }
            return m_AssetBundle;
        }
#if UNLOAD_BUNDLE_ASYNC
        void OnUnloadOperationComplete(AsyncOperation op)
        {
            m_UnloadOperation = null;
            BeginOperation();
        }
#endif

#if UNLOAD_BUNDLE_ASYNC
        internal void Start(ProvideHandle provideHandle, AssetBundleUnloadOperation unloadOp)
#else
        internal void Start(ProvideHandle provideHandle)
#endif
        {
            m_Retries = 0;
            m_AssetBundle = null;
            m_downloadHandler = null;
            m_RequestOperation = null;
            m_WebRequestCompletedCallbackCalled = false;
            m_ProvideHandle = provideHandle;
            m_Options = m_ProvideHandle.Location.Data as AssetBundleRequestOptions;
            m_BytesToDownload = -1;
            m_ProvideHandle.SetProgressCallback(PercentComplete);
            m_ProvideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
            m_ProvideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
#if UNLOAD_BUNDLE_ASYNC
            m_UnloadOperation = unloadOp;
            if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
                m_UnloadOperation.completed += OnUnloadOperationComplete;
            else
#endif
                BeginOperation();
        }

        private bool WaitForCompletionHandler()
        {
#if UNLOAD_BUNDLE_ASYNC
            if (m_UnloadOperation != null && !m_UnloadOperation.isDone)
            {
                m_UnloadOperation.completed -= OnUnloadOperationComplete;
                m_UnloadOperation.WaitForCompletion();
                m_UnloadOperation = null;
                BeginOperation();
            }
#endif

            if (m_RequestOperation == null)
            {
                if (m_WebRequestQueueOperation == null)
                    return false;
                else
                    WebRequestQueue.WaitForRequestToBeActive(m_WebRequestQueueOperation, k_WaitForWebRequestMainThreadSleep);
            }

            //We don't want to wait for request op to complete if it's a LoadFromFileAsync. Only UWR will complete in a tight loop like this.
            if (m_RequestOperation is UnityWebRequestAsyncOperation op)
            {
                while (!UnityWebRequestUtilities.IsAssetBundleDownloaded(op))
                    System.Threading.Thread.Sleep(k_WaitForWebRequestMainThreadSleep);
            }
            
            if (m_RequestOperation is UnityWebRequestAsyncOperation && !m_WebRequestCompletedCallbackCalled)
            {
                WebRequestOperationCompleted(m_RequestOperation);
                m_RequestOperation.completed -= WebRequestOperationCompleted;
            }
            
            var assetBundle = GetAssetBundle();
            if (!m_Completed && m_RequestOperation.isDone)
            {
                m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
                m_Completed = true;
            }

            return m_Completed;
        }

        void AddCallbackInvokeIfDone(AsyncOperation operation, Action<AsyncOperation> callback)
        {
            if (operation.isDone)
                callback(operation);
            else
                operation.completed += callback;
        }

        internal static void GetLoadInfo(ProvideHandle handle, out LoadType loadType, out string path)
        {
            GetLoadInfo(handle.Location, handle.ResourceManager, out loadType, out path);
        }

        internal static void GetLoadInfo(IResourceLocation location, ResourceManager resourceManager, out LoadType loadType, out string path)
        {
            var options = location?.Data as AssetBundleRequestOptions;
            if (options == null)
            {
                loadType = LoadType.None;
                path = null;
                return;
            }

            path = resourceManager.TransformInternalId(location);
            if (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:"))
                loadType = options.UseUnityWebRequestForLocalBundles ? LoadType.Web : LoadType.Local;
            else if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
                loadType = LoadType.Web;
            else if (options.UseUnityWebRequestForLocalBundles)
            {
                path = "file:///" + Path.GetFullPath(path);
                loadType = LoadType.Web;
            }
            else
                loadType = LoadType.Local;
        }

        private void BeginOperation()
        {
            m_DownloadedBytes = 0;
            GetLoadInfo(m_ProvideHandle, out LoadType loadType, out m_TransformedInternalId);

            if (loadType == LoadType.Local)
            {
#if !UNITY_2021_1_OR_NEWER
                if (AsyncOperationHandle.IsWaitingForCompletion)
                    CompleteBundleLoad(AssetBundle.LoadFromFile(m_TransformedInternalId, m_Options == null ? 0 : m_Options.Crc));
                else
#endif
                {
                    m_RequestOperation = AssetBundle.LoadFromFileAsync(m_TransformedInternalId, m_Options == null ? 0 : m_Options.Crc);
                    AddCallbackInvokeIfDone(m_RequestOperation, LocalRequestOperationCompleted);
                }
            }
            else if (loadType == LoadType.Web)
            {
                m_WebRequestCompletedCallbackCalled = false;
                var req = CreateWebRequest(m_TransformedInternalId);
#if ENABLE_ASYNC_ASSETBUNDLE_UWR
                ((DownloadHandlerAssetBundle)req.downloadHandler).autoLoadAssetBundle = !(m_ProvideHandle.Location is DownloadOnlyLocation);
#endif
                req.disposeDownloadHandlerOnDispose = false;

                m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(req);
                if (m_WebRequestQueueOperation.IsDone)
                    BeginWebRequestOperation(m_WebRequestQueueOperation.Result);
                else
                    m_WebRequestQueueOperation.OnComplete += asyncOp => BeginWebRequestOperation(asyncOp);
            }
            else
            {
                m_RequestOperation = null;
                m_ProvideHandle.Complete<AssetBundleResource>(null, false, new RemoteProviderException(string.Format("Invalid path in AssetBundleProvider: '{0}'.", m_TransformedInternalId), m_ProvideHandle.Location));
                m_Completed = true;
            }
        }

        private void BeginWebRequestOperation(AsyncOperation asyncOp)
        {
            m_TimeoutTimer = 0;
            m_TimeoutOverFrames = 0;
            m_LastDownloadedByteCount = 0;
            m_RequestOperation = asyncOp;
            if (m_RequestOperation == null || m_RequestOperation.isDone)
                WebRequestOperationCompleted(m_RequestOperation);
            else
            {
                if (m_Options.Timeout > 0)
                    m_ProvideHandle.ResourceManager.AddUpdateReceiver(this);
                m_RequestOperation.completed += WebRequestOperationCompleted;
            }
        }

        public void Update(float unscaledDeltaTime)
        {
            if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation operation && !operation.isDone)
            {
                if (m_LastDownloadedByteCount != operation.webRequest.downloadedBytes)
                {
                    m_TimeoutTimer = 0;
                    m_TimeoutOverFrames = 0;
                    m_LastDownloadedByteCount = operation.webRequest.downloadedBytes;
                }
                else
                {
                    m_TimeoutTimer += unscaledDeltaTime;
                    if (HasTimedOut)
                        operation.webRequest.Abort();
                    m_TimeoutOverFrames++;
                }
            }
        }

        private void LocalRequestOperationCompleted(AsyncOperation op)
        {
            CompleteBundleLoad((op as AssetBundleCreateRequest).assetBundle);
        }

        private void CompleteBundleLoad(AssetBundle bundle)
        {
            m_AssetBundle = bundle;
            if (m_AssetBundle != null)
                m_ProvideHandle.Complete(this, true, null);
            else
                m_ProvideHandle.Complete<AssetBundleResource>(null, false, new RemoteProviderException(string.Format("Invalid path in AssetBundleProvider: '{0}'.", m_TransformedInternalId), m_ProvideHandle.Location));
            m_Completed = true;
        }

        private void WebRequestOperationCompleted(AsyncOperation op)
        {
            if (m_WebRequestCompletedCallbackCalled)
                return;

            if (m_Options.Timeout > 0)
                m_ProvideHandle.ResourceManager.RemoveUpdateReciever(this);

            m_WebRequestCompletedCallbackCalled = true;
            UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
            var webReq = remoteReq?.webRequest;
            m_downloadHandler = webReq?.downloadHandler as DownloadHandlerAssetBundle;
            UnityWebRequestResult uwrResult = null;
            if (webReq != null && !UnityWebRequestUtilities.RequestHasErrors(webReq, out uwrResult))
            {
                if (!m_Completed)
                {
                    m_ProvideHandle.Complete(this, true, null);
                    m_Completed = true;
                }
#if ENABLE_CACHING
                if (!string.IsNullOrEmpty(m_Options.Hash) && m_Options.ClearOtherCachedVersionsWhenLoaded)
                    Caching.ClearOtherCachedVersions(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
#endif
            }
            else
            {
                if (HasTimedOut)
                    uwrResult.Error = "Request timeout";
                webReq = m_WebRequestQueueOperation.m_WebRequest;
                if (uwrResult == null)
                    uwrResult = new UnityWebRequestResult(m_WebRequestQueueOperation.m_WebRequest);

                m_downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
                bool forcedRetry = false;
                string message = $"Web request failed, retrying ({m_Retries}/{m_Options.RetryCount})...\n{uwrResult}";
#if ENABLE_CACHING
                if (!string.IsNullOrEmpty(m_Options.Hash))
                {
                    CachedAssetBundle cab = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
                    if (Caching.IsVersionCached(cab))
                    {
                        message = $"Web request failed to load from cache. The cached AssetBundle will be cleared from the cache and re-downloaded. Retrying...\n{uwrResult}";
                        Caching.ClearCachedVersion(cab.name, cab.hash);
                        if (m_Options.RetryCount == 0 && m_Retries == 0)
                        {
                            Debug.LogFormat(message);
                            BeginOperation();
                            m_Retries++; //Will prevent us from entering an infinite loop of retrying if retry count is 0
                            forcedRetry = true;
                        }
                    }
                }
#endif
                if (!forcedRetry)
                {
                    if (m_Retries < m_Options.RetryCount && uwrResult.ShouldRetryDownloadError())
                    {
                        m_Retries++;
                        Debug.LogFormat(message);
                        BeginOperation();
                    }
                    else
                    {
                        var exception = new RemoteProviderException($"Unable to load asset bundle from : {webReq.url}", m_ProvideHandle.Location, uwrResult);
                        m_ProvideHandle.Complete<AssetBundleResource>(null, false, exception);
                        m_Completed = true;
                    }
                }
            }
            webReq.Dispose();
        }

        /// <summary>
        /// Unloads all resources associated with this asset bundle.
        /// </summary>
#if UNLOAD_BUNDLE_ASYNC
        public bool Unload(out AssetBundleUnloadOperation unloadOp)
#else
        public void Unload()
#endif
        {
#if UNLOAD_BUNDLE_ASYNC
            unloadOp = null;
            if (m_AssetBundle != null)
            {
                unloadOp = m_AssetBundle.UnloadAsync(true);
                m_AssetBundle = null;
            }
#else
            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
#endif
            if (m_downloadHandler != null)
            {
                m_downloadHandler.Dispose();
                m_downloadHandler = null;
            }
            m_RequestOperation = null;
#if UNLOAD_BUNDLE_ASYNC
            return unloadOp != null;
#endif
        }
    }

    /// <summary>
    /// IResourceProvider for asset bundles.  Loads bundles via UnityWebRequestAssetBundle API if the internalId starts with "http".  If not, it will load the bundle via AssetBundle.LoadFromFileAsync.
    /// </summary>
    [DisplayName("AssetBundle Provider")]
    public class AssetBundleProvider : ResourceProviderBase
    {
#if UNLOAD_BUNDLE_ASYNC
        static Dictionary<string, AssetBundleUnloadOperation> m_UnloadingBundles = new Dictionary<string, AssetBundleUnloadOperation>();
        internal static int UnloadingAssetBundleCount => m_UnloadingBundles.Count;
        internal static int AssetBundleCount => AssetBundle.GetAllLoadedAssetBundles().Count() - UnloadingAssetBundleCount;
        internal static void WaitForAllUnloadingBundlesToComplete()
        {
            if (UnloadingAssetBundleCount > 0)
            {
                var bundles = m_UnloadingBundles.Values.ToArray();
                foreach(var b in bundles)
                    b.WaitForCompletion();
            }
        }
#else
        internal static void WaitForAllUnloadingBundlesToComplete() { }
#endif

        /// <inheritdoc/>
        public override void Provide(ProvideHandle providerInterface)
        {
#if UNLOAD_BUNDLE_ASYNC
            if (m_UnloadingBundles.TryGetValue(providerInterface.Location.InternalId, out var unloadOp))
            {
                if (unloadOp.isDone)
                    unloadOp = null;
            }
            new AssetBundleResource().Start(providerInterface, unloadOp);
#else
            new AssetBundleResource().Start(providerInterface);
#endif

        }

        /// <inheritdoc/>
        public override Type GetDefaultType(IResourceLocation location)
        {
            return typeof(IAssetBundleResource);
        }

        /// <summary>
        /// Releases the asset bundle via AssetBundle.Unload(true).
        /// </summary>
        /// <param name="location">The location of the asset to release</param>
        /// <param name="asset">The asset in question</param>
        public override void Release(IResourceLocation location, object asset)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (asset == null)
            {
                Debug.LogWarningFormat("Releasing null asset bundle from location {0}.  This is an indication that the bundle failed to load.", location);
                return;
            }
            var bundle = asset as AssetBundleResource;
            if (bundle != null)
            {
#if UNLOAD_BUNDLE_ASYNC
                if (bundle.Unload(out var unloadOp))
                {
                    m_UnloadingBundles.Add(location.InternalId, unloadOp);
                    unloadOp.completed += op => m_UnloadingBundles.Remove(location.InternalId);
                }
#else
                bundle.Unload();
#endif
                return;
            }
        }
    }
}
