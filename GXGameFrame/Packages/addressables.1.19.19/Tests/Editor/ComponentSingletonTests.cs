using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.TestTools;

namespace UnityEditor.AddressableAssets.Tests
{
    public class ComponentSingletonTests
    {
        const string kTestSingletonName = "Test Singleton";
        public class TestSingletonWithName : ComponentSingleton<TestSingletonWithName>
        {
            protected override string GetGameObjectName() => kTestSingletonName;
        }

        public class TestSingletonWithDefaultName : ComponentSingleton<TestSingletonWithDefaultName> {}

        [TearDown]
        public void Teardown()
        {
            TestSingletonWithName.DestroySingleton();
            TestSingletonWithDefaultName.DestroySingleton();
            EditorApplication.isPlaying = false;
        }

        [Test]
        public void CallingInstanceInstantiatesSingleton()
        {
            Assert.False(TestSingletonWithName.Exists);
            Assert.NotNull(TestSingletonWithName.Instance, "Expected singleton instance to be returned.");
        }

        [Test]
        public void SingletonIsDestroyedWhenCallingDestroySingleton()
        {
            Assert.NotNull(TestSingletonWithName.Instance);
            TestSingletonWithName.DestroySingleton();
            Assert.False(TestSingletonWithName.Exists);
            Assert.IsEmpty(Object.FindObjectsOfType<TestSingletonWithName>(), "Expected no singleton objects to exists after destroy was called");
        }

        [Test]
        public void InstantiatingSingletonCausesItToBeDestroyedWhenOneAlreadyExists()
        {
            var singleton = TestSingletonWithName.Instance;
            var go = new GameObject("Duplicate");
            var duplicate = go.AddComponent<TestSingletonWithName>();
            Assert.Null(duplicate, "Expected the duplicate singleton to be destroyed when one already exists");
            Assert.AreEqual(singleton, TestSingletonWithName.Instance);
        }

        [Test]
        public void GameObjectNameMatchesOverridenGetGameObjectNameValue()
        {
            Assert.AreEqual(kTestSingletonName, TestSingletonWithName.Instance.gameObject.name);
        }

        [Test]
        public void GameObjectNameMatchesClassName()
        {
            Assert.AreEqual(nameof(TestSingletonWithDefaultName), TestSingletonWithDefaultName.Instance.gameObject.name);
        }

        [UnityTest]
        public IEnumerator ExitingPlayModeDestroysSingleton()
        {
            yield return new EnterPlayMode();

            var instance = TestSingletonWithName.Instance;
            Assert.NotNull(instance);

            yield return new ExitPlayMode();

            // We cant use Assert.Null as we need the override that compares against null when using ==
            Assert.True(instance == null, "Expected singleton instance to be destroyed when leaving play mode.");
            Assert.False(TestSingletonWithName.Exists);
            Assert.IsEmpty(Object.FindObjectsOfType<TestSingletonWithName>(), "Expected no singleton objects to exists after leaving play mode.");
        }

        [UnityTest]
        public IEnumerator EnteringPlayModeDestroysEditorSingleton()
        {
            var instance = TestSingletonWithName.Instance;
            Assert.NotNull(instance);

            yield return new EnterPlayMode();

            // We cant use Assert.Null as we need the override that compares against null when using ==
            Assert.True(instance == null, "Expected editor singleton instance to be destroyed when entering play mode.");
            Assert.False(TestSingletonWithName.Exists);
            Assert.IsEmpty(Object.FindObjectsOfType<TestSingletonWithName>(), "Expected no singleton objects to exists after leaving play mode.");
        }

        [UnityTest]
        public IEnumerator PlaymodeSingletonHasHideFlags_DontSave()
        {
            yield return new EnterPlayMode();
            Assert.True(Application.isPlaying);
            var instance = TestSingletonWithName.Instance;
            Assert.AreEqual(HideFlags.DontSave, instance.hideFlags);
        }

        [Test]
        public void EditmodeSingletonHasHideFlags_HideAndDontSave()
        {
            Assert.False(Application.isPlaying);
            var instance = TestSingletonWithName.Instance;
            Assert.AreEqual(HideFlags.HideAndDontSave, instance.hideFlags);
        }
    }
}
