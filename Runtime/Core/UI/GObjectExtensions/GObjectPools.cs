using System;
using System.Collections.Generic;

namespace GameFrame.Runtime
{
    public class GObjectPools : Singleton<GObjectPools>
    {
        public class GObjectPoolsData
        {
            public object Pool;
            public int Count;
        }

        private Dictionary<Type, GObjectPoolsData> dictionary = new();

        public ObjectPool<T> Spawn<T>(GObjectBaseData obj) where T : GObjectBase, new()
        {
            Type t = typeof(T);
            if (!dictionary.TryGetValue(t, out var poolData))
            {
                poolData = new GObjectPoolsData();
                poolData.Pool = ObjectPoolManager.Instance.CreateObjectPool<T>("GObjectPool", 128, 20, obj);
                dictionary.Add(t, poolData);
            }

            poolData.Count++;
            return (ObjectPool<T>) poolData.Pool;
        }

        public void Unspawn<T>() where T : GObjectBase, new()
        {
            Type t = typeof(T);
            var succ = dictionary.TryGetValue(t, out var poolData);
            if (!succ)
                return;
            if (--poolData.Count == 0)
            {
                ObjectPoolManager.Instance.DeleteObjectPool((ObjectPool<T>) poolData.Pool);
                dictionary.Remove(t);
            }
        }

        public void Clear()
        {
        }
    }
}