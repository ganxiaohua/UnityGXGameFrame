using System;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.TestTools;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace UnityEngine.AddressableAssets.ResourceProviders.Tests
{
    [TestFixture]
    public class ContentCatalogProviderTests : AddressablesTestFixture
    {
        const string k_LocationName = "TestLocation";
        const string k_LocationId = "TestLocationID";
        const string k_CacheLocationId = "CacheLocationID";
        const string k_RemoteLocationId = "RemoteLocationID";
        private const string k_TempAssetFolder = "Assets/TempFolder";
        private const string k_TempBuildFolder = "TempBuildFolder";
        private readonly string m_RuntimeCatalogFilename;

        public ContentCatalogProviderTests()
        {
            m_RuntimeCatalogFilename = "catalog" + m_UniqueTestName + ".bundle";
        }

        ResourceLocationBase m_SimpleLocation = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

        protected override TestBuildScriptMode BuildScriptMode => TestBuildScriptMode.Packed;

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            settings.BundleLocalCatalog = true;
            settings.DefaultGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.AppendHash;
        }

#endif

        [Test]
        public void DetermineIdToLoad_IfLocalCatalogsOnly_ReturnsMainId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "hash", string.Empty }, true);

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfNoDependencies_ReturnsMainId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            var loadedId = contentCatalogOp.DetermineIdToLoad(m_SimpleLocation, null);

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfTooFewDependencies_ReturnsMainId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            var loadedId = contentCatalogOp.DetermineIdToLoad(m_SimpleLocation, new List<object> { 1 });

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfTooManyDependencies_ReturnsMainId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            var loadedId = contentCatalogOp.DetermineIdToLoad(m_SimpleLocation, new List<object> { 1, 2, 3 });

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfOfflineAndNoCache_ReturnsMainId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            var loadedId = contentCatalogOp.DetermineIdToLoad(m_SimpleLocation, new List<object> { string.Empty, string.Empty });

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfOfflineAndHasCache_ReturnsCacheId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { string.Empty, "hash" });

            Assert.AreEqual(k_CacheLocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfOnlineMatchesCache_ReturnsCacheId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "hash", "hash" });

            Assert.AreEqual(k_CacheLocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfDisableContentCatalogUpdateTrue_ForcesLocalId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "hash", "" }, true);

            Assert.AreEqual(k_LocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_IfDisableContentCatalogUpdateTrue_ForcesCachedIdWhenLocalHashExists()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "hash", "local" }, true);
            Assert.AreEqual(k_CacheLocationId, loadedId);
        }

        [Test]
        public void DetermineIdToLoad_SetsLocalHash_WhenDisableContentCatalogIsTrue_AndNoLocalHashExists()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);
            Assert.IsTrue(string.IsNullOrEmpty(contentCatalogOp.m_LocalHashValue));
            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "hash", "" }, true);
            Assert.IsFalse(string.IsNullOrEmpty(contentCatalogOp.m_LocalHashValue));
        }

        [Test]
        public void DetermineIdToLoad_IfOnlineMismatchesCache_ReturnsRemoteId()
        {
            var contentCatalogOp = new ContentCatalogProvider.InternalOp();

            IResourceLocation[] dependencies = new IResourceLocation[(int)ContentCatalogProvider.DependencyHashIndex.Count];

            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Remote] = new ResourceLocationBase(string.Empty, k_RemoteLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));
            dependencies[(int)ContentCatalogProvider.DependencyHashIndex.Cache] = new ResourceLocationBase(string.Empty, k_CacheLocationId, typeof(ContentCatalogProvider).FullName, typeof(object));

            var location = new ResourceLocationBase(k_LocationName, k_LocationId, typeof(ContentCatalogProvider).FullName, typeof(object), dependencies);


            var loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "newHash", "hash" });
            Assert.AreEqual(k_RemoteLocationId, loadedId);

            loadedId = contentCatalogOp.DetermineIdToLoad(location, new List<object> { "newHash", string.Empty });
            Assert.AreEqual(k_RemoteLocationId, loadedId);
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("invalid", typeof(ArgumentException))]
        [TestCase("file.txt", typeof(ArgumentException))]
        public void BundledCatalog_LoadCatalogFromBundle_InvalidBundlePath_ShouldThrow(string path, Type exceptionType)
        {
            Assert.Throws(exceptionType, () => new ContentCatalogProvider.InternalOp.BundledCatalog(path));
        }

        [UnityTest]
        [Ignore("https://jira.unity3d.com/browse/ADDR-1451")]
        public IEnumerator BundledCatalog_LoadCatalogFromBundle_InvalidBundleFileFormat_ShouldFail()
        {
            var bundleFilePath = Path.Combine(k_TempBuildFolder, "catalog.bundle");
            Directory.CreateDirectory(Path.GetDirectoryName(bundleFilePath));

            var bytes = new byte[] { 1, 2, 3, 4, 5, 6 };
            File.WriteAllBytes(bundleFilePath, bytes);

            LogAssert.Expect(LogType.Error, new Regex("Failed to read data for the AssetBundle", RegexOptions.IgnoreCase));

            LogAssert.Expect(LogType.Error, new Regex("Unable to load dependent " +
                $"bundle from location :", RegexOptions.IgnoreCase));

            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.LoadCatalogFromBundleAsync();

            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.IsFalse(bundledCatalog.OpIsSuccess);

            if (Directory.Exists(k_TempBuildFolder))
                Directory.Delete(k_TempBuildFolder, true);
        }

        [UnityTest]
        public IEnumerator BundledCatalog_WhenCatalogIsLocal_LoadCatalogFromBundle_ShouldLoadCatalogAndUnloadResources()
        {
            var bundleFilePath = Path.Combine(Addressables.RuntimePath, m_RuntimeCatalogFilename);

            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.LoadCatalogFromBundleAsync();
            bundledCatalog.OnLoaded += catalogData =>
            {
                Assert.NotNull(catalogData);
                Assert.AreEqual(ResourceManagerRuntimeData.kCatalogAddress, catalogData.ProviderId);
            };

            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.IsTrue(bundledCatalog.OpIsSuccess);
            Assert.Null(bundledCatalog.m_CatalogAssetBundle);
        }

        [UnityTest]
        public IEnumerator BundledCatalog_WhenCatalogIsRemote_LoadCatalogFromBundle_ShouldLoadCatalogAndUnloadResources()
        {
            string localBundleFilePath = Path.Combine(Addressables.RuntimePath, m_RuntimeCatalogFilename);
            string bundleFilePath = "file:///" + Path.GetFullPath(localBundleFilePath);

            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.LoadCatalogFromBundleAsync();
            bundledCatalog.OnLoaded += catalogData =>
            {
                Assert.NotNull(catalogData);
                Assert.AreEqual(ResourceManagerRuntimeData.kCatalogAddress, catalogData.ProviderId);
            };

            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.IsTrue(bundledCatalog.OpIsSuccess);
            Assert.Null(bundledCatalog.m_CatalogAssetBundle);
        }

        [UnityTest]
        public IEnumerator BundledCatalog_WhenRemoteCatalogDoesNotExist_LoadCatalogFromBundle_LogsErrorAndOpFails()
        {
            string bundleFilePath = "file:///doesnotexist.bundle";

            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.LoadCatalogFromBundleAsync();

            LogAssert.Expect(LogType.Error, $"Unable to load dependent bundle from location : {bundleFilePath}");

            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.IsFalse(bundledCatalog.OpIsSuccess);
        }

        [UnityTest]
        public IEnumerator BundledCatalog_LoadCatalogFromBundle_WhenCalledMultipleTimes_OpNotCompleted_FirstShouldSucceedAndOthersShouldFail()
        {
            var bundleFilePath = Path.Combine(Addressables.RuntimePath, m_RuntimeCatalogFilename);

            var timesCalled = 0;
            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.OnLoaded += catalogData =>
            {
                Assert.NotNull(catalogData);
                Assert.AreEqual(ResourceManagerRuntimeData.kCatalogAddress, catalogData.ProviderId);
                timesCalled++;
            };

            bundledCatalog.LoadCatalogFromBundleAsync();
            bundledCatalog.LoadCatalogFromBundleAsync();
            LogAssert.Expect(LogType.Error, new Regex("progress", RegexOptions.IgnoreCase));

            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.AreEqual(1, timesCalled);
        }

        [UnityTest]
        public IEnumerator BundledCatalog_LoadCatalogFromBundle_WhenCalledMultipleTimes_OpCompleted_AllShouldSucceed()
        {
            var bundleFilePath = Path.Combine(Addressables.RuntimePath, m_RuntimeCatalogFilename);

            var timesCalled = 0;
            var bundledCatalog = new ContentCatalogProvider.InternalOp.BundledCatalog(bundleFilePath);
            bundledCatalog.OnLoaded += catalogData =>
            {
                Assert.NotNull(catalogData);
                Assert.AreEqual(ResourceManagerRuntimeData.kCatalogAddress, catalogData.ProviderId);
                timesCalled++;
            };

            bundledCatalog.LoadCatalogFromBundleAsync();
            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            bundledCatalog.LoadCatalogFromBundleAsync();
            yield return new WaitWhile(() => bundledCatalog.OpInProgress);

            Assert.AreEqual(2, timesCalled);
            Assert.IsTrue(bundledCatalog.OpIsSuccess);
        }

        [Test]
        public void ContentCatalogProvider_InternalOp_LoadCatalog_InvalidId_Throws()
        {
            Assert.Throws<NullReferenceException>(() => new ContentCatalogProvider.InternalOp().LoadCatalog("fakeId", false));
        }

        [TestCase("http://127.0.0.1/catalog.json", false)]
        [TestCase("http://127.0.0.1/catalog.bundle", true)]
        public void BundledCatalog_WhenRequestingRemoteCatalog_CanLoadCatalogFromBundle_ReturnsExpectedResult(string internalId, bool result)
        {
            var loc = new ResourceLocationBase(internalId, internalId, typeof(ContentCatalogProvider).FullName, typeof(IResourceLocator));
            ProviderOperation<Object> op = new ProviderOperation<Object>();
            op.Init(m_Addressables.ResourceManager, null, loc, new AsyncOperationHandle<IList<AsyncOperationHandle>>());
            ProvideHandle handle = new ProvideHandle(m_Addressables.ResourceManager, op);

            bool loadCatalogFromLocalBundle = new ContentCatalogProvider.InternalOp().CanLoadCatalogFromBundle(internalId, handle.Location);
            Assert.AreEqual(result, loadCatalogFromLocalBundle);
        }

        [Test]
        public void BundledCatalog_WhenRequestingLocalCatalog_CanLoadCatalogFromBundle_ReturnsTrue()
        {
            string internalId = Path.Combine(Addressables.RuntimePath, m_RuntimeCatalogFilename);
            var loc = new ResourceLocationBase(internalId, internalId, typeof(ContentCatalogProvider).FullName, typeof(IResourceLocator));
            ProviderOperation<Object> op = new ProviderOperation<Object>();
            op.Init(m_Addressables.ResourceManager, null, loc, new AsyncOperationHandle<IList<AsyncOperationHandle>>());
            ProvideHandle handle = new ProvideHandle(m_Addressables.ResourceManager, op);

            bool loadCatalogFromLocalBundle = new ContentCatalogProvider.InternalOp().CanLoadCatalogFromBundle(internalId, handle.Location);
            Assert.IsTrue(loadCatalogFromLocalBundle);
        }
    }
}
