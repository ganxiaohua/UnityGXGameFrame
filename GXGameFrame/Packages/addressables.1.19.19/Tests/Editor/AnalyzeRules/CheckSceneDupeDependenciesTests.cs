using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.SceneManagement;

namespace UnityEditor.AddressableAssets.Tests.AnalyzeRules
{
    public class CheckSceneDupeDependenciesTests : AddressableAssetTestBase
    {
        string k_CheckDupePrefabA => GetAssetPath("checkDupe_prefabA.prefab");
        string k_CheckDupePrefabB => GetAssetPath("checkDupe_prefabB.prefab");
        string k_CheckDupeMyMaterial => GetAssetPath("checkDupe_myMaterial.mat");
        string k_ScenePath => GetAssetPath("dupeSceneTest.unity");
        string k_PrefabWithMaterialPath => GetAssetPath("checkDupe_prefabWithMaterial.prefab");

        protected override void OnInit()
        {
            base.OnInit();

            GameObject prefabA = new GameObject("PrefabA");
            GameObject prefabB = new GameObject("PrefabB");
            GameObject prefabWithMaterial = new GameObject("PrefabWithMaterial");
            var meshA = prefabA.AddComponent<MeshRenderer>();
            var meshB = prefabB.AddComponent<MeshRenderer>();

            var mat = new Material(Shader.Find("Unlit/Color"));
            AssetDatabase.CreateAsset(mat, k_CheckDupeMyMaterial);
            meshA.sharedMaterial = mat;
            meshB.sharedMaterial = mat;

            var meshPrefabWithMaterial = prefabWithMaterial.AddComponent<MeshRenderer>();
            meshPrefabWithMaterial.material = AssetDatabase.LoadAssetAtPath<Material>(k_CheckDupeMyMaterial);

            prefabA.AddComponent<TestBehaviourWithReference>();

            PrefabUtility.SaveAsPrefabAsset(prefabA, k_CheckDupePrefabA);
            PrefabUtility.SaveAsPrefabAsset(prefabB, k_CheckDupePrefabB);
            PrefabUtility.SaveAsPrefabAsset(prefabWithMaterial, k_PrefabWithMaterialPath);
            AssetDatabase.Refresh();
        }

        [Test]
        public void CheckSceneDupe_SceneDependenciesMatchWithExplicitBundleDependencies()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(k_CheckDupePrefabA);

            GameObject n = new GameObject("TestGameObject");
            var refer = n.AddComponent<TestBehaviourWithReference>();
            refer.Reference = go;
            SceneManager.MoveGameObjectToScene(n, scene);
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            var rule = new CheckSceneDupeDependencies();

            EditorBuildSettingsScene editorScene = new EditorBuildSettingsScene(k_ScenePath, true);
            rule.BuiltInResourcesToDependenciesMap(new string[] { editorScene.path });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>() { new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA)) });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(editorScene.path));
            Assert.AreEqual(1, rule.m_ResourcesToDependencies[editorScene.path].Count);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA), rule.m_ResourcesToDependencies[editorScene.path][0].ToString());

            //Cleanup
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        [Test]
        public void CheckSceneDupe_SceneDependenciesMatchWithImplicitBundleDependencies()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(k_PrefabWithMaterialPath), scene);
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            var rule = new CheckSceneDupeDependencies();

            EditorBuildSettingsScene editorScene = new EditorBuildSettingsScene(k_ScenePath, true);
            rule.BuiltInResourcesToDependenciesMap(new string[] { editorScene.path });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>() { new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial)) });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(editorScene.path));
            Assert.AreEqual(1, rule.m_ResourcesToDependencies[editorScene.path].Count);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial), rule.m_ResourcesToDependencies[editorScene.path][0].ToString());

            //Cleanup
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }
        
        [Test]
        public void CheckSceneDupe_SceneDependenciesDoNotIncludeEditorOnly()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            GameObject go = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(k_PrefabWithMaterialPath), scene) as GameObject;
            go.tag = "EditorOnly";
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            var rule = new CheckSceneDupeDependencies();

            EditorBuildSettingsScene editorScene = new EditorBuildSettingsScene(k_ScenePath, true);
            rule.BuiltInResourcesToDependenciesMap(new string[] { editorScene.path });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>() { new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial)) });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(editorScene.path));
            Assert.AreEqual(0, rule.m_ResourcesToDependencies[editorScene.path].Count);

            //Cleanup
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        [Test]
        public void CheckSceneDupe_AllSceneToBundleDependenciesAreReturned()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(k_PrefabWithMaterialPath), scene);
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            var rule = new CheckSceneDupeDependencies();

            EditorBuildSettingsScene editorScene = new EditorBuildSettingsScene(k_ScenePath, true);
            rule.BuiltInResourcesToDependenciesMap(new string[] { editorScene.path });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>()
            {
                new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial))
            });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(editorScene.path));
            Assert.AreEqual(1, rule.m_ResourcesToDependencies[editorScene.path].Count);
            Assert.IsTrue(rule.m_ResourcesToDependencies[editorScene.path].Contains(new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial))));

            //Cleanup
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }
        
        [Test]
        public void CheckSceneDupe_SceneDependenciesDoNotIncludeScripts()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(k_CheckDupePrefabA);
            GameObject g = PrefabUtility.InstantiatePrefab(go, scene) as GameObject;
            g.AddComponent<TestBehaviourWithReference>();
            EditorSceneManager.SaveScene(scene, k_ScenePath);

            var rule = new CheckSceneDupeDependencies();

            EditorBuildSettingsScene editorScene = new EditorBuildSettingsScene(k_ScenePath, true);
            rule.BuiltInResourcesToDependenciesMap(new string[] { editorScene.path });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(editorScene.path));
            bool containsAnyScripts = false;
            foreach ( GUID guid in rule.m_ResourcesToDependencies[editorScene.path])
            {
                string path = AssetDatabase.GUIDToAssetPath(guid.ToString());
                if (path.EndsWith(".cs") || path.EndsWith(".dll"))
                {
                    containsAnyScripts = true;
                    break;
                }
            }
            Assert.IsFalse(containsAnyScripts, "Scripts were included as a duplciate dependency");

            //Cleanup
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }
    }
}
