using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SceneTests
{
    abstract class SceneTests : AddressablesTestFixture
    {
        int m_StartingSceneCount;
        const int numScenes = 2;
        protected List<String> sceneKeys;
        const string prefabKey = "prefabKey";
        internal const string kEmbeddedSceneName = "embeddedassetscene";

        protected internal string GetPrefabKey()
        {
            return prefabKey;
        }

        public SceneTests()
        {
            sceneKeys = new List<string>();
            for (int i = 0; i < numScenes; i++)
            {
                sceneKeys.Add("SceneTests_Scene" + i);
            }
        }

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            AddressableAssetGroup group = settings.CreateGroup("SceneGroup", true, false, false, null, typeof(BundledAssetGroupSchema));
            group.GetSchema<BundledAssetGroupSchema>().BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.OnlyHash;

            // Create prefab
            string prefabPath = CreateAssetPath(tempAssetFolder, prefabKey, ".prefab");
            string prefabGuid = CreatePrefab(prefabPath);
            AddressableAssetEntry prefabEntry = settings.CreateOrMoveEntry(prefabGuid, group, false, false);
            prefabEntry.address = Path.GetFileNameWithoutExtension(prefabEntry.AssetPath);

            // Create scenes
            for (int i = 0; i < numScenes; i++)
            {
                string scenePath = CreateAssetPath(tempAssetFolder, sceneKeys[i], ".unity");
                string sceneGuid = CreateScene(scenePath);
                AddressableAssetEntry sceneEntry = settings.CreateOrMoveEntry(sceneGuid, group, false, false);
                sceneEntry.address = Path.GetFileNameWithoutExtension(sceneEntry.AssetPath);
            }

            {
                string scenePath = CreateAssetPath(tempAssetFolder, kEmbeddedSceneName, ".unity");
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                new GameObject("EmbededMeshGameObject").AddComponent<MeshFilter>().mesh = new Mesh();
                EditorSceneManager.SaveScene(scene, scenePath);
                string sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);
                AddressableAssetEntry sceneEntry = settings.CreateOrMoveEntry(sceneGuid, group, false, false);
                sceneEntry.address = Path.GetFileNameWithoutExtension(sceneEntry.AssetPath);
            }
        }

