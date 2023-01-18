using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.TestTools.Constraints;

namespace UnityEngine.ResourceManagement.Tests
{
    public class ResourceManagerTests
    {
        Action<AsyncOperationHandle, Exception> m_PrevHandler;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_PrevHandler = ResourceManager.ExceptionHandler;
            ResourceManager.ExceptionHandler = null;
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            ResourceManager.ExceptionHandler = m_PrevHandler;
        }

        ResourceManager m_ResourceManager;
        [SetUp]
        public void Setup()
        {
            m_ResourceManager = new ResourceManager();
            m_ResourceManager.CallbackHooksEnabled = false; // default for tests. disabled callback hooks. we will call update manually
        }

        [TearDown]
        public void TearDown()
        {
            Assert.Zero(m_ResourceManager.OperationCacheCount);
            m_ResourceManager.Dispose();
        }

        class TestUpdateReceiver : IUpdateReceiver
        {
            public bool invoked = false;
            public void Update(float unscaledDeltaTime)
            {
                invoked = true;
            }
        }

        [Test]
        public void WhenIUpdateReceiverAdded_CallbackIsInvoked()
        {
            var ur = new TestUpdateReceiver();
            m_ResourceManager.AddUpdateReceiver(ur);
            m_ResourceManager.Update(0);
            Assert.IsTrue(ur.invoked);
            m_ResourceManager.RemoveUpdateReciever(ur);
            ur.invoked = false;
            m_ResourceManager.Update(0);
            Assert.IsFalse(ur.invoked);
        }

        class RMTestOp : AsyncOperationBase<object>
        {
            public int CompletedEventTriggeredCount = 0;

            protected override void Execute()
            {
                m_RM.RegisterForDeferredCallback(this);
            }

            protected override bool InvokeWaitForCompletion() 
            {
                m_RM.Update(1);
                return true; 
            }
        }
        class RMTestUpdateReceiver : IUpdateReceiver
        {
            public int UpdateCount = 0;
            public void Update(float unscaledDeltaTime)
            {
                UpdateCount++;
            }
        }
        [Test]
        public void Reentering_UpdateMethod_ThrowsException()
        {
            var op = new RMTestOp();
            op.Completed += o =>
            {
                (o.m_InternalOp as RMTestOp).CompletedEventTriggeredCount++;
                Assert.Throws<Exception>(() => o.WaitForCompletion());
            };
            var rec = new RMTestUpdateReceiver();
            m_ResourceManager.AddUpdateReceiver(rec);
            m_ResourceManager.StartOperation(op, default);
            op.WaitForCompletion();
            m_ResourceManager.RemoveUpdateReciever(rec);
            Assert.AreEqual(1, op.CompletedEventTriggeredCount);
            Assert.AreEqual(1, rec.UpdateCount);
        }

        class TestUpdateReceiverThatRemovesSelfDuringUpdate : IUpdateReceiver
        {
            public ResourceManager rm;
            public bool removeSelf;
            public int updateCount = 0;
            public void Update(float unscaledDeltaTime)
            {
                updateCount++;
                if (removeSelf)
                    rm.RemoveUpdateReciever(this);
            }
        }

        [Test]
        public void WhenMultipleIUpdateReceivers_AddedToResourceManager_MonoBehaviorCallbackHooksDelegateList_DoesNotGrow()
        {
            var prevCBHooks = m_ResourceManager.CallbackHooksEnabled;
            m_ResourceManager.CallbackHooksEnabled = true;
            var startingCBCount = MonoBehaviourCallbackHooks.Instance.m_OnUpdateDelegate == null ? 0 : MonoBehaviourCallbackHooks.Instance.m_OnUpdateDelegate.GetInvocationList().Length;
            m_ResourceManager.AddUpdateReceiver(new TestUpdateReceiverThatRemovesSelfDuringUpdate() { rm = m_ResourceManager, removeSelf = true });
            Assert.AreEqual(startingCBCount + 1, MonoBehaviourCallbackHooks.Instance.m_OnUpdateDelegate.GetInvocationList().Length);
            m_ResourceManager.AddUpdateReceiver(new TestUpdateReceiverThatRemovesSelfDuringUpdate() { rm = m_ResourceManager, removeSelf = true });
            Assert.AreEqual(startingCBCount + 1, MonoBehaviourCallbackHooks.Instance.m_OnUpdateDelegate.GetInvocationList().Length);
            MonoBehaviourCallbackHooks.Instance.Update();
            m_ResourceManager.CallbackHooksEnabled = prevCBHooks;
        }

