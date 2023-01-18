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
using UnityEngine.Scripting;
using UnityEngine.TestTools.Constraints;

[assembly: Preserve]

namespace UnityEngine.ResourceManagement.Tests
{
    public class BaseOperationBehaviorTests
    {
        Action<AsyncOperationHandle, Exception> m_PrevHandler;
        ResourceManager m_RM;

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

        [SetUp]
        public void Setup()
        {
            m_RM = new ResourceManager();
            m_RM.CallbackHooksEnabled = false; // default for tests. disabled callback hooks. we will call update manually
        }

        [TearDown]
        public void TearDown()
        {
            Assert.Zero(m_RM.OperationCacheCount);
            m_RM.Dispose();
        }

        [Test]
        public void WhenReferenceCountReachesZero_DestroyCallbackInvoked()
        {
            var op = m_RM.CreateCompletedOperation<int>(1, string.Empty);
            int resultInDestroyCallback = 0;
            op.Destroyed += (x) => resultInDestroyCallback = x.Convert<int>().Result;
            op.Release();
            Assert.AreEqual(1, resultInDestroyCallback);
        }

        [Test]
        public void WhileCompletedCallbackIsDeferredOnCompletedOperation_ReferenceCountIsHeld()
        {
            var op = m_RM.CreateCompletedOperation<int>(1, string.Empty);
            int refCount = op.ReferenceCount;
            bool completedCalled = false;
            op.Completed += (x) => completedCalled = true; // callback is deferred to next update
            Assert.AreEqual(refCount + 1, op.ReferenceCount);
            m_RM.Update(0.0f);
            Assert.AreEqual(refCount, op.ReferenceCount);
            Assert.AreEqual(true, completedCalled);
            op.Release();
        }

        [Test]
        public void WhenInDestroyCallback_IncrementAndDecrementReferenceCount_Throws()
        {
            var op = m_RM.CreateCompletedOperation<int>(1, string.Empty);
            int resultInDestroyCallback = 0;
            Exception onInc = null;
            Exception onDec = null;
            op.Destroyed += (x) =>
            {
                try { x.Acquire(); }
                catch (Exception e) { onInc = e; }
                try { x.Release(); }
                catch (Exception e) { onDec = e; }
                resultInDestroyCallback = x.Convert<int>().Result;
            };
            op.Release();
            Assert.NotNull(onInc);
            Assert.NotNull(onDec);
        }

        class MockOperation<T> : AsyncOperationBase<T>
        {
            public Action ExecuteCallback = () => {};
            protected override void Execute()
            {
                ExecuteCallback();
            }
        }

        [Test]
        public void WhenOperationHasDependency_ExecuteNotCalledUntilDependencyCompletes()
        {
            var op1 = new MockOperation<int>();
            var op2 = new MockOperation<int>();
            var handle1 = m_RM.StartOperation(op1, default(AsyncOperationHandle));
            op2.ExecuteCallback = () => { op2.Complete(0, true, string.Empty); };
            var handle2 = m_RM.StartOperation(op2, handle1);
            m_RM.Update(0.0f);
            Assert.AreEqual(false, handle2.IsDone);
            op1.Complete(0, true, null);
            Assert.AreEqual(true, handle2.IsDone);
            handle1.Release();
            handle2.Release();
        }

        [Test]
        public void WhenOperationIsSuccessfulButHasErrorMsg_FailsSilently_CompletesButExceptionHandlerIsCalled()
        {
            bool exceptionHandlerCalled = false;
            ResourceManager.ExceptionHandler += (h, ex) => exceptionHandlerCalled = true;

            var op = m_RM.CreateCompletedOperationInternal<int>(1, true, new Exception("An exception occured."));

            var status = AsyncOperationStatus.None;
            op.Completed += (x) => status = x.Status;

            // callbacks are deferred to next update
            m_RM.Update(0.0f);

            Assert.AreEqual(true, exceptionHandlerCalled);
            Assert.AreEqual(AsyncOperationStatus.Succeeded, status);
            op.Release();
        }

