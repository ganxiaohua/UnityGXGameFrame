using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AddressableAssets.Tests
{
    public class TestObject2 : ScriptableObject
    {
        static public TestObject2 Create(string name)
        {
            var so = CreateInstance<TestObject2>();
            so.name = name;
            return so;
        }
    }
}
