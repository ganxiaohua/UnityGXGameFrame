#if ENABLE_CACHING
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using static UnityEngine.ResourceManagement.ResourceProviders.AssetBundleResource;

namespace UnityEngine.AddressableAssets
{
    class CleanBundleCacheOperation : AsyncOperationBase<bool>, IUpdateReceiver
    {
        AddressablesImpl m_Addressables;
        AsyncOperationHandle<IList<AsyncOperationHandle>> m_DepOp;

        List<string> m_CacheDirsForRemoval;
        Thread m_EnumerationThread;
        string m_BaseCachePath;

        bool m_UseMultiThreading;

        public CleanBundleCacheOperation(AddressablesImpl aa, bool forceSingleThreading)
        {
            m_Addressables = aa;
            m_UseMultiThreading = forceSingleThreading ? false : PlatformUtilities.PlatformUsesMultiThreading(Application.platform);
        }

        public AsyncOperationHandle<bool> Start(AsyncOperationHandle<IList<AsyncOperationHandle>> depOp)
        {
            m_DepOp = depOp.Acquire();
            return m_Addressables.ResourceManager.StartOperation(this, m_DepOp);
        }

        public void CompleteInternal(bool result, bool success, string errorMsg)
        {
            m_DepOp.Release();
            Complete(result, success, errorMsg);
        }

        /// <inheritdoc />
        protected override bool InvokeWaitForCompletion()
        {
            if (!m_DepOp.IsDone)
                m_DepOp.WaitForCompletion();

            if (!HasExecuted)
                InvokeExecute();

            if (m_EnumerationThread != null)
            {
                m_EnumerationThread.Join();
                RemoveCacheEntries();
            }

            return IsDone;
        }

        protected override void Destroy()
        {
            if (m_DepOp.IsValid())
                m_DepOp.Release();
        }

        public override void GetDependencies(List<AsyncOperationHandle> dependencies)
        {
            dependencies.Add(m_DepOp);
        }

        protected override void Execute()
        {
            Assert.AreEqual(null, m_EnumerationThread, "CleanBundleCacheOperation has already executed. A worker thread has already been created.");

            if (m_DepOp.Status == AsyncOperationStatus.Failed)
                CompleteInternal(false, false, "Could not clean cache because a dependent catalog operation failed.");
            else
            {
                HashSet<string> cacheDirsInUse = GetCacheDirsInUse(m_DepOp.Result);

                if (!Caching.ready)
                    CompleteInternal(false, false, "Cache is not ready to be accessed.");

                m_BaseCachePath = Caching.currentCacheForWriting.path;
                if (m_UseMultiThreading)
                {
                    m_EnumerationThread = new Thread(DetermineCacheDirsNotInUse);
                    m_EnumerationThread.Start(cacheDirsInUse);
                }
                else
                {
                    DetermineCacheDirsNotInUse(cacheDirsInUse);
                    RemoveCacheEntries();
                }
            }
        }

        void IUpdateReceiver.Update(float unscaledDeltaTime)
        {
            if (m_UseMultiThreading && !m_EnumerationThread.IsAlive)
            {
                m_EnumerationThread = null;
                RemoveCacheEntries();
            }
        }

        void RemoveCacheEntries()
        {
            foreach (string cacheDir in m_CacheDirsForRemoval)
            {
                string bundlename = Path.GetFileName(cacheDir);
                Caching.ClearAllCachedVersions(bundlename);
            }
            CompleteInternal(true, true, null);
        }

        void DetermineCacheDirsNotInUse(object data)
        {
            DetermineCacheDirsNotInUse((HashSet<string>)data);
        }

        void DetermineCacheDirsNotInUse(HashSet<string> cacheDirsInUse)
        {
            m_CacheDirsForRemoval = new List<string>();
            foreach (var cacheDir in Directory.EnumerateDirectories(m_BaseCachePath, "*", SearchOption.TopDirectoryOnly))
            {
                if (!cacheDirsInUse.Contains(cacheDir))
                    m_CacheDirsForRemoval.Add(cacheDir);
            }
        }

        HashSet<string> GetCacheDirsInUse(IList<AsyncOperationHandle> catalogOps)
        {
            var cacheDirsInUse = new HashSet<string>();
            for (int i = 0; i < catalogOps.Count; i++)
            {
                var locator = catalogOps[i].Result as ResourceLocationMap;

                if (locator == null)
                {
                    var catData = catalogOps[i].Result as ContentCatalogData;
                    if (catData == null)
                        return cacheDirsInUse;
                    locator = catData.CreateCustomLocator(catData.location.PrimaryKey);
                }

                foreach (IList<IResourceLocation> locationList in locator.Locations.Values)
                {
                    foreach (IResourceLocation location in locationList)
                    {
                        if (location.Data is AssetBundleRequestOptions options)
                        {
                            GetLoadInfo(location, m_Addressables.ResourceManager, out LoadType loadType, out string path);
                            if (loadType == LoadType.Web)
                            {
                                string cacheDir = Path.Combine(Caching.currentCacheForWriting.path, options.BundleName); // Cache entries are named in this format "baseCachePath/bundleName/hash"
                                cacheDirsInUse.Add(cacheDir);
                            }
                        }
                    }
                }
            }
            return cacheDirsInUse;
        }
    }
}
#endif
