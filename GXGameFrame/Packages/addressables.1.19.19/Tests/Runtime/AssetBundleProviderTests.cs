using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AddressableTests.SyncAddressables
{
    public abstract class AssetBundleProviderTests : AddressablesTestFixture
    {
        protected string m_PrefabKey = "syncprefabkey";
        protected string m_InvalidKey = "notarealkey";
        protected string m_SceneKey = "syncscenekey";

        const int kForceUWRBundleCount = 10;
        const int kMaxConcurrentRequests = 3;
        string GetForceUWRAddrName(int i) { return $"forceuwrasset{i}"; }

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            AddressableAssetGroup regGroup = settings.CreateGroup("localNoUWRGroup", false, false, true,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            regGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;

            AddressableAssetGroup forceUWRGroup = settings.CreateGroup("ForceUWRGroup", false, false, true,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));
            forceUWRGroup.GetSchema<BundledAssetGroupSchema>().UseUnityWebRequestForLocalBundles = true;
            forceUWRGroup.GetSchema<BundledAssetGroupSchema>().BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
            forceUWRGroup.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;

            settings.MaxConcurrentWebRequests = kMaxConcurrentRequests;

            for (int i = 0; i < kForceUWRBundleCount; i++)
            {
                string s = GetForceUWRAddrName(i);
                string guid = CreatePrefab(tempAssetFolder + $"/{s}.prefab");
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, forceUWRGroup);
                entry.address = s;
            }

            {
                string guid = CreatePrefab(tempAssetFolder + $"/testprefab.prefab");
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, regGroup);
                entry.address = "testprefab";
            }
        }

#endif

        [SetUp]
        public void Setup()
        {
#if ENABLE_CACHING
            Caching.ClearCache();
#endif
            if (m_Addressables != null)
                m_Addressables.WebRequestOverride = null;
        }

        [UnityTest]
        public IEnumerator WhenUWRExceedsMaxLimit_UWRAreQueued()
        {
            List<AsyncOperationHandle<GameObject>> l =
                Enumerable.Range(0, kForceUWRBundleCount).Select(x => m_Addressables.LoadAssetAsync<GameObject>(GetForceUWRAddrName(x))).ToList();
            Assert.AreEqual(kMaxConcurrentRequests, WebRequestQueue.s_ActiveRequests.Count);
            Assert.AreEqual((kForceUWRBundleCount - kMaxConcurrentRequests), WebRequestQueue.s_QueuedOperations.Count);
            foreach (AsyncOperationHandle<GameObject> h in l)
            {
                yield return h;
                h.Release();
            }
        }

        [UnityTest]
        public IEnumerator WhenUWRExceedsMaxLimit_CompletesSynchronously()
        {
            List<AsyncOperationHandle<GameObject>> loadOps =
                Enumerable.Range(0, kForceUWRBundleCount).Select(x => m_Addressables.LoadAssetAsync<GameObject>(GetForceUWRAddrName(x))).ToList();

            AsyncOperationHandle<GameObject> lastHandle = loadOps[kForceUWRBundleCount - 1];
            lastHandle.WaitForCompletion();
            lastHandle.Release();

            for (int i = 0; i < kForceUWRBundleCount - 1; i++)
            {
                AsyncOperationHandle<GameObject> handle = loadOps[i];
                yield return loadOps[i];
                handle.Release();
            }
        }

        [Test]
        public void WhenUWRIsUsed_CompletesSynchronously()
        {
            AsyncOperationHandle<GameObject> h = m_Addressables.LoadAssetAsync<GameObject>(GetForceUWRAddrName(0));
            h.WaitForCompletion();
            h.Release();
        }

        [UnityTest]
        public IEnumerator WhenAssetBundleIsLocal_AndForceUWRIsEnabled_UWRIsUsed()
        {
            AsyncOperationHandle<GameObject> h = m_Addressables.LoadAssetAsync<GameObject>(GetForceUWRAddrName(0));
            Assert.AreEqual(1, WebRequestQueue.s_ActiveRequests.Count);
            yield return h;
            h.Release();
        }

        [UnityTest]
        public IEnumerator WhenAssetBundleIsLocal_AndForceUWRIsDisabled_UWRIsNotUsed()
        {
            AsyncOperationHandle<GameObject> h = m_Addressables.LoadAssetAsync<GameObject>("testprefab");
            Assert.AreEqual(0, WebRequestQueue.s_ActiveRequests.Count);
            yield return h;
            h.Release();
        }

#if ENABLE_ASYNC_ASSETBUNDLE_UWR
        [UnityTest]
        public IEnumerator WhenAssetBundleLoadedThroughUWR_NoMainThreadFileIO()
        {
            AsyncOperationHandle<GameObject> h;
            try
            {
                TestReflectionHelpers.SetResritctMainThreadFileIO(true);
                h = m_Addressables.LoadAssetAsync<GameObject>(GetForceUWRAddrName(0));
                yield return h;
            }
            finally
            {
                TestReflectionHelpers.SetResritctMainThreadFileIO(false);
            }
            h.Release();
        }

