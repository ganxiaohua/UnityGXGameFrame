using UnityEngine;

namespace UnityEditor.AddressableAssets.Tests
{
    public class TestObject : ScriptableObject
    {
        public static TestObject Create(string name, string assetPath = null)
        {
            var obj = CreateInstance<TestObject>();
            obj.name = name;
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.CreateAsset(obj, assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
            return obj;
        }

        internal void AddTestSubObject()
        {
            TestSubObject n = ScriptableObject.CreateInstance<TestSubObject>();
            n.name = "testSubObject";
            AssetDatabase.AddObjectToAsset(n, this);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }
    }
}
