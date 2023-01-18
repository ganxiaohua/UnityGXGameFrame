using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace AddressableTests.AsyncTask
{
    public abstract class AsyncTaskTests : AddressablesTestFixture
    {
        [UnityTest]
        public IEnumerator AsyncTask_MaintainsCorrectRefCountAfterCompletion()
        {
            AddressablesImpl impl = new AddressablesImpl(new DefaultAllocationStrategy());
            var op = impl.InitializeAsync(m_RuntimeSettingsPath);
            var task = op.Task;
            while (!task.IsCompleted)
                yield return null;
            yield return null; //need deferred callbacks to get called
            Assert.IsFalse(op.IsValid());
        }

        [UnityTest]
        [Ignore("Ignoring until task refactor is complete.")]
        public IEnumerator AsyncTask_DoesNotReturnNull_StressTest()
        {
            for (int i = 0; i < 100; i++)
            {
                AddressablesImpl impl = new AddressablesImpl(new DefaultAllocationStrategy());
                var op = impl.InitializeAsync(m_RuntimeSettingsPath);
                var task = op.Task;
                while (!task.IsCompleted)
                    yield return null;
                var result = op.Task.Result;
                yield return null; //need deferred callbacks to get called
                Assert.IsNotNull(op.Task.Result, $"task.Result is null! For task number [{i}]");
                op.Release();
            }
        }
    }
#if UNITY_EDITOR
    class AsyncTaskTests_FastMode : AsyncTaskTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class AsyncTaskTests_VirtualMode : AsyncTaskTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class AsyncTaskTests_PackedPlaymodeMode : AsyncTaskTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    [UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    class AsyncTaskTests_PackedMode : AsyncTaskTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}