#endif

        [UnityTest]
        public IEnumerator WhenWebRequestOverrideIsSet_CallbackIsCalled_AssetBundleProvider()
        {
            bool webRequestOverrideCalled = false;
            m_Addressables.WebRequestOverride = request => webRequestOverrideCalled = true;

            var prev = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var nonExistingPath = "http://127.0.0.1/non-existing-bundle";
            var loc = new ResourceLocationBase(nonExistingPath, nonExistingPath, typeof(AssetBundleProvider).FullName, typeof(AssetBundleResource));
            loc.Data = new AssetBundleRequestOptions();
            var h = m_Addressables.ResourceManager.ProvideResource<AssetBundleResource>(loc);
            yield return h;

            if (h.IsValid()) h.Release();
            LogAssert.ignoreFailingMessages = prev;
            Assert.IsTrue(webRequestOverrideCalled);
        }

        [UnityTest]
        public IEnumerator WhenWebRequestFails_RetriesCorrectAmount_AssetBundleProvider()
        {
            var prev = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var nonExistingPath = "http://127.0.0.1/non-existing-bundle";
            var loc = new ResourceLocationBase(nonExistingPath, nonExistingPath, typeof(AssetBundleProvider).FullName, typeof(AssetBundleResource));
            var d = new AssetBundleRequestOptions();
            d.RetryCount = 3;
            loc.Data = d;

            LogAssert.Expect(LogType.Log, new Regex(@"^(Web request failed, retrying \(0/3)"));
            LogAssert.Expect(LogType.Log, new Regex(@"^(Web request failed, retrying \(1/3)"));
            LogAssert.Expect(LogType.Log, new Regex(@"^(Web request failed, retrying \(2/3)"));
            var h = m_Addressables.ResourceManager.ProvideResource<AssetBundleResource>(loc);
            yield return h;

            if (h.IsValid()) h.Release();
            LogAssert.ignoreFailingMessages = prev;
        }

        [Test]
        [TestCase("Relative/Local/Path", true, false)]
        [TestCase("Relative/Local/Path", true, true)]
        [TestCase("http://127.0.0.1/Web/Path",  false, true)]
        [TestCase("jar:file://Local/Path",  true, false)]
        [TestCase("jar:file://Local/Path",  true, true)]
        public void AssetBundleLoadPathsCorrectForGetLoadInfo(string internalId, bool isLocal, bool useUnityWebRequestForLocalBundles)
        {
            if (internalId.StartsWith("jar") && Application.platform != RuntimePlatform.Android)
                Assert.Ignore($"Skipping test {TestContext.CurrentContext.Test.Name} due jar based tests are only for running on Android Platform.");

            var loc = new ResourceLocationBase("dummy", internalId, "dummy", typeof(Object));
            loc.Data = new AssetBundleRequestOptions { UseUnityWebRequestForLocalBundles = useUnityWebRequestForLocalBundles };
            ProviderOperation<Object> op = new ProviderOperation<Object>();
            op.Init(m_Addressables.ResourceManager, null, loc, new AsyncOperationHandle<IList<AsyncOperationHandle>>());
            ProvideHandle h = new ProvideHandle(m_Addressables.ResourceManager, op);

            AssetBundleResource.GetLoadInfo(h, out AssetBundleResource.LoadType loadType, out string path);
            var expectedLoadType = isLocal ? useUnityWebRequestForLocalBundles ? AssetBundleResource.LoadType.Web : AssetBundleResource.LoadType.Local : AssetBundleResource.LoadType.Web;
            Assert.AreEqual(expectedLoadType, loadType, "Incorrect load type found for internalId " + internalId);
            var expectedPath = internalId;
            if (isLocal && useUnityWebRequestForLocalBundles)
            {
                expectedPath = internalId.StartsWith("jar") ? internalId : "file:///" + Path.GetFullPath(internalId);
            }
            Assert.AreEqual(expectedPath, path);
		}

        [UnityTest]
        public IEnumerator LoadBundleAsync_WithUnfinishedUnload_WaitsForUnloadAndCompletes()
        {
            var h = m_Addressables.LoadAssetAsync<GameObject>("testprefab");
            yield return h;
            Assert.IsNotNull(h.Result);
            h.Release();
            h = m_Addressables.LoadAssetAsync<GameObject>("testprefab");
            yield return h;
            Assert.IsNotNull(h.Result);
            h.Release();
        }

        [UnityTest]
        public IEnumerator LoadBundleSync_WithUnfinishedUnload_WaitsForUnloadAndCompletes()
        {
            var h = m_Addressables.LoadAssetAsync<GameObject>("testprefab");
            yield return h;
            Assert.IsNotNull(h.Result);
            h.Release();
            h = m_Addressables.LoadAssetAsync<GameObject>("testprefab");
            h.WaitForCompletion();
            Assert.IsNotNull(h.Result);
            h.Release();
        }

        [Test]
        // Only testing against important errors instead of full list
        [TestCase("", true)]
        [TestCase("Unknown error", true)]
        [TestCase("Request aborted", false)]
        [TestCase("Unable to write data", false)]
        public void UnityWebRequestResult_ShouldRetryReturnsExpected(string error, bool expected)
        {
            UnityWebRequestResult rst = new UnityWebRequestResult(new UnityWebRequest());
            rst.Error = error;
            bool result = rst.ShouldRetryDownloadError();
            Assert.AreEqual(expected, result, "Unexpected retry value for the input error.");
        }
    }
#if UNITY_EDITOR
    class AssetBundleProviderTests_PackedPlaymodeMode : AssetBundleProviderTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class AssetBundleProviderTests_PackedMode : AssetBundleProviderTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
