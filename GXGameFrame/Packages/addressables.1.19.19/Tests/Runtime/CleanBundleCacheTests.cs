using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace AddressableAssetsIntegrationTests
{
    public abstract class CleanBundleCacheTests : AddressablesTestFixture
    {
        const int kAssetCount = 5;
        const int kMaxConcurrentRequests = 3;

        const string kOldBuildId = "Old";
        const string kNewBuildId = "New";

        string GetNonRefAsset(int i) { return $"{kOldBuildId}{i}"; }
        string GetRefAsset(int i) { return $"{kNewBuildId}{i}"; }

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            CreateGroup(settings, tempAssetFolder, kOldBuildId, GetNonRefAsset);
            RunBuilder(settings, m_UniqueTestName + kOldBuildId);

            settings = AddressableAssetSettings.Create(Path.Combine(tempAssetFolder, "Settings" + kNewBuildId), "AddressableAssetSettings.Tests", false, true);
            CreateGroup(settings, tempAssetFolder, kNewBuildId, GetRefAsset);
            RunBuilder(settings, m_UniqueTestName + kNewBuildId);
        }

        AddressableAssetGroup CreateGroup(AddressableAssetSettings settings, string tempAssetFolder, string buildId, Func<int, string> objNaming)
        {
            AddressableAssetGroup remoteGroup = settings.CreateGroup($"Remote{buildId}", false, false, true,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            remoteGroup.GetSchema<BundledAssetGroupSchema>().UseUnityWebRequestForLocalBundles = true;
            remoteGroup.GetSchema<BundledAssetGroupSchema>().BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            remoteGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;

            settings.MaxConcurrentWebRequests = kMaxConcurrentRequests;

            for (int i = 0; i < kAssetCount; i++)
            {
                string s = objNaming(i);
                string guid = CreatePrefab(tempAssetFolder + $"/{s}.prefab");
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, remoteGroup);
                entry.address = s;
            }

            return remoteGroup;
        }

        protected override void RunBuilder(AddressableAssetSettings settings)
        {
            // Do nothing, we build content during our custom Setup()
        }

#endif
        [UnitySetUp]
        public override IEnumerator RuntimeSetup()
        {
            // Do nothing, we initialize Addressables in our tests
            yield return null;
        }

        [TearDown]
        public override void RuntimeTeardown()
        {
            // Do nothing, we release Addressables in our tests
        }

        protected override void OnRuntimeSetup()
        {
#if ENABLE_CACHING
            Caching.ClearCache();
#endif
            Assert.IsNull(m_Addressables);
        }

        IEnumerator InitializeSettings(string buildId)
        {
            if (m_Addressables != null)
                m_Addressables.ResourceManager.Dispose();
            m_Addressables = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
            m_RuntimeSettingsPath = m_Addressables.ResolveInternalId(GetRuntimeAddressablesSettingsPath(m_UniqueTestName + buildId));
            var op = m_Addressables.InitializeAsync(m_RuntimeSettingsPath, null, false);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            if (op.IsValid())
                op.Release();
        }

        void ReleaseAddressables()
        {
            if (m_Addressables != null)
                m_Addressables.ResourceManager.Dispose();
            m_Addressables = null;
        }

        IEnumerator CacheEntries(Func<int, string> keyNaming)
        {
            for (int i = 0; i < kAssetCount; i++)
            {
                var op = m_Addressables.LoadAssetAsync<GameObject>(keyNaming(i));
                yield return op;
                op.Release();
            }
        }

        Dictionary<string, string> GetCacheEntries(string key)
        {
            var cacheEntries = new Dictionary<string, string>();
            m_Addressables.GetResourceLocations(new object[] { key }, typeof(GameObject), Addressables.MergeMode.Intersection, out IList<IResourceLocation> locations);
            Assert.AreEqual(1, locations?.Count);

            if (locations[0].HasDependencies)
            {
                foreach (IResourceLocation dep in locations[0].Dependencies)
                {
                    if (dep.Data is AssetBundleRequestOptions options)
                    {
                        if (!cacheEntries.ContainsKey(options.BundleName))
                            cacheEntries.Add(options.BundleName, options.Hash);
                    }
                }
            }
            return cacheEntries;
        }

#if ENABLE_CACHING
        HashSet<CachedAssetBundle> GetAllEntries(Func<int, string> keyNaming)
        {
            var cacheEntries = new HashSet<CachedAssetBundle>();
            for (int i = 0; i < kAssetCount; i++)
            {
                var result = GetCacheEntries(keyNaming(i));
                foreach (var entry in result)
                {
                    CachedAssetBundle cab = new CachedAssetBundle(entry.Key, Hash128.Parse(entry.Value));
                    if (!cacheEntries.Contains(cab))
                        cacheEntries.Add(cab);
                }
            }
            return cacheEntries;
        }

        void AssertEntriesAreRemoved(HashSet<CachedAssetBundle> entries)
        {
            foreach (CachedAssetBundle entry in entries)
            {
                Assert.IsFalse(Caching.IsVersionCached(entry));
            }
        }

        void AssertEntriesArePreserved(HashSet<CachedAssetBundle> entries)
        {
            foreach (CachedAssetBundle entry in entries)
            {
                Assert.IsTrue(Caching.IsVersionCached(entry));
            }
        }

#endif

        [UnityTest]
        public IEnumerator WhenValidCatalogId_RemovesNonReferencedBundlesFromCache([Values(true, false)] bool forceSingleThreading)
        {
#if ENABLE_CACHING
            if (BuildScriptMode == TestBuildScriptMode.Fast)
                Assert.Ignore("Bundle caching does not occur when using this playmode.");

            yield return InitializeSettings(kOldBuildId);
            yield return CacheEntries(GetNonRefAsset);
            HashSet<CachedAssetBundle> entriesToRemove = GetAllEntries(GetNonRefAsset);

            yield return InitializeSettings(kNewBuildId);
            yield return CacheEntries(GetRefAsset);
            HashSet<CachedAssetBundle> entriesToPreserve = GetAllEntries(GetRefAsset);
            entriesToRemove.ExceptWith(entriesToPreserve);

            string locatorId = m_Addressables.m_ResourceLocators[0].Locator.LocatorId;
            var handle = m_Addressables.CleanBundleCache(new List<string> { locatorId }, forceSingleThreading);
            yield return handle;
            handle.Release();

            AssertEntriesAreRemoved(entriesToRemove);
            AssertEntriesArePreserved(entriesToPreserve);

            ReleaseAddressables();
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator WhenCatalogIdListNull_UsesLoadedCatalogs_AndRemovesNonReferencedBundlesFromCache()
        {
#if ENABLE_CACHING
            if (BuildScriptMode == TestBuildScriptMode.Fast)
                Assert.Ignore("Bundle caching does not occur when using this playmode.");

            yield return InitializeSettings(kOldBuildId);
            yield return CacheEntries(GetNonRefAsset);
            HashSet<CachedAssetBundle> entriesToRemove = GetAllEntries(GetNonRefAsset);

            yield return InitializeSettings(kNewBuildId);
            yield return CacheEntries(GetRefAsset);
            HashSet<CachedAssetBundle> entriesToPreserve = GetAllEntries(GetRefAsset);
            entriesToRemove.ExceptWith(entriesToPreserve);

            var handle = m_Addressables.CleanBundleCache(null, false);
            yield return handle;
            handle.Release();

            AssertEntriesAreRemoved(entriesToRemove);
            AssertEntriesArePreserved(entriesToPreserve);

            ReleaseAddressables();
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator WhenCatalogIdListNull_AndUsingFastMode_ReturnsException()
        {
#if ENABLE_CACHING
            if (BuildScriptMode != TestBuildScriptMode.Fast)
                Assert.Ignore("Test only intended to run on fast mode.");

            yield return InitializeSettings(kOldBuildId);

            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var handle = m_Addressables.CleanBundleCache(null, false);
            yield return handle;

            Assert.AreEqual("Provided catalogs do not load data from a catalog file. This can occur when using the \"Use Asset Database (fastest)\" playmode script. Bundle cache was not modified.", handle.OperationException.Message);
            handle.Release();

            LogAssert.ignoreFailingMessages = ifm;
            ReleaseAddressables();
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator WhenInvalidCatalogId_ReturnsException()
        {
#if ENABLE_CACHING
            if (BuildScriptMode == TestBuildScriptMode.Fast)
                Assert.Ignore("Bundle caching does not occur when using this playmode.");

            yield return InitializeSettings(kOldBuildId);

            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var handle = m_Addressables.CleanBundleCache(new List<string> { "invalidId" }, false);
            yield return handle;

            Assert.AreEqual("Provided catalogs do not load data from a catalog file. This can occur when using the \"Use Asset Database (fastest)\" playmode script. Bundle cache was not modified.", handle.OperationException.Message);
            handle.Release();

            LogAssert.ignoreFailingMessages = ifm;
            ReleaseAddressables();
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator WhenAnotherOpAlreadyInProgress_ReturnsException()
        {
#if ENABLE_CACHING
            if (BuildScriptMode == TestBuildScriptMode.Fast)
                Assert.Ignore("Bundle caching does not occur when using this playmode.");

            yield return InitializeSettings(kOldBuildId);

            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var handle = m_Addressables.CleanBundleCache(null, false);
            var handle2 = m_Addressables.CleanBundleCache(null, false);
            yield return handle2;

            Assert.AreEqual("Bundle cache is already being cleaned.", handle2.OperationException.Message);
            handle2.Release();

            yield return handle;
            handle.Release();
            LogAssert.ignoreFailingMessages = ifm;
            ReleaseAddressables();
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator WhenCachingDisabled_CleanBundleCache_ReturnsException()
        {
#if !ENABLE_CACHING
            if (BuildScriptMode == TestBuildScriptMode.Fast)
                Assert.Ignore("Bundle caching does not occur when using this playmode.");

            yield return InitializeSettings(kOldBuildId);

            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var handle = m_Addressables.CleanBundleCache(null, false);
            yield return handle;

            Assert.AreEqual("Caching not enabled. There is no bundle cache to modify.", handle.OperationException.Message);
            handle.Release();
            LogAssert.ignoreFailingMessages = ifm;
            ReleaseAddressables();
#else
            Assert.Ignore("Caching is enabled, but test expects to run on caching-disabled platforms.");
            yield return null;
#endif
        }
    }

#if UNITY_EDITOR
    class CleanBundleCacheTests_FastMode : CleanBundleCacheTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }
    class CleanBundleCacheTests_PackedPlaymodeMode : CleanBundleCacheTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class CleanBundleCacheTests_PackedMode : CleanBundleCacheTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
