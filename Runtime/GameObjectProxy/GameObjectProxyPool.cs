using System;

namespace GameFrame.Runtime
{
    public class GameObjectProxyPool : Singleton<GameObjectProxyPool>, IDisposable
    {
        private ObjectPool<GameObjectProxy> objectPool;

        public GameObjectProxyPool()
        {
            objectPool = ObjectPoolManager.Instance.CreateObjectPool<GameObjectProxy>("GameObjectProxyPool", 128, 10);
        }

        public GameObjectProxy Spawn()
        {
            return objectPool.Spawn();
        }

        public void UnSpawn(GameObjectProxy go)
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