        [Test]
        public void WhenIUpdateReceiverRemovesSelfDuringCallback_ListIsMaintained()
        {
            var ur1 = new TestUpdateReceiverThatRemovesSelfDuringUpdate() { rm = m_ResourceManager, removeSelf = false };
            var ur2 = new TestUpdateReceiverThatRemovesSelfDuringUpdate() { rm = m_ResourceManager, removeSelf = true };
            var ur3 = new TestUpdateReceiverThatRemovesSelfDuringUpdate() { rm = m_ResourceManager, removeSelf = false };
            m_ResourceManager.AddUpdateReceiver(ur1);
            m_ResourceManager.AddUpdateReceiver(ur2);
            m_ResourceManager.AddUpdateReceiver(ur3);
            m_ResourceManager.Update(0);
            Assert.AreEqual(1, ur1.updateCount);
            Assert.AreEqual(1, ur2.updateCount);
            Assert.AreEqual(1, ur3.updateCount);
            m_ResourceManager.Update(0);
            Assert.AreEqual(2, ur1.updateCount);
            Assert.AreEqual(1, ur2.updateCount);
            Assert.AreEqual(2, ur3.updateCount);
            m_ResourceManager.RemoveUpdateReciever(ur1);
            m_ResourceManager.RemoveUpdateReciever(ur3);
        }

        class IntOperation : AsyncOperationBase<int>
        {
            protected override void Execute()
            {
                Complete(0, true, null);
            }
        }

        [Test]
        public void WhenOperationReturnsValueType_NoGCAllocs()
        {
            var op = new IntOperation();
            Assert.That(() =>
            {
                var handle = m_ResourceManager.StartOperation(op, default);
                handle.Release();
            }, TestTools.Constraints.Is.Not.AllocatingGCMemory(), "GC Allocation detected");
        }

        [Test]
        public void WhenProviderImplementsIReceiverUpdate_UpdateIsCalledWhileInProviderList()
        {
            MockProvider provider = new MockProvider();
            m_ResourceManager.ResourceProviders.Add(provider);
            m_ResourceManager.Update(0.0f);
            Assert.AreEqual(1, provider.UpdateCount);

            // Update isn't called after removing provider
            m_ResourceManager.ResourceProviders.Remove(provider);
            m_ResourceManager.Update(0.0f);
            Assert.AreEqual(1, provider.UpdateCount);
        }

        class MockInstanceProvider : IInstanceProvider
        {
            public Func<ResourceManager, AsyncOperationHandle<GameObject>, InstantiationParameters, GameObject> ProvideInstanceCallback;
            public Action<ResourceManager, GameObject> ReleaseInstanceCallback;
            public GameObject ProvideInstance(ResourceManager rm, AsyncOperationHandle<GameObject> prefabHandle, InstantiationParameters instantiateParameters)
            {
                return ProvideInstanceCallback(rm, prefabHandle, instantiateParameters);
            }

            public void ReleaseInstance(ResourceManager rm, GameObject instance)
            {
                ReleaseInstanceCallback(rm, instance);
            }
        }

        class GameObjectProvider : IResourceProvider
        {
            public string ProviderId { get { return "GOPRovider"; } }

            public ProviderBehaviourFlags BehaviourFlags { get { return ProviderBehaviourFlags.None; } }

            public bool CanProvide(Type t, IResourceLocation location)
            {
                return t == typeof(GameObject);
            }

            public Type GetDefaultType(IResourceLocation location)
            {
                return typeof(GameObject);
            }

            public bool Initialize(string id, string data) { return true; }

            public void Provide(ProvideHandle provideHandle)
            {
                var result = new GameObject(provideHandle.Location.InternalId);
                provideHandle.Complete(result, true, null);
            }

            public void Release(IResourceLocation location, object asset)
            {
                GameObject.Destroy((GameObject)asset);
            }
        }


        [Test]
        public void ReleaseInstance_BeforeDependencyCompletes_InstantiatesAndReleasesAfterDependencyCompletes()
        {
            var prefabProv = new MockProvider();
            ProvideHandle[] provHandle = new ProvideHandle[1];
            prefabProv.ProvideCallback = h => provHandle[0] = h;
            m_ResourceManager.ResourceProviders.Add(prefabProv);

            ResourceLocationBase locDep = new ResourceLocationBase("prefab", "prefab1", prefabProv.ProviderId, typeof(UnityEngine.GameObject));
            var iProvider = new MockInstanceProvider();
            bool provideCalled = false;
            bool releaseCalled = false;
            iProvider.ProvideInstanceCallback = (rm, prefabHandle, iParam) =>
            {
                provideCalled = true;
                prefabHandle.Release();
                return null;
            };
            iProvider.ReleaseInstanceCallback = (rm, go) =>
            {
                releaseCalled = true;
            };
            var instHandle = m_ResourceManager.ProvideInstance(iProvider, locDep, default(InstantiationParameters));
            Assert.IsFalse(instHandle.IsDone);
            m_ResourceManager.Release(instHandle);
            Assert.IsTrue(instHandle.IsValid());
            Assert.IsFalse(provideCalled);
            Assert.IsFalse(releaseCalled);
            provHandle[0].Complete<GameObject>(null, true, null);
            Assert.IsTrue(provideCalled);
            Assert.IsTrue(releaseCalled);
        }