        [UnityTest]
        public IEnumerator AsyncOperationHandle_TaskIsDelayedUntilAfterDelayedCompletedCallbacks()
        {
            var op = m_RM.CreateCompletedOperationInternal<int>(1, true, null);

            var status = AsyncOperationStatus.None;
            op.Completed += (x) => status = x.Status;
            var t = op.Task;
            Assert.IsFalse(t.IsCompleted);

            // callbacks are deferred to next update
            m_RM.Update(0.0f);

            // the Task may not yet have continues after at this point on the update,
            // give the Synchronization a little time with a yield
            yield return null;

            Assert.IsTrue(t.IsCompleted);
            op.Release();
        }

        [Test]
        public void AsyncOperationHandle_TaskIsCompletedWhenHandleIsCompleteWithoutDelayedCallbacks()
        {
            var op = m_RM.CreateCompletedOperationInternal<int>(1, true, null);
            var t = op.Task;
            Assert.IsTrue(t.IsCompleted);
            op.Release();
        }

        // TODO:
        // public void WhenOperationHasDependency_AndDependencyFails_DependentOpStillExecutes()

        // Bad derived class behavior
        // public void CustomOperation_WhenCompleteCalledBeforeStartOperation_ThrowsOperationDoesNotComplete
        // public void CustomOperation_WhenCompleteCalledMultipleTimes_Throws
        // public void CustomOperation_WhenProgressCallbackThrowsException_ErrorLoggedAndHandleReturnsZero
        // public void CustomOperation_WhenDestroyThrowsException_ErrorLogged
        // public void CustomOperation_WhenExecuteThrows_ErrorLoggedAndOperationSetAsFailed

        // TEST: Per operation update behavior

        // public void AsyncOperationHandle_WhenReleaseOnInvalidHandle_Throws
        // public void AsyncOperationHandle_WhenConvertToIncompatibleHandleType_Throws
        //

        [Test]
        public void AsyncOperationHandle_EventSubscriptions_UnsubscribingToNonSubbedEventsShouldHaveNoEffect()
        {
            var op = new MockOperation<int>();
            var handle = m_RM.StartOperation(op, default(AsyncOperationHandle));

            Assert.False(op.CompletedEventHasListeners);
            handle.Completed -= oph => {};
            Assert.False(op.CompletedEventHasListeners);

            Assert.False(op.DestroyedEventHasListeners);
            handle.Destroyed -= oph => {};
            Assert.False(op.DestroyedEventHasListeners);

            handle.Release();
        }

        internal class ManualDownloadPercentCompleteOperation : AsyncOperationBase<IAssetBundleResource>
        {
            public long m_bytesDownloaded = 0;
            public long m_totalBytes = 1024;
            public bool m_IsDone = false;
            protected override void Execute()
            {
            }

            public void CompleteNow()
            {
                m_bytesDownloaded = m_totalBytes;
                Complete(null, true, null);
            }

            internal override DownloadStatus GetDownloadStatus(HashSet<object> visited)
            {
                return new DownloadStatus() { DownloadedBytes = m_bytesDownloaded, TotalBytes = m_totalBytes, IsDone = m_IsDone };
            }
        }

        static void AssertExpectedDownloadStatus(DownloadStatus dls, long dl, long tot, float per)
        {
            Assert.AreEqual(dl, dls.DownloadedBytes);
            Assert.AreEqual(tot, dls.TotalBytes);
            Assert.AreEqual(per, dls.Percent);
        }

        [Test]
        public void DownloadStatusWithNoBytes_WithIsDoneFalse_Returns_PercentCompleteZero()
        {
            var dls = new DownloadStatus() { DownloadedBytes = 0, TotalBytes = 0, IsDone = false };
            Assert.AreEqual(0f, dls.Percent);
        }

        [Test]
        public void DownloadStatusWithNoBytes_WithIsDoneTrue_Returns_PercentCompleteOne()
        {
            var dls = new DownloadStatus() { DownloadedBytes = 0, TotalBytes = 0, IsDone = true };
            Assert.AreEqual(1f, dls.Percent);
        }

