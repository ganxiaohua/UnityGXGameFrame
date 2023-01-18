using AddressableAssetsIntegrationTests;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
#endif

namespace UnityEngine.AddressableAssets.ResourceProviders.Tests
{
    [TestFixture]
    public abstract class DynamicContentUpdateTests : AddressablesTestFixture
    {
        class TestLocator : IResourceLocator
        {
            Dictionary<object, IList<IResourceLocation>> m_Locations = new Dictionary<object, IList<IResourceLocation>>();
            public IEnumerable<object> Keys => m_Locations.Keys;

            public string LocatorId { get; set; }
            public TestLocator(string id, params ResourceLocationBase[] locs)
            {
                LocatorId = id;
                foreach (var l in locs)
                    m_Locations.Add(l.PrimaryKey, new List<IResourceLocation>(new IResourceLocation[] { l }));
            }

            public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
            {
                return m_Locations.TryGetValue(key, out locations);
            }
        }

        class TestHashProvider : ResourceProviderBase
        {
            string m_Hash;
            public TestHashProvider(string id, string hash)
            {
                m_ProviderId = id;
                m_Hash = hash;
            }

            public override void Provide(ProvideHandle provideHandle)
            {
                provideHandle.Complete(m_Hash, true, null);
            }
        }

        class TestCatalogProvider : ResourceProviderBase
        {
            string m_LocatorId;
            public TestCatalogProvider(string locatorId)
            {
                m_LocatorId = locatorId;
            }

            public override void Provide(ProvideHandle provideHandle)
            {
                var deps = new List<object>();
                provideHandle.GetDependencies(deps);
                provideHandle.Complete(new TestLocator(m_LocatorId), true, null);
            }
        }

        const string kRemoteHashProviderId = "RemoteHashProvider";
        const string kLocalHashProviderId = "LocalHashProvider";
        const string kLocatorId = "Locator";
        const string kNewLocatorId = "NewLocator";

