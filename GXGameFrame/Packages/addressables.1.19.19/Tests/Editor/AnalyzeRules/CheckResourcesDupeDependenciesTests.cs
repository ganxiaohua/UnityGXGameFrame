using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests.AnalyzeRules
{
    public class CheckResourcesDupeDependenciesTests : AddressableAssetTestBase
    {
        string k_CheckDupePrefabA => GetAssetPath("checkDupe_prefabA.prefab");
        string k_CheckDupePrefabB => GetAssetPath("checkDupe_prefabB.prefab");
        string k_CheckDupeMyMaterial => GetAssetPath("checkDupe_myMaterial.mat");
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

            PrefabUtility.SaveAsPrefabAsset(prefabA, k_CheckDupePrefabA);
            PrefabUtility.SaveAsPrefabAsset(prefabB, k_CheckDupePrefabB);
            PrefabUtility.SaveAsPrefabAsset(prefabWithMaterial, k_PrefabWithMaterialPath);
            AssetDatabase.Refresh();
        }

        [Test]
        public void CheckResourcesDupe_ResourcesDependenciesMatchWithExplicitBundleDependencies()
        {
            var rule = new CheckResourcesDupeDependencies();
            rule.BuiltInResourcesToDependenciesMap(new string[] { k_CheckDupePrefabA });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>()
            {
                new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA))
            });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(k_CheckDupePrefabA));
            Assert.AreEqual(1, rule.m_ResourcesToDependencies[k_CheckDupePrefabA].Count);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA), rule.m_ResourcesToDependencies[k_CheckDupePrefabA][0].ToString());
        }

        [Test]
        public void CheckResourcesDupe_ResourcesDependenciesMatchWithImplicitBundleDependencies()
        {
            var rule = new CheckResourcesDupeDependencies();
            rule.BuiltInResourcesToDependenciesMap(new string[] { k_PrefabWithMaterialPath });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>()
            {
                new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial))
            });

            Assert.IsTrue(rule.m_ResourcesToDependencies.ContainsKey(k_PrefabWithMaterialPath));
            Assert.AreEqual(1, rule.m_ResourcesToDependencies[k_PrefabWithMaterialPath].Count);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial), rule.m_ResourcesToDependencies[k_PrefabWithMaterialPath][0].ToString());
        }

        [Test]
        public void CheckResourcesDupe_AllResourcesDependenciesAreReturned()
        {
            var rule = new CheckResourcesDupeDependencies();
            rule.BuiltInResourcesToDependenciesMap(new string[] { k_PrefabWithMaterialPath, k_CheckDupePrefabA });
            rule.IntersectResourcesDepedenciesWithBundleDependencies(new List<GUID>()
            {
                new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial)),
                new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA))
            });

            Assert.IsTrue(rule.m_ResourcesToDependencies[k_PrefabWithMaterialPath].Contains(new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial))));

            Assert.IsTrue(rule.m_ResourcesToDependencies[k_CheckDupePrefabA].Contains(new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupeMyMaterial))));
            Assert.IsTrue(rule.m_ResourcesToDependencies[k_CheckDupePrefabA].Contains(new GUID(AssetDatabase.AssetPathToGUID(k_CheckDupePrefabA))));
        }
    }
}
