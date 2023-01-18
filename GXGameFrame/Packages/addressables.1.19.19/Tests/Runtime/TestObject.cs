using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AddressableAssets.Tests
{
    [CreateAssetMenu(order = 0, fileName = "to", menuName = "Test/TestObject")]
    public class TestObject : ScriptableObject
    {
        static public TestObject Create(string name)
        {
            var obj = CreateInstance<TestObject>();
            obj.name = name;
            return obj;
        }
        
        #if UNITY_EDITOR
        static public TestObject Create(string name, string assetPath)
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
        #endif
    }
}
