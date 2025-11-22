using System;
using UnityEngine;

namespace GameFrame.Runtime
{
    public class UnitGoPool : Singleton<UnitGoPool>, IDisposable
    {
        private ObjectPool<UnityGameObjectItem> objectPool;

        public UnitGoPool()
        {
            objectPool = ObjectPoolManager.Instance.CreateObjectPool<UnityGameObjectItem>("UnitGoPool", 128, 30);
        }

        public UnityGameObjectItem Spawn(Transform parent)
        {
            return objectPool.Spawn(parent);
        }

        public void UnSpawn(UnityGameObjectItem go)
        {
            objectPool.UnSpawn(go);
        }

        public void Dispose()
        {
            ObjectPoolManager.Instance.DeleteObjectPool(objectPool);
            objectPool = null;
        }
    }
}