        [Test]
        public void GroupOperation_WithOpsThatImplementGetDownloadStatus_ComputesExpectedDownloadPercentComplete()
        {
            var ops = new List<AsyncOperationHandle>();
            var mdpco = new List<ManualDownloadPercentCompleteOperation>();
            for (int i = 0; i < 4; i++)
            {
                var o = m_RM.CreateOperation<ManualDownloadPercentCompleteOperation>(typeof(ManualDownloadPercentCompleteOperation), 1, null, null);
                o.Start(m_RM, default, null);
                mdpco.Add(o);
                ops.Add(new AsyncOperationHandle(o));
            }

            var gOp = m_RM.CreateGenericGroupOperation(ops, true);
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 0, 4096, 0);
            mdpco[0].m_bytesDownloaded = 512;
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 512, 4096, .125f);
            foreach (var o in mdpco)
                o.CompleteNow();
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 4096, 4096, 1f);
            m_RM.Release(gOp);
        }

        [Test]
        public void ChainOperation_WithOpThatImplementGetDownloadStatus_ComputesExpectedDownloadPercentComplete()
        {
            var depOp = m_RM.CreateOperation<ManualDownloadPercentCompleteOperation>(typeof(ManualDownloadPercentCompleteOperation), 1, null, null);
            depOp.Start(m_RM, default, null);
            var chainOp = m_RM.CreateChainOperation<object>(new AsyncOperationHandle(depOp), s => m_RM.CreateCompletedOperationInternal<object>(null, true, null));

            AssertExpectedDownloadStatus(chainOp.GetDownloadStatus(), 0, 1024, 0f);
            depOp.m_bytesDownloaded = 512;
            AssertExpectedDownloadStatus(chainOp.GetDownloadStatus(), 512, 1024, .5f);
            depOp.CompleteNow();
            m_RM.Update(.1f);
            Assert.IsTrue(chainOp.IsDone);
            AssertExpectedDownloadStatus(chainOp.GetDownloadStatus(), 1024, 1024, 1f);
            m_RM.Release(chainOp);
        }

        [Test]
        public void PercentComplete_ReturnsZero_WhenChainOperationHasNotBegun()
        {
            var baseOperation = m_RM.CreateChainOperation<AsyncOperationHandle>(
                new AsyncOperationHandle(new ManualPercentCompleteOperation(1f)),
                (obj) =>
                {
                    return new AsyncOperationHandle<AsyncOperationHandle>();
                });

            Assert.AreEqual(0, baseOperation.PercentComplete);
        }

        [Test]
        public void GroupOperation_WithDuplicateOpThatImplementGetDownloadStatus_DoesNotOverCountValues()
        {
            var ops = new List<AsyncOperationHandle>();
            var o = m_RM.CreateOperation<ManualDownloadPercentCompleteOperation>(typeof(ManualDownloadPercentCompleteOperation), 1, null, null);
            o.Start(m_RM, default, null);
            for (int i = 0; i < 4; i++)
                ops.Add(new AsyncOperationHandle(o));

            var gOp = m_RM.CreateGenericGroupOperation(ops, true);
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 0, 1024, 0);
            o.m_bytesDownloaded = 512;
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 512, 1024, .5f);
            o.CompleteNow();
            AssertExpectedDownloadStatus(gOp.GetDownloadStatus(), 1024, 1024, 1f);
            m_RM.Release(gOp);
        }

        class TestOp : AsyncOperationBase<int>
        {
            protected override void Execute()
            {
                InvokeCompletionEvent();
            }
        }

        [Test]
        public void CompletionEvents_AreInvoked_InOrderAdded()
        {
            var op = new TestOp();
            int count = 0;
            op.Completed += o => { Assert.AreEqual(0, count); count++; };
            op.CompletedTypeless += o => { Assert.AreEqual(1, count); count++; };
            op.Completed += o => { Assert.AreEqual(2, count); count++; };
            op.CompletedTypeless += o => { Assert.AreEqual(3, count); count++; };
            op.Start(null, default, null);
            op.Complete(1, true, null);
        }

        [Test]
        public void WhenOperationIsReused_HasExecutedIsReset()
        {
            var op = new TestOp();
            op.Start(null, default, null);
            op.Complete(1, true, null);

            Assert.IsTrue(op.HasExecuted);
            var dep = new AsyncOperationHandle(new TestOp());
            op.Start(null, dep, null);
            Assert.IsFalse(op.HasExecuted);
        }
    }
}