        [UnityTest]
        public IEnumerator CheckForUpdates_Returns_EmptyList_When_HashesMatch()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider), typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId), "same", catalogLoc);
            var op = m_Addressables.CheckForCatalogUpdates(false);
            yield return op;
            Assert.IsNotNull(op.Result);
            Assert.AreEqual(0, op.Result.Count);
            Assert.AreEqual(0, m_Addressables.CatalogsWithAvailableUpdates.Count());
            m_Addressables.Release(op);
        }

        [UnityTest]
        public IEnumerator CheckForUpdates_Returns_NonEmptyList_When_HashesDontMatch()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider), typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId), "same", catalogLoc);
            var op = m_Addressables.CheckForCatalogUpdates(false);
            yield return op;
            Assert.IsNotNull(op.Result);
            Assert.AreEqual(1, op.Result.Count);
            Assert.AreEqual(1, m_Addressables.CatalogsWithAvailableUpdates.Count());
            m_Addressables.Release(op);
        }

        [UnityTest]
        public IEnumerator CheckForUpdates_Initializes_Addressables()
        {
            m_Addressables.hasStartedInitialization = false;
            yield return m_Addressables.CheckForCatalogUpdates();
            Assert.IsTrue(m_Addressables.hasStartedInitialization);
        }

        [UnityTest]
        public IEnumerator CheckForUpdates_Returns_OnlyModifiedResults()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash1", "Remote", kRemoteHashProviderId + 1, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash1", "Local", kLocalHashProviderId + 1, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat1", "cat_id", nameof(TestCatalogProvider), typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            var remoteHashLoc2 = new ResourceLocationBase("RemoteHash2", "Remote", kRemoteHashProviderId + 2, typeof(string));
            var localHashLoc2 = new ResourceLocationBase("LocalHash2", "Local", kLocalHashProviderId + 2, typeof(string));
            var catalogLoc2 = new ResourceLocationBase("cat2", "cat_id", nameof(TestCatalogProvider), typeof(IResourceLocator), remoteHashLoc2, localHashLoc2);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId + 1, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId + 1, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId + 2, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId + 2, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId), "same", catalogLoc);
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId + 2), "same", catalogLoc2);

            var op = m_Addressables.CheckForCatalogUpdates(false);
            yield return op;
            Assert.IsNotNull(op.Result);
            Assert.AreEqual(1, op.Result.Count);
            Assert.AreEqual(kLocatorId + 2, op.Result[0]);
            Assert.AreEqual(1, m_Addressables.CatalogsWithAvailableUpdates.Count());
            Assert.AreEqual(kLocatorId + 2, m_Addressables.CatalogsWithAvailableUpdates.First());
            m_Addressables.Release(op);
        }

        [UnityTest]
        public IEnumerator UpdateContent_UpdatesCatalogs_Returns_ListOfLocators()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", typeof(TestCatalogProvider).FullName, typeof(object), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId, remoteHashLoc, localHashLoc, catalogLoc), "same", catalogLoc);
            var op = m_Addressables.CheckForCatalogUpdates(false);
            yield return op;
            Assert.IsNotNull(op.Result);
            Assert.AreEqual(1, op.Result.Count);
            var updateOp = m_Addressables.UpdateCatalogs(op.Result, false);
            m_Addressables.Release(op);

            yield return updateOp;
            Assert.IsNotNull(updateOp.Result);
            Assert.AreEqual(1, updateOp.Result.Count);
            Assert.AreEqual(kNewLocatorId, updateOp.Result[0].LocatorId);
            m_Addressables.Release(updateOp);
        }

        [UnityTest]
        public IEnumerator UpdateContent_UpdatesCatalogs_Returns_ListOfLocators_WhenCheckForUpdateIsNotCalled()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", typeof(TestCatalogProvider).FullName, typeof(object), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId, remoteHashLoc, localHashLoc, catalogLoc), "same", catalogLoc);

            var updateOp = m_Addressables.UpdateCatalogs(null, false);

            yield return updateOp;
            Assert.IsNotNull(updateOp.Result);
            Assert.AreEqual(1, updateOp.Result.Count);
            Assert.AreEqual(kNewLocatorId, updateOp.Result[0].LocatorId);
            m_Addressables.Release(updateOp);
        }

        [UnityTest]
        public IEnumerator UpdateContent_UpdatesCatalogs_WhenAutoCleanCacheEnabled_RemovesNonReferencedBundlesFromCache()
        {
#if ENABLE_CACHING
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", typeof(TestCatalogProvider).FullName, typeof(object), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId, remoteHashLoc, localHashLoc, catalogLoc), "same", catalogLoc);

            string cachedBundleName = "UpdatesCatalogsTestFakeBundle";
            string hash = "123";
            string fakeCachePath = AddressablesIntegrationTests.CreateFakeCachedBundle(cachedBundleName, hash);

            var updateOp = m_Addressables.UpdateCatalogs(null, false, true);
            yield return updateOp;

            string fakeCacheFolder = Path.GetDirectoryName(fakeCachePath);
            Assert.AreEqual(0, Directory.GetDirectories(fakeCacheFolder).Length);

            m_Addressables.Release(updateOp);
            Directory.Delete(fakeCacheFolder);
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator UpdateContent_UpdatesCatalogs_WhenAutoCleanCacheDisabled_NoBundlesRemovedFromCache()
        {
#if ENABLE_CACHING
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", typeof(TestCatalogProvider).FullName, typeof(object), remoteHashLoc, localHashLoc);

            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId, remoteHashLoc, localHashLoc, catalogLoc), "same", catalogLoc);

            string cachedBundleName = "UpdatesCatalogsTestFakeBundle";
            string hash = "123";
            string fakeCachePath = AddressablesIntegrationTests.CreateFakeCachedBundle(cachedBundleName, hash);

            var updateOp = m_Addressables.UpdateCatalogs(null, false, false);
            yield return updateOp;

            string fakeCacheFolder = Path.GetDirectoryName(fakeCachePath);
            Assert.IsTrue(Directory.GetDirectories(fakeCacheFolder).Length > 0);

            m_Addressables.Release(updateOp);
            Directory.Delete(fakeCacheFolder, true);
#else
            Assert.Ignore("Caching not enabled.");
            yield return null;