#endif
        [SetUp]
        public void SetUp()
        {
            m_StartingSceneCount = m_Addressables.SceneOperationCount;
        }

        [TearDown]
        public void TearDown()
        {
            Assert.AreEqual(m_StartingSceneCount, m_Addressables.SceneOperationCount);
        }

        [UnityTest]
        public IEnumerator CanLoadMultipleScenesAdditively()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);

            var op1 = m_Addressables.LoadSceneAsync(sceneKeys[1], LoadSceneMode.Additive);
            yield return op1;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op1.Status);
            Assert.AreEqual(sceneKeys[1], SceneManager.GetSceneByName(sceneKeys[1]).name);

            yield return UnloadSceneFromHandler(op, m_Addressables);
            yield return UnloadSceneFromHandler(op1, m_Addressables);
        }

        [UnityTest]
        public IEnumerator AddressablesImpl_LoadSceneAsync_FailsLoadNonexistent()
        {
            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            var op = m_Addressables.LoadSceneAsync("testkey");
            yield return op;
            
            Assert.AreEqual(AsyncOperationStatus.Failed,op.Status);
            Assert.IsTrue(op.OperationException.Message.Contains("InvalidKey"));
            LogAssert.ignoreFailingMessages = ifm;
        }

        [UnityTest]
        public IEnumerator LoadSceneAsync_Fails_When_DepsFail()
        {
            var ifm = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            var loc = new ResourceLocationBase("scene", "asdf", typeof(SceneProvider).FullName, typeof(SceneInstance), new ResourceLocationBase("invalid", "nobundle", typeof(AssetBundleProvider).FullName, typeof(AssetBundleResource)));
            var op = m_Addressables.LoadSceneAsync(loc);
            yield return op;

            Assert.AreEqual(AsyncOperationStatus.Failed, op.Status);
            Assert.IsTrue(op.OperationException.Message.Contains("GroupOperation"));
            LogAssert.ignoreFailingMessages = ifm;
        }
        [UnityTest]
        public IEnumerator PercentComplete_NeverHasDecreasedValue_WhenLoadingScene()
        {
            //Setup
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);

            //Test
            float lastPercentComplete = 0f;
            while (!op.IsDone)
            {
                Assert.IsFalse(lastPercentComplete > op.PercentComplete);
                lastPercentComplete = op.PercentComplete;
                yield return null;
            }
            Assert.True(op.PercentComplete == 1 && op.IsDone);
            yield return op;

            //Cleanup
            yield return UnloadSceneFromHandler(op, m_Addressables);
        }

        [UnityTest]
        public IEnumerator WhenSceneUnloaded_InstanitatedObjectsInThatSceneAreReleased()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);
            SceneManager.SetActiveScene(op.Result.Scene);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetActiveScene().name);

            var instOp = m_Addressables.InstantiateAsync(prefabKey);
            yield return instOp;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, instOp.Status);
            Assert.AreEqual(sceneKeys[0], instOp.Result.scene.name);
            
            yield return UnloadSceneFromHandler(op, m_Addressables);
            Assert.IsFalse(instOp.IsValid());
        }

        [UnityTest]
        public IEnumerator WhenSceneUnloadedWithSceneManager_InstanitatedObjectsInThatSceneAreReleased()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);
            SceneManager.SetActiveScene(op.Result.Scene);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetActiveScene().name);

            var instOp = m_Addressables.InstantiateAsync(prefabKey);
            yield return instOp;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, instOp.Status);
            Assert.AreEqual(sceneKeys[0], instOp.Result.scene.name);

            var unloadOp = SceneManager.UnloadSceneAsync(op.Result.Scene);
            yield return unloadOp;
            Assert.IsTrue(unloadOp.isDone);
            Assert.IsNull(SceneManager.GetSceneByName(sceneKeys[0]).name);
            Assert.IsFalse(instOp.IsValid());
        }

        [UnityTest]
        public IEnumerator WhenSceneUnloaded_InstantiatedObjectsInOtherScenesAreNotReleased()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);

            var activeScene = m_Addressables.LoadSceneAsync(sceneKeys[1], LoadSceneMode.Additive);
            yield return activeScene;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, activeScene.Status);
            Assert.AreEqual(sceneKeys[1], SceneManager.GetSceneByName(sceneKeys[1]).name);
            SceneManager.SetActiveScene(activeScene.Result.Scene);
            Assert.AreEqual(sceneKeys[1], SceneManager.GetActiveScene().name);

            Assert.IsNull(GameObject.Find(prefabKey));
            var instOp = m_Addressables.InstantiateAsync(prefabKey);
            yield return instOp;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, instOp.Status);
            Assert.AreEqual(sceneKeys[1], instOp.Result.scene.name);

            yield return UnloadSceneFromHandler(op, m_Addressables);
            Assert.NotNull(GameObject.Find(instOp.Result.name));

            yield return UnloadSceneFromHandler(activeScene, m_Addressables);
            Assert.IsFalse(instOp.IsValid());
        }

        /* Regression test for https://jira.unity3d.com/browse/ADDR-1032
         *
         * Bug occurs when an instantiation happens after a previously completed instantiation.
         * The InstanceOperation is recycled from the previous instantiation, but its m_scene field is not cleaned.
         *
         * Test ensures that when instantiating a prefab and the InstanceOperation is recycled from a previous instantiation,
         * the m_Scene (field in InstanceOperation) should be null until the InstanceOperation is completed.
         */
        [UnityTest]
        public IEnumerator WhenInstantiatingPrefab_AndOperationIsRecycled_SceneIsNullUntilCompletion()
        {
            // Previous instantiation
            var instOp = m_Addressables.InstantiateAsync(prefabKey);
            var internalInstanceOp1 = instOp.m_InternalOp;
            yield return instOp;
            instOp.Release();

            // InstanceOperation we want to test
            var instOp2 = m_Addressables.InstantiateAsync(prefabKey);
            var internalInstanceOp2 = (ResourceManager.InstanceOperation)instOp2.m_InternalOp;

            // Test
            Assert.False(internalInstanceOp2.IsDone, "InstanceOperation2 is not yet completed.");
            Assert.AreEqual(internalInstanceOp1, internalInstanceOp2, "The operation was not recycled");
            Assert.True(string.IsNullOrEmpty(internalInstanceOp2.InstanceScene().name), "Scene was not cleared from InstanceOperation");

            // Cleanup
            yield return internalInstanceOp2;
            yield return instOp2;
            instOp2.Release();
        }

        [UnityTest]
        public IEnumerator ActivateSceneAsync_ReturnsOperation()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;

            var activateScene = op.Result.ActivateAsync();
            yield return activateScene;

            Assert.AreEqual(op.Result.m_Operation, activateScene);

            yield return UnloadSceneFromHandler(op, m_Addressables);
        }

        [UnityTest]
        public IEnumerator SceneTests_LoadSceneHandle_MatchesTrackedHandle()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;

            Assert.AreEqual(1, m_Addressables.m_SceneInstances.Count);
            Assert.IsTrue(m_Addressables.m_SceneInstances.Contains(op));

            yield return UnloadSceneFromHandler(op, m_Addressables);
        }

        [UnityTest]
        public IEnumerator SceneTests_LoadSceneWithChainHandle_MatchesTrackedHandle()
        {
            AddressablesImpl impl = new AddressablesImpl(new DefaultAllocationStrategy());
            var op = m_Addressables.LoadSceneWithChain(impl.InitializeAsync(), sceneKeys[0], LoadSceneMode.Additive);
            yield return op;

            Assert.AreEqual(1, m_Addressables.m_SceneInstances.Count);
            Assert.IsTrue(m_Addressables.m_SceneInstances.Contains(op));

            yield return UnloadSceneFromHandler(op, m_Addressables);
            impl.ResourceManager.Dispose();
        }

        [UnityTest]
        public IEnumerator SceneTests_UnloadScene_RemovesTrackedInstanceOp()
        {
            AddressablesImpl impl = new AddressablesImpl(new DefaultAllocationStrategy());
            var op = m_Addressables.LoadSceneWithChain(impl.InitializeAsync(), sceneKeys[0], LoadSceneMode.Additive);
            yield return op;

            Assert.AreEqual(1, m_Addressables.m_SceneInstances.Count);
            yield return UnloadSceneFromHandler(op, m_Addressables);

            Assert.AreEqual(0, m_Addressables.m_SceneInstances.Count);
            impl.ResourceManager.Dispose();
        }
        
        [UnityTest]
        public IEnumerator SceneTests_UnloadSceneAsync_CanUnloadBaseHandle()
        {
            AddressablesImpl impl = new AddressablesImpl(new DefaultAllocationStrategy());
            var op = m_Addressables.LoadSceneWithChain(impl.InitializeAsync(), sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            
            Assert.AreEqual(1, m_Addressables.m_SceneInstances.Count);
            bool autoReleaseHandle = false;
            op = impl.UnloadSceneAsync((AsyncOperationHandle)op, UnloadSceneOptions.None, autoReleaseHandle);
            yield return op;

            Assert.AreEqual(AsyncOperationStatus.Succeeded,op.Status);
            op.Release();
            yield return op;
            
            Assert.AreEqual(0, m_Addressables.m_SceneInstances.Count);
        }
        
        [UnityTest]
        public IEnumerator SceneTests_UnloadSceneAsync_CanUnloadFromSceneInstance()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            
            Assert.AreEqual(1, m_Addressables.m_SceneInstances.Count);
            var sceneInst = op.m_InternalOp.Result; 
            yield return m_Addressables.UnloadSceneAsync(sceneInst);

            Assert.AreEqual(0, m_Addressables.m_SceneInstances.Count);
        }
        
        [UnityTest]
        public IEnumerator SceneTests_UnloadSceneAsync_UnloadSceneDecreaseRefOnlyOnce()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);

            Addressables.ResourceManager.Acquire(op);
            yield return UnloadSceneFromHandlerRefCountCheck(op, m_Addressables);
            
            // Cleanup
            Addressables.Release(op);
        }
        
        [UnityTest]
        public IEnumerator SceneTests_Release_ReleaseToZeroRefCountUnloadsScene()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);
            
            m_Addressables.Release(op);
            yield return null;
            
            Assert.IsFalse(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsFalse(op.IsValid());
        }
        
        [UnityTest]
        public IEnumerator SceneTests_Release_ReleaseToRefCountZeroWhileLoadingUnloadsAfterLoadCompletes()
        {
            // Setup
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            
            // Test
            op.Completed += s => Assert.IsTrue(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            m_Addressables.Release(op);
            yield return op;
            Assert.IsFalse(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsFalse(op.IsValid());

            // Cleanup
            yield return op;
        }
        
        [UnityTest]
        public IEnumerator SceneTests_Release_ReleaseNotRefCountZeroWhileLoadingDoesntUnloadAfterLoadCompletes()
        {
            // Setup
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            Addressables.ResourceManager.Acquire(op);
            
            // Test
            op.Completed += s => Assert.IsTrue(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Addressables.Release(op);
            yield return op;
            Assert.IsTrue(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsTrue(op.IsValid());

            // Cleanup
            yield return op;
            Assert.IsTrue(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsTrue(op.IsValid());
            m_Addressables.Release(op);
            
            yield return op;
            Assert.IsFalse(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsFalse(op.IsValid());
        }
        
        [UnityTest]
        public IEnumerator SceneTests_Release_ReleaseNotToZeroRefCountDoesNotUnloadScene()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKeys[0], SceneManager.GetSceneByName(sceneKeys[0]).name);
            Addressables.ResourceManager.Acquire(op);
            
            m_Addressables.Release(op);
            yield return null;
            
            Assert.IsTrue(SceneManager.GetSceneByName(sceneKeys[0]).isLoaded);
            Assert.IsTrue(op.IsValid());
            
            // Cleanup
            m_Addressables.Release(op);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GetDownloadSize_DoesNotThrowInvalidKeyException_ForScene()
        {
#if ENABLE_CACHING
            var dOp = m_Addressables.GetDownloadSizeAsync((object)sceneKeys[0]);
            yield return dOp;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, dOp.Status);
#else
            Assert.Ignore();
            yield break;
#endif
        }
    }

#if UNITY_EDITOR
    class SceneTests_FastMode : SceneTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class SceneTests_VirtualMode : SceneTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class SceneTests_PackedPlaymodeMode : SceneTests
    {
        protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } }

        [UnityTest]
        public IEnumerator UnloadScene_ChainsBehindLoadOp_IfLoadOpIsRunning_TypedHandle()
        {
            //Setup
            AsyncOperationHandle<SceneInstance> handle = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);

            //Test
            var unloadHandle = m_Addressables.UnloadSceneAsync(handle);
            yield return unloadHandle;

            //Assert
            Assert.AreEqual(typeof(ChainOperation<SceneInstance, SceneInstance>), unloadHandle.m_InternalOp.GetType(), "Unload a scene while a Load is in progress should have resulted in the unload being chained behind the load op, but wasn't");
            Addressables.Release(unloadHandle);
        }

        [UnityTest]
        public IEnumerator UnloadScene_ChainsBehindLoadOp_IfLoadOpIsRunning_TypelessHandle()
        {
            //Setup
            AsyncOperationHandle handle = m_Addressables.LoadSceneAsync(sceneKeys[0], LoadSceneMode.Additive);

            //Test
            var unloadHandle = m_Addressables.UnloadSceneAsync(handle);
            yield return unloadHandle;

            //Assert
            Assert.AreEqual(typeof(ChainOperationTypelessDepedency<SceneInstance>), unloadHandle.m_InternalOp.GetType(), "Unload a scene while a Load is in progress should have resulted in the unload being chained behind the load op, but wasn't");
            Addressables.Release(unloadHandle);
        }
        
                [UnityTest]
        public IEnumerator SceneTests_UnloadSceneAsync_UnloadSceneAfterAcquireAndDoNotDestroyOnLoadDoesNotUnloadDependenciesUntilSecondRelease()
        {
            // Setup scene
            int bundleCountBeforeTest = AssetBundle.GetAllLoadedAssetBundles().Count();
            var activeScene = m_Addressables.LoadSceneAsync(sceneKeys[1], LoadSceneMode.Additive);
            yield return activeScene;
            
            Assert.AreEqual(AsyncOperationStatus.Succeeded, activeScene.Status);
            Addressables.ResourceManager.Acquire(activeScene);
            Assert.AreEqual(activeScene.ReferenceCount, 2);
            SceneManager.SetActiveScene(activeScene.Result.Scene);
            Assert.AreEqual(sceneKeys[1], SceneManager.GetActiveScene().name);
            
            // Setup obj
            Assert.IsNull(GameObject.Find(GetPrefabKey()));
            var instOp = m_Addressables.InstantiateAsync(GetPrefabKey());
            yield return instOp;
            
            Assert.AreEqual(AsyncOperationStatus.Succeeded, instOp.Status);
            Assert.AreEqual(sceneKeys[1], instOp.Result.scene.name);
            UnityEngine.Object.DontDestroyOnLoad(instOp.Result);
            int bundleCountAfterInstantiate = AssetBundle.GetAllLoadedAssetBundles().Count();
            Assert.Greater(bundleCountAfterInstantiate,bundleCountBeforeTest);
            
            // Test
            yield return UnloadSceneFromHandlerRefCountCheck(activeScene, m_Addressables);
            
            Assert.NotNull(GameObject.Find(instOp.Result.name));
            Assert.IsFalse(activeScene.Result.Scene.isLoaded);
            int bundleCountAfterUnload = AssetBundle.GetAllLoadedAssetBundles().Count();
            Assert.AreEqual(bundleCountAfterInstantiate, bundleCountAfterUnload);
            
            Addressables.Release(activeScene);
            yield return activeScene;

            // Cleanup
            Assert.IsFalse(activeScene.IsValid());
            Addressables.Release(instOp);
            AssetBundleProvider.WaitForAllUnloadingBundlesToComplete();
            int bundleCountEndTest = AssetBundle.GetAllLoadedAssetBundles().Count();
            Assert.AreEqual(bundleCountBeforeTest,bundleCountEndTest);
            Assert.IsFalse(instOp.IsValid());
        }

        [UnityTest]
        public IEnumerator WhenUnloadScene_UnloadEmbeddedAssetsFlagWorks([Values(false,true)] bool unloadEmbeddedAssets)
        {
            // Create scene with embedded asset. Let's use Scriptable Object for ease of use
            var op = m_Addressables.LoadSceneAsync(kEmbeddedSceneName, LoadSceneMode.Additive);
            yield return op;

            // find the ScriptableObject. Get reference to it
            Mesh mesh = GameObject.Find("EmbededMeshGameObject").GetComponent<MeshFilter>().mesh;

            
            UnloadSceneOptions options = unloadEmbeddedAssets ? UnloadSceneOptions.UnloadAllEmbeddedSceneObjects : UnloadSceneOptions.None;
            var unloadOp = m_Addressables.UnloadSceneAsync(op, options, false);
            yield return unloadOp;

            Assert.AreEqual(mesh == null, unloadEmbeddedAssets);
        }
    }
#endif
    //[Bug: https://jira.unity3d.com/browse/ADDR-1215]
    //[UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    //class SceneTests_PackedMode : SceneTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