        // TODO:
        // To test: release via operation,
        // Edge cases: game object fails to load, callback throws exception, Release called on handle before operation completes
        //
        [Test]
        public void ProvideInstance_CanProvide()
        {
            m_ResourceManager.ResourceProviders.Add(new GameObjectProvider());
            ResourceLocationBase locDep = new ResourceLocationBase("prefab", "prefab1", "GOPRovider", typeof(UnityEngine.GameObject));

            MockInstanceProvider iProvider = new MockInstanceProvider();
            InstantiationParameters instantiationParameters = new InstantiationParameters(null, true);
            AsyncOperationHandle<GameObject>[] refResource = new AsyncOperationHandle<GameObject>[1];
            iProvider.ProvideInstanceCallback = (rm, prefabHandle, iParam) =>
            {
                refResource[0] = prefabHandle;
                Assert.AreEqual("prefab1", prefabHandle.Result.name);
                return new GameObject("instance1");
            };
            iProvider.ReleaseInstanceCallback = (rm, go) => { rm.Release(refResource[0]); GameObject.Destroy(go); };

            AsyncOperationHandle<GameObject> obj = m_ResourceManager.ProvideInstance(iProvider, locDep, instantiationParameters);
            m_ResourceManager.Update(0.0f);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, obj.Status);
            Assert.AreEqual("instance1", obj.Result.name);
            Assert.AreEqual(1, m_ResourceManager.OperationCacheCount);
            obj.Release();
        }

        [UnityTest]
        public IEnumerator ProvideResource_WhenRemote_ExceptionHandlerReceivesExceptionWithWebRequestError()
        {
            m_ResourceManager.ResourceProviders.Add(new AssetBundleProvider());

            ResourceLocationBase location = new ResourceLocationBase("nonExistingResource", "http://urlThatCantPossiblyExistsaaaaaaaa.com/bundleName.bundle",
                typeof(AssetBundleProvider).FullName, typeof(IAssetBundleResource));
            location.Data = new AssetBundleRequestOptions()
            {
                BundleName = "bundleName",
                Timeout = 0
            };

            var prevHandler = ResourceManager.ExceptionHandler;

            bool exceptionWithRequestResultReceived = false;
            ResourceManager.ExceptionHandler += (h, ex) =>
            {
                exceptionWithRequestResultReceived |= ex is RemoteProviderException pEx && pEx.WebRequestResult != null;
            };

            AsyncOperationHandle<IAssetBundleResource> handle;

            using (new IgnoreFailingLogMessage())
            {
                handle = m_ResourceManager.ProvideResource<IAssetBundleResource>(location);
                yield return handle;
            }

            ResourceManager.ExceptionHandler = prevHandler;
            Assert.AreEqual(AsyncOperationStatus.Failed, handle.Status);
            Assert.IsTrue(exceptionWithRequestResultReceived);

            handle.Release();
        }

        [UnityTest]
        public IEnumerator WebRequestQueue_CompletesAllOperations()
        {
            int numberOfCompletedOperations = 0;
            int totalOperations = 5000;

            for (int i = 0; i < totalOperations; i++)
            {
                UnityWebRequest uwr = new UnityWebRequest();
                var requestOp = WebRequestQueue.QueueRequest(uwr);
                if (requestOp.IsDone)
                    numberOfCompletedOperations++;
                else
                    requestOp.OnComplete += op => { numberOfCompletedOperations++; };
            }

            while (WebRequestQueue.s_QueuedOperations.Count > 0)
                yield return null;

            Assert.AreEqual(totalOperations, numberOfCompletedOperations);
        }

#if UNITY_EDITOR
        [Test]
        public void AssetDatabaseProvider_LoadAssetAtPath_WhenNotInAssetDatabase_DoesNotThrow()
        {
            var loc = new ResourceLocationBase("name", "id", "providerId", typeof(object));
            ProviderOperation<Object> op = new ProviderOperation<Object>();
            op.Init(m_ResourceManager, null, loc, new AsyncOperationHandle<IList<AsyncOperationHandle>>());
            ProvideHandle handle = new ProvideHandle(m_ResourceManager, op);

            Assert.DoesNotThrow(() => AssetDatabaseProvider.LoadAssetAtPath("doesnotexist", handle));
        }
#endif
    }
}
