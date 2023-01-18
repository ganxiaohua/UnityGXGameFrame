using System;

namespace UnityEngine.AddressableAssets.Tests
{
    [CreateAssetMenu(order = 0, fileName = "towsf", menuName = "Test/TestObjectWithSerializableField")]
    public class TestObjectWithSerializableField : ScriptableObject
    {
        [SerializeField] private SerializableClass ByValueClass;

        static public TestObjectWithSerializableField Create(string name)
        {
            var obj = CreateInstance<TestObjectWithSerializableField>();
            obj.name = name;
            return obj;
        }
    }

    [Serializable]
    public class SerializableClass
    {
    }
}