#endif
        }

        [UnityTest]
        public IEnumerator UpdateContent_UpdatesCatalogs_WhenAutoCleanCacheEnabled_AndCachingDisabled_ReturnsException()
        {
#if !ENABLE_CACHING && !PLATFORM_SWITCH
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", typeof(TestCatalogProvider).FullName, typeof(object), remoteHashLoc, localHashLoc);

            m_Addressables.RuntimePath = m_RuntimeSettingsPath;
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kRemoteHashProviderId, "different"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestHashProvider(kLocalHashProviderId, "same"));
            m_Addressables.ResourceManager.ResourceProviders.Add(new TestCatalogProvider(kNewLocatorId));
            m_Addressables.AddResourceLocator(new TestLocator(kLocatorId, remoteHashLoc, localHashLoc, catalogLoc), "same", catalogLoc);
            LogAssert.Expect(LogType.Error, "System.Exception: Caching not enabled. There is no bundle cache to modify.");
            var updateOp = m_Addressables.UpdateCatalogs(null, false, true);
            yield return updateOp;

            LogAssert.Expect(LogType.Error,
                "OperationException : CompletedOperation, status=Failed, result=False catalogs updated, but failed to clean bundle cache.");
            Assert.AreEqual(updateOp.OperationException.Message, "ChainOperation failed because dependent operation failed");
            Assert.AreEqual(updateOp.OperationException.InnerException.Message, "CompletedOperation, status=Failed, result=False catalogs updated, but failed to clean bundle cache.");

            m_Addressables.Release(updateOp);
#else
            Assert.Ignore("Caching is enabled, but test expects to run on caching-disabled platforms or platform was skipped.");
            yield return null;
