#if UNITY_2019_3_OR_NEWER
using NUnit.Framework;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
#endif
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BuiltInDataTests
{
    abstract class BuiltInDataTests : AddressablesTestFixture
    {
        const string prefabKey = "prefabKey";
        const string sceneKey = "sceneKey";
        int m_StartingSceneCount;
#if UNITY_EDITOR
        EditorBuildSettingsScene[] m_BuiltInSceneCache;

        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            AddressableAssetSettings.CreateBuiltInData(settings);

            AssetDatabase.CreateFolder(tempAssetFolder, "Resources");
            string prefabPath = CreateAssetPath(Path.Combine(tempAssetFolder, "Resources"), prefabKey, ".prefab");
            CreatePrefab(prefabPath);

            string builtInScenePath = CreateAssetPath(tempAssetFolder, sceneKey, ".unity");
            CreateScene(builtInScenePath);
            m_BuiltInSceneCache = BuiltinSceneCache.scenes;
            BuiltinSceneCache.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(builtInScenePath, true)
            };
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
#if UNITY_EDITOR
            BuiltinSceneCache.scenes = m_BuiltInSceneCache;
#endif
        }

        [UnityTest]
        public IEnumerator WhenSceneIsInScenesList_LoadSceneAsync_Succeeds()
        {
            var op = m_Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Additive);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
            Assert.AreEqual(sceneKey, SceneManager.GetSceneByName(sceneKey).name);

            yield return UnloadSceneFromHandler(op, m_Addressables);
        }

        [UnityTest]
        public IEnumerator WhenAssetIsInResources_LoadAssetAsync_Succeeds()
        {
            var op = m_Addressables.LoadAssetAsync<GameObject>(prefabKey);
            yield return op;
            Assert.AreEqual(AsyncOperationStatus.Succeeded, op.Status);
        }
    }

#if UNITY_EDITOR
    class BuiltInDataTests_VirtualMode : BuiltInDataTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class BuiltInDataTests_PackedPlaymodeMode : BuiltInDataTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif
    //[Bug: https://jira.unity3d.com/browse/ADDR-1215]
    //[UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    //class BuiltInDataTests_PackedMode : BuiltInDataTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
#endif
