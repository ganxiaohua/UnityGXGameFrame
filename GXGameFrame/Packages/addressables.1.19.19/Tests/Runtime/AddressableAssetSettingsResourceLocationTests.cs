using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.TestTools;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
#endif

namespace AddressableAssetSettingsResourceLocationTests
{
    public abstract class AddressableAssetSettingsResourceLocationTests : AddressablesTestFixture
    {
        const string k_ValidKey = "key";
        const string k_InvalidKey = "[key]";

#if UNITY_EDITOR
        internal override void Setup(AddressableAssetSettings settings, string tempAssetFolder)
        {
            GameObject testObject = new GameObject("TestObject");
            GameObject testObject2 = new GameObject("TestObject2");
            string path = tempAssetFolder + "/test.prefab";
            string path2 = tempAssetFolder + "/test2.prefab";
            PrefabUtility.SaveAsPrefabAsset(testObject, path);
            PrefabUtility.SaveAsPrefabAsset(testObject2, path2);
            string guid = AssetDatabase.AssetPathToGUID(path);
            string guid2 = AssetDatabase.AssetPathToGUID(path2);

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = k_ValidKey;

            entry = settings.CreateOrMoveEntry(guid2, settings.DefaultGroup);
            entry.address = k_InvalidKey;

            bool currentIgnoreState = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = false;
            LogAssert.Expect(LogType.Error, $"Address '{entry.address}' cannot contain '[ ]'.");
            LogAssert.ignoreFailingMessages = currentIgnoreState;
        }

        protected override void OnRuntimeSetup()
        {
            // Only keep AddressableAssetSettingsLocator
            List<IResourceLocator> locators = m_Addressables.ResourceLocators.ToList();
            foreach (IResourceLocator locator in locators)
            {
                if (locator.GetType() != typeof(AddressableAssetSettingsLocator))
                    m_Addressables.RemoveResourceLocator(locator);
            }
        }

        [Test]
        public void WhenKeyIsValid_AddressableAssetSettingsLocator_ReturnsLocations()
        {
            var res = m_Addressables.GetResourceLocations(k_ValidKey, typeof(GameObject), out IList<IResourceLocation> locs);
            Assert.IsTrue(res);
            Assert.IsNotNull(locs);
            Assert.AreEqual(1, locs.Count);
        }

        [Test]
        public void WhenKeyHasSquareBrackets_AddressableAssetSettingsLocator_ThrowsExceptionAndReturnsNoLocations()
        {
            var res = m_Addressables.GetResourceLocations(k_InvalidKey, typeof(GameObject), out IList<IResourceLocation> locs);
            LogAssert.Expect(LogType.Error, $"Address '{k_InvalidKey}' cannot contain '[ ]'.");
            Assert.IsFalse(res);
            Assert.IsNull(locs);
        }

#endif
    }

#if UNITY_EDITOR
    class AddressableAssetSettingsResourceLocationTests_FastMode : AddressableAssetSettingsResourceLocationTests { protected override TestBuildScriptMode BuildScriptMode { get { return TestBuildScriptMode.Fast; } } }
#endif
}