#endif
        }

        internal class ExceptionTestOperation : AsyncOperationBase<string>
        {
            internal ExceptionTestOperation(Exception ex, string desiredResult, bool success = false)
            {
                m_RM = new ResourceManager();
                Complete(desiredResult, success, ex);
                Result = desiredResult;
            }

            protected override void Execute()
            {
            }
        }

        [Test]
        public void ProcessDependentOpResults_FailsWithErrorMessageOnInternalError()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider),
                typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            var op1 = new ExceptionTestOperation(new Exception("Bad operation"), null);
            var handle1 = new AsyncOperationHandle(op1);
            var results = new List<AsyncOperationHandle>();

            var loc = new TestLocator(kLocatorId);
            var locInfo = new AddressablesImpl.ResourceLocatorInfo(loc, "same", catalogLoc);
            var locInfos = new List<AddressablesImpl.ResourceLocatorInfo>();
            locInfos.Add(locInfo);

            var trivialHashes = new List<string>(new[] {"badHash"});
            results.Add(handle1);
            bool success;
            string errorString;
            var result =
                CheckCatalogsOperation.ProcessDependentOpResults(results, locInfos, trivialHashes, out errorString,
                    out success);

            LogAssert.Expect(LogType.Error, "System.Exception: Bad operation");
            Assert.AreEqual(false, success, "Operation should not succeed when underlying operation op1 has a non null OperationException");
            Assert.AreEqual(true, errorString.Contains("Bad operation"), "Error string should contain the error message thrown by the underlying operation");
            Assert.IsNull(result, "Result should be null in the case where every operation within it failed.");
        }

        [Test]
        public void ProcessDependentOpResults_SucceedsOnNoErrorMessage()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider),
                typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            var op1 = new ExceptionTestOperation(null, "good result", true);
            var handle1 = new AsyncOperationHandle(op1);
            var results = new List<AsyncOperationHandle>();

            var loc = new TestLocator(kLocatorId);
            var locInfo = new AddressablesImpl.ResourceLocatorInfo(loc, "same", catalogLoc);
            var locInfos = new List<AddressablesImpl.ResourceLocatorInfo>();
            locInfos.Add(locInfo);

            var trivialHashes = new List<string>(new[] {"same"});
            results.Add(handle1);
            bool success;
            string errorString;
            var result =
                CheckCatalogsOperation.ProcessDependentOpResults(results, locInfos, trivialHashes, out errorString,
                    out success);

            Assert.AreEqual(true, success, "Operation should succeed when underlying operation op1 has a null OperationException");
            Assert.IsNull(errorString, "Error string should be null when operation is succeeding without errors.");
            Assert.NotNull(result, "Result should only be null when every operation within it fails.");
        }

        [Test]
        public void ProcessDependentOpResults_ReturnsFailureOnOneErrorMessage()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider),
                typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            var op1 = new ExceptionTestOperation(null, "good result", true);
            var op2 = new ExceptionTestOperation(new Exception("Bad operation"), null);
            var handle1 = new AsyncOperationHandle(op1);
            var handle2 = new AsyncOperationHandle(op2);
            var results = new List<AsyncOperationHandle>();
            results.Add(handle1);
            results.Add(handle2);

            var loc = new TestLocator(kLocatorId);
            var locInfo1 = new AddressablesImpl.ResourceLocatorInfo(loc, "same", catalogLoc);
            var locInfo2 = new AddressablesImpl.ResourceLocatorInfo(loc, "bad", catalogLoc);
            var locInfos = new List<AddressablesImpl.ResourceLocatorInfo>();
            locInfos.Add(locInfo1);
            locInfos.Add(locInfo2);

            var trivialHashes = new List<string>(new[] {"same", "good"});

            bool success;
            string errorString;
            var result =
                CheckCatalogsOperation.ProcessDependentOpResults(results, locInfos, trivialHashes, out errorString,
                    out success);

            LogAssert.Expect(LogType.Error, "System.Exception: Bad operation");
            Assert.AreEqual(false, success, "Operation should fail when underlying operation op1 has a null OperationException, even if op2 has a non null OperationException");
            Assert.AreEqual(true, errorString.Contains("Bad operation"), "Error string should contain the error message thrown by the underlying operation");
            Assert.NotNull(result, "Result should only be null if every underlying operation fails.");
            Assert.NotNull(result[0], "Only failed operations should be null in the result list.");
            Assert.IsNull(result[1], "Failed operations should be null in the result list.");
        }

        [Test]
        public void ProcessDependentOpResults_FailsWithMultipleErrorMessageOnMultipleFailures()
        {
            var remoteHashLoc = new ResourceLocationBase("RemoteHash", "Remote", kRemoteHashProviderId, typeof(string));
            var localHashLoc = new ResourceLocationBase("LocalHash", "Local", kLocalHashProviderId, typeof(string));
            var catalogLoc = new ResourceLocationBase("cat", "cat_id", nameof(TestCatalogProvider),
                typeof(IResourceLocator), remoteHashLoc, localHashLoc);

            var op1 = new ExceptionTestOperation(new Exception("Very bad operation"), null);
            var op2 = new ExceptionTestOperation(new Exception("Bad operation"), null);
            var handle1 = new AsyncOperationHandle(op1);
            var handle2 = new AsyncOperationHandle(op2);
            var results = new List<AsyncOperationHandle>();
            results.Add(handle1);
            results.Add(handle2);

            var loc = new TestLocator(kLocatorId);
            var locInfo1 = new AddressablesImpl.ResourceLocatorInfo(loc, "worse", catalogLoc);
            var locInfo2 = new AddressablesImpl.ResourceLocatorInfo(loc, "bad", catalogLoc);
            var locInfos = new List<AddressablesImpl.ResourceLocatorInfo>();
            locInfos.Add(locInfo1);
            locInfos.Add(locInfo2);

            var trivialHashes = new List<string>(new[] {"same", "good"});

            bool success;
            string errorString;
            var result =
                CheckCatalogsOperation.ProcessDependentOpResults(results, locInfos, trivialHashes, out errorString,
                    out success);

            LogAssert.Expect(LogType.Error, "System.Exception: Very bad operation");
            LogAssert.Expect(LogType.Error, "System.Exception: Bad operation");
            Assert.AreEqual(false, success, "Operation should succeed when underlying operation op1 has a null OperationException");
            Assert.AreEqual(true, errorString.Contains("Bad operation"), "Error string should contain the error message thrown by the underlying operation");
            Assert.AreEqual(true, errorString.Contains("Very bad operation"), "Error string should contain the error message thrown by the underlying operation");
            Assert.IsNull(result, "Result list should be null if every operation contained within fails.");
        }
    }

#if UNITY_EDITOR
    class DynamicContentUpdateTests_FastMode : DynamicContentUpdateTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }
    class DynamicContentUpdateTests_VirtualMode : DynamicContentUpdateTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }
    class DynamicContentUpdateTests_PackedPlaymode : DynamicContentUpdateTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif
    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class DynamicContentUpdateTests_Packed : DynamicContentUpdateTests
    {
        protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } }
#if UNITY_EDITOR
        protected override void RunBuilder(AddressableAssetSettings settings)
        {
            //Because we're testing an API that indirectly inits Addr, we need to build using the regular naming convention.
            RunBuilder(settings, "");
        }

#endif
        [UnitySetUp]
        public override IEnumerator RuntimeSetup()
        {
#if ENABLE_CACHING
            Caching.ClearCache();
#endif
            Assert.IsNull(m_Addressables);
            m_Addressables = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
            var op = m_Addressables.InitializeAsync(false);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            OnRuntimeSetup();
            if (op.IsValid())
                op.Release();
        }
    }
}
