using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets.DynamicResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
#endif

namespace Addressables.SamplesTests
{
    public abstract class SamplesTests : AddressablesTestFixture
    {
        string m_AssetReferenceObjectKey = nameof(m_AssetReferenceObjectKey);

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            AddressableAssetGroup assetReference = settings.CreateGroup("assetReferenceSamplesGroup", false, false, true,
                new List<AddressableAssetGroupSchema>(), typeof(BundledAssetGroupSchema));

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            AssetReferenceTestBehavior behavior = go.AddComponent<AssetReferenceTestBehavior>();

            string hasBehaviorPath = tempAssetFolder + "/AssetReferenceBehavior.prefab";

            string referencePath = tempAssetFolder + "/reference.prefab";
            string guid = CreatePrefab(referencePath);
            behavior.Reference = settings.CreateAssetReference(guid);
            behavior.AssetReferenceAddress = referencePath.Replace("\\", "/");

            PrefabUtility.SaveAsPrefabAsset(go, hasBehaviorPath);
            settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(hasBehaviorPath), assetReference, false, false).address = m_AssetReferenceObjectKey;
        }
#endif

        [UnityTest]
        public IEnumerator Samples_GetAddressFromAssetReference_ReturnsCorrectAddress()
        {
            var savedImpl = UnityEngine.AddressableAssets.Addressables.m_AddressablesInstance; 
            UnityEngine.AddressableAssets.Addressables.m_AddressablesInstance = m_Addressables;

            AsyncOperationHandle assetReferenceHandle = m_Addressables.InstantiateAsync(m_AssetReferenceObjectKey);
            yield return assetReferenceHandle;
            Assert.IsNotNull(assetReferenceHandle.Result as GameObject);
            AssetReferenceTestBehavior behavior =
                (assetReferenceHandle.Result as GameObject).GetComponent<AssetReferenceTestBehavior>();

            string returnedAddress = AddressablesUtility.GetAddressFromAssetReference(behavior.Reference);

            Assert.AreEqual(behavior.AssetReferenceAddress, returnedAddress);

            m_Addressables.Release(assetReferenceHandle);
            UnityEngine.AddressableAssets.Addressables.m_AddressablesInstance = savedImpl;
        }
    }

#if UNITY_EDITOR
    class SamplesTests_FastMode : SamplesTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }

    class SamplesTests_VirtualMode : SamplesTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Virtual; } } }

    class SamplesTests_PackedPlaymodeMode : SamplesTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.PackedPlaymode; } } }
#endif

    //Unable to run tests in standalone given how upm-ci handles Samples.  May be possible with a different test setup than currently available.
    //[UnityPlatform(exclude = new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor })]
    //class SamplesTests_PackedMode : SamplesTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Packed; } } }
}
